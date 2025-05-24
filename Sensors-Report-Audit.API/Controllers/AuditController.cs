using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SensorsReportAudit.Models;

namespace SensorsReportAudit.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly ILogger<AuditController> _logger;
        private readonly AuditConfig _config;
        private readonly HttpClient _httpClient;

        public AuditController(ILogger<AuditController> logger, AuditConfig config, HttpClient httpClient)
        {
            _logger = logger;
            _config = config;
            _httpClient = httpClient;
        }

        /// <summary>
        /// POST: /audit
        /// Store an audit record in QuantumLeap
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StoreAuditRecord([FromBody] AuditRecord auditRecord)
        {
            try
            {
                _logger.LogInformation("Received audit record with ID: {Id}", auditRecord.Id);

                // Add a timestamp if not present
                if (auditRecord.Timestamp == default)
                {
                    auditRecord.Timestamp = DateTime.UtcNow;
                }

                // Convert to QuantumLeap entity format
                var entity = auditRecord.ToQuantumLeapEntity();
                var json = JsonConvert.SerializeObject(entity);

                // Send to QuantumLeap
                var endpoint = $"{_config.QuantumLeapBaseUrl}/v2/entities";
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Forward the request to QuantumLeap
                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error storing audit record in QuantumLeap: {Error}", errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                _logger.LogInformation("Successfully stored audit record with ID: {Id}", auditRecord.Id);
                return Created($"/audit/{auditRecord.Id}", auditRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audit record");
                return StatusCode(500, "An error occurred while processing the audit record");
            }
        }

        /// <summary>
        /// GET: /audit
        /// Retrieve audit records from QuantumLeap based on filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditRecords(
            [FromQuery] string? entityId = null,
            [FromQuery] string? entityType = "AuditRecord",
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null,
            [FromQuery] string? userId = null,
            [FromQuery] string? resourceType = null,
            [FromQuery] int limit = 100,
            [FromQuery] int offset = 0)
        {
            try
            {
                _logger.LogInformation("Retrieving audit records");

                // Build query parameters
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(entityId))
                {
                    queryParams.Add($"id={WebUtility.UrlEncode(entityId)}");
                }
                
                if (!string.IsNullOrEmpty(entityType))
                {
                    queryParams.Add($"type={WebUtility.UrlEncode(entityType)}");
                }
                
                // Add date range filters if provided
                if (!string.IsNullOrEmpty(fromDate))
                {
                    queryParams.Add($"fromDate={WebUtility.UrlEncode(fromDate)}");
                }
                
                if (!string.IsNullOrEmpty(toDate))
                {
                    queryParams.Add($"toDate={WebUtility.UrlEncode(toDate)}");
                }
                
                // Add pagination
                queryParams.Add($"limit={limit}");
                queryParams.Add($"offset={offset}");

                // Build QuantumLeap API URL
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
                var url = $"{_config.QuantumLeapBaseUrl}/v2/entities{queryString}";

                // Forward the request to QuantumLeap
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error retrieving audit records from QuantumLeap: {Error}", errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var content = await response.Content.ReadAsStringAsync();
                
                // Post-filter for userId and resourceType if needed
                if (!string.IsNullOrEmpty(userId) || !string.IsNullOrEmpty(resourceType))
                {
                    var entities = JsonConvert.DeserializeObject<List<QuantumLeapEntity>>(content);
                    if (entities != null)
                    {
                        if (!string.IsNullOrEmpty(userId))
                        {
                            entities = entities.Where(e => 
                                e.Attributes.TryGetValue("userId", out var attr) && 
                                attr.Value.ToString() == userId).ToList();
                        }
                        
                        if (!string.IsNullOrEmpty(resourceType))
                        {
                            entities = entities.Where(e => 
                                e.Attributes.TryGetValue("resourceType", out var attr) && 
                                attr.Value.ToString() == resourceType).ToList();
                        }
                        
                        content = JsonConvert.SerializeObject(entities);
                    }
                }

                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit records");
                return StatusCode(500, "An error occurred while retrieving audit records");
            }
        }
    }
}