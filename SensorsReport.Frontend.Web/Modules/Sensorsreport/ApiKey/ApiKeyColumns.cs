namespace SensorsReport.Frontend.SensorsReport.ApiKey.Forms;

[ColumnsScript("SensorsReport.ApiKey")]
[BasedOnRow(typeof(ApiKeyRow), CheckNames = true)]
public class ApiKeyColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }

    [EditLink, Width(300), SortOrder(1)]
    public string? TenantId { get; set; }

    [EditLink, Width(200)]
    public string? ApiKey { get; set; }
}