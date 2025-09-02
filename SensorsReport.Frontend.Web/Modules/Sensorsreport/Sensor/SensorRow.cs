using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.Sensor;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("TG8I")]
[DisplayName("Sensor"), InstanceName("Sensor"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class SensorRow : OrionLDRow<SensorRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id"), NameProperty]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("T0 Name")]
    public string? T0_Name { get => fields.T0_Name[this]; set => fields.T0_Name[this] = value; }
    [DisplayName("T0")]
    public double? T0 { get => fields.T0[this]; set => fields.T0[this] = value; }

    [DisplayName("T0 Unit")]
    public string? T0_Unit { get => fields.T0_Unit[this]; set => fields.T0_Unit[this] = value; }

    [DisplayName("T0 Observed At")]
    public DateTime? T0_ObservedAt { get => fields.T0_ObservedAt[this]; set => fields.T0_ObservedAt[this] = value; }
    [DisplayName("T0 Status")]
    public string? T0_Status { get => fields.T0_Status[this]; set => fields.T0_Status[this] = value; }
    [DisplayName("Rh0 Name")]
    public string? RH0_Name { get => fields.RH0_Name[this]; set => fields.RH0_Name[this] = value; }
    [DisplayName("Rh0")]
    public double? RH0 { get => fields.RH0[this]; set => fields.RH0[this] = value; }
    [DisplayName("Rh0 Unit")]
    public string? RH0_Unit { get => fields.RH0_Unit[this]; set => fields.RH0_Unit[this] = value; }
    [DisplayName("Rh0 Observed At")]
    public DateTime? RH0_ObservedAt { get => fields.RH0_ObservedAt[this]; set => fields.RH0_ObservedAt[this] = value; }
    [DisplayName("Rh0 Status")]
    public string? RH0_Status { get => fields.RH0_Status[this]; set => fields.RH0_Status[this] = value; }
}
