using SensorsReport.OrionLD;

namespace SensorsReport.NotificationRule.Consumer;

public class UsersService(IServiceScopeFactory serviceScopeFactory, ILogger<UsersService> logger) : IUsersService
{
    public async Task<List<UserModel>> GetNotificationUsers(TenantInfo tenant, string notificationId)
    {
        if (string.IsNullOrEmpty(notificationId))
        {
            logger.LogWarning("Notification ID is null or empty.");
            return [];
        }

        var scope = serviceScopeFactory.CreateScope();
        var orionLdService = scope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(tenant);

        var notification = await orionLdService.GetEntityByIdAsync<NotificationModel>(notificationId);

        var (isNotificationValid, notificationValidationMessage) = ValidateNotification(notification);

        if (!isNotificationValid)
        {
            logger.LogWarning("Invalid notification: {Message}", notificationValidationMessage);
            return [];
        }

        var notificationUserObjects = notification?.NotificationUser?.Object ?? [];
        IEnumerable<NotificationUsersModel?> notificationUsers = (await Task.WhenAll(notificationUserObjects
            .Select(orionLdService.GetEntityByIdAsync<NotificationUsersModel>))) ?? [];

        List<UserModel> users = [];
        foreach (var notificationUser in notificationUsers)
        {
            var groupIds = notificationUser?.Groups?.Value?.Select(s => s.Object.FirstOrDefault()) ?? [];
            var groups = await Task.WhenAll(groupIds.Select(groupId => orionLdService.GetEntityByIdAsync<GroupModel>(groupId!)));
            var groupUserIds = groups.SelectMany(g => g?.Users?.Object ?? []).Distinct().ToList();
            var userIds = notificationUser?.Users?.Value?.Select(s => s.Object.FirstOrDefault()) ?? [];

            userIds = [.. userIds, .. groupUserIds];

            users = [.. (await Task.WhenAll(
                userIds
                .Select(userId => orionLdService.GetEntityByIdAsync<UserModel>(userId!))))
                .Where(u => u != null)
                .Cast<UserModel>()
                .DistinctBy(s => s.Id)];
        }

        return users;
    }

    private (bool isValid, string message) ValidateNotification(NotificationModel? notification)
    {
        if (notification == null)
            return (false, "Notification cannot be null.");

        if (notification.Enable?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) != true)
            return (false, "Notification is not enabled.");

        if (notification.NotificationUser?.Object is null || notification.NotificationUser.Object.Count == 0)
            return (false, "Notification does not have any associated users.");

        return (true, "Notification is valid.");
    }
}
