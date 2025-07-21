//using Microsoft.Extensions.Options;
//using SensorsReport.OrionLD;
//using System.Text.Json.Nodes;

//namespace SensorsReport.Webhook.API.Tasks;

//public class CheckSubscriptionStatusBackgroundService : BackgroundService
//{
//    private readonly ILogger<CheckSubscriptionStatusBackgroundService> logger;
//    private readonly IServiceScopeFactory serviceScopeFactory;

//    public CheckSubscriptionStatusBackgroundService(ILogger<CheckSubscriptionStatusBackgroundService> logger, IServiceScopeFactory factory)
//    {
//        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
//        this.serviceScopeFactory = factory ?? throw new ArgumentNullException(nameof(factory), "Service scope factory cannot be null");
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        logger.LogInformation("CheckSubscriptionStatusBackgroundService is starting.");

//        stoppingToken.Register(() => logger.LogInformation("CheckSubscriptionStatusBackgroundService is stopping."));

//        while (!stoppingToken.IsCancellationRequested)
//        {
//            logger.LogInformation("Executing CheckSubscriptionStatus task at {Time}", DateTime.UtcNow);

//            try
//            {
//                var scope = serviceScopeFactory.CreateScope();
//                var orionService = scope.ServiceProvider.GetRequiredService<IOrionLdService>();

//                var hasNext = true;
//                do
//                {
//                    var tenants = await orionService.GetEntitiesAsync<List<EntityModel>>(0, 100, "APIKey");
//                    if (tenants == null || tenants.Count <= 0)
//                    {
//                        hasNext = false;
//                        logger.LogInformation("No more tenants found.");
//                        break;
//                    }

//                    foreach (var tenant in tenants)
//                    {
//                        try
//                        {
//                            var tenantName = string.Empty;

//                            if (tenant.Properties.TryGetValue("TenantID", out var value) &&
//                                value.TryGetProperty("value", out var tenantIdNode))
//                                tenantName = tenantIdNode.GetString() ?? string.Empty;

//                            if (string.IsNullOrEmpty(tenantName))
//                            {
//                                logger.LogWarning("TenantID is missing or empty for Tenant {TenantId}", tenant.Id);
//                                continue;
//                            }

//                            orionService.SetTenant(tenantName);

//                            do {
//                                var subscriptionStatus = await orionService.GetSubscriptionsAsync<List<SubscriptionModel>>(0, 100);
//                                if (subscriptionStatus == null)
//                                {
//                                    logger.LogWarning("Subscription status is null for Tenant {TenantId}", tenant.Id);
//                                    continue;
//                                }

//                                foreach (var subscription in subscriptionStatus)
//                                {
//                                    if (!subscription.IsActive)
//                                    {
//                                        var status = statusValueNode.GetString();
//                                        if (status == "active")
//                                        {
//                                            logger.LogInformation("Subscription is active for Tenant {TenantId}", tenant.Id);
//                                        }
//                                        else
//                                        {
//                                            logger.LogWarning("Subscription is inactive for Tenant {TenantId}", tenant.Id);
//                                        }
//                                    }
//                                    else
//                                    {
//                                        logger.LogWarning("Status property is missing for Subscription in Tenant {TenantId}", tenant.Id);
//                                    }
//                                }

//                            } while (false); // Assuming we only need to check once per tenant
//                        }
//                        catch (Exception ex)
//                        {
//                            logger.LogError(ex, "Error retrieving subscription status for Tenant {TenantId}", tenant.Id);
//                        }
//                    }

//                } while (hasNext);

//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "An error occurred while executing CheckSubscriptionStatus task.");
//            }

//            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//        }

//        logger.LogInformation("CheckSubscriptionStatusBackgroundService has stopped.");
//    }
//}
