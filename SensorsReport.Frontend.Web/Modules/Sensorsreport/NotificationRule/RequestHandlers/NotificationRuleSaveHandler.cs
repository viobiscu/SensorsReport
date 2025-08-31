using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.NotificationRule.NotificationRuleRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationRule.NotificationRuleRow;

namespace SensorsReport.Frontend.SensorsReport.NotificationRule;
public interface INotificationRuleSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationRuleSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationRuleSaveHandler
{
}