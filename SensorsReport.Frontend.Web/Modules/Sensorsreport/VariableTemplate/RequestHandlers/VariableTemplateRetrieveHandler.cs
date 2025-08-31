using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.VariableTemplate;
public interface IVariableTemplateRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class VariableTemplateRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IVariableTemplateRetrieveHandler
{
}