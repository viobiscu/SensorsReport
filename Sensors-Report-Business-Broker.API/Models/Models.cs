using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SensorsReportBusinessBroker.API.Models
{
    public class NotificationRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; } = string.Empty;

        [JsonPropertyName("notifiedAt")]
        public string NotifiedAt { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
    }

    public class BusinessRuleEntities
    {
        public BusinessRuleEntity[] Entities { get; set; } = Array.Empty<BusinessRuleEntity>();
    }

    public class BusinessRuleEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("ruleName")]
        public Property<string>? RuleName { get; set; }

        [JsonPropertyName("ruleContent")]
        public Property<string>? RuleContent { get; set; }

        [JsonPropertyName("targetEntityType")]
        public Property<string>? TargetEntityType { get; set; }

        [JsonPropertyName("enabled")]
        public Property<bool>? Enabled { get; set; }

        [JsonPropertyName("priority")]
        public Property<int>? Priority { get; set; }
    }

    public class Property<T>
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public T Value { get; set; } = default!;
    }

    public class AuditEvent
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}