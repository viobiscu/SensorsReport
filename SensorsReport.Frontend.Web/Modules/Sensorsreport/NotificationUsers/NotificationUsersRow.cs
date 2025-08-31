using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.NotificationUsers;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("NotificationUsers")]
[DisplayName("Notification Users"), InstanceName("Notification User"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class NotificationUsersRow : OrionLDRow<NotificationUsersRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("Enable"), NotNull, QuickSearch, JsonPropertyName("enable")]
    public bool? Enable { get => fields.Enable[this]; set => fields.Enable[this] = value; }
    [DisplayName("Name"), Size(100), NotNull, QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
    [DisplayName("Notifications"), NotNull, JsonPropertyName("notification")]
    public List<string>? Notification { get => fields.Notification[this]; set => fields.Notification[this] = value; }

    [DisplayName("Users"), JsonPropertyName("users")]
    public List<RelationModel<string>>? Users { get => fields.Users[this]; set => fields.Users[this] = value; }

    [DisplayName("Groups"), JsonPropertyName("groups")]
    public List<RelationModel<string>>? Groups { get => fields.Groups[this]; set => fields.Groups[this] = value; }
}

