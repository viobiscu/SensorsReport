using System.Text.Json;

namespace SensorsReport.Webhook.API.Services;

public interface INotifyRuleQueueService
{
    Task EnqueueNotificationAsync(JsonElement notification, TenantInfo tenant, string notificationId);
}
