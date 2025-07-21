using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SensorsReport.Api;
using System.Text;
using System.Text.Json;

namespace SensorsReport.Email.API;

public class RabbitMQEmailService : IEmailQueueService, IDisposable
{
    private readonly AppConfiguration _config;
    private readonly ILogger<RabbitMQEmailService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQEmailService(IOptions<AppConfiguration> config, ILogger<RabbitMQEmailService> logger)
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

            RabbitMqHelpers.InitializeExchange(_channel, _config.RabbitMQExchange);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public async Task<bool> QueueEmailAsync(EmailModel email)
    {
        try
        {
            if (_channel == null || _connection == null || !_connection.IsOpen)
            {
                _logger.LogWarning("RabbitMQ connection is not available, attempting to reconnect");
                InitializeRabbitMQ();
            }

            var messageBody = JsonSerializer.Serialize(new
            {
                email.Id
            });

            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _config.RabbitMQExchange,
                routingKey: _config.RabbitMQRoutingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Email queued successfully for {Recipient} with subject {Subject}", email.ToEmail, email.Subject);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue email for {Recipient} with subject {Subject}", email.ToEmail, email.Subject);
            return await Task.FromResult(false);
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
