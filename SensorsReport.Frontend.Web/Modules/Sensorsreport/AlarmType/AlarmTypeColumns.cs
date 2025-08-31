namespace SensorsReport.Frontend.SensorsReport.AlarmType.Forms;

[ColumnsScript("SensorsReport.AlarmType")]
[BasedOnRow(typeof(AlarmTypeRow), CheckNames = true)]
public class AlarmTypeColumns
{
    [EditLink, AlignRight]
    public string? Id { get; set; }
    [EditLink]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Style { get; set; }

}