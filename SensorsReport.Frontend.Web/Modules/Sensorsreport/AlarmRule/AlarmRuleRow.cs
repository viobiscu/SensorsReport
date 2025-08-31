using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.AlarmRule;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("AlarmRule")]
[DisplayName("Alarm Rules"), InstanceName("Alarm Rule"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class AlarmRuleRow : OrionLDRow<AlarmRuleRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("Name"), QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
    [DisplayName("Unit"), Size(50), JsonPropertyName("unit")]
    public string? Unit { get => fields.Unit[this]; set => fields.Unit[this] = value; }
    [DisplayName("Low"), JsonPropertyName("low")]
    public double? Low { get => fields.Low[this]; set => fields.Low[this] = value; }
    [DisplayName("Pre Low"), JsonPropertyName("preLow")]
    public double? PreLow { get => fields.PreLow[this]; set => fields.PreLow[this] = value; }
    [DisplayName("Pre High"), JsonPropertyName("preHigh")]
    public double? PreHigh { get => fields.PreHigh[this]; set => fields.PreHigh[this] = value; }
    [DisplayName("High"), JsonPropertyName("high")]
    public double? High { get => fields.High[this]; set => fields.High[this] = value; }
}
