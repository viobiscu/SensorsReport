namespace SensorsReport.Frontend.SensorsReport.Sensor.Forms;

[FormScript("SensorsReport.SensorForm")]
[BasedOnRow(typeof(SensorRow), CheckNames = true)]
public class SensorForm
{
    public string? Id { get; set; }
    public string? T0_Name { get; set; }
    public double? T0 { get; set; }
    public string? T0_Unit { get; set; }
    public DateTime? T0_ObservedAt { get; set; }
    public string? T0_Status { get; set; }
    public string? RH0_Name { get; set; }
    public double? RH0 { get; set; }
    public string? RH0_Unit { get; set; }
    public DateTime? RH0_ObservedAt { get; set; }
    public string? RH0_Status { get; set; }
}