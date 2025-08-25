using SensorsReport.Frontend.Administration;
using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.ApiKey;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("APIKey")]
[DisplayName("API Keys"), InstanceName("API Key"), GenerateFields]
[ReadPermission(PermissionKeys.Security)]
[ModifyPermission(PermissionKeys.Security)]
public sealed partial class ApiKeyRow : OLDRow<ApiKeyRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }
    [DisplayName("Tenant Id"), Size(50), NotNull, QuickSearch, NameProperty, JsonPropertyName("TenantID")]
    public string? TenantId { get => fields.TenantId[this]; set => fields.TenantId[this] = value; }
    [DisplayName("Api Key"), Size(50), NotNull, QuickSearch, JsonPropertyName("APIKey")]
    public string? ApiKey { get => fields.ApiKey[this]; set => fields.ApiKey[this] = value; }
}
