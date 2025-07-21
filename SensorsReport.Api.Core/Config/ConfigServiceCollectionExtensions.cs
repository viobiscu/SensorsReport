using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SensorsReport;

public static class ConfigServiceCollectionExtensions
{
    public static IServiceCollection AddConfig<T>(this IServiceCollection services, IConfigurationManager configuration) where T: class
    {
        var configName = typeof(T).GetCustomAttribute<ConfigNameAttribute>()?.Name ?? typeof(T).Name;

        services.Configure<T>(configuration.GetSection(configName));

        return services;
    }

    public static IServiceCollection AutoConfig(this IServiceCollection services, IConfigurationManager configuration)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var configTypes = assembly.GetTypes().Where(s => s.GetCustomAttribute<ConfigNameAttribute>() != null);
            foreach (var type in configTypes)
            {
                var configNameAttribute = type.GetCustomAttribute<ConfigNameAttribute>();
                if (configNameAttribute != null)
                {
                    var configSection = configuration.GetSection(configNameAttribute.Name);

                    RegisterOptionsConfiguration(services, type, configSection);
                }
            }
        }

        return services;
    }

    private static void RegisterOptionsConfiguration(IServiceCollection services, Type configurationType, IConfigurationSection configSection)
    {
        var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.Name == "Configure" && m.IsGenericMethodDefinition)
            .Where(m => m.GetParameters().Length == 2 &&
                       m.GetParameters()[0].ParameterType == typeof(IServiceCollection) &&
                       m.GetParameters()[1].ParameterType == typeof(IConfiguration))
            .FirstOrDefault();

        if (configureMethod != null)
        {
            var genericMethod = configureMethod.MakeGenericMethod(configurationType);

            genericMethod.Invoke(null, [services, configSection]);
        }
    }
}

