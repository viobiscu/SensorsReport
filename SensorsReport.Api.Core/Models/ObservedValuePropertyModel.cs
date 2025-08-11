using System.Text.Json.Serialization;

namespace SensorsReport;

public class ObservedValuePropertyModel<T>
{
    [JsonPropertyName("value")]
    public T? Value { get; set; }
    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }
    [JsonPropertyName("observedAt")]
    [JsonConverter(typeof(FormattedDateTimeOffsetConverter))]
    public DateTimeOffset ObservedAt { get; set; }
}
