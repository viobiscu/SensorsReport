
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sensors_Report_Provision.API.Models;

public class EntityModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();
}

public class PropertyModel
{
    [JsonPropertyName("type")]
    public string Type { get; } = "Property";

    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
}

public class RelationshipModel
{
    [JsonPropertyName("type")]
    public string Type { get; } = "Relationship";

    [JsonPropertyName("object")]
    public List<string> Object { get; set; } = new List<string>();

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
}