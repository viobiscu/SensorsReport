using System.Text.Json.Serialization;

namespace SensorsReport;

public partial class AlarmModel : EntityModel
{
    [JsonPropertyName("description")]
    public ValuePropertyModel<string>? Description { get; set; }

    [JsonPropertyName("status")]
    public ValuePropertyModel<string>? Status { get; set; }

    [JsonPropertyName("severity")]
    public ValuePropertyModel<string>? Severity { get; set; } = AlarmModel.SeverityLevels.None;

    [JsonPropertyName("triggeredAt")]
    public ValuePropertyModel<DateTimeOffset>? TriggeredAt { get; set; }

    [JsonPropertyName("monitors")]
    public RelationshipValuesModel? Monitors { get; set; }

    [JsonPropertyName("threshold")]
    public ValuePropertyModel<double>? Threshold { get; set; }

    [JsonPropertyName("condition")]
    public ValuePropertyModel<string>? Condition { get; set; }

    [JsonPropertyName("measuredValue")]
    public ValuePropertyModel<List<ObservedValuePropertyModel<double>>>? MeasuredValue { get; set; }

    [JsonPropertyName("location")]
    public ValuePropertyModel<Point>? Location { get; set; }
}

public partial class AlarmModel
{
    public static class SeverityLevels
    {
        public readonly static ValuePropertyModel<string> None = new()
        {
            Value = "none"
        };

        public readonly static ValuePropertyModel<string> High = new()
        {
            Value = "high"
        };

        public readonly static ValuePropertyModel<string> Medium = new()
        {
            Value = "medium"
        };

        public readonly static ValuePropertyModel<string> Low = new()
        {
            Value = "low"
        };
    }

    public static class StatusValues
    {
        public readonly static ValuePropertyModel<string> Open = new()
        {
            Value = "open"
        };
        public readonly static ValuePropertyModel<string> Close = new()
        {
            Value = "close"
        };
    }
}
