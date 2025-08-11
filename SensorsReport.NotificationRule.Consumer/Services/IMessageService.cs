using SensorsReport.OrionLD;

namespace SensorsReport.NotificationRule.Consumer
{
    public interface IMessageService
    {
        Task SendEmail(IOrionLdService orionLdService, IEnumerable<UserModel> users, string emailTemplateKey, TenantInfo? tenant, Dictionary<string, string> parameters);
        Task SendSms(IOrionLdService orionLdService, IEnumerable<UserModel> users, string smsTemplateKey, TenantInfo? tenant, Dictionary<string, string> parameters);
    }
}