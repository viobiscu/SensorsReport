using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.EmailTemplate;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("EmailTemplate")]
[DisplayName("Email Templates"), InstanceName("Email Template"), GenerateFields]
[ReadPermission(PermissionKeys.Management)]
[ModifyPermission(PermissionKeys.Management)]
public sealed partial class EmailTemplateRow : OrionLDRow<EmailTemplateRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("Subject"), Size(50), NotNull, QuickSearch, NameProperty, JsonPropertyName("subject")]
    public string? Subject { get => fields.Subject[this]; set => fields.Subject[this] = value; }

    [DisplayName("Body"), NotNull, JsonPropertyName("body")]
    [HtmlContentEditor]
    public string? Body { get => fields.Body[this]; set => fields.Body[this] = value; }
}
