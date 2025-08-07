namespace SensorsReport;

public class TriggerNotificationRuleEvent
{
    public TenantInfo? Tenant { get; set; }
    public string? SensorId { get; set; }
    public string? AlarmId { get; set; }
    public string? PropertyKey { get; set; }
    public string? MetadataKey { get; set; }
}
