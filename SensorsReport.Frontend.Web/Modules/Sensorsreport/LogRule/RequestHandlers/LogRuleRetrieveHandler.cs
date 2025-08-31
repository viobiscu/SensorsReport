using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.LogRule.LogRuleRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.LogRule.LogRuleRow;


namespace SensorsReport.Frontend.SensorsReport.LogRule;
public interface ILogRuleRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class LogRuleRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ILogRuleRetrieveHandler
{
}