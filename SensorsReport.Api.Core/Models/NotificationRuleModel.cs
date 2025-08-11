using System.Text.Json.Serialization;

namespace SensorsReport;

public partial class NotificationRuleModel : EntityModel
{
    [JsonPropertyName("name")]
    public AlarmNamePropertyModel? Name { get; set; }

    [JsonPropertyName("enable")]
    public ValuePropertyModel<string>? Enable { get; set; }
    [JsonPropertyName("notificationChannel")]
    public ValuePropertyModel<string[]>? NotificationChannel { get; set; }
    [JsonPropertyName("consecutiveHits")]
    public ValuePropertyModel<int>? ConsecutiveHits { get; set; }
    [JsonPropertyName("repetAfter")]
    public UnitPropertyModel<int>? RepetAfter { get; set; }
    [JsonPropertyName("notifyIfTimeOut")]
    public UnitPropertyModel<int>? NotifyIfTimeOut { get; set; }
    [JsonPropertyName("notifyIfClose")]
    public ValuePropertyModel<bool>? NotifyIfClose { get; set; }
    [JsonPropertyName("notifyIfAcknowledged")]
    public ValuePropertyModel<bool>? NotifyIfAcknowledged { get; set; }
    [JsonPropertyName("repetIfAcknowledged")]
    public UnitPropertyModel<int>? RepetIfAcknowledged { get; set; }
}


public partial class NotificationModel : EntityModel
{
    [JsonPropertyName("name")]
    public AlarmNamePropertyModel? Name { get; set; }

    [JsonPropertyName("enable")]
    public ValuePropertyModel<string>? Enable { get; set; }

    [JsonPropertyName("notificationRule")]
    public RelationshipModel? NotificationRule { get; set; }

    [JsonPropertyName("notificationUser")]
    public RelationshipModel? NotificationUser { get; set; }
}