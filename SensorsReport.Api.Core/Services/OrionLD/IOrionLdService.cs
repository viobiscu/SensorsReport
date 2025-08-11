
namespace SensorsReport.OrionLD;

public interface IOrionLdService
{
    void SetTenant(string? tenant);
    void SetTenant(TenantInfo tenant);
    Task<HttpResponseMessage> CreateEntityAsync(object entity);
    Task<HttpResponseMessage> CreateSubscriptionAsync(SubscriptionModel subscription);
    Task<HttpResponseMessage> DeleteEntityAsync(string entityId);
    Task<HttpResponseMessage> DeleteSubscriptionAsync(string subscriptionId);
    Task<HttpResponseMessage> GetEntitiesAsync(int offset = 0, int limit = 100, string? type = null);
    Task<T?> GetEntitiesAsync<T>(int offset = 0, int limit = 100, string? type = null) where T : class;
    Task<HttpResponseMessage> GetEntityByIdAsync(string entityId);
    Task<T?> GetEntityByIdAsync<T>(string entityId) where T : class;
    Task<HttpResponseMessage> GetSubscriptionByIdAsync(string subscriptionId);
    Task<T?> GetSubscriptionByIdAsync<T>(string subscriptionId) where T : class;
    Task<HttpResponseMessage> GetSubscriptionsAsync(int offset = 0, int limit = 100);
    Task<T?> GetSubscriptionsAsync<T>(int offset = 0, int limit = 100) where T : class;
    Task<HttpResponseMessage> ReplaceEntityAsync(string entityId, object entity);
    Task<HttpResponseMessage> UpdateEntityAsync(string entityId, object entity);
    Task<HttpResponseMessage> UpdateSubscriptionAsync(string subscriptionId, SubscriptionModel subscription);
}
