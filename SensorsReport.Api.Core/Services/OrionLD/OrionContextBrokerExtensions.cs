
using System.Text.Json;

namespace SensorsReport.OrionLD;

public static class OrionContextBrokerExtensions
{
    public static string CreateEntityUrn(string entityType, string entityId)
    {
        if (string.IsNullOrEmpty(entityType))
            throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));
        if (string.IsNullOrEmpty(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

        if (entityId.StartsWith("urn:ngsi-ld:") && entityId.Contains(':') && entityId.Split(':').Length > 3 && entityId.Split(':')[2] == entityType)
            return entityId;

        entityId = entityId.Split(':').LastOrDefault()?.Trim() ?? entityId;

        return $"urn:ngsi-ld:{entityType}:{entityId}";
    }

    public static async Task<T?> GetContentAsAsync<T>(this HttpResponseMessage response) where T : class
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Deserialize<T>(content, options);
        }
        catch
        {
            return null;
        }
    }
}
