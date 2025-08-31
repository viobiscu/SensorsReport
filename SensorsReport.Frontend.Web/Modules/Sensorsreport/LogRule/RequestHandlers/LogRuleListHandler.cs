using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.LogRule.LogRuleRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.LogRule.LogRuleRow;


namespace SensorsReport.Frontend.SensorsReport.LogRule;
public interface ILogRuleListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class LogRuleListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ILogRuleListHandler
{
}