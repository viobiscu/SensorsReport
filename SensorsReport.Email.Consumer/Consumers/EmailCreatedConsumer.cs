using MassTransit;
using SensorsReport.Email.Consumer;

namespace SensorsReport.Email.API.Consumers;

public class EmailCreatedConsumer(IEmailRepository emailRepository, IEmailService emailService, ILogger<EmailCreatedConsumer> logger) : IConsumer<EmailCreatedEvent>
{
    private readonly IEmailRepository emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
    private readonly IEmailService emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly ILogger<EmailCreatedConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Consume(ConsumeContext<EmailCreatedEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var emailCreatedEvent = context.Message;
        ArgumentNullException.ThrowIfNull(emailCreatedEvent, nameof(emailCreatedEvent));
        ArgumentNullException.ThrowIfNull(emailCreatedEvent.Id, nameof(emailCreatedEvent.Id));

        var email = await emailRepository.ClaimForProcessingAsync(emailCreatedEvent.Id);

        if (email == null)
        {
            logger.LogWarning("Could not claim EmailId: {EmailId}. It might be already processed. Acknowledging message.", emailCreatedEvent.Id);
            return;
        }

        logger.LogInformation("Received EmailCreatedEvent: {@EmailEvent}", emailCreatedEvent);

        await emailRepository.UpdateStatusAsync(email.Id, EmailStatusEnum.Sending);

        logger.LogInformation("Processing message for EmailId: {EmailId}", emailCreatedEvent.Id);

        try
        {
            await emailService.SendEmailAsync(email);
        }
        catch (Exception ex)
        {
            if (context.GetRetryCount() < email.MaxRetryCount)
            {
                logger.LogWarning(ex, "Error sending email for EmailId: {EmailId}. Retrying...", email.Id);
                await emailRepository.UpdateStatusAsync(email.Id, EmailStatusEnum.Retry, ex.Message);
                throw;
            }

            logger.LogError(ex, "Error sending email for EmailId: {EmailId}. Failed... Error: {ErrorMessage}", email.Id, ex.Message);
            await emailRepository.UpdateStatusAsync(email.Id, EmailStatusEnum.Failed, ex.Message);
            return;
        }

        await emailRepository.UpdateStatusAsync(email.Id, EmailStatusEnum.Sent);
        logger.LogInformation("Successfully sent email for EmailId: {EmailId}", email.Id);
    }
}
