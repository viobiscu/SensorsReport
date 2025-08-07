
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorsReport;

public class PropertyModel : PropertyModelBase
{
    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }
}
