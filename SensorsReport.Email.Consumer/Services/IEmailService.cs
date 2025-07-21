
namespace SensorsReport.Email.Consumer;

public interface IEmailService
{
    Task SendEmailAsync(EmailModel emailModel);
}