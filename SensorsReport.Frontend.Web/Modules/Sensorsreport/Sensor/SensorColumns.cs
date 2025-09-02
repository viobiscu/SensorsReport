using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.Sensor.Forms;

[ColumnsScript("SensorsReport.Sensor")]
[BasedOnRow(typeof(SensorRow), CheckNames = true)]
public class SensorColumns
{
    [EditLink, AlignRight]
    public string? Id { get; set; }
    [Width(130), DisplayName("Name")]
    public string? T0_Name { get; set; }
    [Width(30), DisplayName("")]
    public string? T0_Status { get; set; }
    [Width(50)]
    public double? T0 { get; set; }
    [Width(50)]
    public string? T0_Unit { get; set; }
    [Width(130), DateTimeFormatter]
    public DateTime? T0_ObservedAt { get; set; }
    [Width(130), DisplayName("Name")]
    public string? RH0_Name { get; set; }
    [Width(30), DisplayName("")]
    public string? RH0_Status { get; set; }
    [Width(50)]
    public double? RH0 { get; set; }
    [Width(50)]
    public string? RH0_Unit { get; set; }
    [Width(130), DateTimeFormatter]
    public DateTime? RH0_ObservedAt { get; set; }
}