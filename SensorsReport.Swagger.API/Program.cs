using k8s;
using MMLib.SwaggerForOcelot.DependencyInjection;
using NLog;
using NLog.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Kubernetes;
using Ocelot.ServiceDiscovery;
using SensorsReport;
using System.Reflection;

LogManager.Setup((config) => config.ConfigureLogger());
var logger = LogManager.GetLogger("SensorsReport.Swagger.API"); ;
logger.Info("Application starting...");
logger.LogProgramInfo(AppDomain.CurrentDomain, args);

var builder = AppConfig.GetDefaultWebAppBuilder(useTenantHeader: true);
builder.Services.AddLogging().AddNLog();
builder.Services
    .AddSingleton<ServiceDiscoveryFinderDelegate>((serviceProvider, config, downstreamRoute)
        => new KubernetesApiDiscoveryProvider(serviceProvider, config, downstreamRoute));
var kubernetesClient = GetKubernetesClient();
((IConfigurationManager)builder.Configuration).Add(new KubernetesConfigurationSource(kubernetesClient, logger));
builder.Services.AddSingleton(kubernetesClient);
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);


var app = builder.Build();

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.UseOcelot().Wait();
app.Run();

IKubernetes GetKubernetesClient()
{
    IKubernetes client;
    if (builder.Environment.IsDevelopment())
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        client = new Kubernetes(config);
    }
    else
    {
        var inClusterConfig = KubernetesClientConfiguration.InClusterConfig();
        client = new Kubernetes(inClusterConfig);
    }
    return client;
}

