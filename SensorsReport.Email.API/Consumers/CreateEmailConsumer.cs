using MassTransit;
using SensorsReport.Api.Core.MassTransit;
using System.ComponentModel.DataAnnotations;

namespace SensorsReport.Email.API.Consumers;

public class CreateEmailConsumer(IEmailRepository emailRepository, ILogger<CreateEmailConsumer> logger, IEventBus eventBus) : IConsumer<CreateEmailCommand>
{
    private readonly IEmailRepository emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
    private readonly ILogger<CreateEmailConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IEventBus eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

    public async Task Consume(ConsumeContext<CreateEmailCommand> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var emailEvent = context.Message;
        ArgumentNullException.ThrowIfNull(emailEvent, nameof(emailEvent));

        logger.LogInformation("Received CreateEmailEvent: {@EmailEvent}", emailEvent);

        if (string.IsNullOrWhiteSpace(emailEvent.ToEmail) || string.IsNullOrWhiteSpace(emailEvent.Subject) || string.IsNullOrWhiteSpace(emailEvent.BodyHtml))
        {
            logger.LogError("Invalid email event data: {@EmailEvent}", emailEvent);
            throw new ArgumentException("ToEmail, Subject, and BodyHtml cannot be null or empty.");
        }

        if (!string.IsNullOrWhiteSpace(emailEvent.FromEmail) && !new EmailAddressAttribute().IsValid(emailEvent.FromEmail))
        {
            logger.LogError("Invalid FromEmail address: {FromEmail}", emailEvent.FromEmail);
            throw new ArgumentException("Invalid FromEmail address.");
        }

        if (!new EmailAddressAttribute().IsValid(emailEvent.ToEmail))
        {
            logger.LogError("Invalid ToEmail address: {ToEmail}", emailEvent.ToEmail);
            throw new ArgumentException("Invalid ToEmail address.");
        }

        if (string.IsNullOrWhiteSpace(emailEvent.Tenant))
        {
            logger.LogError("Tenant cannot be null or empty.");
            throw new ArgumentException("Tenant cannot be null or empty.");
        }

        var createdEmail = await emailRepository.CreateAsync(new EmailModel
        {
            ToEmail = emailEvent.ToEmail,
            ToName = emailEvent.ToName ?? emailEvent.ToEmail,
            FromEmail = emailEvent.FromEmail,
            FromName = emailEvent.FromName,
            CcEmail = emailEvent.CcEmail,
            CcName = emailEvent.CcName,
            BccEmail = emailEvent.BccEmail,
            BccName = emailEvent.BccName,
            Subject = emailEvent.Subject,
            BodyHtml = emailEvent.BodyHtml,
            Tenant = emailEvent.Tenant,
        });

        await eventBus.PublishAsync(new EmailCreatedEvent
        {
            Id = createdEmail.Id
        });
    }
}
