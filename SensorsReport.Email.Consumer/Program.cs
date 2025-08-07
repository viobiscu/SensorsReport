using NLog;
using SensorsReport;
using SensorsReport.Email.Consumer;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Email.Consumer");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = SensorsReport.AppConfig.GetDefaultWebAppBuilder();

ConfigureServices(builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();


void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IEmailRepository, EmailRepository>();
    services.AddSingleton<IEmailService, EmailService>();
}

[ConfigName(SectionName)]
public class EmailMongoDbConnectionOptions : MongoDbConnectionOptions
{
    public new const string SectionName = nameof(EmailMongoDbConnectionOptions);
}
