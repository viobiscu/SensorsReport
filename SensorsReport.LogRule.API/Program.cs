using NLog;
using SensorsReport;
using SensorsReport.LogRule.API;
using SensorsReport.OrionLD.Extensions;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.LogRule.API");
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
}


