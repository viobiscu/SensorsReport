using System.Text.Json.Serialization;

namespace SensorsReport;

public class UserModel : EntityModel
{
    [JsonPropertyName("username")]
    public ValuePropertyModel<string>? Username { get; set; }
    [JsonPropertyName("email")]
    public ValuePropertyModel<string>? Email { get; set; }
    [JsonPropertyName("firstName")]
    public ValuePropertyModel<string>? FirstName { get; set; }
    [JsonPropertyName("lastName")]
    public ValuePropertyModel<string>? LastName { get; set; }
    [JsonPropertyName("mobile")]
    public ValuePropertyModel<string>? Mobile { get; set; }
    [JsonPropertyName("language")]
    public ValuePropertyModel<string>? Language { get; set; }
}
