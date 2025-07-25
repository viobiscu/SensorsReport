
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
    public Dictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();
}

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
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();

}

public class PropertyModel : PropertyModelBase
{
    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }
}

public class RelationshipValuesModel: PropertyModelBase
{
    [JsonPropertyName("value")]
    [JsonConverter(typeof(PropertyOrPropertyListConverter<RelationshipModel>))]
    public List<RelationshipModel> Value { get; set; } = new List<RelationshipModel>();
}

public class RelationshipModel() : PropertyModelBase(PropertyType.Relationship)
{
    [JsonPropertyName("object")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<string> Object { get; set; } = new List<string>();

    [JsonPropertyName("monitoredAttribute")]
    public ValuePropertyModel<string>? MonitoredAttribute { get; set; }
}

public class PropertyOrPropertyListConverter<T> : JsonConverter<List<T>> where T : PropertyModelBase
{
    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<T>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var model = JsonSerializer.Deserialize<T>(ref reader, options);
                    if (model != null)
                    {
                        list.Add(model);
                    }
                }
            }
            return list;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var model = JsonSerializer.Deserialize<T>(ref reader, options);
            return model != null ? new List<T> { model } : new List<T>();
        }
        throw new JsonException("Expected array or object of PropertyModelBase");
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        if (value.Count == 1)
        {
            JsonSerializer.Serialize(writer, value[0], options);
        }
        else
        {
            foreach (var item in value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }
        }
        writer.WriteEndArray();
    }
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

public class ValuePropertyModel<T> : PropertyModelBase
{
    [JsonPropertyName("value")]
    public T? Value { get; set; }
}

public class ObservedValuePropertyModel<T>
{
    [JsonPropertyName("value")]
    public T? Value { get; set; }
    [JsonPropertyName("observedAt")]
    public DateTimeOffset ObservedAt { get; set; }
}

public class Point
{
    [JsonPropertyName("type")]
    public string? Type { get; set; } // e.g., "Point"

    [JsonPropertyName("coordinates")]
    public List<double> Coordinates { get; set; } = [];
}
