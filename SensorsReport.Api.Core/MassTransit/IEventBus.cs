using MassTransit;

namespace SensorsReport.Api.Core.MassTransit;

public interface IEventBus
{
    IPublishEndpoint PublishEndpoint { get; }
    ISendEndpointProvider SendEndpointProvider { get; }
    Task PublishAsync<T>(T eventMessage, CancellationToken cancellationToken = default) where T : class;
}
