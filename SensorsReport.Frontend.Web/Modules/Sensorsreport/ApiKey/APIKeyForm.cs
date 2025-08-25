namespace SensorsReport.Frontend.SensorsReport.ApiKey.Forms;

[FormScript("SensorsReport.ApiKeyForm")]
[BasedOnRow(typeof(ApiKeyRow), CheckNames = true)]
public class ApiKeyForm
{
    public string? Id { get; set; }
    public string? TenantId { get; set; }
    public string? ApiKey { get; set; }
}