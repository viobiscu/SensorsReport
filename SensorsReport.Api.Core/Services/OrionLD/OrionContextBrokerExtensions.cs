
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorsReport.OrionLD;

public static class OrionContextBrokerExtensions
{
    public static string CreateEntityUrn(string entityType, string entityId)
    {
        if (string.IsNullOrEmpty(entityType))
            throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));
        if (string.IsNullOrEmpty(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

        if (entityId.StartsWith("urn:ngsi-ld:") && entityId.Contains(':') && entityId.Split(':').Length > 3 && entityId.Split(':')[2] == entityType)
            return entityId;

        entityId = entityId.Split(':').LastOrDefault()?.Trim() ?? entityId;

        return $"urn:ngsi-ld:{entityType}:{entityId}";
    }

    public static async Task<T?> GetContentAsAsync<T>(this HttpResponseMessage response) where T : class
    {
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new NullableIntEmptyStringConverter());
        options.Converters.Add(new NullableBoolEmptyStringConverter());
        return JsonSerializer.Deserialize<T>(content, options);
    }
}


public sealed class NullableIntEmptyStringConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (int.TryParse(s, out var value))
                return value;

            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt32();

        return null;
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}

public sealed class NullableBoolEmptyStringConverter : JsonConverter<bool?>
{
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (bool.TryParse(s, out var boolValue))
                return boolValue ? true : false;

            return null;
        }

        if (reader.TokenType == JsonTokenType.True)
            return true;

        if (reader.TokenType == JsonTokenType.False)
            return false;

        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (intValue == 1)
                return true;
            if (intValue == 0)
                return false;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteBooleanValue(value.Value);
        else
            writer.WriteNullValue();
    }
}