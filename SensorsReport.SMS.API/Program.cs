using NLog;
using SensorsReport;
using SensorsReport.Extensions;
using SensorsReport.SMS.API.Repositories;
using SensorsReport.SMS.API.Tasks;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.SMS.API");;
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);

ConfigureServices(builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();


void ConfigureServices(IServiceCollection services)
{
    services.AddTenantServices();
    services.AddScoped<ISmsRepository, SmsRepository>();
    services.AddScoped<IProviderRepository, ProviderRepository>();
    services.AddHostedService<UpdateSmsStatusBackgroundService>();
}

[ConfigName(SectionName)]
public class SmsMongoDbConnectionOptions : MongoDbConnectionOptions
{
    public new const string SectionName = nameof(SmsMongoDbConnectionOptions);
}

[ConfigName(SectionName)]
public class ProviderMongoDbConnectionOptions : MongoDbConnectionOptions
{
    public new const string SectionName = nameof(ProviderMongoDbConnectionOptions);
}


[ConfigName(SectionName)]
public class SmsOptions
{
    public const string SectionName = "SmsOptions";
    public int ProviderTrustTimeoutInSecond { get; set; } = 60 * 30; // Default to 30 minutes
    public int MaxRetryCount { get; set; } = 3; // Default to 3 retries
    public int DefaultTtlInMinutes { get; set; } = 30; // Default TTL for SMS messages
}