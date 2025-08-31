using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.LogRule.LogRuleRow;


namespace SensorsReport.Frontend.SensorsReport.LogRule;
public interface ILogRuleDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class LogRuleDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ILogRuleDeleteHandler
{
    
}