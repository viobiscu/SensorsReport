using k8s;
using System.Text.Json;
using NLog;
using k8s.ClientSets;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Configuration;
using Ocelot.Values;

public class KubernetesConfigurationSource : IConfigurationSource
{
    private readonly IKubernetes _kubernetesClient;
    private readonly Logger _logger;

    public KubernetesConfigurationSource(IKubernetes kubernetesClient, Logger logger)
    {
        _kubernetesClient = kubernetesClient;
        _logger = logger;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new KubernetesOcelotConfigurationProvider(_kubernetesClient, _logger);
    }
}

public class KubernetesOcelotConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly IKubernetes _kubernetesClient;
    private readonly Logger _logger;
    private Timer? _refreshTimer;

    public KubernetesOcelotConfigurationProvider(IKubernetes kubernetesClient, Logger logger)
    {
        _kubernetesClient = kubernetesClient;
        _logger = logger;
    }

    public override void Load()
    {
        try
        {
            var config = GenerateOcelotConfig().Result;
            var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.Info($"Kubernetes configuration loaded successfully from Kubernetes: {configJson}");
            ParseConfigJson(configJson);

            _refreshTimer = new Timer(RefreshConfig, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load Kubernetes configuration");
        }
    }

    private void RefreshConfig(object? state)
    {
        try
        {
            var oldData = new Dictionary<string, string?>(Data);
            Load();

            if (!Data.SequenceEqual(oldData))
            {
                OnReload();
                _logger.Info("Kubernetes configuration reloaded");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to refresh Kubernetes configuration");
        }
    }

    private void ParseConfigJson(string json)
    {
        Data.Clear();
        var jsonDocument = JsonDocument.Parse(json);

        void ParseElement(JsonElement element, string prefix = "")
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
                        ParseElement(property.Value, key);
                    }
                    break;
                case JsonValueKind.Array:
                    for (int i = 0; i < element.GetArrayLength(); i++)
                    {
                        ParseElement(element[i], $"{prefix}:{i}");
                    }
                    break;
                default:
                    Data[prefix] = element.ToString();
                    break;
            }
        }

        ParseElement(jsonDocument.RootElement);
    }

    private async Task<object> GenerateOcelotConfig()
    {
        var services = await _kubernetesClient.CoreV1.ListNamespacedServiceAsync("default");

        var serviceList = services.Items
            .Where(s =>
                s.Metadata.Name.StartsWith("sensors-report-") &&
                s.Metadata.Name.Contains("-api") &&
                !s.Metadata.Name.Contains("swagger"))
            .ToList();


        var routes = serviceList.Select(s => new
        {
            DownstreamPathTemplate = "/api/{everything}",
            DownstreamScheme = "http",
            DownstreamHostAndPorts = new[]
            {
                new
                {
                    Host = $"{s.Metadata.Name}.default.svc.cluster.local",
                    Port = 80
                }
            },
            UpstreamPathTemplate = $"/gateway/{GetUpstreamServicePath(s.Metadata.Name)}/{{everything}}",
            UpstreamHttpMethod = new[] { "Get", "Post", "Put", "Delete", "Patch" },
            SwaggerKey = s.Metadata.Name
        })
        .ToArray();


        var swaggerEndPoints = serviceList.Select(s => new
        {
            Key = s.Metadata.Name,
            Config = new[]
                {
                    new
                    {
                        Name    = GetServiceDisplayName(s.Metadata.Name),
                        Version = GetVersion(s.Metadata.Name),
                        Url     = $"http://{s.Metadata.Name}.default.svc.cluster.local/swagger/v1/swagger.json"
                    }
                }
        })
            .ToArray();

        return new
        {
            Routes = routes,
            SwaggerEndPoints = swaggerEndPoints,
            GlobalConfiguration = new
            {
                ServiceDiscoveryProvider = new
                {
                    Type = "KubernetesApiDiscoveryProvider",
                    PollingInterval = 1000
                }
            }
        };
    }


    private string GetVersion(string serviceName)
    {
        var version = serviceName.Split('-').LastOrDefault();
        if (string.IsNullOrEmpty(version) || version.Equals("api", StringComparison.OrdinalIgnoreCase))
            version = "v1";
        return version;
    }

    private string GetUpstreamServicePath(string serviceName)
    {
        var parts = serviceName.Split('-');
        if (parts.Length >= 3)
            return parts[2];

        return serviceName.Replace("sensors-report-", "");
    }

    private string GetServiceDisplayName(string serviceName)
    {
        var serviceType = GetUpstreamServicePath(serviceName);
        return $"{serviceType.ToUpper()} Service API";
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer?.Dispose();
            _refreshTimer = null;
        }
    }
}

public class KubernetesApiDiscoveryProvider : IServiceDiscoveryProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceProviderConfiguration _config;
    private readonly DownstreamRoute _downstreamRoute;

    public KubernetesApiDiscoveryProvider(IServiceProvider serviceProvider, ServiceProviderConfiguration config, DownstreamRoute downstreamRoute)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _downstreamRoute = downstreamRoute;
    }

    public async Task<List<Service>> GetAsync()
    {
        var services = new List<Service>();
        var kubernetesClient = _serviceProvider.GetRequiredService<IKubernetes>();

        var foundedServices = await kubernetesClient.CoreV1.ListNamespacedServiceAsync("default");
        
        var serviceList = foundedServices.Items
            .Where(s =>
                s.Metadata.Name.StartsWith("sensors-report-") &&
                s.Metadata.Name.Contains("-api") &&
                !s.Metadata.Name.Contains("swagger"))
            .ToList();

        foreach (var service in serviceList)
        {
            services.Add(new Service(service.Metadata.Name, new ServiceHostAndPort(
                $"{service.Metadata.Name}.default.svc.cluster.local",
                80
            ), service.Metadata.Name, GetVersion(service.Metadata.Name), [
                "api",
                "v1",
                service.Metadata.Name
            ]));
        }

        return services;
    }
    
    private string GetVersion(string serviceName)
    {
        var version = serviceName.Split('-').LastOrDefault();
        if (string.IsNullOrEmpty(version) || version.Equals("api", StringComparison.OrdinalIgnoreCase))
            version = "v1";
        return version;
    }
}