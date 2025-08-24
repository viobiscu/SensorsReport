using SensorsReport.OrionLD;

namespace SensorsReport.Frontend.Modules.Common.OrionLDHandlers;

public class OrionLDRetrieveHandler<TRow, TRetrieveRequest, TRetrieveResponse>(IHttpContextAccessor httpContextAccessor) :
    RetrieveRequestHandler<TRow, TRetrieveRequest, TRetrieveResponse>(httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IRequestContext>())
    where TRow : class, IRow, new()
    where TRetrieveRequest : RetrieveRequest
    where TRetrieveResponse : RetrieveResponse<TRow>, new()
{
    private readonly IOrionLdService orionLdService = httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IOrionLdService>();
    private readonly ITenantRetriever tenantRetriever = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ITenantRetriever>();
    protected List<string> columns = [];
    protected string Id = string.Empty;

    /// <inheritdoc/>
    protected override void SelectField(SqlQuery query, Field field)
    {
        this.columns.Add(field.Name);
    }

    /// <inheritdoc/>
    protected override SqlQuery CreateQuery()
    {
        var query = new SqlQuery();

        var idField = ((IIdRow)Row).IdField;
        var id = idField.ConvertValue(Request.EntityId, CultureInfo.InvariantCulture);
        this.Id = id.ToString()!;
        return query;
    }

    /// <inheritdoc/>
    protected override void ExecuteQuery()
    {
        try
        {
            orionLdService.SetTenant(tenantRetriever.CurrentTenantInfo.Tenant);
            orionLdService.SetOptions(OrionLDOptions.KeyValue);
            var entity = orionLdService.GetEntityByIdAsync<TRow>(Id).ConfigureAwait(false).GetAwaiter().GetResult();

            if (entity == null)
                throw DataValidation.EntityNotFoundError(Row, Request.EntityId, Localizer);

            foreach (var field in Row.Fields)
            {
                if (!columns.Contains(field.Name) && field.Name != ((IIdRow)Row).IdField.Name)
                    field.AsObject(entity, null);
            }

            Response.Entity = entity;
        }
        catch (Exception exception)
        {
            foreach (var behavior in behaviors.Value.OfType<IRetrieveExceptionBehavior>())
                behavior.OnException(this, exception);

            throw;
        }
    }
}
