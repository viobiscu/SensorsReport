namespace SensorsReport;

[ConfigName(SectionName)]
public class SmtpOptions
{
    public const string SectionName = nameof(SmtpOptions);
    public string Server { get; set; } = null!;
    public int Port { get; set; } = 587;
    public bool UseSSL { get; set; } = true;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}