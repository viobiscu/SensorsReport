using SensorsReport.OrionLD;
using DataValidation = Serenity.Services.DataValidation;

namespace SensorsReport.Frontend.Modules.Common.OrionLDHandlers;

public class OrionLDDeleteHandler<TRow, TDeleteRequest, TDeleteResponse>(IHttpContextAccessor httpContextAccessor) :
    DeleteRequestHandler<TRow, TDeleteRequest, TDeleteResponse>(httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IRequestContext>())
    where TRow : class, IRow, IIdRow, new()
    where TDeleteRequest : DeleteRequest
    where TDeleteResponse : DeleteResponse, new()
{
    private readonly IOrionLdService orionLdService = httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IOrionLdService>();
    private readonly ITenantRetriever tenantRetriever = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ITenantRetriever>();

    /// <inheritdoc/>
    protected override void LoadEntity()
    {
        orionLdService.SetTenant(tenantRetriever.CurrentTenantInfo);
        orionLdService.SetOptions(OrionLDOptions.KeyValue);

        Row = orionLdService.GetEntityByIdAsync<TRow>(Request.EntityId.ToString()!).ConfigureAwait(false).GetAwaiter().GetResult()!;
        if (Row == null)
        {
            var idField = new TRow().IdField;
            var id = Request.EntityId != null ?
                idField.ConvertValue(Request.EntityId, CultureInfo.InvariantCulture)
                : idField.AsObject(Row);

            throw DataValidation.EntityNotFoundError(Row, id, Localizer);
        }
    }


    /// <inheritdoc/>
    protected override void ExecuteDelete()
    {
        InvokeDeleteAction(() =>
        {
            orionLdService.SetTenant(tenantRetriever.CurrentTenantInfo);
            orionLdService.SetOptions(OrionLDOptions.KeyValue);

            var idField = Row.IdField;
            var id = idField.ConvertValue(Request.EntityId, CultureInfo.InvariantCulture)?.ToString();
            if (string.IsNullOrEmpty(id))
                throw DataValidation.EntityNotFoundError(Row, Request.EntityId, Localizer);

            var delete = orionLdService.DeleteEntityAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();
            if (delete.IsSuccessStatusCode == false && delete.StatusCode != System.Net.HttpStatusCode.NotFound)
                throw new Exception($"Error deleting entity {id} from OrionLD: {delete.ReasonPhrase}");
        });

        InvalidateCacheOnCommit();
    }
}
