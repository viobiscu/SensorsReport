using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.EmailTemplate.EmailTemplateRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.EmailTemplate.EmailTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.EmailTemplate;
public interface IEmailTemplateRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class EmailTemplateRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IEmailTemplateRetrieveHandler
{
}