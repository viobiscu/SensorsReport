using System.Text.Json.Serialization;

namespace SensorsReport;

public class UnitPropertyModel<T> : ValuePropertyModel<T>
{
    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }
}
