namespace SensorsReport.Frontend;

public class ClaimsTenantRetriever(IHttpContextAccessor httpContextAccessor) : ITenantRetriever
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    public TenantInfo CurrentTenantInfo => GetTenantInfo();

    private TenantInfo GetTenantInfo()
    {
        var headers = _httpContextAccessor.HttpContext?.Request.Headers;

        var tenant = headers?["NGSILD-Tenant"].ToString();
        if (string.IsNullOrEmpty(tenant))
            tenant = headers?["Fiware-Service"].ToString();

        var path = headers?["NGSILD-Path"].ToString();
        if (string.IsNullOrEmpty(path))
            path = headers?["Fiware-ServicePath"].ToString();

        var userClaims = _httpContextAccessor.HttpContext?.User?.Claims;
        var claimTenant = userClaims?.FirstOrDefault(c => c.Type == "organization");

        if (claimTenant != null && !string.IsNullOrEmpty(claimTenant.Value))
            tenant = claimTenant.Value;

        tenant ??= _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Demo";

        return new TenantInfo
        {
            Tenant = tenant,
            Path = path
        };
    }
}
