using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using Serenity.Pro.OpenIdClient;
using Serenity.Pro.OpenIddict;
using System.Security.Claims;

namespace SensorsReport.Frontend.Administration.Pages;

public static class ClaimsPrincipalExtensions
{
    public static string? GetFirstClaimValue(this ClaimsPrincipal principal, string type) =>
        principal.FindAll(type)
                 .Select(c => c.Value)
                 .Distinct(StringComparer.Ordinal)
                 .FirstOrDefault();
}

public class OpenIdClientController : OpenIdClientControllerBase<UserExternalLoginRow>
{

    [HttpGet("~/callback/login/{provider}"), Route("~/callback/login/{provider}")]
    public override async Task<IActionResult> CallbackAsync(string provider,
        [FromServices] ISqlConnections sqlConnections,
        [FromServices] IUserClaimCreator userClaimCreator,
        [FromServices] IOptions<OpenIdSettings> options)
    {
        if (options?.Value?.EnableClient != true)
            return NotFound();

        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

        if (result.Principal is not ClaimsPrincipal { Identity.IsAuthenticated: true })
            throw new InvalidOperationException("The external authorization data cannot be used for authentication.");

        if (string.IsNullOrEmpty(result.Principal.GetFirstClaimValue(GetKeyClaimForProvider(provider))))
            throw new InvalidOperationException("Claim key is required but it is not provided by external provider.");

        using var connection = sqlConnections.NewFor<UserExternalLoginRow>();
        var link = result.Properties?.Items.Get(".link_to_user");

        if (link is not null)
        {
            if (!User.IsLoggedIn())
                throw new ValidationError("User is not signed in");
            if (User.GetIdentifier() != link)
                throw new ValidationError("User in the start of the flow and current user is not the same.");

            LinkExternalUserToCurrentUser(connection, provider, result.Principal);
            return LocalRedirect(result.Properties.RedirectUri ?? "/");
        }

        var validationActionResult = ValidateExternalUser(connection, provider, result.Principal);

        if (validationActionResult is not null)
            return validationActionResult;

        var currentApplicationPrincipal = MapExternalUserClaims(connection, provider, result.Principal, userClaimCreator);

        var properties = new AuthenticationProperties(result.Properties.Items);

        //We have to manually redirect because CookieAuthenticationHandler only honors redirect if current uri is login uri
        Response.Redirect(properties.RedirectUri ?? "/");

        return SignIn(currentApplicationPrincipal, properties, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private bool IsUserEmailValidated(ClaimsPrincipal principal)
    {
        return principal.GetFirstClaimValue("email_verified")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
    }

    protected override ClaimsPrincipal MapExternalUserClaims(IDbConnection connection,
     string provider, ClaimsPrincipal externalPrincipal, IUserClaimCreator userClaimCreator)
    {
        var appPrincipal = base.MapExternalUserClaims(connection, provider, externalPrincipal, userClaimCreator);

        var organization = externalPrincipal.GetFirstClaimValue("organization");
        if (!string.IsNullOrWhiteSpace(organization) && appPrincipal.Identity is ClaimsIdentity identity)
        {
            if (!identity.HasClaim(c => c.Type.Equals("organization", StringComparison.Ordinal)))
                identity.AddClaim(new Claim("organization", organization, ClaimValueTypes.String));
        }

        return appPrincipal;
    }

    private bool HasUser(IDbConnection connection, string provider, ClaimsPrincipal principal)
    {
        var providerKey = principal.GetFirstClaimValue(GetKeyClaimForProvider(provider));
        var instance = (IUserExternalLoginRow)new UserExternalLoginRow();

        return connection.Exists<UserExternalLoginRow>(
            instance.LoginProviderField == provider &&
            instance.ProviderKeyField == providerKey);
    }

    private LocalRedirectResult RedirectToSignUp(string provider, ClaimsPrincipal principal)
    {
        var email = principal.GetFirstClaimValue(OpenIddictConstants.Claims.Email);
        var name = principal.GetFirstClaimValue(OpenIddictConstants.Claims.Name) ??
                principal.GetFirstClaimValue(OpenIddictConstants.Claims.Nickname) ??
                principal.GetFirstClaimValue(OpenIddictConstants.Claims.PreferredUsername);

        var emailVerified = principal.GetClaims(OpenIddictConstants.Claims.EmailVerified).Any(x => Convert.ToBoolean(x));

        var externalProviderId = principal.GetFirstClaimValue(GetKeyClaimForProvider(provider));

        var dto = new SignUpWithAccountLinkDTO()
        {
            ProviderKey = externalProviderId,
            Provider = provider,
            DisplayName = name,
            Email = email,
            EmailVerified = emailVerified,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        var externalProviderToken = HttpContext.RequestServices.GetDataProtector("SignUpAccountLinking")
            .Protect(JSON.Stringify(dto));

        var query = new Dictionary<string, string>
        {
            ["ProviderToken"] = externalProviderToken
        };

        return LocalRedirect(QueryHelpers.AddQueryString("~/Account/SignUp", query));
    }

    private bool IsUserPendingActivation(IDbConnection connection, string provider, ClaimsPrincipal principal)
    {
        var providerKey = principal.GetFirstClaimValue(GetKeyClaimForProvider(provider));
        var instance = (IUserExternalLoginRow)new UserExternalLoginRow();

        return connection.Exists<UserExternalLoginRow>(
            instance.LoginProviderField == provider &&
            instance.ProviderKeyField == providerKey) && !IsUserEmailValidated(principal);
    }

    private void ValidateAccountActivity(IDbConnection connection, string provider, ClaimsPrincipal principal)
    {
        var externalProviderId = principal.GetFirstClaimValue(GetKeyClaimForProvider(provider));

        var instance = (IUserExternalLoginRow)new UserExternalLoginRow();

        if (connection.Exists<UserExternalLoginRow>(
            instance.LoginProviderField == provider &&
            instance.ProviderKeyField == externalProviderId &&
            instance.UserIsActiveField != 1 &&
            instance.UserIsActiveField != 0))
        {
            throw new ValidationError("There was an error while validation account details. Please contact your administrator.");
        }
    }

    protected virtual IActionResult ValidateExternalUser(IDbConnection connection, string provider, ClaimsPrincipal principal)
    {
        if (!HasUser(connection, provider, principal))
            return RedirectToSignUp(provider, principal);

        if (IsUserPendingActivation(connection, provider, principal))
            throw new ValidationError("Account is not activated. Please check your email to activate your account.");

        ValidateAccountActivity(connection, provider, principal);

        return null;
    }
}