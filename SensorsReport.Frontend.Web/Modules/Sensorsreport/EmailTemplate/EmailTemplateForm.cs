namespace SensorsReport.Frontend.SensorsReport.EmailTemplate.Forms;

[FormScript("SensorsReport.EmailTemplateForm")]
[BasedOnRow(typeof(EmailTemplateRow), CheckNames = true)]
public class EmailTemplateForm
{
    public string? Id { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}