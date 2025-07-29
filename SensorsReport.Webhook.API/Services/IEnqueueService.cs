using System.Text.Json;

namespace SensorsReport.Webhook.API.Services;

public interface IEnqueueService
{
    Task EnqueueNotificationAsync(JsonElement notification, TenantInfo tenant, string subscriptionId);
}
