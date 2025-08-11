using NLog;
using SensorsReport;
using SensorsReport.AlarmRule.Consumer;
using SensorsReport.NotificationRule.Consumer;
using SensorsReport.OrionLD.Extensions;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.NotificationRule.Consumer");
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = SensorsReport.AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);

builder.Services.AddOrionLdServices(builder.Configuration);
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddHostedService<CheckNotificationsBackgroundService>();

var app = builder.Build();
app.ConfigureAppAndRun();


[ConfigName(SectionName)]
public class NotificationMongoDbConnectionOptions : MongoDbConnectionOptions
{
    public new const string SectionName = nameof(NotificationMongoDbConnectionOptions);
}
