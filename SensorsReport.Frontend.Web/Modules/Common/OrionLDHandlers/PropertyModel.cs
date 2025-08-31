using System.Text.Json.Serialization;

namespace SensorsReport.Frontend;

public class PropertyModel<T>
{
    [JsonPropertyName("value")]
    public T? Value { get; set; }
}
