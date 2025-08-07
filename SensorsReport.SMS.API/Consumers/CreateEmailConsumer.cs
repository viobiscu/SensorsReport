using MassTransit;
using SensorsReport.SMS.API.Models;
using SensorsReport.SMS.API.Repositories;

namespace SensorsReport.Sms.API.Consumers;

public class CreateEmailConsumer(ISmsRepository smsRepository, ILogger<CreateEmailConsumer> logger) : IConsumer<CreateSmsCommand>
{
    private readonly ISmsRepository smsRepository = smsRepository ?? throw new ArgumentNullException(nameof(smsRepository));
    private readonly ILogger<CreateEmailConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task Consume(ConsumeContext<CreateSmsCommand> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var smsEvent = context.Message;
        ArgumentNullException.ThrowIfNull(smsEvent, nameof(smsEvent));

        logger.LogInformation("Received CreateSmsEvent: {@SmsEvent}", smsEvent);

        return smsRepository.CreateAsync(new SmsModel
        {
            PhoneNumber = smsEvent.PhoneNumber,
            Message = smsEvent.Message,
            Timestamp = smsEvent.Timestamp,
            Tenant = smsEvent.Tenant,
            Ttl = smsEvent.Ttl,
            CountryCode = PhoneNumberHelper.GetCountryCode(smsEvent.PhoneNumber),
            Provider = smsEvent.Provider,
            TrackingId = smsEvent.TrackingId,
            MessageType = smsEvent.MessageType,
            CustomData = smsEvent.CustomData,
            RetryCount = smsEvent.RetryCount,
        }, smsEvent.Tenant);
    }
}
