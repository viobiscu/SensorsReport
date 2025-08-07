using System.Text.Json.Serialization;

namespace SensorsReport;

public class Point
{
    [JsonPropertyName("type")]
    public string? Type { get; set; } // e.g., "Point"

    [JsonPropertyName("coordinates")]
    public List<double>? Coordinates { get; set; } = [];

    public override string ToString() => $"[{string.Join(", ", Coordinates ?? [])}]";
}
