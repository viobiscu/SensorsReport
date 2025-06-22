using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;
using OrionldClient;

public class MqttBackgroundService : BackgroundService
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly MqttSubscriber _mqttSubscriber;

    public MqttBackgroundService()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        var mqttHost = ConfigProgram.MqttHost;
        var mqttPort = ushort.Parse(ConfigProgram.MqttPort);
        var mqttTopic = ConfigProgram.MqttTopic;
#pragma warning restore CS8604 // Possible null reference argument.
        // Initialize the MqttSubscriber with your MQTT broker details
        _mqttSubscriber = new MqttSubscriber(mqttHost, mqttPort, mqttTopic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.Info("MqttBackgroundService is starting.");

        stoppingToken.Register(() => Logger.Info("MqttBackgroundService is stopping."));

        try
        {
            await _mqttSubscriber.StartAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in MqttBackgroundService: {ex.Message}");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        Logger.Info("MqttBackgroundService has stopped.");
    }

    public override void Dispose()
    {
        _mqttSubscriber.Dispose();
        base.Dispose();
    }
}
