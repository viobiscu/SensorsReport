
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

public class PropertyModelBase
{
    public static class PropertyType
    {
        public const string Property = "Property";
        public const string Relationship = "Relationship";
    }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    
}

public class PropertyModel : PropertyModelBase
{
    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }

}

public class RelationshipModel : PropertyModelBase
{
    [JsonPropertyName("object")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<string> Object { get; set; } = new List<string>();
}

public class StringOrStringListConverter : JsonConverter<List<string>>
{
    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new List<string> { reader.GetString()! };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<string>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    list.Add(reader.GetString()!);
                }
            }
            return list;
        }
        
        throw new JsonException("Expected string or array of strings");
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        if (value.Count == 1)
        {
            writer.WriteStringValue(value[0]);
        }
        else
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
        }
    }
}