using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorsReport.OrionLD;
using System;

namespace SensorsReport.Extensions;

public static class QuantumLeapServiceCollectionExtensions
{
    public static IServiceCollection AddQuantumLeapService(this IServiceCollection services, IConfigurationManager configuration, string configName = nameof(QuantumLeapOptions))
    {
        services.Configure<QuantumLeapOptions>(configuration.GetSection(configName));

        return services;
    }
}

