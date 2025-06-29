namespace Sensors_Report_Provision.API.Services;

public interface IOrionContextBrokerService
{
    Task<HttpResponseMessage> GetAsync(string endpoint);
    Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content);
    Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content);
    Task<HttpResponseMessage> DeleteAsync(string endpoint);
    Task<HttpResponseMessage> PatchAsync(string endpoint, HttpContent content);
    Task<T?> GetJsonAsync<T>(string endpoint) where T : class;
    Task<T?> PostJsonAsync<T>(string endpoint, object content) where T : class;
    Task<HttpResponseMessage> CreateEntityAsync(object entity);
    Task<HttpResponseMessage> UpdateEntityAsync(string entityId, object entity);
    Task<HttpResponseMessage> DeleteEntityAsync(string entityId);
}
