using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SensorsReport;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sensors_Report_Email.API;

public class EmailModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [ExcludeFromRequest]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("toEmail")]
    [AllowPatch]
    [EmailAddress]
    public string ToEmail { get; set; } = null!;

    [BsonElement("toName")]
    [AllowPatch]
    [MaxLength(250)]
    public string ToName { get; set; } = null!;

    [BsonElement("fromEmail")]
    [AllowPatch]
    [EmailAddress]
    public string? FromEmail { get; set; }

    [BsonElement("fromName")]
    [AllowPatch]
    [MaxLength(250)]
    public string? FromName { get; set; }

    [BsonElement("ccEmail")]
    [AllowPatch]
    [EmailAddress]
    public string? CcEmail { get; set; }
    [BsonElement("ccName")]
    [AllowPatch]
    [MaxLength(250)]
    public string? CcName { get; set; }
    [BsonElement("bccEmail")]
    [AllowPatch]
    [EmailAddress]
    public string? BccEmail { get; set; }
    [BsonElement("bccName")]
    [AllowPatch]
    [MaxLength(250)]
    public string? BccName { get; set; }

    [BsonElement("subject")]
    [AllowPatch]
    [MaxLength(500)]
    public string Subject { get; set; } = null!;

    [BsonElement("body")]
    [AllowPatch]
    public string BodyHtml { get; set; } = null!;

    [ResponseIgnore]
    [ValidateNever]
    public string Tenant { get; set; } = null!;

    [BsonElement("status")]
    [AllowPatch]
    [DefaultValue(EmailStatusEnum.Pending)]
    [BsonDefaultValue(EmailStatusEnum.Pending)]
    public EmailStatusEnum Status { get; set; } = EmailStatus.Default;

    public string StatusMessage => EmailStatus.GetStatusMessage(Status);

    [AllowPatch]
    public DateTime? SentAt { get; set; } // Timestamp when the SMS was sent

    [BsonElement("retryCount")]
    [ExcludeFromRequest]
    [DefaultValue(0)]
    [BsonDefaultValue(0)]
    public int RetryCount { get; set; } = 0;

    [BsonElement("maxRetryCount")]
    [AllowPatch]
    [DefaultValue(3)]
    [BsonDefaultValue(3)]
    public int MaxRetryCount { get; set; } = 3;

    [BsonElement("errorMessage")]
    [ExcludeFromRequest]
    public string? ErrorMessage { get; set; }

    [BsonElement("createdAt")]
    [ExcludeFromRequest]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastUpdatedAt")]
    [ExcludeFromRequest]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class EmailStatus
{
    public static EmailStatusEnum Default => EmailStatusEnum.Pending;
    public static bool IsValidStatus(EmailStatusEnum status)
    {
        return Enum.IsDefined(typeof(EmailStatusEnum), status);
    }

    public static string GetStatusMessage(EmailStatusEnum status)
    {
        return status switch
        {
            EmailStatusEnum.Pending => "The email is pending sending.",
            EmailStatusEnum.Sent => "The email was successfully sent.",
            EmailStatusEnum.Failed => "The email failed to send.",
            EmailStatusEnum.Retry => "The email is waiting in the retry queue.",
            EmailStatusEnum.Queued => "The email has been successfully added to the queue.",
            EmailStatusEnum.Sending => "The email is being processed by the consumer.",
            _ => "Invalid status."
        };
    }
}