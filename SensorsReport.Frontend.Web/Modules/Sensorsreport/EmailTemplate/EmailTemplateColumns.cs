namespace SensorsReport.Frontend.SensorsReport.EmailTemplate.Forms;

[ColumnsScript("SensorsReport.EmailTemplate")]
[BasedOnRow(typeof(EmailTemplateRow), CheckNames = true)]
public class EmailTemplateColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }

    [EditLink, Width(300), SortOrder(1)]
    public string? Subject { get; set; }
    public string? Body { get; set; }
}