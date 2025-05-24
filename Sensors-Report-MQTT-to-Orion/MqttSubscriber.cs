using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using OrionldClient;

public class MqttSubscriber : BackgroundService
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
    private IMqttClient? _mqttClient;
    private readonly string _mqttHost;
    private readonly int _mqttPort;
    private readonly string _mqttTopic;

    public MqttSubscriber(string? mqttHost, int mqttPort, string? mqttTopic)
    {
        _mqttHost = string.IsNullOrEmpty(mqttHost) ? "localhost" : mqttHost;
        _mqttPort = mqttPort <= 0 ? 1883 : mqttPort; // Default MQTT port is 1883
        _mqttTopic = string.IsNullOrEmpty(mqttTopic) ? "#" : mqttTopic; // # subscribes to all topics
        
        Logger.Info($"MQTT Subscriber initialized with Host: {_mqttHost}, Port: {_mqttPort}, Topic: {_mqttTopic}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttFactory = new MqttFactory();
        _mqttClient = mqttFactory.CreateMqttClient();
        var mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttHost, _mqttPort)
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            await ProcessMessage(e.ApplicationMessage.Topic, payload);
        };

        _mqttClient.ConnectedAsync += async e =>
        {
            Logger.Info("Connected to MQTT broker.");
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(_mqttTopic))
                .Build();
            await _mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);
            Logger.Info($"Subscribed to topic {_mqttTopic}");
        };

        _mqttClient.DisconnectedAsync += async e =>
        {
            Logger.Warn("Disconnected from MQTT broker.");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            try
            {
                await _mqttClient.ConnectAsync(mqttOptions, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reconnecting to MQTT broker: {ex.Message}");
            }
        };

        try
        {
            await _mqttClient.ConnectAsync(mqttOptions, stoppingToken);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error connecting to MQTT broker: {ex.Message}");
        }
    }

    private async Task ProcessMessage(string topic, string payload)
    {
        Logger.Trace($"Received topic: {topic}");
        Logger.Trace($"Received message: {payload}");
        OrionldSensorEntity sensorEntity = new OrionldSensorEntity();
        var result = await sensorEntity.TryGetEntityType(payload);
        if (result != "OK")
        {
            Logger.Warn($"Error processing message: {result}");
            return;
        }
        switch (sensorEntity.entityType)
        {
            case EntityType.TG8W:
            case EntityType.TG8I:
                TG8 tG8 = new TG8();
                await tG8.Consume(payload);
                await tG8.CommitToOrion();
                break;
            case EntityType.WiSensor:
                throw new NotImplementedException();
            case EntityType.BlueTooth:
                throw new NotImplementedException();
            case EntityType.TRS:
                throw new NotImplementedException();
            default:
                Logger.Error($"Unknown entity topic: {topic}");
                Logger.Error($"Unknown entity payload: {payload}");
                break;
        }
    }

    public override void Dispose()
    {
        _mqttClient?.Dispose();
        base.Dispose();
    }
}
