using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SensorsReportBusinessBroker.API.Configuration;
using SensorsReportBusinessBroker.API.Models;

namespace SensorsReportBusinessBroker.API.Services
{
    public interface IOrionService
    {
        Task<BusinessRuleEntities> GetBusinessRulesAsync(string tenantId);
        Task UpdateEntityAsync(string entityId, string entityType, object data, string tenantId);
    }

    public class OrionService : IOrionService
    {
        private readonly HttpClient _httpClient;
        private readonly AppConfig _config;
        private readonly JsonSerializerOptions _jsonOptions;

        public OrionService(HttpClient httpClient, AppConfig config)
        {
            _httpClient = httpClient;
            _config = config;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<BusinessRuleEntities> GetBusinessRulesAsync(string tenantId)
        {
            var url = $"{_config.OrionUrl}/ngsi-ld/v1/entities?type=BusinessRule";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Add headers
            request.Headers.Add("Accept", "application/ld+json");
            if (!string.IsNullOrEmpty(tenantId))
            {
                request.Headers.Add("NGSILD-Tenant", tenantId);
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var entities = JsonSerializer.Deserialize<BusinessRuleEntity[]>(content, _jsonOptions);
            
            return new BusinessRuleEntities { Entities = entities ?? Array.Empty<BusinessRuleEntity>() };
        }

        public async Task UpdateEntityAsync(string entityId, string entityType, object data, string tenantId)
        {
            var url = $"{_config.OrionUrl}/ngsi-ld/v1/entities/{entityId}/attrs";
            var content = new StringContent(
                JsonSerializer.Serialize(data, _jsonOptions),
                Encoding.UTF8,
                "application/ld+json");

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = content
            };

            // Add headers
            if (!string.IsNullOrEmpty(tenantId))
            {
                request.Headers.Add("NGSILD-Tenant", tenantId);
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}