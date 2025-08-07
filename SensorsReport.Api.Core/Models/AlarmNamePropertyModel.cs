using System.Text.Json.Serialization;

namespace SensorsReport;

public class AlarmNamePropertyModel : ValuePropertyModel<string>
{
    [JsonPropertyName("attributeDetails")]
    public ValuePropertyModel<string>? AttributeDetails { get; set; }
}
