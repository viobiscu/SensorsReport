using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationRule.NotificationRuleRow;


namespace SensorsReport.Frontend.SensorsReport.NotificationRule;
public interface INotificationRuleDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationRuleDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationRuleDeleteHandler
{
    
}