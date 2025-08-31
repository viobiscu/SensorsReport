using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmRule.AlarmRuleRow;


namespace SensorsReport.Frontend.SensorsReport.AlarmRule;
public interface IAlarmRuleDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmRuleDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmRuleDeleteHandler
{
    
}