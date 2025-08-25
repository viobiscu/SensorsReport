using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.User.Forms;

[FormScript("SensorsReport.UserForm")]
[BasedOnRow(typeof(UserRow), CheckNames = true)]
public class UserForm
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Mobile { get; set; }
    public string? Language { get; set; }
}