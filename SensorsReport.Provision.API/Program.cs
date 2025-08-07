using NLog;
using SensorsReport;
using SensorsReport.OrionLD.Extensions;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Provision.API");;
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = AppConfig.GetDefaultWebAppBuilder();

Configure(builder.Configuration, builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void Configure(IConfigurationManager configuration, IServiceCollection services)
{
    services.AddOrionLdServices(configuration);
    logger.LogSection(configuration, "AppConfig");
}

