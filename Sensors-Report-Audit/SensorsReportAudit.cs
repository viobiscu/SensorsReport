using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SensorsReportAudit.Auth;
using SensorsReportAudit.Models;

namespace SensorsReportAudit
{
    /// <summary>
    /// Main class for SensorsReportAudit library that allows other applications to log audit data to QuantumLeap
    /// </summary>
    public class SensorsReportAudit
    {
        private readonly AuditConfig _config;
        private readonly KeycloakAuthService _authService;
        private readonly ILogger<SensorsReportAudit> _logger;
        private readonly string _apiEndpoint;

        /// <summary>
        /// Initializes a new instance of the SensorsReportAudit class with default configuration from environment variables
        /// </summary>
        /// <param name="logger">Optional logger</param>
        public SensorsReportAudit(ILogger<SensorsReportAudit>? logger = null)
            : this(AuditConfig.FromEnvironment(), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SensorsReportAudit class with the specified configuration
        /// </summary>
        /// <param name="config">Configuration for SensorsReportAudit</param>
        /// <param name="logger">Optional logger</param>
        public SensorsReportAudit(AuditConfig config, ILogger<SensorsReportAudit>? logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? CreateDefaultLogger();
            
            var authLogger = logger == null ? CreateDefaultAuthLogger() : 
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<KeycloakAuthService>();
            
            _authService = new KeycloakAuthService(config, authLogger);
            _apiEndpoint = $"{_config.QuantumLeapBaseUrl}/v2/entities";
        }

        /// <summary>
        /// Logs an audit event to quantum        /// </summary>
        /// <param name="auditRecord">The audit record to log</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> LogAuditAsync(AuditRecord auditRecord)
        {
            try
            {
                if (auditRecord == null)
                {
                    throw new ArgumentNullException(nameof(auditRecord));
                }

                // Convert to QuantumLeap entity format
                var entity = auditRecord.ToQuantumLeapEntity();
                var json = JsonConvert.SerializeObject(entity);

                // Get an authorized HTTP client with a valid token
                using var client = await _authService.CreateAuthorizedClientAsync();
                
                // Send to QuantumLeap 
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_apiEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to log audit record. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("Audit record logged successfully. ID: {Id}", auditRecord.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit record");
                return false;
            }
        }

        /// <summary>
        /// Creates a default logger when none is provided
        /// </summary>
        private ILogger<SensorsReportAudit> CreateDefaultLogger()
        {
            return LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<SensorsReportAudit>();
        }

        /// <summary>
        /// Creates a default auth logger when none is provided
        /// </summary>
        private ILogger<KeycloakAuthService> CreateDefaultAuthLogger()
        {
            return LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<KeycloakAuthService>();
        }
    }
}