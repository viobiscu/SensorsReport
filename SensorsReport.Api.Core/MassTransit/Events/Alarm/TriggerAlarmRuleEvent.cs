namespace SensorsReport;

public class TriggerAlarmRuleEvent
{
    public TenantInfo? Tenant { get; set; }
    public string? SensorId { get; set; }
    public string? PropertyKey { get; set; }
    public string? MetadataKey { get; set; }
}
