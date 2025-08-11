namespace SensorsReport;

public class CreateSmsCommand
{
    public string PhoneNumber { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Default to current UTC time
    public string? Tenant { get; set; } = null!;
    public int Status { get; set; } // e.g., "Sent", "Failed", etc.
    public DateTime? SentAt { get; set; } // Timestamp when the SMS was sent
    public int? Ttl { get; set; } // Time to live for the SMS, if applicable
    public string? CountryCode { get; set; } // Optional country code for the phone number
    public string Provider { get; set; } = null!; // Optional provider name if applicable
    public string? TrackingId { get; set; } // Optional tracking ID for tracking in third-party systems
    public string? MessageType { get; set; } // Optional type of message (e.g., "Alert", "Notification", etc.)
    public string? CustomData { get; set; } // Optional custom data in JSON format for additional metadata
    public int RetryCount { get; set; } = 0; // Number of retries attempted for sending the SMS
}
