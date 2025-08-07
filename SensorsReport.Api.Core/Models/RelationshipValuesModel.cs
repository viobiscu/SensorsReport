using System.Text.Json.Serialization;

namespace SensorsReport;

public class RelationshipValuesModel: PropertyModelBase
{
    [JsonPropertyName("value")]
    [JsonConverter(typeof(PropertyOrPropertyListConverter<RelationshipModel>))]
    public List<RelationshipModel> Value { get; set; } = new List<RelationshipModel>();
}
