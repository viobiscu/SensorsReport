using System.Threading.Tasks;

namespace SensorsReport.Email.API;

public interface IEmailQueueService
{
    Task<bool> QueueEmailAsync(EmailModel email);
}