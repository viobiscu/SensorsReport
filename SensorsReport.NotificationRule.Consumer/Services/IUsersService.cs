
namespace SensorsReport.NotificationRule.Consumer;

public interface IUsersService
{
    Task<List<UserModel>> GetNotificationUsers(TenantInfo tenant, string notificationId);
}