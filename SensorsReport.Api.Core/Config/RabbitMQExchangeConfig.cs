namespace SensorsReport;

[ConfigName(DefaultConfigName)]
public class RabbitMQExchangeConfig
{
    public const string DefaultConfigName = nameof(RabbitMQExchangeConfig);

    public string? RabbitMQConnectionString { get; set; }
    public string? RabbitMQExchange { get; set; }
    public string? RabbitMQRoutingKey { get; set; }
}
