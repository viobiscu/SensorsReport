using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.SmsTemplate.SmsTemplateRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.SmsTemplate.SmsTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.SmsTemplate;
public interface ISmsTemplateRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class SmsTemplateRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISmsTemplateRetrieveHandler
{
}