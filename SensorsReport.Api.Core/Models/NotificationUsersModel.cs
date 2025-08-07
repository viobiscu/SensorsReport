using System.Text.Json.Serialization;

namespace SensorsReport;

public class NotificationUsersModel : EntityModel
{
    [JsonPropertyName("enable")]
    public ValuePropertyModel<bool>? Enable { get; set; }

    [JsonPropertyName("name")]
    public ValuePropertyModel<string>? Name { get; set; }

    [JsonPropertyName("users")]
    public RelationshipValuesModel? Users { get; set; }

    [JsonPropertyName("groups")]
    public RelationshipValuesModel? Groups { get; set; }

    [JsonPropertyName("notification")]
    public RelationshipModel? Notification { get; set; }
}
