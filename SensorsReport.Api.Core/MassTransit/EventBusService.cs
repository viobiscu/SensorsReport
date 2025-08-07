using MassTransit;

namespace SensorsReport.Api.Core.MassTransit;

internal class EventBusService(IPublishEndpoint publishEndpoint, ISendEndpoint sendEndpoint) : IEventBus
{
    public IPublishEndpoint PublishEndpoint { get; } = publishEndpoint;
    public ISendEndpoint SendEndpoint { get; } = sendEndpoint;

    public async Task PublishAsync<T>(T eventMessage, CancellationToken cancellationToken = default) where T : class
    {
        await PublishEndpoint.Publish(eventMessage, cancellationToken);
    }

    public async Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class
    {
        await SendEndpoint.Send(command, cancellationToken);
    }
}
