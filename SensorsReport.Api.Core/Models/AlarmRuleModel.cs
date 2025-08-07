using System.Text.Json.Serialization;

namespace SensorsReport;

public partial class AlarmRuleModel : EntityModel
{
    [JsonPropertyName("name")]
    public AlarmNamePropertyModel? Name { get; set; }

    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }
    [JsonPropertyName("low")]
    public ValuePropertyModel<double>? Low { get; set; }
    [JsonPropertyName("prelow")]
    public ValuePropertyModel<double>? PreLow { get; set; }
    [JsonPropertyName("prehigh")]
    public ValuePropertyModel<double>? PreHigh { get; set; }
    [JsonPropertyName("high")]
    public ValuePropertyModel<double>? High { get; set; }
    [JsonPropertyName("status")]
    public ValuePropertyModel<string>? Status { get; set; }
}


public partial class AlarmRuleModel
{
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