using NLog;
using SensorsReport;
using SensorsReport.Email.API;
using SensorsReport.Email.API.Tasks;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Email.API");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = SensorsReport.AppConfig.GetDefaultWebAppBuilder();

ConfigureServices(builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IEmailRepository, EmailRepository>();
    services.AddHostedService<ReconciliationEmailTask>();
}

[ConfigName(SectionName)]
public class EmailMongoDbConnectionOptions : MongoDbConnectionOptions
{
    public new const string SectionName = nameof(EmailMongoDbConnectionOptions);
}
