using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.SmsTemplate.SmsTemplateRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.SmsTemplate.SmsTemplateRow;

namespace SensorsReport.Frontend.SensorsReport.SmsTemplate;
public interface ISmsTemplateSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class SmsTemplateSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISmsTemplateSaveHandler
{
}