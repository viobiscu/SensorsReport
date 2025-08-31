using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.LogRule;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("LogRule")]
[DisplayName("Log Rules"), InstanceName("Log Rule"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class LogRuleRow : OrionLDRow<LogRuleRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("Name"), QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }
    [DisplayName("Unit"), Size(50), JsonPropertyName("unit")]
    public string? Unit { get => fields.Unit[this]; set => fields.Unit[this] = value; }
    [DisplayName("Low"), JsonPropertyName("low")]
    public double? Low { get => fields.Low[this]; set => fields.Low[this] = value; }
    [DisplayName("High"), JsonPropertyName("high")]
    public double? High { get => fields.High[this]; set => fields.High[this] = value; }
    [DisplayName("Consecutive Hit"), JsonPropertyName("consecutiveHit")]
    public int? ConsecutiveHit { get => fields.ConsecutiveHit[this]; set => fields.ConsecutiveHit[this] = value; }
    [DisplayName("Enabled"), NotNull, JsonPropertyName("enabled")]
    public bool? Enabled { get => fields.Enabled[this]; set => fields.Enabled[this] = value; }
}
