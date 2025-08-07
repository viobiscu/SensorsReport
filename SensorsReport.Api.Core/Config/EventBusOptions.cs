namespace SensorsReport;

[ConfigName(SectionName)]
public class EventBusOptions
{
    public const string SectionName = nameof(EventBusOptions);

    public string Host { get; set; } = "localhost";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public ushort Port { get; set; } = 5672;
    public bool UseSSL { get; set; } = false;

    // Retry settings
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryIntervalSeconds { get; set; } = 5;

    // Circuit breaker settings
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationMinutes { get; set; } = 1;
}

