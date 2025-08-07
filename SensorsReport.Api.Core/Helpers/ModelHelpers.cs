using System.Text.Json;

namespace SensorsReport;

public static class ModelHelpers
{
    public static IEnumerable<(string property, string metadata)> GetProcessableProperties(this Dictionary<string, JsonElement> properties)
    {
        bool filterProperties(string property)
        {
            if (properties.TryGetValue(property, out var value))
            {
                if (value.ValueKind == JsonValueKind.Object)
                {
                    var metadataKey = $"metadata_{property}";
                    return properties.ContainsKey(metadataKey);
                }
            }

            return false;
        }

        return properties.Select(s => s.Key).Where(filterProperties)
            .Select(property => (property, $"metadata_{property}"));
    }
}
