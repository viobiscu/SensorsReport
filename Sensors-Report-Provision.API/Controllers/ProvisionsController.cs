using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sensors_Report_Provision.API.Services;
using Sensors_Report_Provision.API.Models;

namespace Sensors_Report_Provision.API.Controllers;

public class ProvisionRequest
{
    public string TargetTenant { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class ProvisionsController(
    IServiceProvider serviceProvider,
    ILogger<ProvisionsController> logger,
    IOptions<AppConfig> config) : ControllerBase
{
    private readonly IServiceProvider _serviceProvider = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<ProvisionsController> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));
    private readonly AppConfig _config = config?.Value
        ?? throw new ArgumentNullException(nameof(config));

    [HttpPost]
    public async Task<IActionResult> StartProvisioningAsync(ProvisionRequest request)
    {
        if (string.IsNullOrEmpty(request?.TargetTenant))
        {
            _logger.LogError("Target tenant is not specified.");
            return BadRequest("Target tenant must be specified.");
        }

        _logger.LogInformation("Starting provisioning process...");
        _logger.LogInformation($"Target tenant: {request.TargetTenant}");

        var defaultBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        var targetBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        targetBroker.SetTenant(request.TargetTenant);

        var offset = 0;
        var limit = 100;
        var entities = await defaultBroker.GetEntitiesAsync<List<EntityModel>>(offset, limit);

        if (entities == null || entities.Count == 0)
        {
            _logger.LogInformation("No entities found to process.");
            return Ok("No entities found to process.");
        }

        do
        {
            foreach (var entity in entities)
            {
                _logger.LogInformation($"Processing entity: {entity?.Id}");
                if (entity == null || string.IsNullOrEmpty(entity.Id))
                {
                    _logger.LogWarning("Skipping entity with null or empty ID.");
                    continue;
                }
                
                var existingEntity = await targetBroker.GetEntityByIdAsync(entity.Id);
                if (existingEntity.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Entity {entity.Id} already exists in target tenant. Skipping creation.");
                    continue;
                }

                await targetBroker.CreateEntityAsync(entity);
            }

            offset += limit;
            entities = await defaultBroker.GetEntitiesAsync<List<EntityModel>>(offset, limit);

        }
        while (entities?.Count > 0);

        offset = 0;
        var subscriptions = await defaultBroker.GetSubscriptionsAsync<List<SubscriptionModel>>(offset, limit);

        if (subscriptions == null || subscriptions.Count == 0)
        {
            _logger.LogInformation("No subscriptions found to process.");
            return Ok("No subscriptions found to process.");
        }

        do
        {
            foreach (var subscription in subscriptions)
            {
                _logger.LogInformation($"Processing subscription: {subscription.Id}");
                if (subscription == null || string.IsNullOrEmpty(subscription.Id))
                {
                    _logger.LogWarning("Skipping subscription with null or empty ID.");
                    continue;
                }

                var existingSubscription = await targetBroker.GetSubscriptionByIdAsync(subscription.Id);
                if (existingSubscription.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Subscription {subscription.Id} already exists in target tenant. Skipping creation.");
                    continue;
                }
                
                await targetBroker.CreateSubscriptionAsync(subscription);
            }

            offset += limit;
            subscriptions = await defaultBroker.GetSubscriptionsAsync<List<SubscriptionModel>>(offset, limit);
        }
        while (subscriptions?.Count > 0);


        _logger.LogInformation("Provisioning process completed successfully.");

        return Ok();
    }
}
