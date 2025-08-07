using System.Text.Json.Serialization;

namespace SensorsReport;

public class RelationshipModel() : PropertyModelBase(PropertyType.Relationship)
{
    [JsonPropertyName("object")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<string> Object { get; set; } = new List<string>();

    [JsonPropertyName("monitoredAttribute")]
    public ValuePropertyModel<string>? MonitoredAttribute { get; set; }

    [JsonPropertyName("enable")]
    public ValuePropertyModel<bool>? Enable { get; set; }
}
