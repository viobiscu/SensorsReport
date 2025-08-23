using Microsoft.IdentityModel.Tokens;

namespace SensorsReport.Frontend;

public class ExternalProviderCustomSettings
{
    public const string ProviderPlaceholder = "{provider}";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUrí { get; set; } = $"callback/login/{ProviderPlaceholder}";
    public string Issuer { get; set; }
    public string ResponseType { get; set; } = "code";
    public bool SaveTokens { get; set; } = true;    
    public string[] Scopes { get; set; } = [];
    public string SignedOutCallbackPath { get; set; } = $"/callback/logout/{ProviderPlaceholder}";

    public TokenValidationParameters TokenValidationParameters { get; set; } = new();
}
