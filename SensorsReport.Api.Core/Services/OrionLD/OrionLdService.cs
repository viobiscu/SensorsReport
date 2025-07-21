using Microsoft.Extensions.Options;
using SensorsReport;
using SensorsReport.OrionLD.Extensions;
using System.Text;
using System.Text.Json;

namespace SensorsReport.OrionLD;


public class OrionLdService : BaseHttpService<OrionLdService>, IOrionLdService
{
    private string _tenant = "synchro";

    public OrionLdConfig Config { get; }

    private static class Endpoints
    {
        public const string Subscriptions = "/ngsi-ld/v1/subscriptions";
        public const string Entities = "/ngsi-ld/v1/entities";
    }

    public OrionLdService(IOptions<OrionLdConfig> config, IHttpClientFactory httpClientFactory, JsonSerializerOptions? jsonOptions = null)
        : base(httpClientFactory, jsonOptions)
    {
        Config = config?.Value ?? throw new ArgumentNullException(nameof(config), "AppConfig cannot be null");
    }

    public void SetTenant(string? tenant)
    {
        if (string.IsNullOrEmpty(tenant) || Config.MainTenant.Equals(tenant, StringComparison.OrdinalIgnoreCase))
        {
            _tenant = this.Config.MainTenant;
        }
        else
        {
            _tenant = tenant;
        }
    }

    public async Task<HttpResponseMessage> GetSubscriptionsAsync(int offset = 0, int limit = 100)
    {
        return await this.GetAsync($"{Endpoints.Subscriptions}?offset={offset}&limit={limit}");
    }

    public async Task<T?> GetSubscriptionsAsync<T>(int offset = 0, int limit = 100) where T : class
    {
        var response = await this.GetSubscriptionsAsync(offset, limit);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.GetContentAsAsync<T>();
    }

    public async Task<HttpResponseMessage> GetSubscriptionByIdAsync(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));
        return await this.GetAsync($"{Endpoints.Subscriptions}/{subscriptionId}");
    }

    public async Task<T?> GetSubscriptionByIdAsync<T>(string subscriptionId) where T : class
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));
        var response = await this.GetSubscriptionByIdAsync(subscriptionId);
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.GetContentAsAsync<T>();
    }

    public async Task<HttpResponseMessage> CreateSubscriptionAsync(SubscriptionModel subscription)
    {
        var jsonContent = JsonSerializer.Serialize(subscription, _jsonOptions);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        return await this.PostAsync(Endpoints.Subscriptions, stringContent);
    }

    public async Task<HttpResponseMessage> UpdateSubscriptionAsync(string subscriptionId, SubscriptionModel subscription)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));
        var jsonContent = JsonSerializer.Serialize(subscription, _jsonOptions);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        return await this.PatchAsync($"{Endpoints.Subscriptions}/{subscriptionId}", stringContent);
    }

    public async Task<HttpResponseMessage> DeleteSubscriptionAsync(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));
        return await this.DeleteAsync($"{Endpoints.Subscriptions}/{subscriptionId}");
    }

    public async Task<HttpResponseMessage> GetEntitiesAsync(int offset = 0, int limit = 100, string? type = null)
    {
        var query = $"{Endpoints.Entities}?offset={offset}&limit={limit}";
        if (!string.IsNullOrEmpty(type))
            query += $"&type={Uri.EscapeDataString(type)}";

        return await this.GetAsync(query);
    }

    public async Task<T?> GetEntitiesAsync<T>(int offset = 0, int limit = 100, string? type = null) where T : class
    {
        var response = await GetEntitiesAsync(offset, limit, type);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.GetContentAsAsync<T>();
    }

    public async Task<HttpResponseMessage> GetEntityByIdAsync(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));
        return await GetAsync($"{Endpoints.Entities}/{entityId}");
    }

    public async Task<T?> GetEntityByIdAsync<T>(string entityId) where T : class
    {
        if (string.IsNullOrEmpty(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

        return await GetJsonAsync<T>($"{Endpoints.Entities}/{entityId}");
    }

    public async Task<HttpResponseMessage> CreateEntityAsync(object entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

        var jsonContent = JsonSerializer.Serialize(entity, _jsonOptions);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        return await this.PostAsync(Endpoints.Entities, stringContent);
    }

    public async Task<HttpResponseMessage> UpdateEntityAsync(string entityId, object entity)
    {
        if (string.IsNullOrEmpty(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

        var jsonContent = JsonSerializer.Serialize(entity, _jsonOptions);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        return await this.PatchAsync($"{Endpoints.Entities}/{entityId}", stringContent);
    }

    public async Task<HttpResponseMessage> ReplaceEntityAsync(string entityId, object entity)
    {
        if (string.IsNullOrEmpty(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

        var jsonContent = JsonSerializer.Serialize(entity, _jsonOptions);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        return await this.PutAsync($"{Endpoints.Entities}/{entityId}", stringContent);
    }

    public async Task<HttpResponseMessage> DeleteEntityAsync(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

        return await this.DeleteAsync($"{Endpoints.Entities}/{entityId}");
    }

    public static (bool IsValid, string? ErrorMessage) ValidateSubscriptionPayload(object? payload)
    {
        if (payload == null)
            return (false, "Payload cannot be empty");

        JsonElement jsonElement;

        // Handle different payload types
        if (payload is string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return (false, "Payload cannot be empty");

            try
            {
                jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
            }
            catch (JsonException)
            {
                return (false, "Payload must be a valid JSON object");
            }
        }
        else if (payload is JsonElement element)
        {
            jsonElement = element;
        }
        else
        {
            // Try to serialize and deserialize other object types
            try
            {
                var serialized = JsonSerializer.Serialize(payload);
                jsonElement = JsonSerializer.Deserialize<JsonElement>(serialized);
            }
            catch (JsonException)
            {
                return (false, "Payload must be a JSON object");
            }
        }

        // Check if it's a JSON object
        if (jsonElement.ValueKind != JsonValueKind.Object)
            return (false, "Payload must be a JSON object");

        // Required fields for a valid subscription
        var requiredFields = new[] { "type", "notification" };

        // Check required fields
        foreach (var field in requiredFields)
        {
            if (!jsonElement.TryGetProperty(field, out _))
                return (false, $"Missing required field: {field}");
        }

        // Validate type
        if (jsonElement.TryGetProperty("type", out var typeProperty))
        {
            if (typeProperty.ValueKind != JsonValueKind.String ||
                typeProperty.GetString() != "Subscription")
            {
                return (false, "type field must be 'Subscription'");
            }
        }

        // Validate notification object
        if (jsonElement.TryGetProperty("notification", out var notificationProperty))
        {
            if (notificationProperty.ValueKind != JsonValueKind.Object)
                return (false, "notification must be an object");

            if (!notificationProperty.TryGetProperty("endpoint", out _))
                return (false, "notification must contain an endpoint");
        }

        return (true, null);
    }

    private void SetTenantHeaders(string tenant, HttpRequestMessage httpRequestMessage)
    {
        if (httpRequestMessage == null)
            throw new ArgumentNullException(nameof(httpRequestMessage), "HttpRequestMessage cannot be null");

        // Remove existing tenant headers if exist
        httpRequestMessage.Headers.Remove("Fiware-Service");
        httpRequestMessage.Headers.Remove("NGSILD-Tenant");

        if (!string.IsNullOrEmpty(tenant) &&
            !Config.MainTenant.Equals(tenant, StringComparison.OrdinalIgnoreCase))
        {
            httpRequestMessage.Headers.Add("Fiware-Service", tenant);
            httpRequestMessage.Headers.Add("NGSILD-Tenant", tenant);
        }
        else
        {
            var requestUri = httpRequestMessage.RequestUri?.ToString() ?? throw new InvalidOperationException("Request URI cannot be null");
            if (requestUri.StartsWith(Endpoints.Entities, StringComparison.OrdinalIgnoreCase))
            {
                var uriBuilder = new UriBuilder(Config.BrokerUrl);
                uriBuilder.Path = requestUri.Split('?')[0];
                var query = System.Web.HttpUtility.ParseQueryString(requestUri.Split('?').Length > 1 ? requestUri.Split('?')[1] : string.Empty);
                query["local"] = "true";
                uriBuilder.Query = query.ToString();
                httpRequestMessage.RequestUri = uriBuilder.Uri;
            }
        }
    }

    protected override async Task OnBeforeRequestAsync(HttpRequestMessage request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "HttpRequestMessage cannot be null");

        var path = request.RequestUri?.ToString() ?? string.Empty;

        if (path.StartsWith(Endpoints.Subscriptions, StringComparison.OrdinalIgnoreCase) &&
            (request.Method == HttpMethod.Post || request.Method == HttpMethod.Patch))
        {
            var (isValid, errorMessage) = ValidateSubscriptionPayload(request.Content?.ReadAsStringAsync().Result);
            if (!isValid)
                throw new ArgumentException(errorMessage);
        }

        SetTenantHeaders(this._tenant, request);
        await base.OnBeforeRequestAsync(request);
    }
}
