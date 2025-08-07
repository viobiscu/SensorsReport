using NLog;
using SensorsReport;
using SensorsReport.OrionLD.Extensions;
using SensorsReport.Webhook.API.Tasks;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Webhook.API");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);

Configure(builder.Configuration, builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

static void Configure(IConfigurationManager configuration, IServiceCollection services)
{
    services.AddOrionLdServices(configuration);
    services.AddHostedService<AutoRegisterAlarmSubscriptions>();
}

