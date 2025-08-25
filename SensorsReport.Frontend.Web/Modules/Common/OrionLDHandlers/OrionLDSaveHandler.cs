using SensorsReport.OrionLD;

namespace SensorsReport.Frontend.Modules.Common.OrionLDHandlers;

public class OrionLDSaveHandler<TRow, TSaveRequest, TSaveResponse>(IHttpContextAccessor httpContextAccessor) :
    SaveRequestHandler<TRow, TSaveRequest, TSaveResponse>(httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IRequestContext>())
    where TRow : class, IRow, IIdRow, new()
    where TSaveResponse : SaveResponse, new()
    where TSaveRequest : SaveRequest<TRow>, new()
{

    private readonly IOrionLdService orionLdService = httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IOrionLdService>();
    private readonly ITenantRetriever tenantRetriever = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ITenantRetriever>();

    /// <inheritdoc/>
    protected override void LoadOldEntity()
    {
        orionLdService.SetTenant(tenantRetriever.CurrentTenantInfo);
        orionLdService.SetOptions(OrionLDOptions.KeyValue);

        Old = orionLdService.GetEntityByIdAsync<TRow>(Request.EntityId.ToString()!).ConfigureAwait(false).GetAwaiter().GetResult()!;
        if (Old == null)
        {
            var idField = Row.IdField;
            var id = Request.EntityId != null ?
                idField.ConvertValue(Request.EntityId, CultureInfo.InvariantCulture)
                : idField.AsObject(Row);

            throw DataValidation.EntityNotFoundError(Row, id, Localizer);
        }
    }

    /// <inheritdoc/>
    protected override void InvokeSaveAction(Action action)
    {
        orionLdService.SetTenant(tenantRetriever.CurrentTenantInfo);
        orionLdService.SetOptions(OrionLDOptions.KeyValue);
        base.InvokeSaveAction(action);
    }

    /// <inheritdoc/>
    protected override void ExecuteSave()
    {
        if (IsUpdate)
        {
            if (Row.IsAnyFieldAssigned)
            {
                var idField = Row.IdField;

                if (idField.IndexCompare(Old, Row) != 0)
                {

                    InvokeSaveAction(() => orionLdService.UpdateEntityAsync(idField.AsObject(Old).ToString()!, Row));
                }
                else
                {
                    InvokeSaveAction(() => orionLdService.UpdateEntityAsync(idField.AsObject(Row).ToString()!, Row));
                }

                Response.EntityId = idField.AsObject(Row);
                InvalidateCacheOnCommit();
            }
        }
        else if (IsCreate)
        {
            var idField = Row.IdField;
            if (idField is not null &&
                idField.Flags.HasFlag(FieldFlags.AutoIncrement))
            {
                InvokeSaveAction(() =>
                {
                    orionLdService.CreateEntityAsync(Row).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (idField is not null)
                    {
                        var entityId = idField.AsObject(Row);
                        Response.EntityId = entityId;
                        Row.IdField.AsInvariant(Row, entityId);
                    }
                });
            }
            else
            {
                InvokeSaveAction(() =>
                {
                    orionLdService.CreateEntityAsync(Row).ConfigureAwait(false).GetAwaiter().GetResult();
                });

                if (idField is not null)
                    Response.EntityId = idField.AsObject(Row);
            }

            InvalidateCacheOnCommit();
        }
    }
}
