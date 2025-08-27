using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.ApiKey.ApiKeyRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.ApiKey.ApiKeyRow;


namespace SensorsReport.Frontend.SensorsReport.ApiKey;
public interface IApiKeyRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class ApiKeyRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IApiKeyRetrieveHandler
{
    protected override TenantInfo GetTenantInfo() => new();
}