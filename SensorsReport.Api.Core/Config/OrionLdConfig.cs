namespace SensorsReport;

[ConfigName(DefaultConfigName)]
public class OrionLdConfig
{
    public const string DefaultConfigName = nameof(OrionLdConfig);
    public virtual string BrokerUrl { get; set; } = "http://orion-ld-broker:1026";
    public virtual string MainTenant { get; set; } = "synchro";
}