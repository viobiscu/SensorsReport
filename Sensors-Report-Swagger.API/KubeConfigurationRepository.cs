using k8s;
using k8s.Models;
using Microsoft.Extensions.Caching.Memory;
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