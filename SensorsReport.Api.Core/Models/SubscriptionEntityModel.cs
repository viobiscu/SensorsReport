using System.Text.Json.Serialization;

namespace SensorsReport;

public class SubscriptionEntityModel
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

