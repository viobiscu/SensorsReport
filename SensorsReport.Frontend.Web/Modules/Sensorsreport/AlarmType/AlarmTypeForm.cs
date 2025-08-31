namespace SensorsReport.Frontend.SensorsReport.AlarmType.Forms;

[FormScript("SensorsReport.AlarmTypeForm")]
[BasedOnRow(typeof(AlarmTypeRow), CheckNames = true)]
public class AlarmTypeForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Style { get; set; }
}