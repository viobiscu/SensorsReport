
namespace SensorsReport;

public interface ITenantRetriever
{
    TenantInfo CurrentTenantInfo { get; }
}
