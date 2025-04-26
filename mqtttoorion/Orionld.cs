using System;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;


public class Orionld : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly string? _orionUrl;
    private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

    public Orionld()
    {
        if (string.IsNullOrEmpty(ConfigProgram.OrionUrl))
        {
            Logger.Error("Orion URL is not configured.");
            return;
        }
        _orionUrl = ConfigProgram.OrionUrl.TrimEnd('/');
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept", "application/ld+json");
        // add header for Orion-LD link
        //_client.DefaultRequestHeaders.Add("Link", "<https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld>; rel=\"http://www.w3.org/2018/ldp#conformsTo\"");
    }

    public string Context
    {
        get
        {
            if (_client.DefaultRequestHeaders.TryGetValues("NGSILD-Context", out var values))
            {
                return string.Join(",", values);
            }
            return string.Empty;
        }
        set
        {
            _client.DefaultRequestHeaders.Remove("NGSILD-Context");
            _client.DefaultRequestHeaders.Add("NGSILD-Context", value);
        }
    }
    public string TenantID
    {
        get
        {
            if (_client.DefaultRequestHeaders.TryGetValues("NGSILD-Tenant", out var values))
            {
                return string.Join(",", values);
            }
            return string.Empty;
        }
        set
        {
            _client.DefaultRequestHeaders.Remove("NGSILD-Tenant");
            if (!string.IsNullOrEmpty(value))
            {
                _client.DefaultRequestHeaders.Add("NGSILD-Tenant", value);
            }

        }
    }

    public async Task<string> PatchEntityAttributeAsync(string entityId, string attribute, object value)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{entityId}/attrs/{attribute}";
        var jsonPayload = $@"
        {{
            ""@context"": ""https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld"",
            ""value"": {JsonConvert.SerializeObject(value)}
        }}";
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/ld+json");
        Logger.Trace($"Request URL: {url}");
        foreach (var header in _client.DefaultRequestHeaders)
        {
            Logger.Trace($"Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        Logger.Trace($"Header: {content}");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _client.SendAsync(request);
        Logger.Trace($"Response Status Code: {response.StatusCode}");
        foreach (var header in response.Headers)
        {
            Logger.Trace($"Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        Logger.Trace(response.Content.ReadAsStringAsync());
        return await HandleResponse(response);
    }

    public async Task<string> PatchEntityAsync(string entity, string entityId)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{entityId}";
        var content = new StringContent(entity, Encoding.UTF8, "application/ld+json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        //log the request
        Logger.Trace($"Request URL: {url}");
        Logger.Trace($"Request Content: {entity}");
        foreach (var header in _client.DefaultRequestHeaders)
        {
            Logger.Trace($"Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        //log the response
        var response = await _client.SendAsync(request);
        Logger.Trace($"Response Status Code: {response.StatusCode}");
        Logger.Trace($"Response ReasonPhrase: response.ReasonPhrase");
        //log the headers
        foreach (var header in response.Headers)
        {
            Logger.Trace($"Response Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        return await HandleResponse(response);
    }

    public async Task<string> GetEntityAsync(string entityId)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{entityId}";
        var response = await _client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> CreateEntityAsync(object entity)
    {
        AddContext(entity);
        string url = $"{_orionUrl}/ngsi-ld/v1/entities";
        var content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/ld+json");
        Logger.Trace($"CreateEntity Request URL: {url}");
        Logger.Trace($"CreateEntity Request Content: {entity}");
        foreach (var header in _client.DefaultRequestHeaders)
        {
            Logger.Trace($"CreateEntity Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        var response = await _client.PostAsync(url, content);
        Logger.Trace($" CreateEntity Response Status Code: {response.StatusCode}");
        Logger.Trace($"CreateEntity Response ReasonPhrase: {response.ReasonPhrase}");
        //log the headers
        foreach (var header in response.Headers)
        {
            Logger.Trace($"CreateEntity Response Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        return await HandleResponse(response);
    }

    public async Task<string> UpdateEntityAttributeAsync(string entityId, string attribute, object value)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{entityId}/attrs";
        var jsonPayload = $@"
        {{
            ""{attribute}"": {{
                ""type"": ""Property"",
                ""value"": {JsonConvert.SerializeObject(value)}
            }}
        }}";
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/ld+json");
        Logger.Trace($"Request URL: {url}");
        foreach (var header in _client.DefaultRequestHeaders)
        {
            Logger.Trace($"Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        var response = await _client.PatchAsync(url, content);
        return await HandleResponse(response);
    }

    public async Task<string> DeleteEntityAsync(string entityId)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{entityId}";
        var response = await _client.DeleteAsync(url);
        Logger.Trace($"Request URL: {url}");
        foreach (var header in _client.DefaultRequestHeaders)
        {
            Logger.Trace($"Header: {header.Key} - {string.Join(", ", header.Value)}");
        }
        return await HandleResponse(response);
    }

    public async Task<string> BatchCreateEntitiesAsync(object[] entities)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entityOperations/create";
        var content = new StringContent(JsonConvert.SerializeObject(entities), Encoding.UTF8, "application/ld+json");
        var response = await _client.PostAsync(url, content);
        return await HandleResponse(response);
    }

    public async Task<string> BatchUpdateEntitiesAsync(object[] updates)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entityOperations/update";
        var content = new StringContent(JsonConvert.SerializeObject(updates), Encoding.UTF8, "application/ld+json");
        var response = await _client.PostAsync(url, content);
        return await HandleResponse(response);
    }
    public async Task<string> BatchUpsertEntitiesAsync(object[] updates)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entityOperations/upsert";
        var content = new StringContent(JsonConvert.SerializeObject(updates), Encoding.UTF8, "application/ld+json");
        var response = await _client.PostAsync(url, content);
        return await HandleResponse(response);
    }
    public async Task<string> BatchDeleteEntitiesAsync(string[] entityIds)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entityOperations/delete";
        var payload = JsonConvert.SerializeObject(entityIds);
        var content = new StringContent(payload, Encoding.UTF8, "application/ld+json");
        var response = await _client.PostAsync(url, content);
        return await HandleResponse(response);
    }

    public async Task<string> CreateSubscriptionAsync(object subscription)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/subscriptions";
        var content = new StringContent(JsonConvert.SerializeObject(subscription), Encoding.UTF8, "application/ld+json");
        var response = await _client.PostAsync(url, content);
        return await HandleResponse(response);
    }

    public async Task<string> DeleteSubscriptionAsync(string subscriptionId)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/subscriptions/{subscriptionId}";
        var response = await _client.DeleteAsync(url);
        return await HandleResponse(response);
    }

    private async Task<string> HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return $"OK: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
        }
        else
        {
            return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
        }
    }

    public async Task<string> GetVersionAsync()
    {
        if (_client != null)
        {
            string url = $"{_orionUrl}/version";
            var response = await _client.GetAsync(url);
            return await HandleResponse(response);
        }
        else
        {
            return "Client not initialized.";
        }

    }
    private void AddContext(object entity)
    {
        //check if there is @context present in the entity and if not add it
        if (entity is string entityString)
        {
            if (!entityString.Contains("@context"))
            {
                entityString = $"{{ \"@context\": [\"https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld\"], {entityString} }}";
            }
            entity = JsonConvert.DeserializeObject(entityString) ?? new object();
        }
        else if (entity is JObject jsonEntity)
        {
            if (!jsonEntity.ContainsKey("@context"))
            {
                jsonEntity["@context"] = new JArray("https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld");
            }
        }
    }
    public void Dispose()
    {
        _client.Dispose();
    }
}
