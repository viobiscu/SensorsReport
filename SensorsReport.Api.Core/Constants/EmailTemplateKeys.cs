namespace SensorsReport;

public class EmailTemplateKeys
{
    private const string Prefix = "urn:ngsi-ld:EmailTemplate:Email";

    public const string SensorResumeTransmision = $"{Prefix}{nameof(SensorResumeTransmision)}";
    public const string SensorFirstAlarm = $"{Prefix}{nameof(SensorFirstAlarm)}";
    public const string SensorMissTransmision = $"{Prefix}{nameof(SensorMissTransmision)}";
    public const string ReturnToNormal = $"{Prefix}{nameof(ReturnToNormal)}";
    public const string FirstAcknowledgeAlarm = $"{Prefix}{nameof(FirstAcknowledgeAlarm)}";
    public const string RepetAcknowledgeAlarm = $"{Prefix}{nameof(RepetAcknowledgeAlarm)}";
    public const string SensorRepetAlarm = $"{Prefix}{nameof(SensorRepetAlarm)}";
    public const string LocationReconnected = $"{Prefix}{nameof(LocationReconnected)}";
    public const string LocationDisconnected = $"{Prefix}{nameof(LocationDisconnected)}";
    public const string SensorLowBattery = $"{Prefix}{nameof(SensorLowBattery)}";
}
