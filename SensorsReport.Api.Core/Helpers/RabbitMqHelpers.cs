using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorsReport.Api;

public static class RabbitMqHelpers
{
    public static string GetDlxExchangeName(string exchangeName)
    {
        return $"{exchangeName}.dlx";
    }
    public static string GetRetryQueueName(string queueName)
    {
        return $"{queueName}.retry";
    }

    public static Dictionary<string, object> GetRetryQueueArguments(string exchangeName, TimeSpan retryDelay)
    {
        return new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", exchangeName },
            { "x-message-ttl", (int)retryDelay.TotalMilliseconds }
        };
    }

    public static Dictionary<string, object> GetMainQueueArguments(string dlxExchangeName)
    {
        return new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", dlxExchangeName }
        };
    }

    public static void InitializeExchange(IModel channel, string exchangeName, string exchangeType = ExchangeType.Direct)
    {
        channel.ExchangeDeclare(
            exchange: exchangeName,
            type: exchangeType,
            durable: true);
    }

    public static void InitializeRabbitMQ(IModel channel, string exchangeName, string queueName, string routingKey, TimeSpan? retryDelay, string exchangeType = ExchangeType.Direct)
    {
        InitializeExchange(channel, exchangeName, exchangeType);

        IDictionary<string, object>? mainQueueArgs = null;

        if (retryDelay.HasValue && retryDelay.Value.TotalMilliseconds > 0)
        {
            var dlxExchangeName = $"{exchangeName}.dlx";
            var retryQueueName = $"{queueName}.retry";

            var retryQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", exchangeName },
                { "x-message-ttl", Convert.ToInt32(retryDelay.Value.TotalMilliseconds) }
            };

            channel.ExchangeDeclare(exchange: dlxExchangeName, type: exchangeType, durable: true);
            channel.QueueDeclare(queue: retryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: retryQueueArgs);
            channel.QueueBind(queue: retryQueueName, exchange: dlxExchangeName, routingKey: routingKey);

            mainQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", dlxExchangeName }
            };
        }

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainQueueArgs);

        channel.QueueBind(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey);
    }

}
