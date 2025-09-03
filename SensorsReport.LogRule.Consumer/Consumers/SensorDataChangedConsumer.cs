using MassTransit;
using SensorsReport.Api.Core.MassTransit;
using SensorsReport.OrionLD;
using System;
using System.Text.Json;

namespace SensorsReport.LogRule.Consumer.Consumers;

public class SensorDataChangedConsumer(ILogger<SensorDataChangedConsumer> logger, JsonSerializerOptions jsonSerializerOptions, IServiceProvider serviceProvider, IEventBus eventBus) : IConsumer<SensorDataChangedEvent>
{
    private readonly ILogger<SensorDataChangedConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IEventBus eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly JsonSerializerOptions JsonSerializerOptions = jsonSerializerOptions;

    public async Task Consume(ConsumeContext<SensorDataChangedEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var sensorDataChangedEvent = context.Message;

        if (sensorDataChangedEvent is null)
        {
            logger.LogError("Received null SensorDataChangedEvent");
            return;
        }

        if (!IsValid(sensorDataChangedEvent))
        {
            logger.LogError("SensorDataChangedEvent validation failed");
            return;
        }

        var scope = serviceProvider.CreateScope();
        var orionLdService = scope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(sensorDataChangedEvent.Tenant!.Tenant);
        var sensor = await orionLdService.GetEntityByIdAsync<EntityModel>(sensorDataChangedEvent.Item!.Id!);

        if (sensor is null)
        {
            logger.LogError("Sensor with ID {SensorId} not found", sensorDataChangedEvent.Item.Id);
            return;
        }

        var subscription = await orionLdService.GetSubscriptionByIdAsync<SubscriptionModel>(sensorDataChangedEvent.SubscriptionId!);

        if (subscription is null)
        {
            logger.LogError("Subscription with ID {SubscriptionId} not found", sensorDataChangedEvent.SubscriptionId);
            return;
        }

        logger.LogInformation("Processing SensorDataChangedEvent for sensor {SensorId} with subscription {SubscriptionId}",
            sensorDataChangedEvent.Item.Id, sensorDataChangedEvent.SubscriptionId);

        var properties = sensor.Properties!.GetProcessableProperties();

        if (subscription.WatchedAttributes is not null)
            properties = properties.Where(s => subscription.WatchedAttributes.Contains(s.property));

        foreach (var (propertyKey, metadataKey) in properties)
        {
            var sensorMetadataPropertyJson = sensor.Properties!.TryGetValue(metadataKey, out var metadataValue) ? metadataValue : default;
            if (sensorMetadataPropertyJson.ValueKind != JsonValueKind.Object)
            {
                logger.LogWarning("Metadata for attribute {Attribute} is not an object", propertyKey);
                continue;
            }

            var sensorPropertyJson = sensor.Properties.TryGetValue(propertyKey, out var propertyValue) ? propertyValue : default;
            if (sensorPropertyJson.ValueKind != JsonValueKind.Object)
            {
                logger.LogWarning("Property {Property} is not an object", propertyKey);
                continue;
            }

            var sensorPropertyMetadata = sensorMetadataPropertyJson.Deserialize<MetaPropertyModel>(JsonSerializerOptions);
            if (sensorPropertyMetadata?.LogRule is null)
            {
                logger.LogWarning("LogRule metadata for property {Property} is null", propertyKey);
                continue;
            }

            var sensorProperty = sensorPropertyJson.Deserialize<EntityPropertyModel>(JsonSerializerOptions);
            if (sensorProperty is null)
            {
                logger.LogWarning("Property {Property} deserialization failed", propertyKey);
                continue;
            }

            var triggerAlarmRuleEvent = new TriggerAlarmRuleEvent
            {
                Tenant = sensorDataChangedEvent.Tenant,
                SensorId = sensor.Id,
                PropertyKey = propertyKey,
                MetadataKey = metadataKey
            };

            var logRule = (await orionLdService.GetEntityByIdAsync<LogRuleModel>(sensorPropertyMetadata.LogRule?.Object?.FirstOrDefault()!));

            if (logRule is null)
            {
                logger.LogWarning("LogRule with ID {LogRuleId} not found", sensorPropertyMetadata.LogRule?.Object?.FirstOrDefault());
                await PublishMessage(triggerAlarmRuleEvent, sensorProperty.Value, sensorProperty.Unit?.Value);
                continue;
            }

            if (logRule.Enabled?.Value == false)
            {
                logger.LogInformation("LogRule {LogRuleId} is disabled, skipping processing", logRule.Id);
                await ResetConsecutiveHit(sensor.Id!, propertyKey, metadataKey);
                await PublishMessage(triggerAlarmRuleEvent, sensorProperty.Value, sensorProperty.Unit?.Value);
                continue;
            }

            var isAboveLow = logRule.Low?.Value is null || sensorProperty.Value > logRule.Low.Value;
            var isBelowHigh = logRule.High?.Value is null || sensorProperty.Value < logRule.High.Value;
            var isInRange = isAboveLow && isBelowHigh;

            if (isInRange)
            {
                logger.LogInformation("Sensor property {Property} value {Value} is within the defined thresholds", propertyKey, sensorProperty.Value);
                await ResetConsecutiveHit(sensor.Id!, propertyKey, metadataKey);
                await PublishMessage(triggerAlarmRuleEvent, sensorProperty.Value, sensorProperty.Unit?.Value);
                continue;
            }

            if (!isAboveLow)
                logger.LogWarning("Sensor property {Property} value {Value} is below the low threshold {LowThreshold}", propertyKey, sensorProperty.Value, logRule.Low?.Value);

            if (!isBelowHigh)
                logger.LogWarning("Sensor property {Property} value {Value} is above the high threshold {HighThreshold}", propertyKey, sensorProperty.Value, logRule.High?.Value);

            var consecutiveHit = (sensorPropertyMetadata?.LogRuleConsecutiveHit?.Value ?? 0) + 1;
            var isFaulty = consecutiveHit >= logRule.ConsecutiveHit?.Value;
            await SetConsecutiveHit(sensor.Id!, propertyKey, metadataKey, consecutiveHit, isFaulty);

            if (!isFaulty)
            {
                logger.LogInformation("Sensor property {Property} value {Value} has not yet reached the consecutive violations threshold", propertyKey, sensorProperty.Value);
                continue;
            }

            logger.LogWarning("Sensor property {Property} value {Value} has exceeded the consecutive violations threshold {ConsecutiveHits}", propertyKey, sensorProperty.Value, consecutiveHit);
        }

        async Task PublishMessage(TriggerAlarmRuleEvent publishEvent, double? value, string? unit)
        {
            await eventBus.PublishAsync(publishEvent);
            await eventBus.PublishAsync(new SensorDataHistoryLogEvent
            {
                Tenant = publishEvent.Tenant,
                SensorId = publishEvent.SensorId,
                PropertyKey = publishEvent.PropertyKey,
                MetadataKey = publishEvent.MetadataKey,
                ObservedAt = DateTimeOffset.UtcNow,
                Value = value,
                Unit = unit
            });
        }

        async Task SetConsecutiveHit(string entityId, string propKey, string metaPropKey, int consecutiveHit, bool isFaulty)
        {
            var consecutiveHitUpdatePatch = new Dictionary<string, Dictionary<string, object>>
            {
                [metaPropKey] = new Dictionary<string, object>
                {
                    ["logRuleConsecutiveHit"] = new ValuePropertyModel<int>
                    {
                        Value = consecutiveHit
                    }
                }
            };

            var statusText = isFaulty ? EntityPropertyModel.StatusValues.Faulty : EntityPropertyModel.StatusValues.Operational;
            consecutiveHitUpdatePatch[metaPropKey].Add(
                "status", new ObservedValuePropertyModel<string>
                {
                    Value = statusText,
                    ObservedAt = DateTimeOffset.UtcNow
                }
            );

            await orionLdService.UpdateEntityAsync(entityId, consecutiveHitUpdatePatch);
        }

        async Task ResetConsecutiveHit(string entityId, string propKey, string metaPropKey) =>
            await SetConsecutiveHit(entityId, propKey, metaPropKey, 0, false);
    }


    public bool IsValid(SensorDataChangedEvent sensorDataChangedEvent)
    {
        if (sensorDataChangedEvent is null)
        {
            logger.LogError("Validation failed: sensorDataChangedEvent is null");
            return false;
        }

        if (string.IsNullOrWhiteSpace(sensorDataChangedEvent.Tenant?.Tenant))
        {
            logger.LogError("Validation failed: sensorDataChangedEvent.Tenant?.Tenant is null or whitespace");
            return false;
        }

        if (string.IsNullOrWhiteSpace(sensorDataChangedEvent.SubscriptionId))
        {
            logger.LogError("Validation failed: sensorDataChangedEvent.SubscriptionId is null or whitespace");
            return false;
        }

        if (sensorDataChangedEvent.Item is null)
        {
            logger.LogError("Validation failed: sensorDataChangedEvent.Item is null");
            return false;
        }

        if (string.IsNullOrWhiteSpace(sensorDataChangedEvent.Item.Id))
        {
            logger.LogError("Validation failed: sensorDataChangedEvent.Item.Id is null or whitespace");
            return false;
        }

        if (string.IsNullOrWhiteSpace(sensorDataChangedEvent.Item.Type))
        {
            logger.LogError("Validation failed: sensorDataChangedEvent.Item.Type is null or whitespace");
            return false;
        }

        return true;
    }
}
