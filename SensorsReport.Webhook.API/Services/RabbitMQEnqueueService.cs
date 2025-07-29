using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SensorsReport.Api;
using System.Text;
using System.Text.Json;

namespace SensorsReport.Webhook.API.Services;

public class RabbitMQEnqueueService : IEnqueueService
{
    private readonly RabbitMQExchangeConfig _config;
    private readonly ILogger<RabbitMQEnqueueService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQEnqueueService(IOptions<RabbitMQExchangeConfig> config, ILogger<RabbitMQEnqueueService> logger)
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

            if (string.IsNullOrEmpty(_config.RabbitMQExchange))
                throw new InvalidOperationException("RabbitMQ exchange is not configured.");

            factory.Uri = new Uri(_config.RabbitMQConnectionString);

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            RabbitMqHelpers.InitializeExchange(_channel, _config.RabbitMQExchange!, ExchangeType.Direct);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public Task EnqueueNotificationAsync(JsonElement notification, TenantInfo tenant, string subscriptionId)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");

        try
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();
            writer.WriteString("subscriptionId", subscriptionId);
            // Write tenant information
            writer.WritePropertyName("tenant");
            writer.WriteStartObject();
            writer.WriteString("tenant", tenant.Tenant);
            if (!string.IsNullOrEmpty(tenant.Path))
                writer.WriteString("path", tenant.Path);
            writer.WriteEndObject();

            writer.WritePropertyName("data");
            notification.WriteTo(writer);

            writer.WriteEndObject();
            writer.Flush();

            var message = Encoding.UTF8.GetString(stream.ToArray());
            _logger.LogInformation("Enqueuing notification for tenant: {Tenant}", tenant.Tenant);
            _logger.LogDebug("Notification content: {Message}", message);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            _channel.BasicPublish(
                exchange: _config.RabbitMQExchange,
                routingKey: string.Empty,
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
}