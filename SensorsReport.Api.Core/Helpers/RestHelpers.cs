using System;
using System.Text.Json;

namespace SensorsReport;

public static class RestHelpers
{
    public static void PatchModel<T>(this T target, JsonElement source) where T : class
    {
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            if (Attribute.GetCustomAttribute(property, typeof(AllowPatchAttribute)) is AllowPatchAttribute &&
                source.TryGetProperty(property.Name, out var value))
            {
                // check if the property allows null values (including nullable types)
                if (property.PropertyType.IsValueType &&
                    Nullable.GetUnderlyingType(property.PropertyType) == null &&
                    value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                property.SetValue(target, value.Deserialize(property.PropertyType));
            }
        }
    }
}
