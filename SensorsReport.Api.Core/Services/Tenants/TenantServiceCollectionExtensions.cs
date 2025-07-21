using System;
using Microsoft.Extensions.DependencyInjection;

namespace SensorsReport.Extensions;

public static class TenantServiceCollectionExtensions
{
    public static IServiceCollection AddTenantServices(this IServiceCollection services)
    {
        services.AddTransient<ITenantRetriever, TenantRetriever>();
        return services;
    }
}
