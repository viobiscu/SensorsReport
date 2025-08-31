namespace SensorsReport.Frontend.SensorsReport.Alarm.Forms;

[FormScript("SensorsReport.AlarmForm")]
[BasedOnRow(typeof(AlarmRow), CheckNames = true)]
public class AlarmForm
{
    public string? Id { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public List<RelationModel<string>>? Monitors { get; set; }
    public double? Threshold { get; set; }
    public string? Condition { get; set; }
    public List<MeasuredModel<double>>? MeasuredValue { get; set; }
}