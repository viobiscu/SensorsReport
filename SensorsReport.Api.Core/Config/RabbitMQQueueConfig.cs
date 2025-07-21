namespace SensorsReport;

[ConfigName(DefaultConfigName)]
public class RabbitMQQueueConfig : RabbitMQExchangeConfig
{
    public new const string DefaultConfigName = nameof(RabbitMQQueueConfig);

    public string? RabbitMQQueue { get; set; }
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}