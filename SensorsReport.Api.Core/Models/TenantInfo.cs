
using System.Text.Json.Serialization;

namespace SensorsReport;

public class TenantInfo
{

    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = null!;

    [JsonPropertyName(name: "path")]
    public string? Path { get; set; } = null!;
}
