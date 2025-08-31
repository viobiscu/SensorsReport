using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.Group;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("Group")]
[DisplayName("Groups"), InstanceName("Group"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class GroupRow : OrionLDRow<GroupRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("Name"), Size(50), NotNull, QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
    [DisplayName("Users"), JsonPropertyName("users")]
    public List<string> Users { get => fields.Users[this]; set => fields.Users[this] = value; }
}
