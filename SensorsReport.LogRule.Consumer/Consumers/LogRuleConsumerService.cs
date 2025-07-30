using SensorsReport.OrionLD;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorsReport.LogRule.Consumer;

public class LogRuleConsumerService : BackgroundService, IDisposable
{
    private readonly ILogger<LogRuleConsumerService> logger;
    private readonly IQueueService queueService;
    private readonly IEnqueueService enqueueService;
    private readonly IServiceProvider serviceProvider;

    public LogRuleConsumerService(ILogger<LogRuleConsumerService> logger,
        IQueueService queueService,
        IEnqueueService enqueueService,
        IServiceProvider serviceProvider)
    {

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService), "Queue service cannot be null");
        this.enqueueService = enqueueService ?? throw new ArgumentNullException(nameof(enqueueService), "Enqueue service cannot be null");
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "Service provider cannot be null");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Log Rule Business Service is starting.");
        stoppingToken.Register(() => logger.LogInformation("Log Rule Business Service is stopping."));

        try
        {
            await queueService.StartConsumingAsync(ProcessMessageAsync, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unhandled exception occurred in LogRuleConsumerService. The service is stopping.");
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

        if (metaProp.Value.TryGetProperty("LogRule", out var logRuleElement))
        {
            if (logRuleElement.ValueKind != JsonValueKind.Object)
                return (false, string.Format("Received property {0} with invalid LogRule format.", metaProp.Key));

            if (!logRuleElement.TryGetProperty("object", out var logRuleIdElement) || logRuleIdElement.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(logRuleIdElement.GetString()))
                return (false, string.Format("Received property {0} without valid LogRule object ID.", metaProp.Key));
        }
        try
        {
            var propertyData = prop.Value.Deserialize<EntityPropertyModel>();
            var metadata = metaProp.Value.Deserialize<MetaPropertyModel>();
            if (propertyData == null || propertyData.Value == null || propertyData.ObservedAt == null)
                return (false, string.Format("Received property {0} with invalid data.", prop.Key));

            if (string.IsNullOrEmpty(metadata!.LogRule?.Object.FirstOrDefault()))
                return (false, string.Format("There is no LogRule for {0}.", prop.Key));
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize property {Property}: {Message}", prop.Key, ex.Message);
            return (false, string.Format("Received property {0} with invalid JSON format.", prop.Key));
        }

        return (true, null);
    }

    public (bool isValid, string? errorMessage) IsValidLogRule(LogRuleModel? logRule)
    {
        if (logRule == null)
            return (false, "Received null LogRule.");
        if (string.IsNullOrEmpty(logRule.Id))
            return (false, "Received LogRule with null or empty Id.");
        if (string.IsNullOrEmpty(logRule.Type) || !logRule.Type.Equals("LogRule", StringComparison.OrdinalIgnoreCase))
            return (false, "Received LogRule with invalid Type.");
        if (logRule.Enabled?.Value == false)
            return (false, "Log rule is not active");
        return (true, null);
    }

    private async Task PassToNextQueue(SubscriptionEventModel message, string propName, ulong deliveryTag)
    {
        var data = message.Data!.First();
        if (data == null) return;
        if (data.Properties[propName].TryGetProperty("Alarm", out var alarm) && alarm.ValueKind == JsonValueKind.Object)
        {
            logger.LogInformation("Passing message to Alarm queue for Entity Id: {EntityId}, Tenant: {Tenant}", data.Id, message.Tenant?.Tenant);
            await enqueueService.EnqueueAlarmAsync(message);
        }
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

        var properties = (subscription.WatchedAttributes != null && subscription.WatchedAttributes.Count > 0) ? entity.Properties.Where(p => subscription.WatchedAttributes!.Contains(p.Key)) : entity.Properties;

        SubscriptionEventModel createSubscriptionEventModel(KeyValuePair<string, JsonElement> prop)
        {
            return new SubscriptionEventModel
            {
                SubscriptionId = subscriptionData.SubscriptionId,
                Tenant = subscriptionData.Tenant,
                Data = [new EntityModel
                {
                    Id = entity.Id,
                    Type = entity.Type,
                    Properties = new Dictionary<string, JsonElement> { [prop.Key] = prop.Value }
                }],
            };
        }

        async Task<(int consecutiveHit, string status)> IncrementConsecutiveHit(string propKey, MetaPropertyModel propertyData, int maxConsecutiveHit, string entityId)
        {
            if (propertyData.ConsecutiveHit == null)
            {
                propertyData.ConsecutiveHit = new ValuePropertyModel<int> { Value = 1 };
            }
            else
            {
                propertyData.ConsecutiveHit.Value++;
            }

            var consecutiveHitUpdatePatch = new Dictionary<string, Dictionary<string, object>>();

            consecutiveHitUpdatePatch[GetMetaPropertyName(propKey)] = new Dictionary<string, object>
            {
                ["consecutiveHit"] = new ValuePropertyModel<int>
                {
                    Value = propertyData.ConsecutiveHit.Value
                }
            };

            var statusText = propertyData.ConsecutiveHit.Value >= maxConsecutiveHit ? "faulty" : "operational";
            consecutiveHitUpdatePatch[GetMetaPropertyName(propKey)].Add(
                "status", new ObservedValuePropertyModel<string>
                {
                    Value = statusText,
                    ObservedAt = DateTimeOffset.UtcNow
                }
            );

            await orionLd.UpdateEntityAsync(entity.Id!, consecutiveHitUpdatePatch);

            return (propertyData.ConsecutiveHit.Value, statusText);
        }

        async Task ResetConsecutiveHit(string propKey, MetaPropertyModel propertyData, string entityId)
        {
            if (propertyData.ConsecutiveHit != null && propertyData.ConsecutiveHit.Value > 0)
            {
                propertyData.ConsecutiveHit.Value = 0;
                var resetPatch = new Dictionary<string, Dictionary<string, object>>
                {
                    [GetMetaPropertyName(propKey)] = new Dictionary<string, object>
                    {
                        ["consecutiveHit"] = new ValuePropertyModel<int> { Value = 0 },
                        ["status"] = new ObservedValuePropertyModel<string>
                        {
                            Value = "operational",
                            ObservedAt = DateTimeOffset.UtcNow
                        }
                    }
                };
                await orionLd.UpdateEntityAsync(entity.Id!, resetPatch);
            }
        }

        var metaProperties = properties.Where(s => s.Key.StartsWith("metadata_"));
        var propKeys = metaProperties.Select(s => s.Key.Replace("metadata_", "")).ToList();

        properties = properties.Where(p => propKeys.Contains(p.Key));

        foreach (var prop in properties)
        {
            var metaProp = metaProperties.FirstOrDefault(m => m.Key == GetMetaPropertyName(prop.Key));
            var (isValid, errorMessage) = IsValidProperty(prop, metaProp);
            if (!isValid)
            {
                logger.LogWarning("Skipping property {Property} in Entity Id: {EntityId}, Tenant: {Tenant}, Message: {Message} ", prop.Key, entity.Id, subscriptionData.Tenant?.Tenant, errorMessage);
                await PassToNextQueue(createSubscriptionEventModel(prop), prop.Key, deliveryTag);
                continue;
            }

            var propertyData = prop.Value.Deserialize<EntityPropertyModel>()!;
            var metadata = metaProp.Value.Deserialize<MetaPropertyModel>()!;
            var logRuleId = metadata.LogRule?.Object.FirstOrDefault()!;

            logger.LogInformation("Processing log rule {LogRuleId} for property {Property}, Tenant: {Tenant}", logRuleId, prop.Key, subscriptionData.Tenant?.Tenant);

            var logRule = (await orionLd.GetEntityByIdAsync<LogRuleModel>(logRuleId))!;

            (isValid, errorMessage) = IsValidLogRule(logRule);

            if (!isValid)
            {
                logger.LogWarning("Skipping processing for LogRule {LogRuleId} in Entity Id: {EntityId}, Tenant: {Tenant}, Message: {Message}", logRuleId, entity.Id, subscriptionData.Tenant?.Tenant, errorMessage);
                await PassToNextQueue(createSubscriptionEventModel(prop), prop.Key, deliveryTag);
                continue;
            }

            logger.LogInformation("LogRule {LogRuleId} is valid. Processing property {Property} for Entity Id: {EntityId}, Tenant: {Tenant}", logRuleId, prop.Key, entity.Id, subscriptionData.Tenant?.Tenant);

            if (logRule.Low!.Value > propertyData.Value)
            {
                logger.LogInformation("LogRule {LogRuleId} is below range for property {Property}, Entity Id: {EntityId}, Tenant: {Tenant}", logRuleId, prop.Key, entity.Id, subscriptionData.Tenant?.Tenant);
                await IncrementConsecutiveHit(prop.Key, metadata, logRule.ConsecutiveHit!.Value, entity.Id!);
            }
            else if (logRule.High!.Value < propertyData.Value)
            {
                logger.LogInformation("LogRule {LogRuleId} is above range for property {Property}, Entity Id: {EntityId}, Tenant: {Tenant}", logRuleId, prop.Key, entity.Id, subscriptionData.Tenant?.Tenant);
                await IncrementConsecutiveHit(prop.Key, metadata, logRule.ConsecutiveHit!.Value, entity.Id!);
            }
            else
            {
                if (!string.IsNullOrEmpty(metadata.Status?.Value) && metadata.Status.Value.Equals(EntityPropertyModel.StatusValues.Faulty, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("LogRule {LogRuleId} is faulty for property {Property}, Entity Id: {EntityId}, Tenant: {Tenant}. Resetting consecutive hit.", logRuleId, prop.Key, entity.Id, subscriptionData.Tenant?.Tenant);
                    await ResetConsecutiveHit(prop.Key, metadata, entity.Id!);
                }
                else
                {
                    logger.LogInformation("LogRule {LogRuleId} is within range for property {Property}, Entity Id: {EntityId}, Tenant: {Tenant}, but no status change needed.", logRuleId, prop.Key, entity.Id, subscriptionData.Tenant?.Tenant);
                }

                await PassToNextQueue(createSubscriptionEventModel(prop), prop.Key, deliveryTag);
            }
        }

        queueService.AcknowledgeMessage(deliveryTag);
    }


    public override void Dispose()
    {
        base.Dispose();
        logger.LogInformation("Log Rule Business Service disposed.");
    }
}

public partial class EntityPropertyModel : PropertyModelBase
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
    [JsonPropertyName("LogRule")]
    public RelationshipModel? LogRule { get; set; }
    [JsonPropertyName("consecutiveHit")]
    public ValuePropertyModel<int>? ConsecutiveHit { get; set; }
    [JsonPropertyName("status")]
    public ObservedValuePropertyModel<string>? Status { get; set; }
}

public partial class LogRuleModel : EntityModel
{
    [JsonPropertyName("name")]
    public LogNamePropertyModel? Name { get; set; }

    [JsonPropertyName("unit")]
    public ValuePropertyModel<string>? Unit { get; set; }
    [JsonPropertyName("low")]
    public ValuePropertyModel<double>? Low { get; set; }
    [JsonPropertyName("high")]
    public ValuePropertyModel<double>? High { get; set; }
    [JsonPropertyName("enabled")]
    public ValuePropertyModel<bool>? Enabled { get; set; }
    [JsonPropertyName("consecutiveHit")]
    public ValuePropertyModel<int>? ConsecutiveHit { get; set; }
}

public class LogNamePropertyModel : ValuePropertyModel<string>
{
    [JsonPropertyName("attributeDetails")]
    public ValuePropertyModel<string>? AttributeDetails { get; set; }

    [JsonPropertyName("value")]
    public new string? Value { get; set; }
}

public partial class EntityPropertyModel
{
    public static class StatusValues
    {
        public readonly static string Faulty = "faulty";
        public readonly static string Operational = "operational";
    }
}

public partial class LogRuleModel
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