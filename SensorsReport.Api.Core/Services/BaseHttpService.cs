using System.Text;
using System.Text.Json;

namespace SensorsReport;

public class BaseHttpService
{
    internal readonly HttpClient _httpClient;
    protected readonly JsonSerializerOptions _jsonOptions;

    public BaseHttpService(IHttpClientFactory httpClientFactory, JsonSerializerOptions? jsonOptions = null)
    {
        _httpClient = httpClientFactory.CreateClient("OrionContextBroker");
        _jsonOptions = jsonOptions ?? GetJsonOptions();
    }

    protected virtual JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    protected virtual async Task<HttpResponseMessage> ExecuteRequestAsync(
        HttpRequestMessage request)
    {
        // Before request hook - can modify the request
        await OnBeforeRequestAsync(request);

        try
        {
            var response = await _httpClient.SendAsync(request);
            // After response hook
            await OnAfterResponseAsync(response, request);

            return response;
        }
        catch (Exception ex)
        {
            await OnRequestErrorAsync(ex, request);
            throw;
        }
    }

    protected virtual Task OnBeforeRequestAsync(HttpRequestMessage request)
    {
        // Override in derived classes to modify request (headers, content, etc.)
        return Task.CompletedTask;
    }

    protected virtual Task OnAfterResponseAsync(HttpResponseMessage response, HttpRequestMessage request)
    {
        // Override in derived classes
        return Task.CompletedTask;
    }

    protected virtual Task OnRequestErrorAsync(Exception exception, HttpRequestMessage request)
    {
        // Override in derived classes
        return Task.CompletedTask;
    }

    public async virtual Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        return await ExecuteRequestAsync(request);
    }

    public async virtual Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        return await ExecuteRequestAsync(request);
    }

    public async virtual Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        return await ExecuteRequestAsync(request);
    }

    public async virtual Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        return await ExecuteRequestAsync(request);
    }

    public async virtual Task<HttpResponseMessage> PatchAsync(string endpoint, HttpContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
        {
            Content = content
        };
        return await ExecuteRequestAsync(request);
    }

    public async virtual Task<T?> GetJsonAsync<T>(string endpoint) where T : class
    {
        var response = await GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
            return null;

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
    }

    public async virtual Task<T?> PostJsonAsync<T>(string endpoint, object content) where T : class
    {
        var jsonContent = JsonSerializer.Serialize(content, _jsonOptions);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await PostAsync(endpoint, stringContent);
        if (!response.IsSuccessStatusCode)
            return null;

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
    }
}