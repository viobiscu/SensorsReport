using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.AlarmType;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("AlarmTypes")]
[DisplayName("Alarm Types"), InstanceName("Alarm Type"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class AlarmTypeRow : OrionLDRow<AlarmTypeRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("Name"), QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
    [DisplayName("Description"), JsonPropertyName("description")]
    public string? Description { get => fields.Description[this]; set => fields.Description[this] = value; }
    [DisplayName("Style"), JsonPropertyName("style")]
    public string? Style { get => fields.Style[this]; set => fields.Style[this] = value; }
}
