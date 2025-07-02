using Microsoft.Extensions.Options;
using SensorsReport;
using Sensors_Report_Provision.API.Services;
using NLog;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Provision.API");;
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args, [
    "OrionContextBrokerUrl",
    "MainTenant"
]);

var builder = AppConfig.GetDefaultWebAppBuilder();

ConfigureConfigs(builder.Configuration, builder.Services);
ConfigureServices(builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void ConfigureConfigs(IConfigurationManager configuration, IServiceCollection services)
{
    services.Configure<AppConfiguration>(configuration.GetSection("AppConfig"));
}

void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient("OrionContextBroker", (serviceProvider, client) =>
    {
        var appConfig = serviceProvider.GetRequiredService<IOptions<AppConfiguration>>().Value;
        client.BaseAddress = new Uri(appConfig.OrionContextBrokerUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        MaxConnectionsPerServer = 10
    });

    services.AddTransient<IOrionContextBrokerService, OrionContextBrokerService>();
}

public class AppConfiguration
{
    public string OrionContextBrokerUrl { get; set; } = "http://orion-ld-broker:1026";
    public string MainTenant { get; set; } = "synchro";
}