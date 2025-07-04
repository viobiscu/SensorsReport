using Microsoft.Extensions.Options;
using SensorsReport;
using NLog;
using System.Reflection;
using SensorsReport.Api.Core.Services.Tenants;
using Sensors_Report_SMS.API.Repositories;
using Sensors_Report_SMS.API.Tasks;

[assembly: AssemblyTitle("SensorsReport.SMS.API")]
[assembly: AssemblyDescription("API for SMS in Sensors Report")]

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.SMS.API");;
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);

ConfigureConfigs(builder.Configuration, builder.Services);
ConfigureServices(builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void ConfigureConfigs(IConfigurationManager configuration, IServiceCollection services)
{
    services.Configure<AppConfiguration>(configuration.GetSection("AppConfig"));
    logger.LogSection(configuration, "AppConfig");
}

void ConfigureServices(IServiceCollection services)
{
    services.AddTenantServices();
    services.AddScoped<ISmsRepository, SmsRepository>();
    services.AddScoped<IProviderRepository, ProviderRepository>();
    services.AddHostedService<UpdateSmsStatusBackgroundService>();
}

public class AppConfiguration
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? SmsCollectionName { get; set; }
    public string? ProviderCollectionName { get; set; }
    public int ProviderTrustTimeoutInSecond { get; set; } = 60 * 30; // Default to 30 minutes
    public int MaxRetryCount { get; set; } = 3; // Default to 3 retries
    public int DefaultTtlInMinutes { get; set; } = 30; // Default TTL for SMS messages
}