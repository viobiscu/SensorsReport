using Microsoft.Extensions.Options;
using SensorsReport;
using Sensors_Report_Provision.API.Services;

var logger = SensorsReport.AppConfig.GetLogger();
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = SensorsReport.AppConfig.GetDefaultWebAppBuilder();

ConfigureConfigs(builder.Configuration, builder.Services);
ConfigureServices(builder.Services);

var app = builder.Build();
app.ConfigureAppAndRun();

void ConfigureConfigs(IConfigurationManager configuration, IServiceCollection services)
{
    services.Configure<AppConfig>(configuration.GetSection("AppConfig"));
}

void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient("OrionContextBroker", (serviceProvider, client) =>
    {
        var appConfig = serviceProvider.GetRequiredService<IOptions<AppConfig>>().Value;
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

public class AppConfig
{
    public string OrionContextBrokerUrl { get; set; } = "http://orion-ld-broker:1026";
    public string MainTenant { get; set; } = "synchro";
}