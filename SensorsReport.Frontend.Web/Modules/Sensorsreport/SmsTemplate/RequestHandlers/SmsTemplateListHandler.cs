using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.SmsTemplate.SmsTemplateRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.SmsTemplate.SmsTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.SmsTemplate;
public interface ISmsTemplateListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class SmsTemplateListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISmsTemplateListHandler
{
}