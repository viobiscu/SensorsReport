using Microsoft.Extensions.Options;
using NLog;
using SensorsReport.Email.Consumer;
using SensorsReport;
using System.Reflection;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Email.Consumer");
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
    services.AddSingleton<IEmailService, EmailService>();
    services.AddHostedService<EmailConsumerService>();
}

public class AppConfiguration
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? EmailCollectionName { get; set; }
    public string? RabbitMQConnectionString { get; set; }
    public string RabbitMQExchange { get; set; } = "sensorsreport.exchange.notification.email";
    public string RabbitMQQueue { get; set; } = "sensorsreport.queue.notification.email";
    public string RabbitMQRoutingKey { get; set; } = "sensorsreport.routingkey.notification.email";
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;
}