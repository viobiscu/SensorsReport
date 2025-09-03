namespace SensorsReport;

public class TriggerAlarmRuleEvent
{
    public TenantInfo? Tenant { get; set; }
    public string? SensorId { get; set; }
    public string? PropertyKey { get; set; }
    public string? MetadataKey { get; set; }
}

public class SensorDataHistoryLogEvent
{
    public TenantInfo? Tenant { get; set; }
    public string? SensorId { get; set; }
    public string? PropertyKey { get; set; }
    public string? MetadataKey { get; set; }
    public DateTimeOffset? ObservedAt { get; set; }
    public double? Value { get; set; }
    public string? Unit { get; set; }
}