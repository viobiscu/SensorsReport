using Microsoft.Extensions.Options;
using NLog;
using Sensors_Report_Email.API;
using Sensors_Report_Email.API.Tasks;
using SensorsReport;
using System.Reflection;

[assembly: AssemblyTitle("SensorsReport.Email.API")]
[assembly: AssemblyDescription("API for Email in Sensors Report")]

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Email.API");
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
    services.AddSingleton<IEmailRepository, EmailRepository>();
    services.AddSingleton<IQueueService, RabbitMQService>();
    services.AddHostedService<ReconciliationEmailTask>();
}

public class AppConfiguration
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? EmailCollectionName { get; set; }
    public string? RabbitMQConnectionString { get; set; }
    public string RabbitMQExchange { get; set; } = "sensorsreport.exchange.notification.email";
    public string RabbitMQRoutingKey { get; set; } = "sensorsreport.routingkey.notification.email";
}