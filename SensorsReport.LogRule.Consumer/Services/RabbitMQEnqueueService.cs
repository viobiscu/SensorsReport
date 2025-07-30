using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SensorsReport.Api;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorsReport.LogRule.Consumer;

public class RabbitMQEnqueueService : IEnqueueService
{
    private readonly RabbitMQTargetQueueConfig _targetQueueConfig;
    private readonly RabbitMQQueueConfig _queueConfig;
    private readonly ILogger<RabbitMQEnqueueService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQEnqueueService(IOptions<RabbitMQTargetQueueConfig> targetConfig, IOptions<RabbitMQQueueConfig> queueConfig, ILogger<RabbitMQEnqueueService> logger)
    {
        _targetQueueConfig = targetConfig.Value ?? throw new ArgumentNullException(nameof(targetConfig));
        _queueConfig = queueConfig.Value ?? throw new ArgumentNullException(nameof(queueConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrEmpty(_queueConfig.RabbitMQConnectionString))
            throw new InvalidOperationException("RabbitMQ connection string is not configured.");
        if (string.IsNullOrEmpty(_targetQueueConfig.AlarmExchange))
            throw new InvalidOperationException("RabbitMQ Alarm exchange is not configured.");
        if (string.IsNullOrEmpty(_targetQueueConfig.NotificationExchange))
            throw new InvalidOperationException("RabbitMQ Notification exchange is not configured.");

        InitializeRabbitMQ(_queueConfig.RabbitMQConnectionString, _targetQueueConfig.AlarmExchange);
        InitializeRabbitMQ(_queueConfig.RabbitMQConnectionString, _targetQueueConfig.NotificationExchange);

    }

    private void InitializeRabbitMQ(string connectionString, string exchange)
    {
        try
        {
            var factory = new ConnectionFactory();

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("RabbitMQ connection string is not configured.");

            if (string.IsNullOrEmpty(exchange))
                throw new InvalidOperationException("RabbitMQ exchange is not configured.");

            factory.Uri = new Uri(connectionString);

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            RabbitMqHelpers.InitializeExchange(_channel, exchange!, ExchangeType.Direct);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public Task EnqueueNotificationAsync(SubscriptionEventModel model)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");

        try
        {

            var message = JsonSerializer.Serialize(model);
            _logger.LogInformation("Enqueuing notification for tenant: {Tenant}", model.Tenant!.Tenant);
            _logger.LogDebug("Notification content: {Message}", message);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            _channel.BasicPublish(
                exchange: _targetQueueConfig.NotificationExchange,
                routingKey: _targetQueueConfig.NotificationRoutingKey,
                basicProperties: properties,
                body: body);
            _logger.LogInformation("Notification enqueued successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue notification");
            throw;
        }
        return Task.CompletedTask;
    }

    public Task EnqueueAlarmAsync(SubscriptionEventModel model)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");

        try
        {
            var message = JsonSerializer.Serialize(model);
            _logger.LogInformation("Enqueuing alarm check for tenant: {Tenant}", model.Tenant!.Tenant);
            _logger.LogDebug("Notification content: {Message}", message);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            _channel.BasicPublish(
                exchange: _targetQueueConfig.AlarmExchange,
                routingKey: _targetQueueConfig.AlarmRoutingKey,
                basicProperties: properties,
                body: body);
            _logger.LogInformation("Alarm check enqueued successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue alarm check");
            throw;
        }
        return Task.CompletedTask;
    }
}