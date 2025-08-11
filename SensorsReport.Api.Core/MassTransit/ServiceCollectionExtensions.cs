using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace SensorsReport.Api.Core.MassTransit;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services,
            Action<IRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(configure =>
        {
            configureConsumers?.Invoke(configure);
            configure.DisableUsageTelemetry();

            configure.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<EventBusOptions>>().Value;

                cfg.UseNewtonsoftJsonSerializer();

                cfg.Host(options.Host, options.Port, options.VirtualHost, Assembly.GetExecutingAssembly()?.GetName().Name, config =>
                {
                    config.Username(options.Username);
                    config.Password(options.Password);

                    if (options.UseSSL)
                        config.UseSsl();
                });

                cfg.UseMessageRetry(r =>
                {
                    r.Intervals(
                        TimeSpan.FromSeconds(options.RetryIntervalSeconds),
                        TimeSpan.FromSeconds(options.RetryIntervalSeconds * 2),
                        TimeSpan.FromSeconds(options.RetryIntervalSeconds * 4)
                    );

                    r.Ignore<ArgumentNullException>();
                });

                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TripThreshold = options.CircuitBreakerThreshold;
                    cb.ActiveThreshold = options.CircuitBreakerThreshold - 1;
                    cb.TrackingPeriod = TimeSpan.FromMinutes(options.CircuitBreakerDurationMinutes);
                });

                cfg.UseRateLimit(1000, TimeSpan.FromSeconds(1));

                cfg.ConfigureEndpoints(context);

                cfg.UseInMemoryOutbox(context, x =>
                {
                    x.ConcurrentMessageDelivery = true;
                });
            });
        });


        services.AddScoped<IEventBus, EventBusService>();

        return services;
    }
}
