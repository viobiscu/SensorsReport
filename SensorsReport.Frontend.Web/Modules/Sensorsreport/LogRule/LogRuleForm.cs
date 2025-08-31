namespace SensorsReport.Frontend.SensorsReport.LogRule.Forms;

[FormScript("SensorsReport.LogRuleForm")]
[BasedOnRow(typeof(LogRuleRow), CheckNames = true)]
public class LogRuleForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public double? Low { get; set; }
    public double? High { get; set; }
    public int? ConsecutiveHit { get; set; }
    public bool? Enabled { get; set; }
}