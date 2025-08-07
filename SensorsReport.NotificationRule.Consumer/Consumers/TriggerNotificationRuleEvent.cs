using MassTransit;
using SensorsReport.Api.Core.MassTransit;
using SensorsReport.OrionLD;
using System;
using System.Linq;
using System.Text.Json;

namespace SensorsReport.AlarmRule.Consumer.Consumers;

public class TriggerNotificationRuleConsumer(ILogger<TriggerNotificationRuleConsumer> logger, IServiceProvider serviceProvider, IEventBus eventBus) : IConsumer<TriggerNotificationRuleEvent>
{
    private readonly ILogger<TriggerNotificationRuleConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IEventBus eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

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
        var orionLdService = scope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(triggerNotificationRuleEvent.Tenant!.Tenant);
        var sensor = await orionLdService.GetEntityByIdAsync<EntityModel>(triggerNotificationRuleEvent.SensorId!);

        if (sensor is null)
        {
            logger.LogError("Sensor with ID {SensorId} not found", triggerNotificationRuleEvent.SensorId);
            return;
        }

        logger.LogInformation("Processing TriggerNotificationRuleEvent for sensor {SensorId} with property {PropertyKey} and metadata {MetadataKey}",
            triggerNotificationRuleEvent.SensorId, triggerNotificationRuleEvent.PropertyKey, triggerNotificationRuleEvent.MetadataKey);

        var propertyJson = sensor.Properties.FirstOrDefault(s => s.Key.Equals(triggerNotificationRuleEvent.PropertyKey, StringComparison.OrdinalIgnoreCase)).Value;
        var metadataJson = sensor.Properties.FirstOrDefault(s => s.Key.Equals(triggerNotificationRuleEvent.MetadataKey, StringComparison.OrdinalIgnoreCase)).Value;

        if (propertyJson.ValueKind != JsonValueKind.Object || metadataJson.ValueKind != JsonValueKind.Object)
        {
            logger.LogError("Property or metadata for sensor {SensorId} with property {PropertyKey} and metadata {MetadataKey} is not an object",
                triggerNotificationRuleEvent.SensorId, triggerNotificationRuleEvent.PropertyKey, triggerNotificationRuleEvent.MetadataKey);
            return;
        }

        var sensorProperty = propertyJson.Deserialize<EntityPropertyModel>();
        if (sensorProperty is null)
        {
            logger.LogError("Deserialization of property for sensor {SensorId} with property {PropertyKey} failed",
                triggerNotificationRuleEvent.SensorId, triggerNotificationRuleEvent.PropertyKey);
            return;
        }

        var sensorMetadata = metadataJson.Deserialize<MetaPropertyModel>();
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
        var notificationUserIds = notification?.NotificationUser?.Object;

        if (notificationUserIds is null || notificationUserIds.Count == 0)
        {
            logger.LogWarning("No notification users found for Notification with ID {NotificationId}", notificationId);
            return;
        }

        IEnumerable<NotificationUsersModel?> notificationUsers = (await Task.WhenAll(notificationUserIds.Select(userId => orionLdService.GetEntityByIdAsync<NotificationUsersModel>(userId)))) ?? Array.Empty<NotificationUsersModel?>();

        notificationUsers = notificationUsers.Where(user => user?.Enable?.Value == true).DistinctBy(s => s!.Id);

        if (!notificationUsers.Any())
        {
            logger.LogWarning("No enabled notification users found for Notification with ID {NotificationId}", notificationId);
            return;
        }

        List<UserModel> users = [];

        foreach (var notificationUser in notificationUsers)
        {
            var groupIds = notificationUser?.Groups?.Value?.Select(s => s.Object.FirstOrDefault()) ?? [];
            var groups = await Task.WhenAll(groupIds.Select(groupId => orionLdService.GetEntityByIdAsync<GroupModel>(groupId!)));
            var groupUserIds = groups.SelectMany(g => g?.Users?.Object ?? []).Distinct().ToList();
            var userIds = notificationUser?.Users?.Value?.Select(s => s.Object.FirstOrDefault()) ?? [];

            userIds = [.. userIds, .. groupUserIds];

            users = [.. (await Task.WhenAll(
                userIds
                .Select(userId => orionLdService.GetEntityByIdAsync<UserModel>(userId!))))
                .Where(u => u != null)
                .Cast<UserModel>()];
        }

        if (users.Count == 0)
        {
            logger.LogWarning("No users found for Notification with ID {NotificationId}", notificationId);
            return;
        }

        var parameters = new Dictionary<string, string>
        {
            { "EventType", "First Alarm" },
            { "SensorId", sensor.Id! },
            { "SensorName", triggerNotificationRuleEvent.PropertyKey! },
            { "SensorLocation", alarm.Location?.Value?.Coordinates?.ToString() ?? "Unknown" },
            { "AlarmType", alarm.Condition?.Value ?? "Unknown" },
            { "AlarmDescription", alarm.Description?.Value ?? "No description" },
            { "AlarmStatus", alarm.Status!.Value! },
            { "AttributeValue", sensorProperty.Value?.ToString() ?? "N/A" },
            { "AttributeUnit", sensorProperty.Unit?.Value ?? "N/A" },
            { "UserName", string.Join(", ", users.Select(u => $"{u.FirstName} {u.LastName}")) }
        };

        if (alarm.Status.Value?.Equals(AlarmModel.StatusValues.Close) == true && notificationRule.NotifyIfClose?.Value != true)
        {
            logger.LogInformation("Alarm with ID {AlarmId} is closed and NotifyIfClose is false, skipping notification", triggerNotificationRuleEvent.AlarmId);
            return;
        }

        if (notificationRule.NotificationChannel?.Value?.Contains("Email") == true)
            await Task.WhenAll(users.Select(user => SendEmail(orionLdService, user, EmailTemplateKeys.SensorFirstAlarm, parameters)));

        if (notificationRule.NotificationChannel?.Value?.Contains("Sms") == true)
            await Task.WhenAll(users.Select(user => SendSms(orionLdService, user, SmsTemplateKeys.SmsSensorFirstAlarm, triggerNotificationRuleEvent.Tenant!.Tenant, parameters)));

        logger.LogInformation("Sending notification for alarm with ID {AlarmId}", triggerNotificationRuleEvent.AlarmId);
    }

    private async Task SendSms(IOrionLdService orionLdService, UserModel user, string smsTemplateKey, string tenant, Dictionary<string, string> parameters)
    {
        if (user.Mobile?.Value is null)
        {
            logger.LogWarning("User {UserId} does not have a mobile number", user.Id);
            return;
        }
        var smsTemplate = await orionLdService.GetEntityByIdAsync<SmsTemplateModel>(smsTemplateKey);
        var body = TemplateHelper.FormatString(smsTemplate?.Message?.Value ?? DefaultSmsMessage, parameters);
        await eventBus.PublishAsync<CreateSmsCommand>(new CreateSmsCommand
        {
            Message = body,
            PhoneNumber = user.Mobile.Value,
            Tenant = tenant,
            MessageType = "Alarm"
        });
    }

    private async Task SendEmail(IOrionLdService orionLdService, UserModel user, string emailTemplateKey, Dictionary<string, string> parameters)
    {
        if (user.Email?.Value is null)
        {
            logger.LogWarning("User {UserId} does not have an email address", user.Id);
            return;
        }

        var emailTemplate = await orionLdService.GetEntityByIdAsync<EmailTemplateModel>(emailTemplateKey);

        var subject = TemplateHelper.FormatString(emailTemplate?.Subject?.Value ?? DefaultEmailSubject, parameters);
        var body = TemplateHelper.FormatString(emailTemplate?.Body?.Value ?? DefaultEmailBody, parameters);

        await eventBus.PublishAsync<CreateEmailCommand>(new CreateEmailCommand
        {
            ToEmail = user.Email?.Value,
            ToName = $"{user.FirstName} {user.LastName}",
            Subject = subject,
            BodyHtml = body
        });
    }

    private const string DefaultSmsMessage = "{{AlarmDescription}} {{SensorId}} {{SensorName}} {{SensorLocation}} {{AlarmType}} {{AttributeValue}} {{AttributeUnit}}";
    private const string DefaultEmailSubject = "SensorsReport - Alarm Notification";
    private const string DefaultEmailBody = """
        Dear {{UserName}},
        The following alarm has occurred:
        Alarm Description: {{AlarmDescription}}
        Sensor Location: {{SensorLocation}}
        Sensor Name: {{SensorName}}
        Sensor ID: {{SensorId}}
        Current Value: {{AttributeValue}} {{AttributeUnit}}

        Regards,
        http://www.sensorsreport.com
    """;

}
