namespace SensorsReport.Frontend.SensorsReport.VariableTemplate.Forms;

[ColumnsScript("SensorsReport.VariableTemplate")]
[BasedOnRow(typeof(VariableTemplateRow), CheckNames = true)]
public class VariableTemplateColumns
{
    [EditLink, AlignRight]
    public string? Id { get; set; }
    [EditLink]
    public string? Name { get; set; }

}