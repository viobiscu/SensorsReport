using System.Text.Json.Serialization;

namespace SensorsReport;

public class SubscriptionNotificationModel
{
    [JsonPropertyName("attributes")]
    public List<string>? Attributes { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("endpoint")]
    public SubscriptionEndpointModel? Endpoint { get; set; }
}

