using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using SensorsReport.Frontend.Administration;
using Serenity.Pro.OpenIdClient;
using Serenity.Web.Providers;

namespace SensorsReport.Frontend.Membership.Pages;
public partial class AccountPage : Controller
{
    [HttpGet]
    public ActionResult SignUp([FromQuery] string providerToken)
    {
        var model = new SignupPageModel();
        if (providerToken is not null)
        {
            var dto = JSON.Parse<SignUpWithAccountLinkDTO>(HttpContext.RequestServices
                .GetDataProtector("SignUpAccountLinking").Unprotect(providerToken));
            model.DisplayName = dto.DisplayName;
            model.Email = dto.Email;
            model.ExternalProviderToken = providerToken;
        }
        return View(MVC.Views.Membership.Account.SignUp.SignUpPage, model);
    }
    private SignUpResponse SignUpWithAccountLink(
        IDbConnection connection,
        SignUpRequest request,
        IEmailSender emailSender,
        IPermissionKeyLister permissionKeyLister,
        ISiteAbsoluteUrl siteAbsoluteUrl,
        IUserClaimCreator userClaimCreator,
        IOptions<Serenity.Pro.OpenIddict.OpenIdSettings> options)
    {
        ArgumentNullException.ThrowIfNull(userClaimCreator);

        var dto = JSON.Parse<SignUpWithAccountLinkDTO>(HttpContext.RequestServices
            .GetDataProtector("SignUpAccountLinking").Unprotect(request.ExternalProviderToken));

        if (dto.ExpiresAt < DateTime.UtcNow)
            throw new ValidationError("Provider token is expired");

        short isActive = options.Value.ForceActivation != true && request.Email == dto.Email && dto.EmailVerified ? (short)1 : (short)0;

        var extLogin = UserExternalLoginRow.Fields;
        if (connection.Exists<UserExternalLoginRow>(extLogin.ProviderKey == dto.ProviderKey &&
                                                    extLogin.LoginProvider == dto.Provider))
            throw new ValidationError($"This {dto.Provider} account is linked to another user");

        if (connection.Exists<UserRow>(
                UserRow.Fields.Username == request.Email |
                UserRow.Fields.Email == request.Email))
            throw new ValidationError("EmailInUse", MembershipValidationTexts.EmailInUse.ToString(Localizer));

        using var uow = new UnitOfWork(connection);

        var username = request.Email;
        var displayName = request.DisplayName;
        var email = request.Email;

        var userId = (int)connection.InsertAndGetID(new UserRow
        {
            Username = username,
            Source = "extn",
            DisplayName = displayName,
            Email = email,
            PasswordHash = SiteMembershipProvider.ComputeSHA512(Guid.NewGuid().ToString()),
            PasswordSalt = "unassigned",
            IsActive = isActive,
            InsertDate = DateTime.Now,
            InsertUserId = 1,
            LastDirectoryUpdate = DateTime.Now
        });

        connection.Insert(new UserExternalLoginRow
        {
            LoginProvider = dto.Provider,
            ProviderKey = dto.ProviderKey,
            UserId = userId
        });

        if (isActive != 1)
            SendActivationEmail(siteAbsoluteUrl, emailSender, userId, username, displayName, email);

        uow.Commit();

        if (isActive == 1)
        {
            var principal = userClaimCreator.CreatePrincipal(username, authType: "External");
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).GetAwaiter()
                .GetResult();
        }


        return new SignUpResponse()
        {
            NeedsActivation = isActive != 1
        };
    }
    private string SendActivationEmail(ISiteAbsoluteUrl siteAbsoluteUrl, IEmailSender emailSender, int userId,
        string username, string displayName, string email)
    {
        var token = HttpContext.RequestServices.GetDataProtector(ActivatePurpose)
            .ProtectBinary(bw =>
            {
                bw.Write(DateTime.UtcNow.AddHours(3).ToBinary());
                bw.Write(userId);
            });

        var externalUrl = siteAbsoluteUrl.GetExternalUrl();
        var activateLink = UriHelper.Combine(externalUrl, "Account/Activate?t=");
        activateLink += Uri.EscapeDataString(token);

        var emailModel = new ActivateEmailModel
        {
            Username = username,
            DisplayName = displayName,
            ActivateLink = activateLink
        };

        var emailSubject = SignUpFormTexts.ActivateEmailSubject.ToString(Localizer);
        var emailBody = Serenity.Web.TemplateHelper.RenderViewToString(HttpContext.RequestServices,
            MVC.Views.Membership.Account.SignUp.ActivateEmail, emailModel);

        ArgumentNullException.ThrowIfNull(emailSender);

        emailSender.Send(subject: emailSubject, body: emailBody, mailTo: email);
        return token;
    }

    [HttpPost, JsonRequest]
    public Result<SignUpResponse> SignUp(SignUpRequest request,
        [FromServices] IEmailSender emailSender,
        [FromServices] IPasswordStrengthValidator passwordRuleValidator,
        [FromServices] IPermissionKeyLister permissionKeyLister,
        [FromServices] ISiteAbsoluteUrl siteAbsoluteUrl,
        [FromServices] ITypeSource typeSource,
        [FromServices] IUserProvider userProvider,
        [FromServices] IOptions<RecaptchaSettings> recaptchaOptions,
        [FromServices] IOptions<EnvironmentSettings> environmentOptions
        , [FromServices] IOptions<Serenity.Pro.OpenIddict.OpenIdSettings> openIdOptions
    )
    {
        return this.UseConnection("Default", connection =>
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrEmpty(recaptchaOptions?.Value?.SiteKey) ||
                !string.IsNullOrEmpty(recaptchaOptions?.Value?.SecretKey))
            {
                RecaptchaValidation.Validate(recaptchaOptions.Value.SecretKey, request.Recaptcha, Localizer);
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
            if (request.ExternalProviderToken is not null)
                return SignUpWithAccountLink(connection, request, emailSender, permissionKeyLister, siteAbsoluteUrl, userProvider,
                    openIdOptions);
            ArgumentException.ThrowIfNullOrEmpty(request.Password);
            passwordRuleValidator.Validate(request.Password);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.DisplayName);

            if (connection.Exists<UserRow>(
                    UserRow.Fields.Username == request.Email |
                    UserRow.Fields.Email == request.Email))
            {
                throw new ValidationError("EmailInUse", MembershipValidationTexts.EmailInUse.ToString(Localizer));
            }

            using var uow = new UnitOfWork(connection);
            string salt = null;
            var hash = UserHelper.GenerateHash(request.Password ?? "", ref salt);
            var displayName = request.DisplayName.TrimToEmpty();
            var email = request.Email;
            var username = request.Email;

            var userId = (int)connection.InsertAndGetID(new UserRow
            {
                Username = username,
                Source = "sign",
                DisplayName = displayName,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = 0,
                InsertDate = DateTime.Now,
                InsertUserId = 1,
                LastDirectoryUpdate = DateTime.Now
            });

            SendActivationEmail(
                siteAbsoluteUrl, emailSender, userId, username, displayName, email);

            uow.Commit();

            userProvider.RemoveCachedUser(userId.ToInvariant(), username);

            return new SignUpResponse();
        });
    }

    private const string ActivatePurpose = "Activate";

    [HttpGet]
    public ActionResult Activate(string t, [FromServices] ISqlConnections sqlConnections)
    {
        ArgumentNullException.ThrowIfNull(sqlConnections);

        using var connection = sqlConnections.NewByKey("Default");
        using var uow = new UnitOfWork(connection);
        int userId;
        try
        {
            using var br = HttpContext.RequestServices.GetDataProtector(ActivatePurpose)
                .UnprotectBinary(t);

            var dt = DateTime.FromBinary(br.ReadInt64());
            if (dt < DateTime.UtcNow)
                return Error(MembershipValidationTexts.InvalidActivateToken.ToString(Localizer));

            userId = br.ReadInt32();
        }
        catch (Exception)
        {
            return Error(MembershipValidationTexts.InvalidActivateToken.ToString(Localizer));
        }

        var user = uow.Connection.TryById<UserRow>(userId);
        if (user == null || user.IsActive != 0)
            return Error(MembershipValidationTexts.InvalidActivateToken.ToString(Localizer));

        uow.Connection.UpdateById(new UserRow
        {
            UserId = user.UserId.Value,
            IsActive = 1
        });

        Cache.InvalidateOnCommit(uow, UserRow.Fields);
        uow.Commit();

        return new RedirectResult("~/Account/Login?activated=" + Uri.EscapeDataString(user.Email));
    }
}