using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SensorsReportAudit.Models
{
    /// <summary>
    /// Represents an entity in QuantumLeap/Orion-LD format
    /// </summary>
    public class QuantumLeapEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty; // Default to an empty string

        [JsonProperty("type")]
        public string EntityType { get; set; } = string.Empty;

        [JsonProperty("attributes")]
        public Dictionary<string, AttributeValue> Attributes { get; set; } = new Dictionary<string, AttributeValue>();
    }

    /// <summary>
    /// Represents an attribute value in QuantumLeap/Orion-LD format
    /// </summary>
    public class AttributeValue
    {
        [JsonProperty("value")]
        public object Value { get; set; } = null!;

        [JsonProperty("type")]
        public string Type { get; set; } = "Text"; // Default type is Text



        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Represents an audit record that can be converted to QuantumLeap format
    /// </summary>
    public class AuditRecord
    {
        public string Id { get; set; } = $"urn:ngsi-ld:AuditRecord:{Guid.NewGuid()}";
        public string Type { get; set; } = "AuditRecord";
        public string ActionType { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "success";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Converts the audit record to a QuantumLeap entity
        /// </summary>
        public QuantumLeapEntity ToQuantumLeapEntity()
        {
            var entity = new QuantumLeapEntity
            {
                Id = Id,
                EntityType = "AuditRecord",
                Attributes = new Dictionary<string, AttributeValue>
                {
                    ["actionType"] = new AttributeValue { Type = "Text", Value = ActionType },
                    ["userId"] = new AttributeValue { Type = "Text", Value = UserId },
                    ["userName"] = new AttributeValue { Type = "Text", Value = UserName },
                    ["resourceId"] = new AttributeValue { Type = "Text", Value = ResourceId },
                    ["resourceType"] = new AttributeValue { Type = "Text", Value = ResourceType },
                    ["description"] = new AttributeValue { Type = "Text", Value = Description },
                    ["status"] = new AttributeValue { Type = "Text", Value = Status },
                    ["timestamp"] = new AttributeValue { Type = "DateTime", Value = Timestamp.ToString("o") }
                }
            };

            // Add any additional data fields
            foreach (var item in AdditionalData)
            {
                string type = DetermineAttributeType(item.Value);
                entity.Attributes[item.Key] = new AttributeValue
                {
                    Type = type,
                    Value = item.Value
                };
            }

            return entity;
        }

        private string DetermineAttributeType(object value)
        {
            return value switch
            {
                int or long or short or byte or sbyte or uint or ulong or ushort => "Number",
                float or double or decimal => "Number",
                bool => "Boolean",
                DateTime => "DateTime",
                DateTimeOffset => "DateTime",
                string => "Text",
                _ => "Object"
            };
        }
    }
}