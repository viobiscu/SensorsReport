namespace SensorsReport.Frontend.SensorsReport.VariableTemplate.Forms;

[FormScript("SensorsReport.VariableTemplateForm")]
[BasedOnRow(typeof(VariableTemplateRow), CheckNames = true)]
public class VariableTemplateForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}