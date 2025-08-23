using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Client.AspNetCore;
using Serenity.Pro.OpenIddict;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace SensorsReport.Frontend.Membership.Pages;
[Route("Account/[action]")]
public partial class AccountPage(ITwoLevelCache cache, ITextLocalizer localizer) : Controller
{
    protected ITwoLevelCache Cache { get; } = cache ?? throw new ArgumentNullException(nameof(cache));
    protected ITextLocalizer Localizer { get; } = localizer ?? throw new ArgumentNullException(nameof(localizer));

    [HttpGet]
    public ActionResult Login(int? denied, string activated, string returnUrl,
        [FromServices] IOptions<SensorsReportOpenIdSettings> options
    )
    {
        if (denied == 1)
            return View(MVC.Views.Errors.AccessDenied,
                ("~/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl)));

        if (options?.Value?.EnableClient != true)
            return NotFound();

        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictClientAspNetCoreConstants.Properties.ProviderName] = "Keycloak"
        })
        {
            RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
        };

        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public ActionResult AccessDenied(string returnURL)
    {
        return View(MVC.Views.Errors.AccessDenied, (object)returnURL);
    }

    [HttpPost, JsonRequest]
    public Result<ServiceResponse> Login(LoginRequest request,
        [FromServices] ISqlConnections sqlConnections,
        [FromServices] IUserPasswordValidator passwordValidator,
        [FromServices] IUserClaimCreator userClaimCreator,
        [FromServices] ITwoFactorManager twoFactorManager = null)
    {
        throw new ValidationError("DisabledLoginMethod", " Login directly with username and password is disabled. Please use an external login provider.");
    }

    private ViewResult Error(string message)
    {
        return View(MVC.Views.Errors.ValidationError, new ValidationError(message));
    }

    public string KeepAlive()
    {
        return "OK";
    }

    public ActionResult Signout([FromServices] IOptions<SensorsReportOpenIdSettings> options)
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.RequestServices.GetService<IElevationHandler>()?.DeleteToken();

        var keycloak = options.Value.ExternalProviders["Keycloak"];
        return new RedirectResult(keycloak.SignedOutCallbackPath);
    }
}