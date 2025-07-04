using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SensorsReport;

public class TenantRetriever(IHttpContextAccessor httpContextAccessor) : ITenantRetriever
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    public TenantInfo CurrentTenantInfo => GetTenantInfo();

    private TenantInfo GetTenantInfo()
    {
        var headers = _httpContextAccessor.HttpContext?.Request.Headers;

        var tenant = headers?["NGSILD-Tenant"].ToString();
        if (string.IsNullOrEmpty(tenant))
            tenant = headers?["Fiware-Service"].ToString();

        if (string.IsNullOrEmpty(tenant))
            throw new HttpRequestException("Tenant header is missing.", null, System.Net.HttpStatusCode.BadRequest);

        var path = headers?["NGSILD-Path"].ToString();
        if (string.IsNullOrEmpty(path))
            path = headers?["Fiware-ServicePath"].ToString();

        return new TenantInfo
        {
            Tenant = tenant,
            Path = path
        };
    }
}
