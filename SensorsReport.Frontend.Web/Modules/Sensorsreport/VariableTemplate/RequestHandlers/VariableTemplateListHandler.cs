using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.VariableTemplate;
public interface IVariableTemplateListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class VariableTemplateListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IVariableTemplateListHandler
{
}