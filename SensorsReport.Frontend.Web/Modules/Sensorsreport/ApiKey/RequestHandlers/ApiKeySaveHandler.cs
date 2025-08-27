using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.ApiKey.ApiKeyRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.ApiKey.ApiKeyRow;

namespace SensorsReport.Frontend.SensorsReport.ApiKey;
public interface IApiKeySaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class ApiKeySaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IApiKeySaveHandler
{
    protected override TenantInfo GetTenantInfo() => new();
}