using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;

namespace SensorsReport.AlarmRule.Consumer;

public interface INotificationRepository
{
    Task<NotificationMonitorModel?> GetByAlarmForProcessingAsync(string alarmId);
    Task<List<NotificationMonitorModel>> GetNextNotificationProcessingAsync(int skip = 0, int take = 10);
    Task<NotificationMonitorModel> CreateAsync(NotificationMonitorModel notificationMonitor);
    Task<NotificationMonitorModel?> GetAsync(string id);
    Task<NotificationMonitorModel> UpdateAsync(string id, NotificationMonitorModel notificationMonitor);
    Task<NotificationMonitorModel> UpdateStatusAsync(string id, NotificationMonitorStatusEnum status, string? errorMessage = null);
}


public class NotificationMonitorModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [ExcludeFromRequest]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("alarmId")]
    public string? AlarmId { get; set; }

    [BsonElement("ruleId")]
    public string? RuleId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastUpdatedAt")]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("status")]
    public NotificationMonitorStatusEnum Status { get; set; } = NotificationMonitorStatusEnum.Watching;

    [BsonElement("errorMessage")]
    public string? Message { get; set; }

    [BsonElement("lastNotificationSentAt")]
    public DateTime? LastNotificationSentAt { get; set; }

    [BsonElement("isTimedOut")]
    public bool IsTimedOut { get; set; } = false;

    [BsonElement("timedOutAt")]
    public DateTime? TimedOutAt { get; set; }

    [BsonElement("emailChannelActive")]
    public bool EmailChannelActive { get; set; }
    [BsonElement("smsChannelActive")]
    public bool SmsChannelActive { get; set; }
    [BsonElement("tenant")]
    public TenantInfo? Tenant { get; set; }
    [BsonElement("sensorId")]
    public string? SensorId { get; set; }
    [BsonElement("sensorName")]
    public string? SensorName { get; set; }
    [BsonElement("notificationId")]
    public string? NotificationId { get; set; }
}

public enum NotificationMonitorStatusEnum
{
    [Description("Watching")]
    Watching = 0,

    [Description("Processing")]
    Processing = 1,

    [Description("Acknowledged")]
    Acknowledged = 2,

    [Description("TimedOut")]
    TimedOut = 3,

    [Description("Completed")]
    Completed = 4,

    [Description("Error")]
    Error = 5
}

public static class TimeUnitNames
{
    public static string[] Second = new[] { "sec", "seconds", "s" };
    public static string[] Minute = new[] { "min", "minutes", "m" };
    public static string[] Hour = new[] { "hr", "hours", "h", "hour" };
    public static string[] Day = new[] { "day", "days", "d" };

    public static TimeSpan ToTimeSpan(this UnitPropertyModel<int> time)
    {
        if (time.Unit == null || string.IsNullOrEmpty(time.Unit.Value))
            throw new ArgumentException("Time unit is not specified.");

        if (time.Value <= 0)
            throw new ArgumentException("Time value must be greater than zero.");

        return time.Unit!.Value switch
        {
            var unit when Second.Contains(unit) => TimeSpan.FromSeconds(time.Value),
            var unit when Minute.Contains(unit) => TimeSpan.FromMinutes(time.Value),
            var unit when Hour.Contains(unit) => TimeSpan.FromHours(time.Value),
            var unit when Day.Contains(unit) => TimeSpan.FromDays(time.Value),
            _ => throw new ArgumentException($"Unsupported time unit: {time.Unit.Value}")
        };
    }
}