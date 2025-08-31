using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.SmsTemplate.SmsTemplateRow;


namespace SensorsReport.Frontend.SensorsReport.SmsTemplate;
public interface ISmsTemplateDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class SmsTemplateDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISmsTemplateDeleteHandler
{
    
}