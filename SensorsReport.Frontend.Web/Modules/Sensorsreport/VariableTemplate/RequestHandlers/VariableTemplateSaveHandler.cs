using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow;

namespace SensorsReport.Frontend.SensorsReport.VariableTemplate;
public interface IVariableTemplateSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class VariableTemplateSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IVariableTemplateSaveHandler
{
}