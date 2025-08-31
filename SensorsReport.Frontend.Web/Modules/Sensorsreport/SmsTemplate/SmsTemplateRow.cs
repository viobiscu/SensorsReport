using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.SmsTemplate;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("SMSTemplate")]
[DisplayName("Sms Templates"), InstanceName("Sms Template"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class SmsTemplateRow : OrionLDRow<SmsTemplateRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("Name"), Size(50), NotNull, QuickSearch, NameProperty, JsonPropertyName("name")]
    public string? Name { get => fields.Name[this]; set => fields.Name[this] = value; }

    [DisplayName("Message"), NotNull, JsonPropertyName("message")]
    [TextAreaEditor]
    public string? Message { get => fields.Message[this]; set => fields.Message[this] = value; }
}
