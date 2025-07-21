using System.ComponentModel;

namespace SensorsReport.SMS.API.Models;

public enum SmsStatusEnum
{
    [Description("Pending")]
    Pending,
    [Description("Entrusted")]
    Entrusted,
    [Description("Sent")]
    Sent,
    [Description("Failed")]
    Failed,
    [Description("Expired")]
    Expired,
    [Description("Error")]
    Error,
    [Description("Unknown")]
    Unknown
}
