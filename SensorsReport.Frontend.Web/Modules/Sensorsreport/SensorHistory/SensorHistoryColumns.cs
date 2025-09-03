namespace SensorsReport.Frontend.SensorsReport.SensorHistory.Forms;

[ColumnsScript("SensorsReport.SensorHistory")]
[BasedOnRow(typeof(SensorHistoryRow), CheckNames = true)]
public class SensorHistoryColumns
{
    [EditLink]
    public long? Id { get; set; }
    public string? Tenant { get; set; }
    public string? SensorId { get; set; }
    public string? PropertyKey { get; set; }
    public string? MetadataKey { get; set; }
    [Width(120)]
    public DateTime? ObservedAt { get; set; }
    public double? Value { get; set; }
    public string? Unit { get; set; }
    [Width(120)]
    public DateTime CreatedAt { get; set; }
}