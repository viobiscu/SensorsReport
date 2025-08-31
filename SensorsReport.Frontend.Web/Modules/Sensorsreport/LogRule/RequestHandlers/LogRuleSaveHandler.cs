using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.LogRule.LogRuleRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.LogRule.LogRuleRow;

namespace SensorsReport.Frontend.SensorsReport.LogRule;
public interface ILogRuleSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class LogRuleSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ILogRuleSaveHandler
{
}