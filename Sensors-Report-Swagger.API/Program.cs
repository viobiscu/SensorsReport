using k8s;
using KubeClient;
using MMLib.SwaggerForOcelot.Repositories;
using MMLib.SwaggerForOcelot.ServiceDiscovery;
using NLog;
using Ocelot.Configuration.Repository;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Kubernetes;
using SensorsReport;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

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

var swagger = app.Services.GetService<ISwaggerEndPointProvider>();
var all = swagger?.GetAll();

logger.Info("Swagger Endpoints: {SwaggerEndpoints}", JsonSerializer.Serialize(swagger?.GetAll()) ?? "None");
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("./swagger", permanent: true);
        return;
    }
    await next.Invoke();
});

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.ConfigureApp(async c =>
{
    await c.UseOcelot();
}, apiVersion: null!);

app.Run();

void ConfigureConfigs(IConfigurationManager configuration, IServiceCollection services)
{
}

void ConfigureServices(IServiceCollection services)
{
    if (builder.Environment.IsDevelopment())
    {
        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("Kubernetes config file not found.", configPath);
        }

        var kubeConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(configPath);
        services.AddSingleton<IKubernetes>(new Kubernetes(kubeConfig));
    }
    else
    {
        services.AddKubeClient(usePodServiceAccount: true);
    }

    services.AddMemoryCache();
    services.AddOcelot(builder.Configuration).AddKubernetes();
    services.AddSwaggerForOcelot(builder.Configuration);
    services.AddEndpointsApiExplorer();
    services.AddTransient<IFileConfigurationRepository, KubeConfigurationRepository>();
    services.AddTransient<ISwaggerEndPointProvider, SwaggerEndPointKubernetesProvider>();
    services.AddTransient<ISwaggerServiceDiscoveryProvider, SwaggerServiceDiscoveryKubernetesProvider>();
}
