using System.Text.Json.Serialization;

namespace SensorsReport;

public partial class MetaPropertyModel : PropertyModelBase
{
    [JsonPropertyName("alarmRule")]
    public RelationshipModel? AlarmRule { get; set; }
    [JsonPropertyName("alarm")]
    public RelationshipModel? Alarm { get; set; }
    [JsonPropertyName("logRule")]
    public RelationshipModel? LogRule { get; set; }
    [JsonPropertyName("logRuleConsecutiveHit")]
    public ValuePropertyModel<int>? LogRuleConsecutiveHit { get; set; }
    [JsonPropertyName("status")]
    public ObservedValuePropertyModel<string>? Status { get; set; }

    [JsonPropertyName("notification")]
    public RelationshipModel? Notification { get; set; }
}
