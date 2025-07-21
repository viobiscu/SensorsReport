using System.Text.Json.Serialization;

namespace SensorsReport;

public class SubscriptionEndpointModel
{
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("accept")]
    public string? Accept { get; set; }
}

public class SubscriptionNotificationModel
{
    [JsonPropertyName("attributes")]
    public List<string>? Attributes { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("endpoint")]
    public SubscriptionEndpointModel? Endpoint { get; set; }
}

public class SubscriptionEntityModel
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

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

