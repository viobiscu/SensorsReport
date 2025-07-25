
using System.Text.Json.Serialization;

namespace SensorsReport;

public class SubscriptionEventModel
{

    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; set; }
    [JsonPropertyName("tenant")]
    public TenantInfo? Tenant { get; set; }
    [JsonPropertyName("data")]
    public EntityModel[]? Data { get; set; }
}