using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Client;
using Serenity.Extensions.DependencyInjection;
using Serenity.Pro.OpenIdClient;
using OpenIddict.Client.AspNetCore;

namespace SensorsReport.Frontend;

public static class CustomOpenIdClientServiceCollectionExtensions
{
    public static void AddExternalAuthenticationProvider(this IServiceCollection collection,
        SensorsReportOpenIdSettings settings, Action<OpenIddictClientWebIntegrationBuilder, SensorsReportOpenIdSettings> configure)
    {
        collection.TryAddSingleton<IUserClaimCreator, DefaultUserClaimCreator>();
        collection.TryAddSingleton<IElevationHandler, DefaultElevationHandler>();

        collection
            .AddOpenIddict()
            .AddCore(OpenIddictServiceCollectionExtensions.ConfigureDefaultOpenIdCoreOptions)
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();

                if (string.IsNullOrEmpty(settings.EncryptionCertificateThumbprint))
                    options.AddDevelopmentEncryptionCertificate();
                else
                    options.AddEncryptionCertificate(settings.EncryptionCertificateThumbprint);

                if (string.IsNullOrEmpty(settings.SigningCertificateThumbprint))
                    options.AddDevelopmentSigningCertificate();
                else
                    options.AddSigningCertificate(settings.SigningCertificateThumbprint);

                options.UseSystemNetHttp();

                options.UseAspNetCore()
                    .EnableStatusCodePagesIntegration()
                    .EnableRedirectionEndpointPassthrough()
                    .DisableTransportSecurityRequirement()
                    .EnablePostLogoutRedirectionEndpointPassthrough();

                var integrationBuilder = options.UseWebProviders();
                options.RemoveEventHandler(OpenIddictClientAspNetCoreHandlers
                    .ProcessLocalErrorResponse<OpenIddictClientEvents.ProcessErrorContext>.Descriptor);
                options.AddEventHandler(ViewErrorResponseHandler<OpenIddictClientEvents.ProcessErrorContext>
                    .Descriptor);

                configure?.Invoke(integrationBuilder, settings);
            });
    }


    public static OpenIddictClientWebIntegrationBuilder AddKeycloak(this OpenIddictClientWebIntegrationBuilder builder, SensorsReportOpenIdSettings settings,
        Action<OpenIddictClientWebIntegrationBuilder.Keycloak>? configure = null, bool optional = false)
    {
        var providerName = "Keycloak";
        if (settings.ExternalProviders != null &&
            settings.ExternalProviders.TryGetValue(providerName, out var providerSettings)
            && !string.IsNullOrEmpty(providerSettings?.ClientId)
            && !string.IsNullOrEmpty(providerSettings?.ClientSecret))
        {
            builder.AddKeycloak(config =>
            {
                config.SetClientId(providerSettings.ClientId)
                    .SetClientSecret(providerSettings.ClientSecret)
                    .SetIssuer(providerSettings.Issuer)
                    .SetRedirectUri(providerSettings.RedirectUrí.Replace(ExternalProviderCustomSettings.ProviderPlaceholder, providerName))
                    .AddScopes(providerSettings.Scopes);
                configure?.Invoke(config);
            });
        }
        else if (!optional)
        {
            throw ArgumentExceptions.OutOfRange(settings, "OpenIdSettings:ExternalProviders:Keycloak");
        }

        return builder;
    }
}
