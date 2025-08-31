using System.Text.Json.Serialization;

namespace SensorsReport.Frontend;

public class RelationModel<T>
{
    [JsonPropertyName("object")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<T>? Object { get; set; }
    [JsonPropertyName("enable")]
    public PropertyModel<bool?>? Enable { get; set; }
    [JsonPropertyName("monitoredAttribute")]
    public PropertyModel<string?>? MonitoredAttribute { get; set; }
}

public class MeasuredModel<T>
{
    [JsonPropertyName("value")]
    public T? Value { get; set; }
    [JsonPropertyName("unit")]
    public PropertyModel<string?>? Unit { get; set; }
    [JsonPropertyName("observedAt")]
    public DateTime? ObservedAt { get; set; }

}