using Serenity.Pro.OpenIddict;

namespace SensorsReport.Frontend;

[DefaultSectionKey(SectionKey)]
public class SensorsReportOpenIdSettings
{
    public const string SectionKey = "OpenIdSettings";

    public bool EnableServer { get; set; }
    public bool EnableClient { get; set; }

    public OpenIdScope[] Scopes { get; set; }
    public string EncryptionCertificateThumbprint { get; set; }
    public string SigningCertificateThumbprint { get; set; }
    public string FallbackIconUrl { get; set; } = "~/Serenity.Pro.OpenIddict/default-app-icon.svg";
    public bool ForceActivation { get; set; } = false;
    public Dictionary<string, ExternalProviderCustomSettings> ExternalProviders { get; set; }

    public IEnumerable<OpenIdScope> GetScopes() => Scopes ?? DefaultScopes;

    public static readonly OpenIdScope[] DefaultScopes =
    [
        new () { Name = "profile" },
        new () { Name = "email" },
    ];

}
