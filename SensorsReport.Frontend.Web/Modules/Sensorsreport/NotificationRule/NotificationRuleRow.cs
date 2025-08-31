using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.NotificationRule;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("NotificationRule")]
[DisplayName("Notification Rules"), InstanceName("Notification Rule"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class NotificationRuleRow : OrionLDRow<NotificationRuleRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("Enable"), NotNull, QuickSearch, JsonPropertyName("enable")]
    public bool? Enable { get => fields.Enable[this]; set => fields.Enable[this] = value; }
    [DisplayName("Name"), Size(100), NotNull, QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
    [DisplayName("Consecutive Hits"), JsonPropertyName("consecutiveHits")]
    public int? ConsecutiveHits { get => fields.ConsecutiveHits[this]; set => fields.ConsecutiveHits[this] = value; }
    [DisplayName("Repeat After (min)"), JsonPropertyName("repeatAfter")]
    public int? RepeatAfter { get => fields.RepeatAfter[this]; set => fields.RepeatAfter[this] = value; }
    [DisplayName("Notify If Close"), JsonPropertyName("notifyIfClose")]
    public bool? NotifyIfClose { get => fields.NotifyIfClose[this]; set => fields.NotifyIfClose[this] = value; }
    [DisplayName("Notify If Acknowledged (min)"), JsonPropertyName("notifyIfAcknowledged")]
    public bool? NotifyIfAcknowledged { get => fields.NotifyIfAcknowledged[this]; set => fields.NotifyIfAcknowledged[this] = value; }
    [DisplayName("Repeat If Acknowledged (min)"), JsonPropertyName("repeatIfAcknowledged")]
    public int? RepeatIfAcknowledged { get => fields.RepeatIfAcknowledged[this]; set => fields.RepeatIfAcknowledged[this] = value; }
    [DisplayName("Notify If Time Out"), JsonPropertyName("notifyIfTimeOut")]
    public int? NotifyIfTimeOut { get => fields.NotifyIfTimeOut[this]; set => fields.NotifyIfTimeOut[this] = value; }
    [DisplayName("Notification Channel"), JsonPropertyName("notificationChannel")]
    public List<string> NotificationChannel { get => fields.NotificationChannel[this]; set => fields.NotificationChannel[this] = value; }
}
