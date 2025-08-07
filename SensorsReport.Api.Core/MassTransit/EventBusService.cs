using MassTransit;
using MassTransit.Transports;

namespace SensorsReport.Api.Core.MassTransit;

internal class EventBusService(IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider) : IEventBus
{
    public IPublishEndpoint PublishEndpoint { get; } = publishEndpoint;
    public ISendEndpointProvider SendEndpointProvider { get; } = sendEndpointProvider;

    public async Task PublishAsync<T>(T eventMessage, CancellationToken cancellationToken = default) where T : class
    {
        await PublishEndpoint.Publish(eventMessage, cancellationToken);
    }

    public async Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class
    {
        await SendEndpointProvider.Send(command, cancellationToken);
    }
}
