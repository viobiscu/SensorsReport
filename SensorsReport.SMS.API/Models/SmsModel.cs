using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SensorsReport;

namespace SensorsReport.SMS.API.Models;

public class SmsModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [ExcludeFromRequest]
    public string? Id { get; set; }
    [AllowPatch]
    public string PhoneNumber { get; set; } = null!;
    [AllowPatch]
    public string Message { get; set; } = null!;
    [Description("The UTC timestamp when the SMS was created or logged.")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [ResponseIgnore]
    [ValidateNever]
    public string Tenant { get; set; } = null!;
    [AllowPatch]
    public SmsStatusEnum Status { get; set; } = SmsStatus.Default; // e.g., "Sent", "Failed", etc.
    public string StatusMessage => SmsStatus.GetStatusMessage(Status);
    [AllowPatch]
    public DateTime? SentAt { get; set; } // Timestamp when the SMS was sent
    [AllowPatch]
    public int? Ttl { get; set; } = (int)TimeSpan.FromMinutes(5).TotalMinutes; // Time to live for the SMS, if applicable
    [AllowPatch]
    public string? CountryCode { get; set; } // Optional country code for the phone number
    [AllowPatch]
    public string Provider { get; set; } = null!; // Optional provider name if applicable
    [AllowPatch]
    public string? TrackingId { get; set; } // Optional tracking ID for tracking in third-party systems
    [AllowPatch]
    public string? MessageType { get; set; } // Optional type of message (e.g., "Alert", "Notification", etc.)
    [AllowPatch]
    public string? CustomData { get; set; } // Optional custom data in JSON format for additional metadata
    [AllowPatch]
    public int RetryCount { get; set; } = 0; // Number of retries attempted for sending the SMS

    [JsonExtensionData]
    public Dictionary<string, JsonElement> OtherFields { get; set; } = new Dictionary<string, JsonElement>();
}
