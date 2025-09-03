using MassTransit;
using SensorsReport.Frontend.SensorsReport.SensorHistory;

namespace SensorsReport.Frontend.Modules.Common.Consumers;
public class SensorDataHistoryLogConsumer(ILogger<SensorDataHistoryLogConsumer> logger,
    IServiceResolver<ISensorHistorySaveHandler> handlerResolver,
    IUserAccessor userAccessor,
    IUserClaimCreator userClaimCreator,
    IPermissionService permissionService,
    ISqlConnections sqlConnections) : IConsumer<SensorDataHistoryLogEvent>
{
    private readonly ILogger<SensorDataHistoryLogConsumer> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceResolver<ISensorHistorySaveHandler> handlerResolver = handlerResolver ?? throw new ArgumentNullException(nameof(handlerResolver));
    private readonly IUserAccessor userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
    private readonly IUserClaimCreator userClaimCreator = userClaimCreator ?? throw new ArgumentNullException(nameof(userClaimCreator));
    private readonly IPermissionService permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
    private readonly ISqlConnections sqlConnections = sqlConnections ?? throw new ArgumentNullException(nameof(sqlConnections));

    public async Task Consume(ConsumeContext<SensorDataHistoryLogEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var sensorDataHistoryLogEvent = context.Message;
        ArgumentNullException.ThrowIfNull(sensorDataHistoryLogEvent, nameof(sensorDataHistoryLogEvent));

        logger.LogInformation("Received SensorDataHistoryLogEvent: {SensorDataHistoryLogEvent}", sensorDataHistoryLogEvent);

        var impersonator = userAccessor as IImpersonator;
        var transientGrantor = permissionService as ITransientGrantor;

        var principal = userClaimCreator.CreatePrincipal("admin", "HistoryLogImpersonation");

        using var conn = sqlConnections.NewFor<SensorHistoryRow>();
        using var uow = new UnitOfWork(conn);

        var saveHandler = handlerResolver.Resolve();

        impersonator?.Impersonate(principal);
        transientGrantor?.GrantAll();
        try
        {
            var createResponse = saveHandler.Create(uow, new SaveRequest<SensorHistoryRow>
            {
                Entity = new SensorHistoryRow
                {
                    SensorId = sensorDataHistoryLogEvent.SensorId,
                    PropertyKey = sensorDataHistoryLogEvent.PropertyKey,
                    MetadataKey = sensorDataHistoryLogEvent.MetadataKey,
                    ObservedAt = sensorDataHistoryLogEvent.ObservedAt?.UtcDateTime,
                    Value = sensorDataHistoryLogEvent.Value,
                    Unit = sensorDataHistoryLogEvent.Unit,
                    Tenant = sensorDataHistoryLogEvent.Tenant?.Tenant
                }
            });

            uow.Commit();
            logger.LogInformation("Sensor history record created with ID: {Id}", createResponse.EntityId);
        }
        finally
        {

            transientGrantor?.UndoGrant();
            impersonator?.UndoImpersonate();
        }

        await Task.CompletedTask;
    }
}
