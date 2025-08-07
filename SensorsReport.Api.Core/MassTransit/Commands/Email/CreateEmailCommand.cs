namespace SensorsReport;

public class CreateEmailCommand
{
    public string? ToEmail { get; set; }
    public string? ToName { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? CcEmail { get; set; }
    public string? CcName { get; set; }
    public string? BccEmail { get; set; }
    public string? BccName { get; set; }
    public string? Subject { get; set; }
    public string? BodyHtml { get; set; }
    public string? Tenant { get; set; }
}
