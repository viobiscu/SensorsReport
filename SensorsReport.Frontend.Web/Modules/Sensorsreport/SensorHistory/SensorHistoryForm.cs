namespace SensorsReport.Frontend.SensorsReport.SensorHistory.Forms;

[FormScript("SensorsReport.SensorHistory")]
[BasedOnRow(typeof(SensorHistoryRow), CheckNames = true)]
public class SensorHistoryForm
{
    public long? Id { get; set; }
    public string? Tenant { get; set; }
    public string? SensorId { get; set; }
    public string? PropertyKey { get; set; }
    public string? MetadataKey { get; set; }
    public DateTime? ObservedAt { get; set; }
    public double? Value { get; set; }
    public string? Unit { get; set; }
    public DateTime CreatedAt { get; set; }
}