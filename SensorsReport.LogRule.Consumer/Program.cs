using NLog;
using SensorsReport;
using SensorsReport.LogRule.Consumer;
using SensorsReport.OrionLD.Extensions;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.LogRule.Consumer");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = SensorsReport.AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);

Configure(builder.Configuration, builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void Configure(IConfigurationManager configuration, IServiceCollection services)
{
    services.AddOrionLdServices(configuration);
    services.AddTransient<ILogRuleService, LogRuleService>();
    services.AddSingleton<IQueueService, RabbitMQService>();
    services.AddSingleton<IEnqueueService, RabbitMQEnqueueService>();
    services.AddHostedService<LogRuleConsumerService>();
}


[ConfigName(DefaultConfigName)]
public class RabbitMQTargetQueueConfig
{
    public const string DefaultConfigName = nameof(RabbitMQTargetQueueConfig);

    public string? AlarmExchange { get; set; }
    public string? AlarmRoutingKey { get; set; }
    public string? NotificationExchange { get; set; }
    public string? NotificationRoutingKey { get; set; }
}
