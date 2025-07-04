using Microsoft.Extensions.Options;
using Sensors_Report_SMS.API.Models;
using Sensors_Report_SMS.API.Repositories;

namespace Sensors_Report_SMS.API.Tasks;

public class UpdateSmsStatusBackgroundService : BackgroundService
{
    private readonly ILogger<UpdateSmsStatusBackgroundService> logger;
    private readonly ISmsRepository smsRepository;
    private readonly IProviderRepository providerRepository;
    private readonly IOptions<AppConfiguration> appConfig;

    public UpdateSmsStatusBackgroundService(ILogger<UpdateSmsStatusBackgroundService> logger, IServiceScopeFactory factory, IOptions<AppConfiguration> appConfig)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        var scope = factory.CreateScope();
        this.smsRepository = scope.ServiceProvider.GetRequiredService<ISmsRepository>();
        this.providerRepository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        this.appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig), "AppConfiguration cannot be null");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("UpdateSmsStatusBackgroundService is starting.");

        stoppingToken.Register(() => logger.LogInformation("UpdateSmsStatusBackgroundService is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Executing SMS status update task at {Time}", DateTime.UtcNow);

            try
            {
                var entrustedSms = await smsRepository.GetAsync(null, null, null, 10, 0, SmsStatusEnum.Entrusted);
                if (entrustedSms == null || entrustedSms.Count == 0)
                {
                    logger.LogInformation("No SMS records found with status 'Entrusted' to update.");
                }
                else
                {
                    foreach (var sms in entrustedSms)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            logger.LogInformation("Cancellation requested, stopping SMS status update task.");
                            return;
                        }

                        logger.LogInformation("Processing SMS with ID {SmsId} and status '{Status}'", sms.Id, Enum.GetName(sms.Status));
                        var currentStatus = sms.Status;
                        if (sms.SentAt <= DateTime.UtcNow.AddSeconds(-appConfig.Value.ProviderTrustTimeoutInSecond))
                        {
                            logger.LogInformation("Updating SMS with ID {SmsId} to 'Pending' status due to timeout.", sms.Id);
                            sms.Status = SmsStatusEnum.Pending;
                            sms.RetryCount++;
                            logger.LogInformation("SMS with ID {SmsId} has been retried {RetryCount} times.", sms.Id, sms.RetryCount);
                            if (sms.Provider != null)
                            {
                                logger.LogInformation("Updating provider status for SMS with ID {SmsId} to 'Unavailable'.", sms.Id);
                                await providerRepository.SetProviderStatusAsync(sms.Provider, ProviderStatusEnum.Unavailable);
                            }
                        }

                        if (sms.RetryCount >= appConfig.Value.MaxRetryCount)
                        {
                            logger.LogInformation("Updating SMS with ID {SmsId} to 'Failed' status due to max retry count.", sms.Id);
                            sms.Status = SmsStatusEnum.Failed;
                        }

                        if (sms.Timestamp.Add(TimeSpan.FromMinutes(sms.Ttl ?? appConfig.Value.DefaultTtlInMinutes)) < DateTime.UtcNow)
                        {
                            logger.LogInformation("Updating SMS with ID {SmsId} to 'Expired' status due to TTL expiration.", sms.Id);
                            sms.Status = SmsStatusEnum.Expired;
                        }

                        if (sms.Status == currentStatus)
                        {
                            logger.LogInformation("No status change for SMS with ID {SmsId}. Current status is '{Status}'.", sms.Id, Enum.GetName(sms.Status));
                            continue;
                        }

                        logger.LogInformation("Updating SMS with ID {SmsId} to '{status}' status.", sms.Id, Enum.GetName(sms.Status));
                        await smsRepository.UpdateAsync(sms.Id!, sms);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the SMS status update task");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        logger.LogInformation("UpdateSmsStatusBackgroundService has stopped.");
    }
}
