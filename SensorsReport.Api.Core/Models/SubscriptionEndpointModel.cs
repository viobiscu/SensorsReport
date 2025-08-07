using System.Text.Json.Serialization;

namespace SensorsReport;

public class SubscriptionEndpointModel
{
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("accept")]
    public string? Accept { get; set; }
}

