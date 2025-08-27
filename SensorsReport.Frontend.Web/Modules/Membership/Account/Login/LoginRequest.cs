namespace SensorsReport.Frontend.Membership;

[FormScript("Membership.Login")]
[BasedOnRow(typeof(Administration.UserRow), CheckNames = true)]
public class LoginRequest : ServiceRequest
{
    [Placeholder("user name")]
    public string Username { get; set; }

    [PasswordEditor, Required(true), Placeholder("password")]
    public string Password { get; set; }

    [Ignore]
    public string TwoFactorToken { get; set; }

    [Ignore]
    public string TwoFactorCode { get; set; }
}