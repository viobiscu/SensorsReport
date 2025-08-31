using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.AlarmRule.AlarmRuleRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmRule.AlarmRuleRow;


namespace SensorsReport.Frontend.SensorsReport.AlarmRule;
public interface IAlarmRuleRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmRuleRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmRuleRetrieveHandler
{
}