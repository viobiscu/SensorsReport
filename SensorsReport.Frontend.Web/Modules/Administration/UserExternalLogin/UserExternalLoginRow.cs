using Serenity.Pro.OpenIdClient;

namespace SensorsReport.Frontend.Administration;
[ConnectionKey("Default"), Module("Administration"), TableName("UserExternalLogins")]
[DisplayName("User External Logins"), InstanceName("User External Login"), GenerateFields]
[ReadPermission("Administration:General")]
[ModifyPermission("Administration:General")]
public sealed partial class UserExternalLoginRow : Row<UserExternalLoginRow.RowFields>, IIdRow, INameRow, IUserExternalLoginRow
{
    const string jUser = nameof(jUser);

    [DisplayName("ID"), Identity, IdProperty]
    public int? UserExternalLoginId { get => fields.UserExternalLoginId[this]; set => fields.UserExternalLoginId[this] = value; }

    [DisplayName("Login Provider"), Size(100), PrimaryKey, NotNull, QuickSearch, NameProperty]
    public string LoginProvider { get => fields.LoginProvider[this]; set => fields.LoginProvider[this] = value; }

    [DisplayName("Provider Key"), Size(200), PrimaryKey, NotNull]
    public string ProviderKey { get => fields.ProviderKey[this]; set => fields.ProviderKey[this] = value; }

    [DisplayName("User"), NotNull, ForeignKey("Users", "UserId"), LeftJoin(jUser), TextualField(nameof(Username))]
    public int? UserId { get => fields.UserId[this]; set => fields.UserId[this] = value; }

    [DisplayName("Username"), Expression($"{jUser}.[Username]")]
    public string Username { get => fields.Username[this]; set => fields.Username[this] = value; }

    [DisplayName("User Activated"), Expression($"{jUser}.[IsActive]")]
    public short? UserIsActive { get => fields.UserIsActive[this]; set => fields.UserIsActive[this] = value; }

    Field IUserExternalLoginRow.UserIdField => fields.UserId;
    Int16Field IUserExternalLoginRow.UserIsActiveField => fields.UserIsActive;
    StringField IUserExternalLoginRow.UsernameField => fields.Username;
    StringField IUserExternalLoginRow.LoginProviderField => fields.LoginProvider;
    StringField IUserExternalLoginRow.ProviderKeyField => fields.ProviderKey;
}
