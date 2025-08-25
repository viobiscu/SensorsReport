using SensorsReport.Frontend.Administration;
using SensorsReport.Frontend.Common;
using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.SensorsReport.User;

[ConnectionKey("Default"), Module("SensorsReport"), TableName("User")]
[DisplayName("Users"), InstanceName("User"), GenerateFields]
[ReadPermission(PermissionKeys.Security)]
[ModifyPermission(PermissionKeys.Security)]
public sealed partial class UserRow : OLDRow<UserRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), IdProperty, JsonPropertyName("id")]
    public string? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("Username"), JsonPropertyName("username"), NameProperty, QuickSearch]
    public string Username { get => fields.Username[this]; set => fields.Username[this] = value; }

    [DisplayName("Email"), JsonPropertyName("email"), QuickSearch]
    public string Email { get => fields.Email[this]; set => fields.Email[this] = value; }

    [DisplayName("First Name"), JsonPropertyName("firstName"), QuickSearch]
    public string FirstName { get => fields.FirstName[this]; set => fields.FirstName[this] = value; }

    [DisplayName("Last Name"), JsonPropertyName("lastName"), QuickSearch]
    public string LastName { get => fields.LastName[this]; set => fields.LastName[this] = value; }

    [DisplayName("Mobile"), JsonPropertyName("mobile"), QuickSearch]
    public string Mobile { get => fields.Mobile[this]; set => fields.Mobile[this] = value; }

    [DisplayName("Language"), JsonPropertyName("language")]
    public string Language { get => fields.Language[this]; set => fields.Language[this] = value; }
}
