using System.Text.Json.Serialization;

namespace SensorsReport;

public class ValuePropertyModel<T> : PropertyModelBase
{
    [JsonPropertyName("value")]
    public T? Value { get; set; }
}
