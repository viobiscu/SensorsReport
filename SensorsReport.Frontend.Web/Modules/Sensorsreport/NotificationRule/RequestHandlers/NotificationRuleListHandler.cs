using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.NotificationRule.NotificationRuleRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationRule.NotificationRuleRow;


namespace SensorsReport.Frontend.SensorsReport.NotificationRule;
public interface INotificationRuleListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class NotificationRuleListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationRuleListHandler
{
}