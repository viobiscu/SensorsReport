
namespace SensorsReport.LogRule.Consumer;

public interface IEnqueueService
{
    Task EnqueueAlarmAsync(SubscriptionEventModel model);
    Task EnqueueNotificationAsync(SubscriptionEventModel model);
}
