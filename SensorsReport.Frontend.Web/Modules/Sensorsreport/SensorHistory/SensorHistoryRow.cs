namespace SensorsReport.Frontend.SensorsReport.SensorHistory;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("SensorHistory")]
[DisplayName("Sensor History"), InstanceName("Sensor History"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
[LookupScript(Permission = "*")]
public sealed partial class SensorHistoryRow : Row<SensorHistoryRow.RowFields>
{
    [DisplayName("Id"), Identity, IdProperty]
    public long? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("Tenant"), Size(64), QuickSearch, LookupInclude]
    public string? Tenant { get => fields.Tenant[this]; set => fields.Tenant[this] = value; }
    [DisplayName("Sensor Id"), Size(64), QuickSearch, LookupInclude]
    public string? SensorId { get => fields.SensorId[this]; set => fields.SensorId[this] = value; }
    [DisplayName("Property Key"), Size(128), QuickSearch, LookupInclude]
    public string? PropertyKey { get => fields.PropertyKey[this]; set => fields.PropertyKey[this] = value; }
    [DisplayName("Metadata Key"), Size(128), QuickSearch, LookupInclude]
    public string? MetadataKey { get => fields.MetadataKey[this]; set => fields.MetadataKey[this] = value; }
    [DisplayName("Observed At"), QuickSearch, LookupInclude, DateTimeFormatter]
    public DateTime? ObservedAt { get => fields.ObservedAt[this]; set => fields.ObservedAt[this] = value; }
    [DisplayName("Value"), QuickSearch, LookupInclude]
    public double? Value { get => fields.Value[this]; set => fields.Value[this] = value; }
    [DisplayName("Unit"), Size(32), QuickSearch, LookupInclude]
    public string? Unit { get => fields.Unit[this]; set => fields.Unit[this] = value; }
    [DisplayName("Created At"), NotNull, Insertable(false), Updatable(false), DateTimeFormatter]
    public DateTime? CreatedAt { get => fields.CreatedAt[this]; set => fields.CreatedAt[this] = value; }
}