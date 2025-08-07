using System.Text.Json.Serialization;

namespace SensorsReport;

public class GroupModel : EntityModel
{
    [JsonPropertyName("name")]
    public ValuePropertyModel<string>? Name { get; set; }
    [JsonPropertyName("users")]
    public RelationshipModel? Users { get; set; }
}
