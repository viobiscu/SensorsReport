namespace SensorsReport.Frontend.SensorsReport.SmsTemplate.Forms;

[FormScript("SensorsReport.SmsTemplateForm")]
[BasedOnRow(typeof(SmsTemplateRow), CheckNames = true)]
public class SmsTemplateForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Message { get; set; }
}