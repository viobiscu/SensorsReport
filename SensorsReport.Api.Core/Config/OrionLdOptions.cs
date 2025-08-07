namespace SensorsReport;

[ConfigName(SectionName)]
public class OrionLdOptions
{
    public const string SectionName = nameof(OrionLdOptions);
    public virtual string BrokerUrl { get; set; } = "http://orion-ld-broker:1026";
    public virtual string MainTenant { get; set; } = "synchro";
}
