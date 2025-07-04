using Microsoft.Extensions.Options;
using SensorsReport;
using NLog;
using System.Reflection;

[assembly: AssemblyTitle("SensorsReport.Provision.API")]
[assembly: AssemblyDescription("API for provisioning sensors in Sensors Report")]

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
}
