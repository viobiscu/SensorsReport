using Microsoft.Extensions.Options;
using NLog;
using SensorsReport;
using SensorsReport.Extensions;
using SensorsReport.OrionLD.Extensions;
using SensorsReport.Webhook.API.Services;
using SensorsReport.Webhook.API.Tasks;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Webhook.API");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = SensorsReport.AppConfig.GetDefaultWebAppBuilder();

Configure(builder.Configuration, builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void Configure(IConfigurationManager configuration, IServiceCollection services)
{
    services.AddOrionLdServices(configuration);
    services.Configure<AppConfiguration>(configuration.GetSection("AppConfiguration"));
    logger.LogSection(configuration, "AppConfiguration");
    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppConfiguration>>().Value);
    services.AddSingleton<INotifyRuleQueueService, RabbitMQNotifyRuleService>();
    services.AddTenantServices();
    services.AddHostedService<AutoRegisterAlarmSubscriptions>();
}

public class AppConfiguration
{
    public string OrionContextBrokerUrl { get; set; } = "http://orion-ld-broker:1026";
    public string QuantumLeapHost { get; set; } = "quantum.sensorsreport.net:8668";
    public string? RabbitMQConnectionString { get; set; }
    public string RabbitMQExchange { get; set; } = "sensorsreport.exchange.webhook";
}

