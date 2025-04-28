using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using RulesEngine.Models;
using SensorsReportBusinessBroker.API.Models;
using SensorsReportBusinessBroker.API.Services;

namespace SensorsReportBusinessBroker.API.Controllers
{
    [ApiController]
    [Route("v1")]
    public class NotificationController : ControllerBase
    {
        private readonly IOrionService _orionService;
        private readonly IAuditService _auditService;
        private readonly RulesEngine.RulesEngine _rulesEngine;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public NotificationController(
            IOrionService orionService,
            IAuditService auditService, 
            RulesEngine.RulesEngine rulesEngine)
        {
            _orionService = orionService;
            _auditService = auditService;
            _rulesEngine = rulesEngine;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpPost("notify")]
        [Consumes("application/ld+json")]
        public async Task<IActionResult> Notify([FromBody] NotificationRequest notification)
        {
            try
            {
                // Extract tenant and location from headers
                string tenantId = Request.Headers.TryGetValue("NGSILD-Tenant", out var tenant) ? tenant.ToString() : string.Empty;
                string location = Request.Headers.TryGetValue("Fiware-ServicePath", out var path) ? path.ToString() : string.Empty;

                // Log the notification
                _logger.Info($"Received notification: {JsonSerializer.Serialize(notification, _jsonOptions)}");

                // Get business rules from Orion-LD
                var businessRules = await _orionService.GetBusinessRulesAsync(tenantId);
                
                if (businessRules.Entities.Length == 0)
                {
                    _logger.Info("No business rules found");
                    return Ok();
                }

                // Process each entity in the notification
                foreach (var entity in notification.Data ?? new List<Dictionary<string, object>>())
                {
                    string entityId = entity["id"]?.ToString() ?? string.Empty;
                    string entityType = entity["type"]?.ToString() ?? string.Empty;

                    // Apply business rules
                    await ProcessBusinessRules(businessRules.Entities, entity, entityId, entityType, tenantId);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error processing notification: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        private async Task ProcessBusinessRules(
            BusinessRuleEntity[] businessRules, 
            Dictionary<string, object> entityData, 
            string entityId, 
            string entityType, 
            string tenantId)
        {
            foreach (var rule in businessRules.OrderBy(r => r.Priority?.Value ?? 999))
            {
                if (rule.Enabled?.Value != true || 
                    !string.IsNullOrEmpty(rule.TargetEntityType?.Value) && 
                    rule.TargetEntityType.Value != entityType)
                {
                    continue;
                }

                try
                {
                    // Parse rule content as a workflow rule
                    var ruleContent = rule.RuleContent?.Value;
                    if (string.IsNullOrEmpty(ruleContent))
                    {
                        continue;
                    }

                    // Parse rule content
                    var workflowRule = JsonSerializer.Deserialize<Workflow>(ruleContent, _jsonOptions);
                    if (workflowRule == null)
                    {
                        continue;
                    }

                    _rulesEngine.AddOrUpdateWorkflow(workflowRule);
                    
                    // Convert entity to input for rules engine
                    var input = new RuleParameter("entity", entityData);
                    
                    // Execute rules
                    var results = await _rulesEngine.ExecuteAllRulesAsync(workflowRule.WorkflowName, input);
                    
                    // Process results
                    foreach (var result in results.Where(r => r.IsSuccess))
                    {
                        if (result.ActionResult == null)
                            continue;

                        try
                        {
                            // Convert ActionResult to JSON then to Dictionary
                            string actionResultJson = JsonSerializer.Serialize(result.ActionResult, _jsonOptions);
                            var actionResult = JsonSerializer.Deserialize<Dictionary<string, object>>(actionResultJson, _jsonOptions);

                            if (actionResult == null || !actionResult.TryGetValue("action", out var actionObj))
                                continue;

                            // Get action string
                            string action = actionObj?.ToString() ?? string.Empty;
                            if (string.IsNullOrEmpty(action))
                                continue;

                            switch (action)
                            {
                                case "UpdateOrion":
                                    if (actionResult.TryGetValue("data", out var dataObj))
                                    {
                                        await _orionService.UpdateEntityAsync(entityId, entityType, dataObj, tenantId);
                                    }
                                    break;
                                
                                case "SendAudit":
                                    if (actionResult.TryGetValue("auditData", out var auditObj))
                                    {
                                        // Convert audit data to AuditEvent
                                        string auditJson = JsonSerializer.Serialize(auditObj, _jsonOptions);
                                        var auditEvent = JsonSerializer.Deserialize<AuditEvent>(auditJson, _jsonOptions);
                                        
                                        if (auditEvent != null)
                                        {
                                            auditEvent.EntityId = entityId;
                                            auditEvent.EntityType = entityType;
                                            auditEvent.Timestamp = DateTime.UtcNow.ToString("o");
                                            
                                            await _auditService.SendAuditEventAsync(auditEvent, tenantId);
                                        }
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Error processing rule result: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error executing rule {rule.Id}: {ex.Message}");
                    // Continue with other rules even if one fails
                }
            }
        }
    }
}