using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using SensorsReport.OrionLD;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    private async Task ProcessMessageAsync(string message, ulong deliveryTag)
    {
        using var scope = serviceProvider.CreateScope();

        try
        {
            var webhookData = JsonSerializer.Deserialize<WebhookData>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (webhookData == null || webhookData.Data == null)
            {
                logger.LogWarning("Received message with null or empty Data. Rejecting message permanently. DeliveryTag: {DeliveryTag}", deliveryTag);
                queueService.AcknowledgeMessage(deliveryTag);
                return;
            }

            var orionLd = scope.ServiceProvider.GetRequiredService<IOrionLdService>();
            if (!string.IsNullOrEmpty(webhookData.Tenant?.Tenant))
            {
                orionLd.SetTenant(webhookData?.Tenant?.Tenant ?? string.Empty);
            }

            var entity = webhookData!.Data.FirstOrDefault();
        
            if (entity == null || string.IsNullOrEmpty(entity.Id))
        {
                logger.LogWarning("Received message with null or empty Entity Id. Rejecting message permanently. DeliveryTag: {DeliveryTag}", deliveryTag);
            queueService.AcknowledgeMessage(deliveryTag);
                return;
            }

            if (entity?.Type?.Equals("Alarm", StringComparison.OrdinalIgnoreCase) == true)
            {
                queueService.AcknowledgeMessage(deliveryTag);
                return;
            }

            logger.LogInformation("Processing message for Entity Id: {EntityId}, DeliveryTag: {DeliveryTag}", entity!.Id, deliveryTag);

            foreach (var prop in entity.Properties)
            {
                if (prop.Value.ValueKind != JsonValueKind.Object)
                    continue;

                prop.Value.TryGetProperty("AlarmRule", out var alarmRuleProperty);
                if (alarmRuleProperty.ValueKind != JsonValueKind.Object)
                    continue;

                alarmRuleProperty.TryGetProperty("object", out var alarmRuleIdProperty);
                if (alarmRuleIdProperty.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(alarmRuleIdProperty.GetString()))
                {
                    logger.LogWarning("No alarm rule for property {Property}, skipping", prop.Key);
                    continue;
                }

                var alarmRuleId = alarmRuleIdProperty.GetString()!;
                logger.LogInformation("Processing alarm rule {AlarmRuleId} for property {Property}", alarmRuleId, prop.Key);

                var alarmRule = await orionLd.GetEntityByIdAsync<EntityModel>(alarmRuleId);

                if (alarmRule == null)
                {
                    logger.LogWarning("Alarm rule {AlarmRuleId} not found, skipping", alarmRuleId);
                    continue;
                }

                var oboservedAt = prop.Value.GetProperty("observedAt").GetDateTimeOffset();
                var value = prop.Value.GetProperty("value").GetDouble();

                var low = alarmRule.Properties.FirstOrDefault(p => p.Key == "low").Value;
                var preLow = alarmRule.Properties.FirstOrDefault(p => p.Key == "prelow").Value;
                var preHigh = alarmRule.Properties.FirstOrDefault(p => p.Key == "prehigh").Value;
                var high = alarmRule.Properties.FirstOrDefault(p => p.Key == "high").Value;
                var unit = alarmRule.Properties.FirstOrDefault(p => p.Key == "unit").Value;
                var alarmId = $"urn:ngsi-ld:Alarm:{entity.Id.Split(":").Last()}";
                var alarm = await orionLd.GetEntityByIdAsync<EntityModel>(alarmId);
                var isNewAlarm = false;

                if (alarm == null)
                {
                    isNewAlarm = true;
                    logger.LogInformation("Creating empty alarm for Entity Id: {EntityId}", entity.Id);
                    alarm = new EntityModel
                    {
                        Id = alarmId,
                        Type = "Alarm",
                        Properties = new Dictionary<string, JsonElement>
                        {
                            {
                                "belongto", JsonSerializer.SerializeToElement(new RelationshipModel
                                {
                                    Type = PropertyModelBase.PropertyType.Relationship,
                                    Object = [entity.Id]
                                })
                            }
                        },
                    };
                }

                var prevStatus = "close";
                var additionalData = new Dictionary<string, JsonElement>();

                if (alarm.Properties.TryGetValue(prop.Key, out var existingProp))
                {
                    alarm.Properties.Remove(prop.Key); // remove previous record
                    prevStatus = existingProp.GetProperty("status").GetProperty("value").GetString() ?? "close";
                    if (existingProp.TryGetProperty("additionalData", out var existingAdditionalData))
                    {
                        additionalData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existingAdditionalData.GetRawText()) ?? new Dictionary<string, JsonElement>();
                        additionalData = additionalData.Where(kv =>
                            kv.Key != "observedAt" &&
                            kv.Key != "unit" &&
                            kv.Key != "status")
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
                    }
                }

                logger.LogInformation("Updating existing property {Property} in alarm {AlarmId}", prop.Key, alarm.Id);
                var updatedProp = new PropertyModel
                {
                    Type = PropertyModelBase.PropertyType.Property,
                    Value = prop.Value.GetProperty("value"),
                    AdditionalData = new Dictionary<string, JsonElement>
                    {
                        { "observedAt", prop.Value.GetProperty("observedAt") },
                        { "unit", unit },
                        {
                            "status", JsonSerializer.SerializeToElement(new PropertyModel
                            {
                                Type = PropertyModelBase.PropertyType.Property,
                                Value = JsonSerializer.SerializeToElement("open")
                            })
                        },
                        {
                            "previousStatus", JsonSerializer.SerializeToElement(new PropertyModel
                            {
                                Type = PropertyModelBase.PropertyType.Property,
                                Value = JsonSerializer.SerializeToElement(prevStatus)
                            })
                        }
                    }
                };

                foreach (var kv in additionalData)
                    updatedProp.AdditionalData[kv.Key] = kv.Value;

                var status = "close";
                var alarmtype = string.Empty;

                if (low.TryGetProperty("value", out var lowValue) && lowValue.GetDouble() > value)
                {
                    logger.LogInformation("Value {Value} is below low threshold {Low}. Triggering alarm rule {AlarmRuleId}", value, lowValue.GetDouble(), alarmRuleId);
                    status = "open";
                    alarmtype = "low";
                }
                else if (high.TryGetProperty("value", out var highValue) && highValue.GetDouble() < value)
                {
                    logger.LogInformation("Value {Value} is above high threshold {High}. Triggering alarm rule {AlarmRuleId}", value, highValue.GetDouble(), alarmRuleId);
                    status = "open";
                    alarmtype = "high";
                }
                else if (preLow.TryGetProperty("value", out var preLowValue) && preLowValue.GetDouble() > value)
                {
                    logger.LogInformation("Value {Value} is below pre-low threshold {PreLow}. Triggering alarm rule {AlarmRuleId}", value, preLowValue.GetDouble(), alarmRuleId);
                    status = "open";
                    alarmtype = "prelow";
                }
                else if (preHigh.TryGetProperty("value", out var preHighValue) && preHighValue.GetDouble() < value)
                {
                    logger.LogInformation("Value {Value} is above pre-high threshold {PreHigh}. Triggering alarm rule {AlarmRuleId}", value, preHighValue.GetDouble(), alarmRuleId);
                    status = "open";
                    alarmtype = "prehigh";
        }
        else
        {
                    logger.LogInformation("Value {Value} is within thresholds. No action needed for alarm rule {AlarmRuleId}", value, alarmRuleId);
                }

                updatedProp.AdditionalData["status"] = JsonSerializer.SerializeToElement(new PropertyModel
                {
                    Type = PropertyModelBase.PropertyType.Property,
                    Value = JsonSerializer.SerializeToElement(status)
                });

                updatedProp.AdditionalData["alarmType"] = JsonSerializer.SerializeToElement(new PropertyModel
                {
                    Type = PropertyModelBase.PropertyType.Property,
                    Value = JsonSerializer.SerializeToElement(alarmtype)
                });

                alarm.Properties[prop.Key] = JsonSerializer.SerializeToElement(updatedProp);

                if (isNewAlarm)
                {
                    logger.LogInformation("Creating new alarm {AlarmId}", alarm.Id);
                    await orionLd.CreateEntityAsync(alarm);
                }
                else
                {
                    logger.LogInformation("Updating existing alarm {AlarmId}", alarm.Id);
                    await orionLd.UpdateEntityAsync(alarmId, alarm);
                }

                if (prop.Value.TryGetProperty("Alarm", out var alarmElement) &&
                    alarmElement.TryGetProperty("object", out var relationshipAlarmId) &&
                    relationshipAlarmId.GetString()?.Equals(alarmId, StringComparison.OrdinalIgnoreCase) == false)
                {
                    var relationshipUpdate = new Dictionary<string, object>
                    {
                        [prop.Key] = new Dictionary<string, object>
                        {
                            ["Alarm"] = new RelationshipModel
                            {
                                Type = PropertyModelBase.PropertyType.Relationship,
                                Object = [alarmId]
                            }
                        }
                    };

                    await orionLd.UpdateEntityAsync(entity.Id, relationshipUpdate);
                }
            }


            queueService.AcknowledgeMessage(deliveryTag);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the message: {Message}", message);
            queueService.RejectMessage(deliveryTag, requeue: false);
        }
    }


    public override void Dispose()
    {
        base.Dispose();
        logger.LogInformation("Alarm Rule Business Service disposed.");
    }
}

