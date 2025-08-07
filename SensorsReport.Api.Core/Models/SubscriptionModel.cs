using System.Text.Json.Serialization;

namespace SensorsReport;

public class SubscriptionModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("entities")]
    public List<SubscriptionEntityModel>? Entities { get; set; }

    [JsonPropertyName("watchedAttributes")]
    public List<string>? WatchedAttributes { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("notification")]
    public SubscriptionNotificationModel? Notification { get; set; }

    [JsonPropertyName("throttling")]
    public int? Throttling { get; set; }

}

