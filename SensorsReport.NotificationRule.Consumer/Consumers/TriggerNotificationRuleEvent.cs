using MassTransit;
using SensorsReport.Api.Core.MassTransit;
using SensorsReport.NotificationRule.Consumer;
using SensorsReport.OrionLD;
using System.Text.Json;

namespace SensorsReport.AlarmRule.Consumer.Consumers;

public class TriggerNotificationRuleConsumer(ILogger<TriggerNotificationRuleConsumer> logger,
    JsonSerializerOptions jsonSerializerOptions,
    IServiceProvider serviceProvider,
    INotificationRepository repository) : IConsumer<TriggerNotificationRuleEvent>
{
    private readonly ILogger<TriggerNotificationRuleConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly INotificationRepository repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly JsonSerializerOptions JsonSerializerOptions = jsonSerializerOptions;

    public async Task Consume(ConsumeContext<TriggerNotificationRuleEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var triggerNotificationRuleEvent = context.Message;

        if (triggerNotificationRuleEvent is null)
        {
            logger.LogError("Received null TriggerNotificationRuleEvent");
            return;
        }

        var scope = serviceProvider.CreateScope();
        var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
        var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
        var orionLdService = scope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(triggerNotificationRuleEvent.Tenant!);

        var sensor = await orionLdService.GetEntityByIdAsync<EntityModel>(triggerNotificationRuleEvent.SensorId!);

        if (sensor is null)
        {
            logger.LogError("Sensor with ID {SensorId} not found", triggerNotificationRuleEvent.SensorId);
            return;
        }

        logger.LogInformation("Processing TriggerNotificationRuleEvent for sensor {SensorId} with property {PropertyKey} and metadata {MetadataKey}",
            triggerNotificationRuleEvent.SensorId, triggerNotificationRuleEvent.PropertyKey, triggerNotificationRuleEvent.MetadataKey);

        var propertyJson = sensor.Properties!.FirstOrDefault(s => s.Key.Equals(triggerNotificationRuleEvent.PropertyKey, StringComparison.OrdinalIgnoreCase)).Value;
        var metadataJson = sensor.Properties!.FirstOrDefault(s => s.Key.Equals(triggerNotificationRuleEvent.MetadataKey, StringComparison.OrdinalIgnoreCase)).Value;

        if (propertyJson.ValueKind != JsonValueKind.Object || metadataJson.ValueKind != JsonValueKind.Object)
        {
            logger.LogError("Property or metadata for sensor {SensorId} with property {PropertyKey} and metadata {MetadataKey} is not an object",
                triggerNotificationRuleEvent.SensorId, triggerNotificationRuleEvent.PropertyKey, triggerNotificationRuleEvent.MetadataKey);
            return;
        }

        var sensorProperty = propertyJson.Deserialize<EntityPropertyModel>(JsonSerializerOptions);
        if (sensorProperty is null)
        {
            logger.LogError("Deserialization of property for sensor {SensorId} with property {PropertyKey} failed",
                triggerNotificationRuleEvent.SensorId, triggerNotificationRuleEvent.PropertyKey);
            return;
        }

        var sensorMetadata = metadataJson.Deserialize<MetaPropertyModel>(JsonSerializerOptions);
        if (sensorMetadata?.AlarmRule is null)
        {
            logger.LogWarning("AlarmRule metadata for property {PropertyKey} is null", triggerNotificationRuleEvent.PropertyKey);
            return;
        }

        var notificationId = sensorMetadata.Notification?.Object?.FirstOrDefault();

        if (string.IsNullOrEmpty(notificationId))
        {
            logger.LogWarning("Notification ID is null or empty for sensor {SensorId} with property {PropertyKey}",
                triggerNotificationRuleEvent.SensorId, triggerNotificationRuleEvent.PropertyKey);
            return;
        }

        var notification = await orionLdService.GetEntityByIdAsync<NotificationModel>(notificationId);

        if (notification is null)
        {
            logger.LogError("Notification with ID {NotificationId} not found", notificationId);
            return;
        }

        if (notification.Enable?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) != true)
        {
            logger.LogInformation("Notification with ID {NotificationId} is disabled", notificationId);
            return;
        }

        if (notification.NotificationRule?.Object is null || notification.NotificationRule.Object.Count == 0)
        {
            logger.LogWarning("No NotificationRule found for Notification with ID {NotificationId}", notificationId);
            return;
        }

        var notificationRuleId = notification.NotificationRule.Object.FirstOrDefault();

        if (string.IsNullOrEmpty(notificationRuleId))
        {
            logger.LogWarning("NotificationRule ID is null or empty for Notification with ID {NotificationId}", notificationId);
            return;
        }

        var notificationRule = await orionLdService.GetEntityByIdAsync<NotificationRuleModel>(notificationRuleId);

        if (notificationRule is null)
        {
            logger.LogError("NotificationRule with ID {NotificationRuleId} not found", notificationRuleId);
            return;
        }

        var alarm = await orionLdService.GetEntityByIdAsync<AlarmModel>(triggerNotificationRuleEvent.AlarmId!);

        if (alarm is null)
        {
            logger.LogError("Alarm with ID {AlarmId} not found", triggerNotificationRuleEvent.AlarmId);
            return;
        }

        if (alarm.MeasuredValue?.Value?.Count < notificationRule.ConsecutiveHits?.Value)
        {
            logger.LogInformation("Alarm with ID {AlarmId} has not reached the required consecutive hits for notification", triggerNotificationRuleEvent.AlarmId);
            return;
        }

        List<UserModel> users = await usersService.GetNotificationUsers(triggerNotificationRuleEvent.Tenant!, notificationId);

        if (users.Count == 0)
        {
            logger.LogWarning("No users found for Notification with ID {NotificationId}", notificationId);
            return;
        }

        var parameters = new Dictionary<string, string>
        {
            { "EventType", "First Alarm" },
            { "SensorId", triggerNotificationRuleEvent.SensorId! },
            { "SensorName", triggerNotificationRuleEvent.PropertyKey! },
            { "SensorLocation", alarm.Location?.Value?.Coordinates?.ToString() ?? "Unknown" },
            { "AlarmType", alarm.Condition?.Value ?? "Unknown" },
            { "AlarmDescription", alarm.Description?.Value ?? "No description" },
            { "AlarmStatus", alarm.Status!.Value! },
            { "AttributeValue", sensorProperty.Value?.ToString() ?? "N/A" },
            { "AttributeUnit", sensorProperty.Unit?.Value ?? "N/A" }
        };

        var isEmailChannelActive = notificationRule.NotificationChannel?.Value?.Any(s => s.Equals("Email", StringComparison.OrdinalIgnoreCase)) == true;
        var isSmsChannelActive = notificationRule.NotificationChannel?.Value?.Any(s => s.Equals("Sms", StringComparison.OrdinalIgnoreCase)) == true;

        var existingMonitoringModel = await repository.GetByAlarmForProcessingAsync(triggerNotificationRuleEvent.AlarmId!);
        var isCreate = false;
        if (existingMonitoringModel == null)
        {
            existingMonitoringModel = new NotificationMonitorModel
            {
                AlarmId = triggerNotificationRuleEvent.AlarmId,
                RuleId = notificationRule.Id,
                NotificationId = notificationId,
                SensorId = triggerNotificationRuleEvent.SensorId,
                SensorName = triggerNotificationRuleEvent.PropertyKey,
                Status = NotificationMonitorStatusEnum.Processing,
                Tenant = triggerNotificationRuleEvent.Tenant,
                LastUpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                EmailChannelActive = isEmailChannelActive,
                SmsChannelActive = isSmsChannelActive
            };

            isCreate = true;
        }

        if (isCreate)
        {
            logger.LogInformation("Creating new NotificationMonitor record for Alarm ID {AlarmId}", triggerNotificationRuleEvent.AlarmId);
            await repository.CreateAsync(existingMonitoringModel);
        }

        existingMonitoringModel.Status = NotificationMonitorStatusEnum.Watching;
        existingMonitoringModel.LastUpdatedAt = DateTime.UtcNow;

        if (alarm.Status.Value?.Equals(AlarmModel.StatusValues.Close.Value, StringComparison.OrdinalIgnoreCase) == true && notificationRule.NotifyIfClose?.Value == true)
        {
            logger.LogInformation("Alarm with ID {AlarmId} is closed, proceeding with notification", triggerNotificationRuleEvent.AlarmId);
            if (isEmailChannelActive)
                await messageService.SendEmail(orionLdService, users, EmailTemplateKeys.ReturnToNormal, triggerNotificationRuleEvent.Tenant, parameters);

            if (isSmsChannelActive)
                await messageService.SendSms(orionLdService, users, SmsTemplateKeys.SmsReturnToNormal, triggerNotificationRuleEvent.Tenant, parameters);

            existingMonitoringModel.Status = NotificationMonitorStatusEnum.Completed;

            logger.LogInformation("Sending notification for closed alarm with ID {AlarmId}", triggerNotificationRuleEvent.AlarmId);
        }
        else if (alarm.Status.Value?.Equals(AlarmModel.StatusValues.Acknowledged.Value, StringComparison.OrdinalIgnoreCase) == true && notificationRule.NotifyIfAcknowledged?.Value == true)
        {
            logger.LogInformation("Alarm with ID {AlarmId} is acknowledged, proceeding with notification", triggerNotificationRuleEvent.AlarmId);
            if (isEmailChannelActive)
                await messageService.SendEmail(orionLdService, users, EmailTemplateKeys.FirstAcknowledgeAlarm, triggerNotificationRuleEvent.Tenant, parameters);

            if (isSmsChannelActive)
                await messageService.SendSms(orionLdService, users, SmsTemplateKeys.SmsFirstAcknowledgeAlarm, triggerNotificationRuleEvent.Tenant, parameters);

            existingMonitoringModel.Status = NotificationMonitorStatusEnum.Acknowledged;

            logger.LogInformation("Sending notification for acknowledged alarm with ID {AlarmId}", triggerNotificationRuleEvent.AlarmId);
        }
        else if (isCreate)
        {
            if (isEmailChannelActive)
                await messageService.SendEmail(orionLdService, users, EmailTemplateKeys.SensorFirstAlarm, triggerNotificationRuleEvent.Tenant, parameters);

            if (isSmsChannelActive)
                await messageService.SendSms(orionLdService, users, SmsTemplateKeys.SmsSensorFirstAlarm, triggerNotificationRuleEvent.Tenant, parameters);

            logger.LogInformation("Sending notification for alarm with ID {AlarmId}", triggerNotificationRuleEvent.AlarmId);
        }

        await repository.UpdateAsync(existingMonitoringModel.Id, existingMonitoringModel);
        logger.LogInformation("Notification processing completed for Alarm ID {AlarmId}, monitoring: {MonitoringJson}",
            triggerNotificationRuleEvent.AlarmId, JsonSerializer.Serialize(existingMonitoringModel, JsonSerializerOptions));
    }
}
