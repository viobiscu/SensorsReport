using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.EmailTemplate.EmailTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.EmailTemplate;
public interface IEmailTemplateDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class EmailTemplateDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IEmailTemplateDeleteHandler
{
    
}