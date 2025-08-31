using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.Alarm.Forms;

[ColumnsScript("SensorsReport.Alarm")]
[BasedOnRow(typeof(AlarmRow), CheckNames = true)]
public class AlarmColumns
{
    [EditLink, AlignRight]
    public string? Id { get; set; }
    [EditLink]
    public string? Description { get; set; }
    public string? Status { get; set;}
    public string? Severity { get; set;}
    public List<RelationModel<string>>? Monitors { get; set;}
    public double? Threshold { get; set;}
    public string? Condition { get; set;}
    public List<MeasuredModel<double>>? MeasuredValue { get; set;}

}