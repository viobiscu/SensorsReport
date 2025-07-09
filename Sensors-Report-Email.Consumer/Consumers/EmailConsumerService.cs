using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.Json;

namespace Sensors_Report_Email.Consumer;

public class EmailConsumerService : BackgroundService, IDisposable
{
    private readonly AppConfiguration appConfig;
    private readonly MongoClient mongoClient;
    private readonly ILogger<EmailConsumerService> logger;
    private readonly IQueueService queueService;
    private readonly IServiceProvider serviceProvider;
    private readonly IEmailService emailService;

    public EmailConsumerService(ILogger<EmailConsumerService> logger, IOptions<AppConfiguration> appConfig, IQueueService queueService, IEmailService emailService, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(appConfig);
        ArgumentNullException.ThrowIfNull(appConfig.Value.ConnectionString, nameof(appConfig.Value.ConnectionString));
        ArgumentNullException.ThrowIfNull(appConfig.Value.EmailCollectionName, nameof(appConfig.Value.EmailCollectionName));
        ArgumentNullException.ThrowIfNull(appConfig.Value.DatabaseName, nameof(appConfig.Value.DatabaseName));
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQConnectionString, nameof(appConfig.Value.RabbitMQConnectionString));
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQExchange, nameof(appConfig.Value.RabbitMQExchange));
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQQueue, nameof(appConfig.Value.RabbitMQQueue));
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQRoutingKey, nameof(appConfig.Value.RabbitMQRoutingKey));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.appConfig = appConfig.Value ?? throw new ArgumentNullException(nameof(appConfig.Value), "AppConfiguration cannot be null");
        this.mongoClient = new MongoClient(this.appConfig.ConnectionString);
        this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService), "Queue service cannot be null");
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "Service provider cannot be null");
        this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService), "Email service cannot be null");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email Consumer Service is starting.");
        stoppingToken.Register(() => logger.LogInformation("Email Consumer Service is stopping."));

        try
        {
            await queueService.StartConsumingAsync(ProcessMessageAsync, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unhandled exception occurred in EmailConsumerService. The service is stopping.");
            Environment.Exit(1);
        }
    }

    private async Task ProcessMessageAsync(string message, ulong deliveryTag)
    {
        string? emailId = null;
        using var scope = serviceProvider.CreateScope();
        var emailRepository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();

        try
        {
            var messageBody = JsonDocument.Parse(message).RootElement;
            emailId = messageBody.GetProperty("Id").GetString();

            if (string.IsNullOrEmpty(emailId))
            {
                logger.LogWarning("Received message with empty EmailId. Rejecting message permanently. DeliveryTag: {DeliveryTag}", deliveryTag);
                queueService.RejectMessage(deliveryTag, requeue: false);
                return;
            }

            logger.LogInformation("Processing message for EmailId: {EmailId}, DeliveryTag: {DeliveryTag}", emailId, deliveryTag);

            var email = await emailRepository.ClaimForProcessingAsync(emailId);

            if (email == null)
            {
                logger.LogWarning("Could not claim EmailId: {EmailId}. It might be already processed. Acknowledging message.", emailId);
                queueService.AcknowledgeMessage(deliveryTag);
                return;
            }

            await emailService.SendEmailAsync(email);

            await emailRepository.UpdateStatusAsync(email.Id, EmailStatusEnum.Sent);
            logger.LogInformation("Successfully sent email for EmailId: {EmailId}", email.Id);

            queueService.AcknowledgeMessage(deliveryTag);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing EmailId: {EmailId}. Initiating retry/failure logic. DeliveryTag: {DeliveryTag}", emailId, deliveryTag);

            if (string.IsNullOrEmpty(emailId))
            {
                queueService.RejectMessage(deliveryTag, requeue: false);
                return;
            }

            try
            {
                var currentEmail = await emailRepository.GetAsync(emailId);
                if (currentEmail != null && currentEmail.RetryCount < currentEmail.MaxRetryCount)
                {
                    logger.LogWarning("Retrying EmailId: {EmailId}. Current retry count: {RetryCount}", emailId, currentEmail.RetryCount);
                    await emailRepository.UpdateStatusAsync(emailId, EmailStatusEnum.Retry, ex.Message);
                    queueService.RejectMessage(deliveryTag, requeue: false);
                }
                else
                {
                    logger.LogError("All retries failed for EmailId: {EmailId}. Moving to Failed status.", emailId);
                    await emailRepository.UpdateStatusAsync(emailId, EmailStatusEnum.Failed, ex.Message);
                    queueService.RejectMessage(deliveryTag, requeue: false); // when reclaiming, we do not requeue the message to prevent infinite loops
                }
            }
            catch (Exception innerEx)
            {
                logger.LogCritical(innerEx, "A critical error occurred during the error handling for EmailId: {EmailId}. Rejecting message to prevent loss.", emailId);
                // Even if we fail to update the status, we still reject the message to prevent loss
                queueService.RejectMessage(deliveryTag, requeue: false);
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        logger.LogInformation("Email Consumer Service disposed.");
    }
}