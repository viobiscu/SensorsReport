using DocumentFormat.OpenXml.Office2010.Excel;
using SensorsReport.OrionLD;
using Serenity.Services;

namespace SensorsReport.Frontend.Modules.Common.OrionLDHandlers;

public class OrionLDListHandler<TRow, TListRequest, TListResponse>(IHttpContextAccessor httpContextAccessor) :
    ListRequestHandler<TRow, TListRequest, TListResponse>(httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IRequestContext>())
    where TRow : class, IRow, new ()
    where TListRequest : ListRequest
    where TListResponse : ListResponse<TRow>, new ()
{
    protected readonly IOrionLdService orionLdService = httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IOrionLdService>();
    protected readonly ITenantRetriever tenantRetriever = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ITenantRetriever>();
    protected int skip = 0;
    protected int take = Int32.MaxValue;

    protected virtual TenantInfo GetTenantInfo()
    {
        return tenantRetriever.CurrentTenantInfo;
    }


    /// <inheritdoc/>
    protected override SqlQuery CreateQuery()
    {
        var query = new SqlQuery();

        this.skip = Request.Skip;
        this.take = Request.Take;

        return query;
    }

    /// <inheritdoc/>
    protected override void ExecuteQuery()
    {
        try
        {
            orionLdService.SetTenant(GetTenantInfo());
            orionLdService.SetOptions(OrionLDOptions.KeyValue);

            var entities = orionLdService.GetEntitiesAsync<List<TRow>>(0, 1000, Row.Table).ConfigureAwait(false).GetAwaiter().GetResult();

            if (entities?.Any() == true)
            {
                Response.TotalCount = entities.Count;

                entities = entities.Skip(skip).Take(take).ToList();

                foreach (var entity in entities)
                {
                    foreach (var field in Row.Fields)
                    {
                        if (Request.IncludeColumns != null && !Request.IncludeColumns.TryGetValue(field.Name, out _))
                            field.AsObject(entity, null);
                    }
                }

                Response.Entities.AddRange(entities);
            }
        }
        catch (Exception exception)
        {
            foreach (var behavior in behaviors.Value.OfType<IListExceptionBehavior>())
                behavior.OnException(this, exception);

            throw;
        }
    }
}
