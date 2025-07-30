using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using SensorsReport.OrionLD;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SensorsReport.AlarmRule.Consumer;

public class AlarmRuleConsumerService : BackgroundService, IDisposable
{
    private readonly ILogger<AlarmRuleConsumerService> logger;
    private readonly IQueueService queueService;
    private readonly IServiceProvider serviceProvider;

    public AlarmRuleConsumerService(ILogger<AlarmRuleConsumerService> logger,
        IOptions<RabbitMQQueueConfig> appConfig,
        IQueueService queueService,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(appConfig);
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQConnectionString, nameof(appConfig.Value.RabbitMQConnectionString));
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQExchange, nameof(appConfig.Value.RabbitMQExchange));
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQQueue, nameof(appConfig.Value.RabbitMQQueue));
        ArgumentNullException.ThrowIfNull(appConfig.Value.RabbitMQRoutingKey, nameof(appConfig.Value.RabbitMQRoutingKey));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService), "Queue service cannot be null");
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "Service provider cannot be null");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Alarm Rule Business Service is starting.");
        stoppingToken.Register(() => logger.LogInformation("Alarm Rule Business Service is stopping."));

        try
        {
            await queueService.StartConsumingAsync(ProcessMessageAsync, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unhandled exception occurred in AlarmRuleConsumerService. The service is stopping.");
            Environment.Exit(1);
        }
    }

    private bool IsValidMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Received empty or null message.");
            return false;
        }

        try
        {
            var subscriptionData = JsonSerializer.Deserialize<SubscriptionEventModel>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (subscriptionData == null || subscriptionData.Data == null || !subscriptionData.Data.Any())
            {
                logger.LogWarning("Received message with null or empty Data: {Message}", message);
                return false;
            }

            if (subscriptionData.Data.Any(d => d == null || string.IsNullOrEmpty(d.Id)))
            {
                logger.LogWarning("Received message with null or empty Entity Id in Data: {Message}", message);
                return false;
            }

            if (subscriptionData.Data.Any(d => d.Type?.Equals("Alarm", StringComparison.OrdinalIgnoreCase) == true))
            {
                logger.LogInformation("Received message with Alarm type in Data, skipping processing: {Message}", message);
                return false;
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Received invalid JSON message: {Message}", message);
            return false;
        }

        return true;
    }

    private (bool isValid, string? errorMessage) IsValidProperty(KeyValuePair<string, JsonElement> prop, KeyValuePair<string, JsonElement> metaProp)
    {
        if (string.IsNullOrEmpty(prop.Key))
            return (false, "Received property with null or empty key.");

        if (prop.Value.ValueKind != JsonValueKind.Object)
            return (false, string.Format("Received property {0} with non-object value.", prop.Key));

        if (!metaProp.Value.TryGetProperty("AlarmRule", out var alarmRuleElement) || alarmRuleElement.ValueKind != JsonValueKind.Object)
            return (false, string.Format("Received property {0} without valid AlarmRule object.", prop.Key));

        if (!alarmRuleElement.TryGetProperty("object", out var alarmRuleIdElement) || alarmRuleIdElement.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(alarmRuleIdElement.GetString()))
            return (false, string.Format("Received property {0} without valid AlarmRule object ID.", prop.Key));

        try
        {
            var propertyData = prop.Value.Deserialize<EntityPropertyModel>();
            var metadata = metaProp.Value.Deserialize<MetaPropertyModel>();
            if (propertyData == null || propertyData.Value == null || propertyData.ObservedAt == null)
                return (false, string.Format("Received property {0} with invalid data.", prop.Key));

            if (string.IsNullOrEmpty(metadata!.AlarmRule?.Object.FirstOrDefault()))
                return (false, string.Format("There is no AlarmRule for {0}.", prop.Key));
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize property {Property}: {Message}", prop.Key, ex.Message);
            return (false, string.Format("Received property {0} with invalid JSON format.", prop.Key));
        }

        return (true, null);
    }

    public (bool isValid, string? errorMessage) IsValidAlarmRule(AlarmRuleModel? alarmRule)
    {
        if (alarmRule == null)
            return (false, "Received null AlarmRule.");
        if (string.IsNullOrEmpty(alarmRule.Id))
            return (false, "Received AlarmRule with null or empty Id.");
        if (string.IsNullOrEmpty(alarmRule.Type) || !alarmRule.Type.Equals("AlarmRule", StringComparison.OrdinalIgnoreCase))
            return (false, "Received AlarmRule with invalid Type.");
        if (!string.IsNullOrEmpty(alarmRule.Status?.Value) && !string.Equals(alarmRule.Status?.Value, AlarmRuleModel.StatusValues.Active.Value, StringComparison.OrdinalIgnoreCase))
            return (false, "Alarm rule is not active");
        return (true, null);
    }

    private string GetMetaPropertyName(string propName) => $"metadata_{propName}";

    private async Task ProcessMessageAsync(string message, ulong deliveryTag)
    {
        using var scope = serviceProvider.CreateScope();

        if (!IsValidMessage(message))
        {
            logger.LogWarning("Invalid message received. Rejecting message permanently. DeliveryTag: {DeliveryTag}", deliveryTag);
            queueService.AcknowledgeMessage(deliveryTag);
            return;
        }

        var subscriptionData = JsonSerializer.Deserialize<SubscriptionEventModel>(message)!;

        var orionLd = scope.ServiceProvider.GetRequiredService<IOrionLdService>();
        if (!string.IsNullOrEmpty(subscriptionData.Tenant?.Tenant))
            orionLd.SetTenant(subscriptionData.Tenant?.Tenant ?? string.Empty);

        var entity = subscriptionData!.Data!.FirstOrDefault();
        logger.LogInformation("Processing message for Entity Id: {EntityId}, DeliveryTag: {DeliveryTag}, Tenant: {Tenant}", entity!.Id, deliveryTag, subscriptionData.Tenant?.Tenant);

        var subscription = await orionLd.GetSubscriptionByIdAsync<SubscriptionModel>(subscriptionData.SubscriptionId!)!;
        if (subscription == null)
        {
            logger.LogWarning("Subscription with Id {SubscriptionId} not found. Skipping message. DeliveryTag: {DeliveryTag}", subscriptionData.SubscriptionId, deliveryTag);
            queueService.AcknowledgeMessage(deliveryTag);
            return;
        }

        // ensure only watched attributes by subscription are processed
        var properties = (subscription.WatchedAttributes != null && subscription.WatchedAttributes.Count > 0) ? entity.Properties.Where(p => subscription.WatchedAttributes!.Contains(p.Key)) : entity.Properties;

        var metaProperties = properties.Where(s => s.Key.StartsWith("metadata_"));
        var propKeys = metaProperties.Select(s => s.Key.Replace("metadata_", "")).ToList();

        properties = properties.Where(p => propKeys.Contains(p.Key));

        foreach (var prop in properties)
        {
            var metaProp = metaProperties.FirstOrDefault(m => m.Key == GetMetaPropertyName(prop.Key));
            var (isValid, errorMessage) = IsValidProperty(prop, metaProp);
            if (!isValid)
            {
                logger.LogWarning("Skipping property {Property} in Entity Id: {EntityId}, Tenant: {Tenant}, Message: {Message} ", prop.Key, entity.Id, subscriptionData?.Tenant?.Tenant, errorMessage);
                continue;
            }

            var propertyData = prop.Value.Deserialize<EntityPropertyModel>()!;
            var metadata = metaProp.Value.Deserialize<MetaPropertyModel>()!;
            var alarmRuleId = metadata.AlarmRule?.Object.FirstOrDefault()!;

            logger.LogInformation("Processing alarm rule {AlarmRuleId} for property {Property}, Tenant: {Tenant}", alarmRuleId, prop.Key, subscriptionData?.Tenant?.Tenant);

            var alarmRule = (await orionLd.GetEntityByIdAsync<AlarmRuleModel>(alarmRuleId))!;

            (isValid, errorMessage) = IsValidAlarmRule(alarmRule);

            if (!isValid)
            {
                logger.LogWarning("Skipping processing for AlarmRule {AlarmRuleId} in Entity Id: {EntityId}, Tenant: {Tenant}, Message: {Message}", alarmRuleId, entity.Id, subscriptionData?.Tenant?.Tenant, errorMessage);
                continue;
            }

            var alarmId = metadata.Alarm?.Object?.FirstOrDefault();
            var alarm = new AlarmModel();
            var isNew = true;

            if (string.IsNullOrEmpty(alarmId))
            {
                alarmId = $"urn:ngsi-ld:Alarm:{Guid.NewGuid():D}";
            }
            else
            {
                try
                {
                    var existingAlarm = (await orionLd.GetEntityByIdAsync<AlarmModel>(alarmId!));
                    if (existingAlarm != null && existingAlarm.Status?.Value?.Equals(AlarmModel.StatusValues.Close.Value, StringComparison.OrdinalIgnoreCase) != true)
                    {
                        alarm = existingAlarm;
                        isNew = false;
                    }
                    else
                    {
                        alarmId = $"urn:ngsi-ld:Alarm:{Guid.NewGuid():D}";
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to retrieve existing alarm with Id {AlarmId}. It may not exist, not in correct format or is not accessible. DeliveryTag: {DeliveryTag}, Tenant: {Tenant}", alarmId, deliveryTag, subscriptionData?.Tenant?.Tenant);
                    isNew = true;
                }
            }

            alarm.Id = alarmId;
            alarm.Type = "Alarm";
            alarm.Severity = AlarmModel.SeverityLevels.High;

            var prevStatus = (alarm.Status?.Value ?? AlarmModel.StatusValues.Close.Value)!;

            var sensorValue = propertyData.Value!;

            if (sensorValue < alarmRule.Low?.Value)
            {
                logger.LogInformation("Sensor value {SensorValue} is below low threshold {LowThreshold} for AlarmRule {AlarmRuleId}, Tenant: {Tenant}", sensorValue, alarmRule.Low?.Value, alarmRuleId, subscriptionData?.Tenant?.Tenant);
                alarm.Description = new()
                {
                    Value = string.Format("Sensor value for probe {0} is below low threshold", prop.Key),
                };
                alarm.Status = AlarmModel.StatusValues.Open;
                alarm.Threshold = alarmRule.Low;
            }
            else if (sensorValue < alarmRule.PreLow?.Value)
            {
                logger.LogInformation("Sensor value {SensorValue} is below pre-low threshold {PreLowThreshold} for AlarmRule {AlarmRuleId}, Tenant: {Tenant}", sensorValue, alarmRule.PreLow?.Value, alarmRuleId, subscriptionData?.Tenant?.Tenant);
                alarm.Description = new()
                {
                    Value = string.Format("Sensor value for probe {0} is below pre-low threshold", prop.Key),
                };
                alarm.Status = AlarmModel.StatusValues.Open;
                alarm.Threshold = alarmRule.PreLow;
            }
            else if (sensorValue > alarmRule.High?.Value)
            {
                logger.LogInformation("Sensor value {SensorValue} is above high threshold {HighThreshold} for AlarmRule {AlarmRuleId}, Tenant: {Tenant}", sensorValue, alarmRule.High?.Value, alarmRuleId, subscriptionData?.Tenant?.Tenant);
                alarm.Description = new()
                {
                    Value = string.Format("Sensor value for probe {0} is above high threshold", prop.Key),
                };
                alarm.Status = AlarmModel.StatusValues.Open;
                alarm.Threshold = alarmRule.High;
            }
            else if (sensorValue > alarmRule.PreHigh?.Value)
            {
                logger.LogInformation("Sensor value {SensorValue} is above pre-high threshold {PreHighThreshold} for AlarmRule {AlarmRuleId}, Tenant: {Tenant}", sensorValue, alarmRule.PreHigh?.Value, alarmRuleId, subscriptionData?.Tenant?.Tenant);
                alarm.Description = new()
                {
                    Value = string.Format("Sensor value for probe {0} is above pre-high threshold", prop.Key),
                };
                alarm.Status = AlarmModel.StatusValues.Open;
                alarm.Threshold = alarmRule.PreHigh;
            }
            else
            {
                logger.LogInformation("Sensor value {SensorValue} is within thresholds for AlarmRule {AlarmRuleId}, Tenant: {Tenant}", sensorValue, alarmRuleId, subscriptionData?.Tenant?.Tenant);

                if (isNew)
                {
                    queueService.AcknowledgeMessage(deliveryTag);
                    return;
                }

                alarm.Description = new()
                {
                    Value = string.Format("Sensor value for probe {0} is within thresholds", prop.Key),
                };
                alarm.Status = AlarmModel.StatusValues.Close;
                alarm.Threshold = null;
            }

            if (!prevStatus.Equals(alarm.Status.Value, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Alarm status changed from {PreviousStatus} to {CurrentStatus} for AlarmRule {AlarmRuleId}, Tenant: {Tenant}", prevStatus, alarm.Status.Value, alarmRuleId, subscriptionData?.Tenant?.Tenant);
                alarm.TriggeredAt = new ValuePropertyModel<DateTimeOffset>
                {
                    Value = DateTimeOffset.UtcNow
                };
                logger.LogInformation("Alarm triggered at {TriggeredAt} for AlarmRule {AlarmRuleId}, Tenant: {Tenant}", alarm.TriggeredAt.Value, alarmRuleId, subscriptionData?.Tenant?.Tenant);
            }

            alarm.Monitors = alarm.Monitors ?? new();
            alarm.Monitors.Value ??= new List<RelationshipModel>();

            var existingMonitorRelation = alarm.Monitors.Value.FirstOrDefault(s => s.Object.FirstOrDefault()?.Equals(entity.Id, StringComparison.OrdinalIgnoreCase) == true);
            if (existingMonitorRelation == null || existingMonitorRelation.MonitoredAttribute?.Value?.Equals(prop.Key, StringComparison.OrdinalIgnoreCase) != true)
            {
                alarm.Monitors.Value.Add(new RelationshipModel
                {
                    Object = [entity.Id],
                    MonitoredAttribute = new()
                    {
                        Value = prop.Key
                    }
                });
            }

            alarm.MeasuredValue ??= new();
            alarm.MeasuredValue.Value ??= [];
            alarm.MeasuredValue.Value.Add(new ObservedValuePropertyModel<double>
            {
                Value = sensorValue.Value,
                ObservedAt = propertyData.ObservedAt!.Value
            });

            if (isNew)
            {
                await orionLd.CreateEntityAsync(alarm);
            }
            else
            {
                await orionLd.UpdateEntityAsync(alarmId, alarm);
            }

            if (metadata.Alarm?.Object.FirstOrDefault()?.Equals(alarmId, StringComparison.OrdinalIgnoreCase) != true)
            {
                var relationshipUpdate = new Dictionary<string, object>
                {
                    [GetMetaPropertyName(prop.Key)] = new Dictionary<string, object>
                    {
                        ["Alarm"] = new RelationshipModel
                        {
                            Object = [alarmId]
                        }
                    }
                };

                await orionLd.UpdateEntityAsync(entity.Id!, relationshipUpdate);
                var latestEntity = await orionLd.GetEntityByIdAsync<EntityModel>(entity.Id!);
                logger.LogInformation("Updated Entity {EntityId} with new Alarm relationship for property {Property}, Tenant: {Tenant}, Value: {Value}", entity.Id, prop.Key, subscriptionData?.Tenant?.Tenant, JsonSerializer.Serialize(latestEntity));
            }
        }

        queueService.AcknowledgeMessage(deliveryTag);
    }


    public override void Dispose()
    {
        base.Dispose();
        logger.LogInformation("Alarm Rule Business Service disposed.");
    }
}

public class EntityPropertyModel : PropertyModelBase
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }
    [JsonPropertyName("observedAt")]
    public DateTimeOffset? ObservedAt { get; set; }
    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }
}

public partial class MetaPropertyModel : PropertyModelBase
{
    [JsonPropertyName("AlarmRule")]
    public RelationshipModel? AlarmRule { get; set; }
    [JsonPropertyName("Alarm")]
    public RelationshipModel? Alarm { get; set; }
    [JsonPropertyName("previousValue")]
    public double? PreviousValue { get; set; }
    [JsonPropertyName("previousValueObservedAt")]
    public DateTimeOffset? PreviousValueObservedAt { get; set; }
}


public partial class AlarmRuleModel : EntityModel
{
    [JsonPropertyName("name")]
    public AlarmNamePropertyModel? Name { get; set; }

    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }
    [JsonPropertyName("low")]
    public ValuePropertyModel<double>? Low { get; set; }
    [JsonPropertyName("prelow")]
    public ValuePropertyModel<double>? PreLow { get; set; }
    [JsonPropertyName("prehigh")]
    public ValuePropertyModel<double>? PreHigh { get; set; }
    [JsonPropertyName("high")]
    public ValuePropertyModel<double>? High { get; set; }
    [JsonPropertyName("status")]
    public ValuePropertyModel<string>? Status { get; set; }
}

public class AlarmNamePropertyModel : ValuePropertyModel<string>
{
    [JsonPropertyName("attributeDetails")]
    public ValuePropertyModel<string>? AttributeDetails { get; set; }

    [JsonPropertyName("value")]
    public new string? Value { get; set; }
}

public partial class AlarmModel : EntityModel
{
    [JsonPropertyName("description")]
    public ValuePropertyModel<string>? Description { get; set; }

    [JsonPropertyName("status")]
    public ValuePropertyModel<string>? Status { get; set; }

    [JsonPropertyName("severity")]
    public ValuePropertyModel<string>? Severity { get; set; } = AlarmModel.SeverityLevels.None;

    [JsonPropertyName("triggeredAt")]
    public ValuePropertyModel<DateTimeOffset>? TriggeredAt { get; set; }

    [JsonPropertyName("monitors")]
    public RelationshipValuesModel? Monitors { get; set; }

    [JsonPropertyName("threshold")]
    public ValuePropertyModel<double>? Threshold { get; set; }

    [JsonPropertyName("measuredValue")]
    public ValuePropertyModel<List<ObservedValuePropertyModel<double>>>? MeasuredValue { get; set; }

    [JsonPropertyName("location")]
    public ValuePropertyModel<Point>? Location { get; set; }
}

public partial class AlarmModel
{
    public static class SeverityLevels
    {
        public readonly static ValuePropertyModel<string> None = new()
        {
            Value = "none"
        };

        public readonly static ValuePropertyModel<string> High = new()
        {
            Value = "high"
        };

        public readonly static ValuePropertyModel<string> Medium = new()
        {
            Value = "medium"
        };

        public readonly static ValuePropertyModel<string> Low = new()
        {
            Value = "low"
        };
    }

    public static class StatusValues
    {
        public readonly static ValuePropertyModel<string> Open = new()
        {
            Value = "open"
        };
        public readonly static ValuePropertyModel<string> Close = new()
        {
            Value = "close"
        };
    }
}

public partial class AlarmRuleModel
{
    public static class StatusValues
    {
        public readonly static ValuePropertyModel<string> Active = new()
        {
            Value = "active"
        };

        public readonly static ValuePropertyModel<string> Deleted = new()
        {
            Value = "deleted"
        };

        public readonly static ValuePropertyModel<string> Disabled = new()
        {
            Value = "disabled"
        };
    }
}