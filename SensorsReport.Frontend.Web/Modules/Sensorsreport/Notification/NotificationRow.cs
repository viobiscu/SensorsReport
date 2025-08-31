using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.Notification;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("Notification")]
[DisplayName("Notifications"), InstanceName("Notification"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class NotificationRow : OrionLDRow<NotificationRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("Enable"), NotNull, QuickSearch, JsonPropertyName("enable")]
    public bool? Enable { get => fields.Enable[this]; set => fields.Enable[this] = value; }
    [DisplayName("Name"), Size(100), NotNull, QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
    [DisplayName("Notification Rule"), JsonPropertyName("notificationRule")]
    public string? NotificationRule { get => fields.NotificationRule[this]; set => fields.NotificationRule[this] = value; }
    [DisplayName("Notification User"), JsonPropertyName("notificationUser")]
    public string? NotificationUser { get => fields.NotificationUser[this]; set => fields.NotificationUser[this] = value; }
    [DisplayName("SMS"), JsonPropertyName("SMS")]
    public List<RelationModel<string>>? SMS { get => fields.SMS[this]; set => fields.SMS[this] = value; }
    [DisplayName("Email"), JsonPropertyName("email")]
    public List<RelationModel<string>>? Email { get => fields.Email[this]; set => fields.Email[this] = value; }
    //monitors
    [DisplayName("Monitors"), JsonPropertyName("monitors")]
    public List<RelationModel<string>>? Monitors { get => fields.Monitors[this]; set => fields.Monitors[this] = value; }
}

