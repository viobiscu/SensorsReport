using MassTransit;
using MassTransit.Transports;

namespace SensorsReport.Api.Core.MassTransit;

public interface IEventBus
{
    public IPublishEndpoint PublishEndpoint { get; }
    public ISendEndpoint SendEndpoint { get; }
    Task PublishAsync<T>(T eventMessage, CancellationToken cancellationToken = default) where T : class;
}
