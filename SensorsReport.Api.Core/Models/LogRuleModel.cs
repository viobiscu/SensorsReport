using System.Text.Json.Serialization;

namespace SensorsReport.LogRule.Consumer.Consumers;

public partial class LogRuleModel : EntityModel
{
    [JsonPropertyName("name")]
    public LogNamePropertyModel? Name { get; set; }

    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }
    [JsonPropertyName("low")]
    public ValuePropertyModel<double>? Low { get; set; }
    [JsonPropertyName("high")]
    public ValuePropertyModel<double>? High { get; set; }
    [JsonPropertyName("enabled")]
    public ValuePropertyModel<bool>? Enabled { get; set; }
    [JsonPropertyName("consecutiveHit")]
    public ValuePropertyModel<int>? ConsecutiveHit { get; set; }

    public static class StatusValues
    {
        public readonly static ValuePropertyModel<string> Active = new()
        {
            Value = "active"
        };

        public readonly static ValuePropertyModel<string> Deleted = new()
        {
            Value = "deleted"
        };

        public readonly static ValuePropertyModel<string> Disabled = new()
        {
            Value = "disabled"
        };
    }
}
