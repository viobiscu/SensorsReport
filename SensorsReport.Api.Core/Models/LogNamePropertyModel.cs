using System.Text.Json.Serialization;

namespace SensorsReport.LogRule.Consumer.Consumers;

public class LogNamePropertyModel : ValuePropertyModel<string>
{
    [JsonPropertyName("attributeDetails")]
    public ValuePropertyModel<string>? AttributeDetails { get; set; }

    [JsonPropertyName("value")]
    public new string? Value { get; set; }
}
