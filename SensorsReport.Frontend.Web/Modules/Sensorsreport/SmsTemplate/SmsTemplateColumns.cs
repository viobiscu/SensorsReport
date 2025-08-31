namespace SensorsReport.Frontend.SensorsReport.SmsTemplate.Forms;

[ColumnsScript("SensorsReport.SmsTemplate")]
[BasedOnRow(typeof(SmsTemplateRow), CheckNames = true)]
public class SmsTemplateColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }

    [EditLink, Width(300), SortOrder(1)]
    public string? Name { get; set; }
    public string? Message { get; set; }
}