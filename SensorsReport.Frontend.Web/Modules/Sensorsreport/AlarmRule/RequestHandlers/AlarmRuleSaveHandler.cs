using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.AlarmRule.AlarmRuleRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmRule.AlarmRuleRow;

namespace SensorsReport.Frontend.SensorsReport.AlarmRule;
public interface IAlarmRuleSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmRuleSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmRuleSaveHandler
{
}