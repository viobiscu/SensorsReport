using System.Threading.Tasks;

namespace Sensors_Report_Email.API;

public interface IQueueService
{
    /// <summary>
    /// Sends an email message to the queue.
    /// </summary>
    /// <param name="email">The email model to send.</param>
    /// <returns>True if the email was successfully sent to the queue, otherwise false.</returns>
    Task<bool> QueueEmailAsync(EmailModel email);
}