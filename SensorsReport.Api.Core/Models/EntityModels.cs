
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorsReport;

public class EntityModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
