namespace SensorsReport;

[ConfigName(DefaultConfigName)]
public class QuantumLeapConfig
{
    public const string DefaultConfigName = nameof(QuantumLeapConfig);
    public virtual string QuantumLeapHost { get; set; } = "quantum.sensorsreport.net:8668";
    public virtual string MainTenant { get; set; } = "synchro";
}

