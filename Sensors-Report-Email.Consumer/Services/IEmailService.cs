
namespace Sensors_Report_Email.Consumer
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailModel emailModel);
    }
}