using System.Text.Json.Serialization;

namespace SensorsReport;

public class EntityPropertyModel : PropertyModelBase
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }
    [JsonPropertyName("observedAt")]
    public DateTimeOffset? ObservedAt { get; set; }
    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }

    public static class StatusValues
    {
        public readonly static string Faulty = "faulty";
        public readonly static string Operational = "operational";
    }
}
