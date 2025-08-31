using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.EmailTemplate.EmailTemplateRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.EmailTemplate.EmailTemplateRow;

namespace SensorsReport.Frontend.SensorsReport.EmailTemplate;
public interface IEmailTemplateSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class EmailTemplateSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IEmailTemplateSaveHandler
{
}