using MassTransit;
using SensorsReport.Api.Core.MassTransit;
using SensorsReport.OrionLD;
using System.Text.Json;

namespace SensorsReport.AlarmRule.Consumer.Consumers;

public class TriggerAlarmRuleConsumer(ILogger<TriggerAlarmRuleConsumer> logger, JsonSerializerOptions jsonSerializerOptions, IServiceProvider serviceProvider, IEventBus eventBus) : IConsumer<TriggerAlarmRuleEvent>
{
    private readonly ILogger<TriggerAlarmRuleConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IEventBus eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly JsonSerializerOptions JsonSerializerOptions = jsonSerializerOptions;
    public async Task Consume(ConsumeContext<TriggerAlarmRuleEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var triggerAlarmRuleEvent = context.Message;

        if (triggerAlarmRuleEvent is null)
        {
            logger.LogError("Received null TriggerAlarmRuleEvent");
            return;
        }

        var scope = serviceProvider.CreateScope();
        var orionLdService = scope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(triggerAlarmRuleEvent.Tenant!.Tenant);
        var sensor = await orionLdService.GetEntityByIdAsync<EntityModel>(triggerAlarmRuleEvent.SensorId!);

        if (sensor is null)
        {
            logger.LogError("Sensor with ID {SensorId} not found", triggerAlarmRuleEvent.SensorId);
            return;
        }

        logger.LogInformation("Processing TriggerAlarmRuleEvent for sensor {SensorId} with property {PropertyKey} and metadata {MetadataKey}",
            triggerAlarmRuleEvent.SensorId, triggerAlarmRuleEvent.PropertyKey, triggerAlarmRuleEvent.MetadataKey);

        var propertyJson = sensor.Properties!.FirstOrDefault(s => s.Key.Equals(triggerAlarmRuleEvent.PropertyKey, StringComparison.OrdinalIgnoreCase)).Value;
        var metadataJson = sensor.Properties!.FirstOrDefault(s => s.Key.Equals(triggerAlarmRuleEvent.MetadataKey, StringComparison.OrdinalIgnoreCase)).Value;

        if (propertyJson.ValueKind != JsonValueKind.Object || metadataJson.ValueKind != JsonValueKind.Object)
        {
            logger.LogError("Property or metadata for sensor {SensorId} with property {PropertyKey} and metadata {MetadataKey} is not an object",
                triggerAlarmRuleEvent.SensorId, triggerAlarmRuleEvent.PropertyKey, triggerAlarmRuleEvent.MetadataKey);
            return;
        }

        var sensorProperty = propertyJson.Deserialize<EntityPropertyModel>(JsonSerializerOptions);
        if (sensorProperty is null)
        {
            logger.LogError("Deserialization of property for sensor {SensorId} with property {PropertyKey} failed",
                triggerAlarmRuleEvent.SensorId, triggerAlarmRuleEvent.PropertyKey);
            return;
        }

        var sensorMetadata = metadataJson.Deserialize<MetaPropertyModel>(JsonSerializerOptions);
        if (sensorMetadata?.AlarmRule is null)
        {
            logger.LogWarning("AlarmRule metadata for property {PropertyKey} is null", triggerAlarmRuleEvent.PropertyKey);
            return;
        }

        var alarmRule = (await orionLdService.GetEntityByIdAsync<AlarmRuleModel>(sensorMetadata.AlarmRule?.Object?.FirstOrDefault()!));
        if (alarmRule is null)
        {
            logger.LogWarning("AlarmRule with ID {AlarmRuleId} not found", sensorMetadata.AlarmRule?.Object?.FirstOrDefault());
            return;
        }

        var alarm = new AlarmModel
        {
            Id = $"urn:ngsi-ld:Alarm:{Guid.NewGuid():D}",
            Type = "Alarm",
            Severity = AlarmModel.SeverityLevels.High
        };
        var isNew = true;
        if (sensorMetadata.Alarm?.Object?.FirstOrDefault() is string alarmId)
        {
            var existingAlarm = await orionLdService.GetEntityByIdAsync<AlarmModel>(alarmId);
            if (existingAlarm is not null && existingAlarm.Status?.Value?.Equals(AlarmModel.StatusValues.Close.Value, StringComparison.OrdinalIgnoreCase) == false)
            {
                alarm = existingAlarm;
                isNew = false;
            }
        }

        if (sensorProperty.Value < alarmRule.Low?.Value)
        {
            logger.LogInformation("Sensor property {PropertyKey} value {Value} is below the low threshold {LowThreshold}",
                triggerAlarmRuleEvent.PropertyKey, sensorProperty.Value, alarmRule.Low?.Value);
            alarm.Status = AlarmModel.StatusValues.Open;
            alarm.Description = new()
            {
                Value = string.Format("Sensor value for probe {0} is below low threshold", triggerAlarmRuleEvent.PropertyKey),
            };
            alarm.Threshold = alarmRule.Low;
            alarm.Condition = new()
            {
                Value = "Low threshold exceeded"
            };
        }
        else if (sensorProperty.Value < alarmRule.PreLow?.Value)
        {
            logger.LogInformation("Sensor property {PropertyKey} value {Value} is below the pre-low threshold {PreLowThreshold}",
                triggerAlarmRuleEvent.PropertyKey, sensorProperty.Value, alarmRule.PreLow?.Value);
            alarm.Status = AlarmModel.StatusValues.Open;
            alarm.Description = new()
            {
                Value = string.Format("Sensor value for probe {0} is below pre-low threshold", triggerAlarmRuleEvent.PropertyKey),
            };
            alarm.Threshold = alarmRule.PreLow;
            alarm.Condition = new()
            {
                Value = "Pre-low threshold exceeded"
            };
        }
        else if (sensorProperty.Value > alarmRule.High?.Value)
        {
            logger.LogInformation("Sensor property {PropertyKey} value {Value} is above the high threshold {HighThreshold}",
                triggerAlarmRuleEvent.PropertyKey, sensorProperty.Value, alarmRule.High?.Value);
            alarm.Status = AlarmModel.StatusValues.Open;
            alarm.Description = new()
            {
                Value = string.Format("Sensor value for probe {0} is above high threshold", triggerAlarmRuleEvent.PropertyKey),
            };
            alarm.Threshold = alarmRule.High;
            alarm.Condition = new()
            {
                Value = "High threshold exceeded"
            };
        }
        else if (sensorProperty.Value > alarmRule.PreHigh?.Value)
        {
            logger.LogInformation("Sensor property {PropertyKey} value {Value} is above the pre-high threshold {PreHighThreshold}",
                triggerAlarmRuleEvent.PropertyKey, sensorProperty.Value, alarmRule.PreHigh?.Value);
            alarm.Status = AlarmModel.StatusValues.Open;
            alarm.Description = new()
            {
                Value = string.Format("Sensor value for probe {0} is above pre-high threshold", triggerAlarmRuleEvent.PropertyKey),
            };
            alarm.Threshold = alarmRule.PreHigh;
            alarm.Condition = new()
            {
                Value = "Pre-high threshold exceeded"
            };
        }
        else
        {
            logger.LogInformation("Sensor property {PropertyKey} value {Value} is within the defined thresholds",
                triggerAlarmRuleEvent.PropertyKey, sensorProperty.Value);
            alarm.Status = AlarmModel.StatusValues.Close;
            alarm.Description = new()
            {
                Value = string.Format("Sensor value for probe {0} is within the defined thresholds", triggerAlarmRuleEvent.PropertyKey),
            };
            alarm.Threshold = null;
            alarm.Condition = new()
            {
                Value = "Within defined thresholds"
            };
        }

        alarm.Monitors ??= new();
        alarm.Monitors.Value ??= [];

        var existingMonitorRelation = alarm.Monitors.Value.FirstOrDefault(s => s.Object.FirstOrDefault()?.Equals(triggerAlarmRuleEvent.SensorId, StringComparison.OrdinalIgnoreCase) == true);
        if (existingMonitorRelation == null || existingMonitorRelation.MonitoredAttribute?.Value?.Equals(triggerAlarmRuleEvent.PropertyKey, StringComparison.OrdinalIgnoreCase) != true)
        {
            alarm.Monitors.Value.Add(new RelationshipModel
            {
                Object = [triggerAlarmRuleEvent.SensorId],
                MonitoredAttribute = new()
                {
                    Value = triggerAlarmRuleEvent.PropertyKey
                }
            });
        }

        alarm.MeasuredValue ??= new();
        alarm.MeasuredValue.Value ??= [];
        var observedValue = new ObservedValuePropertyModel<double>
        {
            Value = sensorProperty.Value.GetValueOrDefault(),
            ObservedAt = sensorProperty.ObservedAt.GetValueOrDefault(DateTime.UtcNow),
        };
        alarm.MeasuredValue.Value.Add(observedValue);

        if (isNew)
        {
            await orionLdService.CreateEntityAsync(alarm);
        }
        else
        {
            await orionLdService.UpdateEntityAsync(alarm.Id!, alarm);
        }

        if (sensorMetadata.Alarm?.Object.FirstOrDefault()?.Equals(alarm.Id, StringComparison.OrdinalIgnoreCase) != true)
        {
            var relationshipUpdate = new Dictionary<string, object>
            {
                [triggerAlarmRuleEvent.MetadataKey!] = new Dictionary<string, object>
                {
                    ["Alarm"] = new RelationshipModel
                    {
                        Object = [alarm.Id]
                    }
                }
            };

            await orionLdService.UpdateEntityAsync(triggerAlarmRuleEvent.SensorId!, relationshipUpdate);
            var latestEntity = await orionLdService.GetEntityByIdAsync<EntityModel>(triggerAlarmRuleEvent.SensorId!);
            logger.LogInformation("Updated Entity {EntityId} with new Alarm relationship for property {Property}, Tenant: {Tenant}, Value: {Value}",
                latestEntity?.Id, triggerAlarmRuleEvent.PropertyKey, triggerAlarmRuleEvent.Tenant?.Tenant, sensorProperty.Value);
        }

        await eventBus.PublishAsync(new TriggerNotificationRuleEvent
        {
            PropertyKey = triggerAlarmRuleEvent.PropertyKey,
            SensorId = triggerAlarmRuleEvent.SensorId,
            Tenant = triggerAlarmRuleEvent.Tenant,
            MetadataKey = triggerAlarmRuleEvent.MetadataKey,
            AlarmId = alarm.Id
        });
    }
}
