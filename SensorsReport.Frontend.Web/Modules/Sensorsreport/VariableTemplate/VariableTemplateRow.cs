using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.VariableTemplate;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("VariableTemplate")]
[DisplayName("Variable Templates"), InstanceName("Variable Template"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class VariableTemplateRow : OrionLDRow<VariableTemplateRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("Name"), QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
}
