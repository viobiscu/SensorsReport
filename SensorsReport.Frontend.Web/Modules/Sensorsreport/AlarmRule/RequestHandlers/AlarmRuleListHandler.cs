using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.AlarmRule.AlarmRuleRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmRule.AlarmRuleRow;


namespace SensorsReport.Frontend.SensorsReport.AlarmRule;
public interface IAlarmRuleListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class AlarmRuleListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmRuleListHandler
{
}