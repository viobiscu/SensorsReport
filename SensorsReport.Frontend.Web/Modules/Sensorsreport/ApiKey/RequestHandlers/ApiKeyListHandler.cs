using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.ApiKey.ApiKeyRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.ApiKey.ApiKeyRow;


namespace SensorsReport.Frontend.SensorsReport.ApiKey;
public interface IApiKeyListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class ApiKeyListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IApiKeyListHandler
{
    protected override TenantInfo GetTenantInfo() => new();
}