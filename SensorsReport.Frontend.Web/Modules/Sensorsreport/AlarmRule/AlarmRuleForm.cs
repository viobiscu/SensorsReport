namespace SensorsReport.Frontend.SensorsReport.AlarmRule.Forms;

[FormScript("SensorsReport.AlarmRuleForm")]
[BasedOnRow(typeof(AlarmRuleRow), CheckNames = true)]
public class AlarmRuleForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public double? Low { get; set; }
    public double? PreLow { get; set; }
    public double? PreHigh { get; set; }
    public double? High { get; set; }
}