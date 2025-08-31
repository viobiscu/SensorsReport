using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.Alarm;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("Alarm")]
[DisplayName("Alarm"), InstanceName("Alarm"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class AlarmRow : OrionLDRow<AlarmRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("Description"), QuickSearch, NameProperty, JsonPropertyName("description")]
    public string? Description { get => fields.Description[this]; set => fields.Description[this] = value; }
    [DisplayName("Status"), JsonPropertyName("status")]
    public string? Status { get => fields.Status[this]; set => fields.Status[this] = value; }
    [DisplayName("Severity"), JsonPropertyName("severity")]
    public string? Severity { get => fields.Severity[this]; set => fields.Severity[this] = value; }
    [DisplayName("Monitors"), JsonPropertyName("monitors")]
    public List<RelationModel<string>>? Monitors { get => fields.Monitors[this]; set => fields.Monitors[this] = value; }
    [DisplayName("Threshold"), JsonPropertyName("threshold")]
    public double? Threshold { get => fields.Threshold[this]; set => fields.Threshold[this] = value; }
    [DisplayName("Condition"), JsonPropertyName("condition")]
    public string? Condition { get => fields.Condition[this]; set => fields.Condition[this] = value; }
    [DisplayName("Measured Value"), JsonPropertyName("measuredValue")]
    public List<MeasuredModel<double>>? MeasuredValue { get => fields.MeasuredValue[this]; set => fields.MeasuredValue[this] = value; }
}
