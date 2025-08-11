using SensorsReport.NotificationRule.Consumer;
using SensorsReport.OrionLD;
using System.Text.Json;

namespace SensorsReport.AlarmRule.Consumer;

public class CheckNotificationsBackgroundService : BackgroundService
{
    private readonly ILogger<CheckNotificationsBackgroundService> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public CheckNotificationsBackgroundService(ILogger<CheckNotificationsBackgroundService> logger, IServiceScopeFactory factory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.serviceScopeFactory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CheckNotificationsBackgroundService is starting.");

        stoppingToken.Register(() => logger.LogInformation("CheckNotificationsBackgroundService is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            using var serviceScope = serviceScopeFactory.CreateScope();
            var repository = serviceScope.ServiceProvider.GetRequiredService<INotificationRepository>();

            logger.LogInformation("Executing Notification status update task at {Time}", DateTime.UtcNow);

            try
            {
                var skip = 0;
                var take = 10;
                var processingNotification = await repository.GetNextNotificationProcessingAsync(skip, take);

                while (processingNotification != null && processingNotification.Count > 0)
                {
                    foreach (var candidateNotification in processingNotification)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            logger.LogInformation("Cancellation requested, stopping notification status update task.");
                            return;
                        }

                        logger.LogInformation("Processing notification with ID: {NotificationId} with AlarmId: {AlarmId}", candidateNotification.Id, candidateNotification.AlarmId);

                        try
                        {

                            using var serviceScopeCandidate = serviceScopeFactory.CreateScope();
                            var orionLdService = serviceScopeCandidate.ServiceProvider.GetRequiredService<IOrionLdService>();
                            orionLdService.SetTenant(candidateNotification.Tenant!);

                            if (candidateNotification.Status.Equals(NotificationMonitorStatusEnum.Processing))
                            {
                                logger.LogWarning("Notification with ID: {NotificationId} is already in processing status and execution time over exceeded. Fixing status to Watching.", candidateNotification.Id);
                                await repository.UpdateStatusAsync(candidateNotification.Id, NotificationMonitorStatusEnum.Watching, "Processing status exceeded execution time");
                            }

                            if (candidateNotification.AlarmId == null)
                            {
                                logger.LogWarning("Notification with ID: {NotificationId} has no associated AlarmId, skipping.", candidateNotification.Id);
                                await repository.UpdateStatusAsync(candidateNotification.Id, candidateNotification.Status, "No associated AlarmId");
                                continue;
                            }

                            var notification = await repository.GetByAlarmForProcessingAsync(candidateNotification.AlarmId!);

                            if (notification == null)
                            {
                                logger.LogWarning("No notification found for AlarmId: {AlarmId}, skipping.", candidateNotification.AlarmId);
                                continue;
                            }

                            var metadataKey = $"metadata_{candidateNotification.SensorName}";
                            var entity = await orionLdService.GetEntityByIdAsync<EntityModel>(notification.SensorId!);
                            if (entity == null)
                            {
                                logger.LogWarning("Entity with SensorId: {SensorId} not found, skipping notification processing.", notification.SensorId);
                                await repository.UpdateStatusAsync(notification.Id, NotificationMonitorStatusEnum.Error, "Entity not found");
                                continue;
                            }

                            if (entity.Properties?.ContainsKey(metadataKey) != true)
                            {
                                logger.LogWarning("Metadata key: {MetadataKey} not found in entity properties for SensorId: {SensorId}, skipping notification processing.", metadataKey, notification.SensorId);
                                await repository.UpdateStatusAsync(notification.Id, NotificationMonitorStatusEnum.Error, "Metadata key not found");
                                continue;
                            }

                            if (!entity.Properties[metadataKey].TryGetProperty("Alarm", out var alarmRelation))
                            {
                                logger.LogWarning("Alarm relation not found in metadata for SensorId: {SensorId}, skipping notification processing.", notification.SensorId);
                                await repository.UpdateStatusAsync(notification.Id, NotificationMonitorStatusEnum.Error, "Alarm relation not found in metadata");
                                continue;
                            }

                            var alarmId = alarmRelation.Deserialize<RelationshipModel>()?.Object?.FirstOrDefault();

                            if (string.IsNullOrEmpty(alarmId))
                            {
                                logger.LogWarning("AlarmId not found in metadata for SensorId: {SensorId}, skipping notification processing.", notification.SensorId);
                                await repository.UpdateStatusAsync(notification.Id, NotificationMonitorStatusEnum.Error, "AlarmId not found in metadata");
                                continue;
                            }

                            if (notification.AlarmId != alarmId)
                            {
                                logger.LogWarning("Notification AlarmId: {NotificationAlarmId} does not match entity AlarmId: {AlarmId}, skipping notification processing.", notification.AlarmId, alarmId);
                                await repository.UpdateStatusAsync(notification.Id, NotificationMonitorStatusEnum.Error, "AlarmId mismatch");
                                continue;
                            }

                            var alarm = await orionLdService.GetEntityByIdAsync<AlarmModel>(alarmId);
                            if (alarm == null)
                            {
                                logger.LogWarning("Alarm with ID: {AlarmId} not found, skipping notification processing.", alarmId);
                                await repository.UpdateStatusAsync(notification.Id, NotificationMonitorStatusEnum.Error, "Alarm not found");
                                continue;
                            }

                            if (string.Equals(alarm.Status?.Value, AlarmModel.StatusValues.Close.Value, StringComparison.OrdinalIgnoreCase))
                            {
                                logger.LogInformation("Alarm with ID: {AlarmId} is already closed, skipping notification processing.", alarmId);
                                await repository.UpdateStatusAsync(notification.Id, NotificationMonitorStatusEnum.Completed, "Alarm is closed");
                                return;
                            }

                            await NotifyIfTimeout(serviceScope, repository, notification, stoppingToken);
                            await RepeatAcknowledgmentIfNeeded(serviceScope, repository, notification, stoppingToken);
                            await RepeatNotificationIfNeeded(serviceScope, repository, notification, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred while processing notification with ID: {NotificationId}. Error: {Message}", candidateNotification.Id, ex.Message);
                            await repository.UpdateStatusAsync(candidateNotification.Id, NotificationMonitorStatusEnum.Error, ex.Message);
                        }
                    }

                    skip += take;
                    processingNotification = await repository.GetNextNotificationProcessingAsync(skip, take);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking notifications: {Message}", ex.Message);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        logger.LogInformation("CheckNotificationsBackgroundService has stopped.");
    }

    private async Task NotifyIfTimeout(IServiceScope serviceScope, INotificationRepository repository, NotificationMonitorModel notificationMonitor, CancellationToken stoppingToken)
    {
        var orionLdService = serviceScope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(notificationMonitor.Tenant!);
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUsersService>();
        var messageService = serviceScope.ServiceProvider.GetRequiredService<IMessageService>();
        logger.LogInformation("Checking if notification with ID: {NotificationId} has timed out.", notificationMonitor.Id);
        var notificationRule = await orionLdService.GetEntityByIdAsync<NotificationRuleModel>(notificationMonitor.RuleId!);
        if (notificationRule == null)
        {
            logger.LogWarning("Notification rule with ID: {RuleId} not found, skipping timeout check.", notificationMonitor.RuleId);
            await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "Notification rule not found");
            return;
        }

        if (notificationRule.NotifyIfTimeOut == null || notificationRule.NotifyIfTimeOut.Value <= 0)
        {
            logger.LogInformation("Notification rule with ID: {RuleId} does not require timeout notification, skipping.", notificationMonitor.RuleId);
            return;
        }

        var timeSpan = notificationRule.NotifyIfTimeOut.ToTimeSpan();
        if (notificationMonitor.CreatedAt.Add(timeSpan) <= DateTime.UtcNow)
        {
            notificationMonitor.IsTimedOut = true;
            notificationMonitor.TimedOutAt = DateTime.UtcNow;
            notificationMonitor.Status = NotificationMonitorStatusEnum.TimedOut;
            notificationMonitor.Message = $"Notification timed out after {notificationRule.NotifyIfTimeOut} minutes.";
            logger.LogInformation("Notification with ID: {NotificationId} has timed out after {Timeout} minutes.", notificationMonitor.Id, notificationRule.NotifyIfTimeOut.ToTimeSpan().TotalMinutes);

            var users = await userService.GetNotificationUsers(notificationMonitor.Tenant!, notificationMonitor.NotificationId!);
            if (users == null || users.Count == 0)
            {
                logger.LogWarning("No users found for notification entity with ID: {NotificationId}, skipping notification.", notificationMonitor.NotificationId!);
                await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "No users found for notification");
                return;
            }

            var alarm = await orionLdService.GetEntityByIdAsync<AlarmModel>(notificationMonitor.AlarmId!);

            if (alarm == null)
            {
                logger.LogWarning("Alarm with ID: {AlarmId} not found for notification with ID: {NotificationId}, skipping timeout notification.", notificationMonitor.AlarmId, notificationMonitor.Id);
                await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "Alarm not found");
                return;
            }

            var parameters = GetParameters(notificationMonitor, alarm);

            if (notificationMonitor.EmailChannelActive)
            {
                logger.LogInformation("Sending email notification for timed out notification with ID: {NotificationId}.", notificationMonitor.Id);
                await messageService.SendEmail(orionLdService, users, EmailTemplateKeys.SensorTimeoutAlarm, notificationMonitor.Tenant, parameters);
            }

            if (notificationMonitor.SmsChannelActive)
            {
                logger.LogInformation("Sending SMS notification for timed out notification with ID: {NotificationId}.", notificationMonitor.Id);
                await messageService.SendSms(orionLdService, users, SmsTemplateKeys.SmsTimeoutAlarm, notificationMonitor.Tenant, parameters);
            }

            notificationMonitor.LastNotificationSentAt = DateTime.UtcNow;
            await repository.UpdateAsync(notificationMonitor.Id, notificationMonitor);
        }
        else
        {
            logger.LogInformation("Notification with ID: {NotificationId} has not yet timed out, skipping.", notificationMonitor.Id);
        }
    }

    private async Task RepeatAcknowledgmentIfNeeded(IServiceScope serviceScope, INotificationRepository repository, NotificationMonitorModel notificationMonitor, CancellationToken stoppingToken)
    {
        var orionLdService = serviceScope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(notificationMonitor.Tenant!);
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUsersService>();
        var messageService = serviceScope.ServiceProvider.GetRequiredService<IMessageService>();
        logger.LogInformation("Checking if notification with ID: {NotificationId} needs acknowledgment.", notificationMonitor.Id);
        if (notificationMonitor.Status != NotificationMonitorStatusEnum.Acknowledged)
        {
            logger.LogInformation("Notification with ID: {NotificationId} is not acknowledged, skipping acknowledgment check.", notificationMonitor.Id);
            return;
        }

        var notificationRule = await orionLdService.GetEntityByIdAsync<NotificationRuleModel>(notificationMonitor.RuleId!);
        if (notificationRule == null)
        {
            logger.LogWarning("Notification rule with ID: {RuleId} not found, skipping acknowledgment check.", notificationMonitor.RuleId);
            await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "Notification rule not found");
            return;
        }

        if (notificationRule.RepetIfAcknowledged == null || notificationRule.RepetIfAcknowledged.Value <= 0)
        {
            logger.LogInformation("Notification rule with ID: {RuleId} does not require repeated acknowledgment, skipping.", notificationMonitor.RuleId);
            return;
        }

        var timeSpan = notificationRule.RepetIfAcknowledged.ToTimeSpan();
        var lastNotification = notificationMonitor.LastNotificationSentAt ?? notificationMonitor.CreatedAt;

        if (lastNotification.Add(timeSpan) <= DateTime.UtcNow)
        {
            logger.LogInformation("Notification with ID: {NotificationId} needs repeated acknowledgment.", notificationMonitor.Id);
            var users = await userService.GetNotificationUsers(notificationMonitor.Tenant!, notificationMonitor.NotificationId!);
            if (users == null || users.Count == 0)
            {
                logger.LogWarning("No users found for notification with ID: {NotificationId}, skipping acknowledgment.", notificationMonitor.Id);
                await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "No users found for notification");
                return;
            }
            var alarm = await orionLdService.GetEntityByIdAsync<AlarmModel>(notificationMonitor.AlarmId!);
            if (alarm == null)
            {
                logger.LogWarning("Alarm with ID: {AlarmId} not found for notification with ID: {NotificationId}, skipping acknowledgment.", notificationMonitor.AlarmId, notificationMonitor.Id);
                await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "Alarm not found");
                return;
            }
            var parameters = GetParameters(notificationMonitor, alarm);

            if (notificationMonitor.EmailChannelActive)
            {
                logger.LogInformation("Sending email acknowledgment for notification with ID: {NotificationId}.", notificationMonitor.Id);
                await messageService.SendEmail(orionLdService, users, EmailTemplateKeys.RepetAcknowledgeAlarm, notificationMonitor.Tenant, parameters);
            }
            if (notificationMonitor.SmsChannelActive)
            {
                logger.LogInformation("Sending SMS acknowledgment for notification with ID: {NotificationId}.", notificationMonitor.Id);
                await messageService.SendSms(orionLdService, users, SmsTemplateKeys.SmsRepetAcknowledgeAlarm, notificationMonitor.Tenant, parameters);
            }

            notificationMonitor.LastNotificationSentAt = DateTime.UtcNow;
            await repository.UpdateAsync(notificationMonitor.Id, notificationMonitor);
        }
        else
        {
            logger.LogInformation("Notification with ID: {NotificationId} does not need repeated acknowledgment yet.", notificationMonitor.Id);
        }
    }

    private async Task RepeatNotificationIfNeeded(IServiceScope serviceScope, INotificationRepository repository, NotificationMonitorModel notificationMonitor, CancellationToken stoppingToken)
    {
        var orionLdService = serviceScope.ServiceProvider.GetRequiredService<IOrionLdService>();
        orionLdService.SetTenant(notificationMonitor.Tenant!);
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUsersService>();
        var messageService = serviceScope.ServiceProvider.GetRequiredService<IMessageService>();
        logger.LogInformation("Checking if notification with ID: {NotificationId} needs repeated notification.", notificationMonitor.Id);
        if (notificationMonitor.Status != NotificationMonitorStatusEnum.Watching)
        {
            logger.LogInformation("Notification with ID: {NotificationId} is not in 'Watching' status, skipping repeated notification check.", notificationMonitor.Id);
            return;
        }

        var notificationRule = await orionLdService.GetEntityByIdAsync<NotificationRuleModel>(notificationMonitor.RuleId!);
        if (notificationRule == null)
        {
            logger.LogWarning("Notification rule with ID: {RuleId} not found, skipping repeated notification check.", notificationMonitor.RuleId);
            await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "Notification rule not found");
            return;
        }
        if (notificationRule.RepetAfter == null || notificationRule.RepetAfter.Value <= 0)
        {
            logger.LogInformation("Notification rule with ID: {RuleId} does not require repeated notification, skipping.", notificationMonitor.RuleId);
            return;
        }
        var timeSpan = notificationRule.RepetAfter.ToTimeSpan();
        var lastNotification = notificationMonitor.LastNotificationSentAt ?? notificationMonitor.CreatedAt;
        if (lastNotification.Add(timeSpan) <= DateTime.UtcNow)
        {
            logger.LogInformation("Notification with ID: {NotificationId} needs repeated notification.", notificationMonitor.Id);
            var users = await userService.GetNotificationUsers(notificationMonitor.Tenant!, notificationMonitor.NotificationId!);
            if (users == null || users.Count == 0)
            {
                logger.LogWarning("No users found for notification with ID: {NotificationId}, skipping repeated notification.", notificationMonitor.Id);
                await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "No users found for notification");
                return;
            }
            var alarm = await orionLdService.GetEntityByIdAsync<AlarmModel>(notificationMonitor.AlarmId!);
            if (alarm == null)
            {
                logger.LogWarning("Alarm with ID: {AlarmId} not found for notification with ID: {NotificationId}, skipping repeated notification.", notificationMonitor.AlarmId,
                    notificationMonitor.Id);
                await repository.UpdateStatusAsync(notificationMonitor.Id, NotificationMonitorStatusEnum.Error, "Alarm not found");
                return;
            }
            var parameters = GetParameters(notificationMonitor, alarm);
            if (notificationMonitor.EmailChannelActive)
            {
                logger.LogInformation("Sending email notification for notification with ID: {NotificationId}.", notificationMonitor.Id);
                await messageService.SendEmail(orionLdService, users, EmailTemplateKeys.SensorRepetAlarm, notificationMonitor.Tenant, parameters);
            }
            if (notificationMonitor.SmsChannelActive)
            {
                logger.LogInformation("Sending SMS notification for notification with ID: {NotificationId}.", notificationMonitor.Id);
                await messageService.SendSms(orionLdService, users, SmsTemplateKeys.SmsSensorRepetAlarm, notificationMonitor.Tenant, parameters);
            }
            notificationMonitor.LastNotificationSentAt = DateTime.UtcNow;
            await repository.UpdateAsync(notificationMonitor.Id, notificationMonitor);
        }
        else
        {
            logger.LogInformation("Notification with ID: {NotificationId} does not need repeated notification yet.", notificationMonitor.Id);
        }
    }

    private static Dictionary<string, string> GetParameters(NotificationMonitorModel notificationMonitor, AlarmModel alarm)
    {
        var lastValue = alarm.MeasuredValue?.Value?.LastOrDefault();
        var parameters = new Dictionary<string, string>
            {
                { "EventType", "First Alarm" },
                { "SensorId", notificationMonitor.SensorId! },
                { "SensorName", notificationMonitor.SensorName! },
                { "SensorLocation", alarm.Location?.Value?.Coordinates?.ToString() ?? "Unknown" },
                { "AlarmType", alarm.Condition?.Value ?? "Unknown" },
                { "AlarmDescription", alarm.Description?.Value ?? "No description" },
                { "AlarmStatus", alarm.Status!.Value! },
                { "AttributeValue", lastValue!.Value.ToString() ?? "N/A" },
                { "AttributeUnit", lastValue.Unit?.Value ?? "N/A" }
            };
        return parameters;
    }
}
