using System.Text;
using System.Text.Json;

namespace Sensors_Report_Provision.API.Services;

public class QuantumLeapService : IQuantumLeapService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public QuantumLeapService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("QuantumLeap");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        return await _httpClient.GetAsync(endpoint);
    }

    public async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content)
    {
        return await _httpClient.PostAsync(endpoint, content);
    }

    public async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content)
    {
        return await _httpClient.PutAsync(endpoint, content);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await _httpClient.DeleteAsync(endpoint);
    }

    public async Task<T?> GetJsonAsync<T>(string endpoint) where T : class
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        
        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
    }

    public async Task<T?> PostJsonAsync<T>(string endpoint, object content) where T : class
    {
        var jsonContent = JsonSerializer.Serialize(content, _jsonOptions);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(endpoint, stringContent);
        response.EnsureSuccessStatusCode();
        
        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
    }
}
