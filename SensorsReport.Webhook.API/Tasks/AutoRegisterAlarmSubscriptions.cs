using SensorsReport.OrionLD;

namespace SensorsReport.Webhook.API.Tasks;

public class AutoRegisterAlarmSubscriptions : BackgroundService
{
    private readonly ILogger<AutoRegisterAlarmSubscriptions> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public AutoRegisterAlarmSubscriptions(ILogger<AutoRegisterAlarmSubscriptions> logger, IServiceScopeFactory factory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.serviceScopeFactory = factory ?? throw new ArgumentNullException(nameof(factory), "Service scope factory cannot be null");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Executing AutoRegisterAlarmSubscriptions task at {Time}", DateTime.UtcNow);

        try
        {
            var scope = serviceScopeFactory.CreateScope();
            var orionService = scope.ServiceProvider.GetRequiredService<IOrionLdService>();

            var hasNext = true;
            var offset = 0;
            do
            {
                var tenants = await orionService.GetEntitiesAsync<List<EntityModel>>(offset, 100, "APIKey");
                if (tenants == null || tenants.Count <= 0)
                {
                    hasNext = false;
                    logger.LogInformation("No more tenants found.");
                    break;
                }

                foreach (var tenant in tenants)
                {
                    try
                    {
                        var tenantName = string.Empty;

                        if (tenant.Properties.TryGetValue("TenantID", out var value) &&
                            value.TryGetProperty("value", out var tenantIdNode))
                            tenantName = tenantIdNode.GetString() ?? string.Empty;

                        if (string.IsNullOrEmpty(tenantName))
                        {
                            logger.LogWarning("TenantID is missing or empty for Tenant {TenantId}", tenant.Id);
                            continue;
                        }

                        orionService.SetTenant(tenantName);

                        var subscriptionStatus = await orionService.GetSubscriptionByIdAsync<SubscriptionModel>("urn:ngsi-ld:Subscription:Alarm");
                        if (subscriptionStatus == null)
                        {
                            await orionService.CreateSubscriptionAsync(new SubscriptionModel
                            {
                                Id = "urn:ngsi-ld:Subscription:Alarm",
                                Type = "Subscription",
                                Entities =
                                [
                                    new() {
                                    Type = "Alarm"
                                }
                                ],
                                Notification = new SubscriptionNotificationModel
                                {
                                    Endpoint = new SubscriptionEndpointModel
                                    {
                                        Uri = "http://sensors-report-webhook-api/api/webhook",
                                        Accept = "application/json"
                                    }
                                },
                                Status = "active",
                                IsActive = true,
                            });
                        }
                        else
                        {
                            await orionService.UpdateSubscriptionAsync("urn:ngsi-ld:Subscription:Alarm",
                                new SubscriptionModel
                                {
                                    Id = "urn:ngsi-ld:Subscription:Alarm",
                                    Type = "Subscription",
                                    Entities =
                                    [
                                        new() {
                                            Type = "Alarm"
                                        }
                                    ],
                                    Notification = new SubscriptionNotificationModel
                                    {
                                        Endpoint = new SubscriptionEndpointModel
                                        {
                                            Uri = "http://sensors-report-webhook-api/api/webhook",
                                            Accept = "application/json"
                                        }
                                    },
                                    Status = "active",
                                    IsActive = true,
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while processing tenant {TenantId}.", tenant.Id);
                    }
                }

                offset += 100;
            } while (hasNext);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing CheckSubscriptionStatus task.");
        }


        logger.LogInformation("AutoRegisterAlarmSubscriptions has stopped.");
    }
}
