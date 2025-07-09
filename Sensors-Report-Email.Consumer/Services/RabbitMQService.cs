using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SensorsReport.Api;

namespace Sensors_Report_Email.Consumer;

public class RabbitMQService : IQueueService, IDisposable
{
    private readonly AppConfiguration _config;
    private readonly ILogger<RabbitMQService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQService(IOptions<AppConfiguration> config, ILogger<RabbitMQService> logger)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory();

            if (string.IsNullOrEmpty(_config.RabbitMQConnectionString))
                throw new InvalidOperationException("RabbitMQ connection string is not configured.");


            factory.Uri = new Uri(_config.RabbitMQConnectionString);

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            RabbitMqHelpers.InitializeRabbitMQ(_channel, _config.RabbitMQExchange, _config.RabbitMQQueue, _config.RabbitMQRoutingKey, _config.RetryDelay);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }


    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    public Task StartConsumingAsync(Func<string, ulong, Task> onMessageReceived, CancellationToken cancellationToken)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var deliveryTag = ea.DeliveryTag;

            await onMessageReceived(message, deliveryTag);
        };

        _channel.BasicConsume(queue: _config.RabbitMQQueue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public void AcknowledgeMessage(ulong deliveryTag)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");
        _channel.BasicAck(deliveryTag, multiple: false);
        _logger.LogInformation("Message with DeliveryTag {DeliveryTag} acknowledged.", deliveryTag);
    }

    public void RejectMessage(ulong deliveryTag, bool requeue = false)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");
        _channel.BasicNack(deliveryTag, multiple: false, requeue: requeue);
        _logger.LogInformation("Message with DeliveryTag {DeliveryTag} rejected. Requeue: {Requeue}", deliveryTag, requeue);
    }
}
