
namespace SensorsReport;

public class WebhookData
{
    public string? NotificationId { get; set; }
    public TenantInfo? Tenant { get; set; }
    public EntityModel[]? Data { get; set; }
}