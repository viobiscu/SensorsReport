using k8s;
using k8s.Models;
using Microsoft.Extensions.Caching.Memory;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Repositories;
using MMLib.SwaggerForOcelot.ServiceDiscovery;
using Ocelot.Configuration;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class KubeConfigurationRepository : IFileConfigurationRepository
{
    private readonly ILogger<KubeConfigurationRepository> _logger;
    private readonly IMemoryCache _cache;
    private readonly IKubernetes _kubernetesClient;
    private const string OcelotConfigCacheKey = "OcelotConfiguration";

    public KubeConfigurationRepository(
        ILogger<KubeConfigurationRepository> logger,
        IMemoryCache cache,
        IKubernetes kubernetesClient)
    {
        _logger = logger;
        _cache = cache;
        _kubernetesClient = kubernetesClient;
    }

    public async Task<Response<FileConfiguration>> Get()
    {
        if (_cache.TryGetValue(OcelotConfigCacheKey, out FileConfiguration? cachedConfig))
        {
            _logger.LogInformation("Configuration found in cache.");
            if (cachedConfig != null)
            {
                return new OkResponse<FileConfiguration>(cachedConfig);
            }
        }

        _logger.LogInformation("Configuration not found in cache. Fetching from Kubernetes API...");

        try
        {
            V1ServiceList services = await _kubernetesClient.CoreV1.ListNamespacedServiceAsync("default");

            var routes = new List<FileRoute>();

            foreach (V1Service service in services.Items)
            {
                var serviceName = service.Metadata.Name;
                if (string.IsNullOrEmpty(serviceName))
                {
                    _logger.LogWarning("Service name is null or empty. Skipping this service.");
                    continue;
                }

                if (!serviceName.StartsWith("sensors-report-", StringComparison.OrdinalIgnoreCase) ||
                    !serviceName.Contains("-api", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Service '{serviceName}' does not match naming convention. Skipping.");
                    continue;
                }

                var upstreamPathTemplate = serviceName.Replace("sensors-report-", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("-api", string.Empty, StringComparison.OrdinalIgnoreCase);

                upstreamPathTemplate = upstreamPathTemplate.StartsWith("/") ? upstreamPathTemplate : $"/{upstreamPathTemplate}";
                upstreamPathTemplate = $"{upstreamPathTemplate}/{{everything}}";

                _logger.LogInformation($"Processing service '{serviceName}' with upstream path template '{upstreamPathTemplate}'.");


                var route = new FileRoute
                {
                    UpstreamPathTemplate = upstreamPathTemplate,
                    DownstreamPathTemplate = "/{everything}",
                    DownstreamScheme = "http",
                    ServiceName = service.Metadata.Name,
                    LoadBalancerOptions = new FileLoadBalancerOptions
                    {
                        Type = "LeastConnection"
                    }
                };
                routes.Add(route);
            }

            var fileConfig = new FileConfiguration
            {
                Routes = routes,
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Type = "Kubernetes"
                    }
                }
            };

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

            _cache.Set(OcelotConfigCacheKey, fileConfig, cacheEntryOptions);

            _logger.LogInformation($"Successfully generated configuration with {routes.Count} routes from Kubernetes.");
            return new OkResponse<FileConfiguration>(fileConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get configuration from Kubernetes API.");
            return new OkResponse<FileConfiguration>(new FileConfiguration());
        }
    }

    public Task<Response> Set(FileConfiguration fileConfiguration)
    {
        return Task.FromResult<Response>(new OkResponse());
    }
}

public class SwaggerEndPointKubernetesProvider : ISwaggerEndPointProvider
{
    private readonly ILogger<KubeConfigurationRepository> _logger;
    private readonly IMemoryCache _cache;
    private readonly IKubernetes _kubernetesClient;
    private const string SwaggerEndPointConfigCacheKey = "SwaggerEndPointConfiguration";

    public SwaggerEndPointKubernetesProvider(
        ILogger<KubeConfigurationRepository> logger,
        IMemoryCache cache,
        IKubernetes kubernetesClient)
    {
        _logger = logger;
        _cache = cache;
        _kubernetesClient = kubernetesClient;
    }

    public IReadOnlyList<SwaggerEndPointOptions> GetAll()
    {
        var cachedConfig = _cache.Get<List<SwaggerEndPointOptions>>(SwaggerEndPointConfigCacheKey);
        if (cachedConfig != null)
        {
            _logger.LogInformation("Swagger endpoint configuration found in cache.");
            return cachedConfig;
        }
        cachedConfig = new List<SwaggerEndPointOptions>();

        _logger.LogInformation("Swagger endpoint configuration not found in cache. Fetching from Kubernetes API...");

        try
        {
            V1ServiceList services = _kubernetesClient.CoreV1.ListNamespacedService("default");

            var routes = new List<FileRoute>();

            foreach (V1Service service in services.Items)
            {
                var serviceName = service.Metadata.Name;
                if (string.IsNullOrEmpty(serviceName))
                {
                    _logger.LogWarning("Service name is null or empty. Skipping this service.");
                    continue;
                }

                if (!serviceName.StartsWith("sensors-report-", StringComparison.OrdinalIgnoreCase) ||
                    !serviceName.Contains("-api", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Service '{serviceName}' does not match naming convention. Skipping.");
                    continue;
                }


                var url = $"http://{serviceName}.default.svc.cluster.local/swagger/v1/swagger.json";
                var config = new SwaggerEndPointOptions
                {
                    Key = serviceName,
                    Config = [
                        new SwaggerEndPointConfig
                        {
                            Name = serviceName,
                            Url = url,
                            Version = "v1",
                            Service = new SwaggerService()
                            {
                                Name = serviceName,
                                Path = "/swagger/v1/swagger.json",
                            }
                        }
                    ]
                };

                cachedConfig.Add(config);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Swagger endpoint configuration from Kubernetes API.");
            return new List<SwaggerEndPointOptions>();
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
        _cache.Set(SwaggerEndPointConfigCacheKey, cachedConfig, cacheEntryOptions);
        _logger.LogInformation($"Successfully generated Swagger endpoint configuration with {cachedConfig.Count} endpoints from Kubernetes.");
        return cachedConfig;
    }

    public SwaggerEndPointOptions GetByKey(string key)
    {
        return this.GetAll().FirstOrDefault(x =>
            x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) ??
            throw new KeyNotFoundException($"Swagger endpoint configuration for key '{key}' not found.");
    }
}

public class SwaggerServiceDiscoveryKubernetesProvider : ISwaggerServiceDiscoveryProvider
{
    public Task<Uri> GetSwaggerUriAsync(SwaggerEndPointConfig endPoint, MMLib.SwaggerForOcelot.Configuration.RouteOptions route)
    {
        if (endPoint == null || string.IsNullOrEmpty(endPoint.Name))
        {
            throw new ArgumentException("Invalid endpoint configuration.");
        }

        var serviceUri = $"http://{endPoint.Url}/swagger/v1/swagger.json";

        return Task.FromResult(new Uri(serviceUri));
    }
}