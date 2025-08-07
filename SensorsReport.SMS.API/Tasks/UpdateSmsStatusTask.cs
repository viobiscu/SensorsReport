using Microsoft.Extensions.Options;
using SensorsReport.SMS.API.Models;
using SensorsReport.SMS.API.Repositories;

namespace SensorsReport.SMS.API.Tasks;

public class UpdateSmsStatusBackgroundService : BackgroundService
{
    private readonly ILogger<UpdateSmsStatusBackgroundService> logger;
    private readonly ISmsRepository smsRepository;
    private readonly IProviderRepository providerRepository;
    private readonly IOptions<SmsOptions> smsOptions;

    public UpdateSmsStatusBackgroundService(ILogger<UpdateSmsStatusBackgroundService> logger, IServiceScopeFactory factory, IOptions<SmsOptions> smsOptions)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        var scope = factory.CreateScope();
        this.smsRepository = scope.ServiceProvider.GetRequiredService<ISmsRepository>();
        this.providerRepository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        this.smsOptions = smsOptions ?? throw new ArgumentNullException(nameof(smsOptions), "SmsOptions cannot be null");
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

                        logger.LogInformation("Processing SMS with ID {SmsId}, status '{Status}' and tenant '{Tenant}'", sms.Id, Enum.GetName(sms.Status), sms.Tenant);
                        var currentStatus = sms.Status;
                        if (sms.SentAt <= DateTime.UtcNow.AddSeconds(-smsOptions.Value.ProviderTrustTimeoutInSecond))
                        {
                            logger.LogInformation("Updating SMS with ID {SmsId} to 'Pending' status due to timeout. SentAt: {SentAt}, Provider Trust Timeout: {ProviderTrustTimeout} second", sms.Id, sms.SentAt, smsOptions.Value.ProviderTrustTimeoutInSecond);
                            sms.Status = SmsStatusEnum.Pending;
                            sms.RetryCount++;
                            logger.LogInformation("SMS with ID {SmsId} has been retried {RetryCount} times.", sms.Id, sms.RetryCount);
                            if (sms.Provider != null)
                            {
                                logger.LogInformation("Updating provider status for SMS with ID {SmsId} to 'Unavailable'.", sms.Id);
                                await providerRepository.SetProviderStatusAsync(sms.Provider, ProviderStatusEnum.Unavailable);
                            }
                            else
                            {
                                logger.LogWarning("SMS with ID {SmsId} with tenant {Tenant} has no provider associated, skipping provider status update.", sms.Id, sms.Tenant);
                            }
                        }

                        if (sms.RetryCount >= smsOptions.Value.MaxRetryCount)
                        {
                            logger.LogInformation("Updating SMS with ID {SmsId} to 'Failed' status due to max retry count (MaxRetry: {MaxRetry}).", sms.Id, smsOptions.Value.MaxRetryCount);
                            sms.Status = SmsStatusEnum.Failed;
                        }

                        var actualTtl = TimeSpan.FromMinutes(sms.Ttl ?? smsOptions.Value.DefaultTtlInMinutes);
                        if (sms.Timestamp.Add(actualTtl) < DateTime.UtcNow)
                        {
                            logger.LogInformation("Updating SMS with ID {SmsId} to 'Expired' status due to TTL expiration (TTL: {TTL}).", sms.Id, actualTtl);
                            sms.Status = SmsStatusEnum.Expired;
                        }

                        if (sms.Status == currentStatus)
                        {
                            logger.LogInformation("No status change for SMS with ID {SmsId}. Current status is '{Status}'.", sms.Id, Enum.GetName(sms.Status));
                            continue;
                        }

                        logger.LogInformation("Updating SMS with ID {SmsId} to '{status}' status.", sms.Id, Enum.GetName(sms.Status));
                        await smsRepository.UpdateAsync(sms.Id!, sms, sms.Tenant);
                    }
                }

                var pendingSms = await smsRepository.GetAsync(null, null, null, 10, 0, SmsStatusEnum.Pending);

                if (pendingSms == null || pendingSms.Count == 0)
                {
                    logger.LogInformation("No SMS records found with status 'Pending' to update.");
                }
                else
                {
                    foreach (var sms in pendingSms)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            logger.LogInformation("Cancellation requested, stopping SMS status update task.");
                            return;
                        }

                        logger.LogInformation("Processing SMS with ID {SmsId}, status '{Status}' and tenant '{Tenant}'", sms.Id, Enum.GetName(sms.Status), sms.Tenant);

                        var currentStatus = sms.Status;

                        var actualTtl = TimeSpan.FromMinutes(sms.Ttl ?? smsOptions.Value.DefaultTtlInMinutes);
                        if (sms.Timestamp <= DateTime.UtcNow.Add(-actualTtl))
                        {
                            logger.LogInformation("Updating SMS with ID {SmsId} to 'Expired' status due to TTL expiration (TTL: {TTL}).", sms.Id, actualTtl);
                            sms.Status = SmsStatusEnum.Expired;
                        }

                        if (sms.Status == currentStatus)
                        {
                            logger.LogInformation("No status change for SMS with ID {SmsId}. Current status is '{Status}'.", sms.Id, Enum.GetName(sms.Status));
                            continue;
                        }

                        logger.LogInformation("Updating SMS with ID {SmsId} to '{status}' status.", sms.Id, Enum.GetName(sms.Status));
                        await smsRepository.UpdateAsync(sms.Id!, sms, sms.Tenant);
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
