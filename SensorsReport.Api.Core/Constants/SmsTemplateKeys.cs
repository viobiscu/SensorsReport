namespace SensorsReport;

public class SmsTemplateKeys
{
    private const string Prefix = "urn:ngsi-ld:SMSTemplate:Sms";
    public const string SmsSensorFirstAlarm = $"{Prefix}{nameof(SmsSensorFirstAlarm)}";
    public const string SmsReturnToNormal = $"{Prefix}{nameof(SmsReturnToNormal)}";
    public const string SmsSensorMissTransmision = $"{Prefix}{nameof(SmsSensorMissTransmision)}";
    public const string SmsSensorResumeTransmision = $"{Prefix}{nameof(SmsSensorResumeTransmision)}";
    public const string SmsUserAdded = $"{Prefix}{nameof(SmsUserAdded)}";
    public const string SmsUserDeleted = $"{Prefix}{nameof(SmsUserDeleted)}";
    public const string SmsGroupAdded = $"{Prefix}{nameof(SmsGroupAdded)}";
    public const string SmsGroupDeleted = $"{Prefix}{nameof(SmsGroupDeleted)}";
    public const string SmsSensorLowBattery = $"{Prefix}{nameof(SmsSensorLowBattery)}";
    public const string SmsLocationDisconnected = $"{Prefix}{nameof(SmsLocationDisconnected)}";
    public const string SmsLocationReconnected = $"{Prefix}{nameof(SmsLocationReconnected)}";
    public const string SmsSensorChangeConfiguration = $"{Prefix}{nameof(SmsSensorChangeConfiguration)}";
    public const string SmsPowerFail = $"{Prefix}{nameof(SmsPowerFail)}";
    public const string SmsSensorRepetAlarm = $"{Prefix}{nameof(SmsSensorRepetAlarm)}";
    public const string SmsRepetAcknowledgeAlarm = $"{Prefix}{nameof(SmsRepetAcknowledgeAlarm)}";
    public const string SmsFirstAcknowledgeAlarm = $"{Prefix}{nameof(SmsFirstAcknowledgeAlarm)}";
}