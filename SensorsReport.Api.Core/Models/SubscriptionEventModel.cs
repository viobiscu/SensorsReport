
using System.Text.Json.Serialization;

namespace SensorsReport;

public class SubscriptionEventModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; set; }
    [JsonPropertyName("tenant")]
    public TenantInfo? Tenant { get; set; }
    [JsonPropertyName("data")]
    public EntityModel[]? Data { get; set; }

    [JsonIgnore]
    public EntityModel? Item => Data?.FirstOrDefault();
}
