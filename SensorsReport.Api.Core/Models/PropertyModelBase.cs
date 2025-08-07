
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorsReport;

public class PropertyModelBase(string type = PropertyModelBase.PropertyType.Property)
{
    public static class PropertyType
    {
        public const string Property = "Property";
        public const string Relationship = "Relationship";
    }

    [JsonPropertyName("type")]
    public string? Type { get; set; } = type;

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = [];

}
