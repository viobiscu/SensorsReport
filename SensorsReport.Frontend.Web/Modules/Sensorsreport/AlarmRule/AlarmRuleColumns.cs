namespace SensorsReport.Frontend.SensorsReport.AlarmRule.Forms;

[ColumnsScript("SensorsReport.AlarmRule")]
[BasedOnRow(typeof(AlarmRuleRow), CheckNames = true)]
public class AlarmRuleColumns
{
    [EditLink, AlignRight]
    public string? Id { get; set; }
    [EditLink]
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public double? Low { get; set; }
    public double? PreLow { get; set; }
    public double? PreHigh { get; set; }
    public double? High { get; set; }

}