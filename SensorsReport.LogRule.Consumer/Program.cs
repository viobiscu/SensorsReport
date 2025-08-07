using NLog;
using SensorsReport;
using SensorsReport.OrionLD.Extensions;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.LogRule.Consumer");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);

builder.Services.AddOrionLdServices(builder.Configuration);

var app = builder.Build();
app.ConfigureAppAndRun();
