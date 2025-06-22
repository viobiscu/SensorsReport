using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SensorsReportBusinessBroker.API.Configuration;
using SensorsReportBusinessBroker.API.Models;

namespace SensorsReportBusinessBroker.API.Services
{
    public interface IAuditService
    {
        Task SendAuditEventAsync(AuditEvent auditEvent, string tenantId);
    }

    public class AuditService : IAuditService
    {
        private readonly HttpClient _httpClient;
        private readonly AppConfig _config;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuditService(HttpClient httpClient, AppConfig config)
        {
            _httpClient = httpClient;
            _config = config;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task SendAuditEventAsync(AuditEvent auditEvent, string tenantId)
        {
            try
            {
                var url = $"{_config.AuditServiceUrl}/api/audit";
                var content = new StringContent(
                    JsonSerializer.Serialize(auditEvent, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                // Add headers if needed
                if (!string.IsNullOrEmpty(tenantId))
                {
                    request.Headers.Add("X-Tenant-Id", tenantId);
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log the exception but don't propagate it to avoid breaking the main flow
                Console.WriteLine($"Error sending audit event: {ex.Message}");
            }
        }
    }
}