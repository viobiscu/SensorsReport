using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.VariableTemplate;
public interface IVariableTemplateDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class VariableTemplateDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IVariableTemplateDeleteHandler
{
    
}