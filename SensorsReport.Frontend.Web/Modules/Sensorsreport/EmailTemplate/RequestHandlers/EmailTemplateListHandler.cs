using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.EmailTemplate.EmailTemplateRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.EmailTemplate.EmailTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.EmailTemplate;
public interface IEmailTemplateListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class EmailTemplateListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IEmailTemplateListHandler
{
}