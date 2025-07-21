using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace SensorsReport.OrionLD.Extensions;

public static class OrionLdBrokerServiceCollectionExtensions
{
    public static IServiceCollection AddOrionLdServices(this IServiceCollection services, IConfigurationManager configuration, string configName = nameof(OrionLdConfig))
    {
        services.Configure<OrionLdConfig>(configuration.GetSection(configName));
        services.AddTransient<IOrionLdService, OrionLdService>();

        services.AddHttpClient(nameof(OrionLdService), (serviceProvider, client) =>
        {
            var appConfig = serviceProvider.GetRequiredService<IOptions<OrionLdConfig>>().Value;
            client.BaseAddress = new Uri(appConfig.BrokerUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = 10
        });

        return services;
    }
}
