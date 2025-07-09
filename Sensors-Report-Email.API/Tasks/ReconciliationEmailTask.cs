using Microsoft.Extensions.Options;

namespace Sensors_Report_Email.API.Tasks;

public class ReconciliationEmailTask : BackgroundService
{
    private readonly ILogger<ReconciliationEmailTask> logger;
    private readonly IEmailRepository emailRepository;
    private readonly IQueueService queueService;
    private readonly IOptions<AppConfiguration> appConfig;

    public ReconciliationEmailTask(ILogger<ReconciliationEmailTask> logger,
        IServiceScopeFactory factory,
        IOptions<AppConfiguration> appConfig)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        var scope = factory.CreateScope();
        this.emailRepository = scope.ServiceProvider.GetRequiredService<IEmailRepository>()
            ?? throw new ArgumentNullException(nameof(emailRepository), "EmailRepository cannot be null");
        this.queueService = scope.ServiceProvider.GetRequiredService<IQueueService>()
            ?? throw new ArgumentNullException(nameof(queueService), "QueueService cannot be null");

        this.appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig), "AppConfiguration cannot be null");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ReconciliationEmailTask started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var emails = await emailRepository.GetReconciliationEmails();
                if (emails.Count > 0)
                {
                    foreach (var email in emails)
                    {
                        await queueService.QueueEmailAsync(email);
                        logger.LogInformation("Queued email with ID: {Id} for processing.", email.Id);
                        await emailRepository.UpdateStatusAsync(email.Id, EmailStatusEnum.Queued);
                        logger.LogInformation("Updated status of email with ID: {Id} to Queued.", email.Id);
                    }
                }
                else
                {
                    logger.LogInformation("No pending emails found for reconciliation.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing emails for reconciliation.");
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Adjust the delay as needed
        }
        logger.LogInformation("ReconciliationEmailTask stopped.");
    }
}
