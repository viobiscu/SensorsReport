using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.ApiKey.ApiKeyRow;


namespace SensorsReport.Frontend.SensorsReport.ApiKey;
public interface IApiKeyDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class ApiKeyDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IApiKeyDeleteHandler
{
}