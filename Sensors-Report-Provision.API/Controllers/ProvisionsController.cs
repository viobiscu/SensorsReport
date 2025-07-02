using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sensors_Report_Provision.API.Services;
using Sensors_Report_Provision.API.Models;
using System.Text.Json;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartProvisioningAsync(ProvisionRequest request)
    {
        if (string.IsNullOrEmpty(request?.TargetTenant))
        {
            _logger.LogError("Target tenant is not specified.");
            return BadRequest("Target tenant must be specified.");
        }

        _logger.LogInformation("Starting provisioning process...");
        _logger.LogInformation($"Target tenant: {request.TargetTenant}");

        var sourceBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        var targetBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        targetBroker.SetTenant(request.TargetTenant);

        var offset = 0;
        var limit = 100;
        var entities = await sourceBroker.GetEntitiesAsync<List<EntityModel>>(offset, limit);

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
            entities = await sourceBroker.GetEntitiesAsync<List<EntityModel>>(offset, limit);

        }
        while (entities?.Count > 0);

        offset = 0;
        var subscriptions = await sourceBroker.GetSubscriptionsAsync<List<SubscriptionModel>>(offset, limit);

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
            subscriptions = await sourceBroker.GetSubscriptionsAsync<List<SubscriptionModel>>(offset, limit);
        }
        while (subscriptions?.Count > 0);


        _logger.LogInformation("Provisioning process completed successfully.");

        return Ok();
    }

    [HttpPost("/entity/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProvisionEntityById(string entityId, ProvisionRequest request)
    {
        if (string.IsNullOrEmpty(request?.TargetTenant))
        {
            _logger.LogError("Target tenant is not specified.");
            return BadRequest("Target tenant must be specified.");
        }

        _logger.LogInformation($"Provisioning entity {entityId} for tenant {request.TargetTenant}");

        var sourceBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        var targetBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        targetBroker.SetTenant(request.TargetTenant);

        var entity = await sourceBroker.GetEntityByIdAsync(entityId);
        if (!entity.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to retrieve entity {entityId} from default broker.");
            return NotFound($"Entity {entityId} not found in default broker.");
        }

        var existingEntity = await targetBroker.GetEntityByIdAsync(entityId);
        if (existingEntity.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Entity {entityId} already exists in target tenant. Updating relations.");
            await CopyRelatedEntities(sourceBroker, targetBroker, entityId, request.TargetTenant);
            _logger.LogInformation($"Entity {entityId} updated in target tenant {request.TargetTenant}.");
            return Ok($"Entity {entityId} already exists in target tenant.");
        }

        await CopyRelatedEntities(sourceBroker, targetBroker, entityId, request.TargetTenant);
        await targetBroker.CreateEntityAsync(entity);
        _logger.LogInformation($"Successfully provisioned entity {entityId} to tenant {request.TargetTenant}");

        return Ok();
    }

    [HttpPost("/subscription/{subscriptionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProvisionSubscriptionById(string subscriptionId, ProvisionRequest request)
    {
        if (string.IsNullOrEmpty(request?.TargetTenant))
        {
            _logger.LogError("Target tenant is not specified.");
            return BadRequest("Target tenant must be specified.");
        }

        _logger.LogInformation($"Provisioning subscription {subscriptionId} for tenant {request.TargetTenant}");

        var sourceBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        var targetBroker = _serviceProvider.GetRequiredService<IOrionContextBrokerService>();
        targetBroker.SetTenant(request.TargetTenant);

        var subscriptionResponse = await sourceBroker.GetSubscriptionByIdAsync(subscriptionId);
        if (!subscriptionResponse.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to retrieve subscription {subscriptionId} from default broker.", subscriptionResponse.GetContentAsAsync<object>());
            return NotFound($"Subscription {subscriptionId} not found in default broker.");
        }

        var existingSubscriptionResponse = await targetBroker.GetSubscriptionByIdAsync(subscriptionId);
        if (existingSubscriptionResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Subscription {subscriptionId} already exists in target tenant. Skipping creation.");
            return Ok($"Subscription {subscriptionId} already exists in target tenant.");
        }

        var subscription = await subscriptionResponse.GetContentAsAsync<SubscriptionModel>();
        if (subscription == null)
        {
            _logger.LogError($"Failed to deserialize subscription {subscriptionId}.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to deserialize subscription.");
        }

        await targetBroker.CreateSubscriptionAsync(subscription);
        _logger.LogInformation($"Successfully provisioned subscription {subscriptionId} to tenant {request.TargetTenant}");

        return Ok();
    }

    private async Task CopyRelatedEntities(IOrionContextBrokerService sourceBroker, IOrionContextBrokerService targetBroker, string entityId, string targetTenant)
    {
        _logger.LogInformation($"Copying related entities for {entityId} to {targetTenant}");

        var entity = await sourceBroker.GetEntityByIdAsync<EntityModel>(entityId);

        if (entity == null)
        {
            _logger.LogError($"Entity {entityId} not found in source broker.");
            return;
        }

        foreach (var entityProperty in entity.Properties)
            await ProcessPropertyRecursively(sourceBroker, targetBroker, entityProperty.Value, targetTenant);
    }

    private async Task ProcessPropertyRecursively(IOrionContextBrokerService sourceBroker, IOrionContextBrokerService targetBroker, JsonElement propertyValue, string targetTenant)
    {
        if (propertyValue.ValueKind == JsonValueKind.Object)
        {
            var propertyModel = JsonSerializer.Deserialize<PropertyModelBase>(propertyValue.GetRawText());
            if (propertyModel?.Type?.Equals(PropertyModelBase.PropertyType.Relationship, StringComparison.OrdinalIgnoreCase) == true)
            {
                await ProcessRelationship(sourceBroker, targetBroker, propertyValue, targetTenant);
            }
            else
            {
                foreach (var nestedProperty in propertyValue.EnumerateObject())
                {
                    await ProcessPropertyRecursively(sourceBroker, targetBroker, nestedProperty.Value, targetTenant);
                }
            }
        }
        else if (propertyValue.ValueKind == JsonValueKind.Array)
        {
            foreach (var arrayElement in propertyValue.EnumerateArray())
            {
                await ProcessPropertyRecursively(sourceBroker, targetBroker, arrayElement, targetTenant);
            }
        }
    }

    private async Task ProcessRelationship(IOrionContextBrokerService sourceBroker, IOrionContextBrokerService targetBroker, JsonElement relationshipValue, string targetTenant)
    {
        var relationship = JsonSerializer.Deserialize<RelationshipModel>(relationshipValue.GetRawText());
        if (relationship == null || relationship.Object == null || relationship.Object.Count == 0)
        {
            _logger.LogWarning($"Relationship has no objects to copy.");
            return;
        }

        foreach (var relatedEntityId in relationship.Object)
        {
            _logger.LogInformation($"Copying related entity {relatedEntityId}");
            var relatedEntity = await sourceBroker.GetEntityByIdAsync<EntityModel>(relatedEntityId);
            if (relatedEntity == null)
            {
                _logger.LogWarning($"Related entity {relatedEntityId} not found in source broker.");
                continue;
            }

            var existingRelatedEntity = await targetBroker.GetEntityByIdAsync(relatedEntityId);
            if (existingRelatedEntity.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Related entity {relatedEntityId} already exists in target tenant. Skipping creation.");
                continue;
            }

            _logger.LogInformation($"Creating related entity {relatedEntityId} in target tenant {targetTenant}");
            await CopyRelatedEntities(sourceBroker, targetBroker, relatedEntityId, targetTenant);
            await targetBroker.CreateEntityAsync(relatedEntity);
            _logger.LogInformation($"Successfully created related entity {relatedEntityId} in target tenant {targetTenant}");
        }
    }
}