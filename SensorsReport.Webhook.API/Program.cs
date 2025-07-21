using Microsoft.Extensions.Options;
using NLog;
using SensorsReport;
using SensorsReport.Extensions;
using SensorsReport.Webhook.API.Services;
using System.Reflection;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Webhook.API");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = SensorsReport.AppConfig.GetDefaultWebAppBuilder();

ConfigureConfigs(builder.Configuration, builder.Services);
ConfigureServices(builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void ConfigureConfigs(IConfigurationManager configuration, IServiceCollection services)
{
    services.Configure<AppConfiguration>(configuration.GetSection("AppConfiguration"));
    logger.LogSection(configuration, "AppConfiguration");
}

void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppConfiguration>>().Value);
    services.AddSingleton<INotifyRuleQueueService, RabbitMQNotifyRuleService>();
    services.AddTenantServices();
}

public class AppConfiguration
{
    public string OrionContextBrokerUrl { get; set; } = "http://orion-ld-broker:1026";
    public string QuantumLeapHost { get; set; } = "quantum.sensorsreport.net:8668";
    public string? RabbitMQConnectionString { get; set; }
    public string RabbitMQExchange { get; set; } = "sensorsreport.exchange.webhook";
}

