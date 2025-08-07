namespace SensorsReport;

[ConfigName(SectionName)]
public class QuantumLeapOptions
{
    public const string SectionName = nameof(QuantumLeapOptions);
    [Obsolete("Use host instead")]
    public virtual string QuantumLeapHost { get; set; } = "quantum.sensorsreport.net:8668";
    public virtual string Host { get; set; } = "quantum.sensorsreport.net:8668";
    public virtual string MainTenant { get; set; } = "synchro";
}

