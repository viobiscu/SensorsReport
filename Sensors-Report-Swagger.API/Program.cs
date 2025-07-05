using KubeClient;
using NLog;
using Ocelot.Configuration.Repository;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Kubernetes;
using SensorsReport;
using System.Reflection;

[assembly: AssemblyTitle("SensorsReport.Swagger.API")]
[assembly: AssemblyDescription("Swagger in Sensors Report")]

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Swagger.API"); ;
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);

ConfigureConfigs(builder.Configuration, builder.Services);
ConfigureServices(builder.Services);

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("./swagger/index.html", permanent: true);
        return;
    }
    await next.Invoke();
});

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.ConfigureAppAndRun(async c =>
{
    await c.UseOcelot();
});

void ConfigureConfigs(IConfigurationManager configuration, IServiceCollection services)
{
}

void ConfigureServices(IServiceCollection services)
{
    services.AddKubeClient(usePodServiceAccount: true);
    services.AddMemoryCache();
    services.AddSingleton<IFileConfigurationRepository, KubeConfigurationRepository>();
    services.AddOcelot(builder.Configuration).AddKubernetes();
    services.AddSwaggerForOcelot(builder.Configuration);
    services.AddEndpointsApiExplorer();
}
