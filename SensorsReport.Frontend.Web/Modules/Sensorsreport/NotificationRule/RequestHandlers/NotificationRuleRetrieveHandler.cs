using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.NotificationRule.NotificationRuleRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationRule.NotificationRuleRow;


namespace SensorsReport.Frontend.SensorsReport.NotificationRule;
public interface INotificationRuleRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationRuleRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationRuleRetrieveHandler
{
}