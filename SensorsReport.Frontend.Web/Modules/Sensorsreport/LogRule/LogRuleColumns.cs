namespace SensorsReport.Frontend.SensorsReport.LogRule.Forms;

[ColumnsScript("SensorsReport.LogRule")]
[BasedOnRow(typeof(LogRuleRow), CheckNames = true)]
public class LogRuleColumns
{
    [EditLink, AlignRight]
    public string? Id { get; set; }
    [EditLink]
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public double? Low { get; set; }
    public double? High { get; set; }
    public int? ConsecutiveHit { get; set; }
    public bool? Enabled { get; set; }
}