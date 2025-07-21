using System.ComponentModel;

namespace SensorsReport.Email.API;

public enum EmailStatusEnum
{
    [Description("Pending")]
    Pending, // Write to DB, waiting for queue (critical for reconciliation)
    [Description("Queued")]
    Queued,        // Successfully added to queue
    [Description("Sending")]
    Sending,       // Being processed by consumer
    [Description("Sent")]
    Sent,          // Successfully sent
    [Description("Failed")]
    Failed,        // Sending failed
    [Description("Retrying")]
    Retry,         // Waiting in retry queue
}