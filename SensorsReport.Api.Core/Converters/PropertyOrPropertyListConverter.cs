
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorsReport;

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
