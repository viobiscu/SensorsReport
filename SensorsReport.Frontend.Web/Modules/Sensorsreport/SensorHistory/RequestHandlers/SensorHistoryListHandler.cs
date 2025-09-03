using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow;


namespace SensorsReport.Frontend.SensorsReport.SensorHistory;
public interface ISensorHistoryListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class SensorHistoryListHandler(IRequestContext context, ITenantRetriever tenantRetriever) :
    ListRequestHandler<MyRow, MyRequest, MyResponse>(context), ISensorHistoryListHandler
{
    private readonly ITenantRetriever tenantRetriever = tenantRetriever;

    protected override void PrepareQuery(SqlQuery query)
    {
        base.PrepareQuery(query);
        if (!Permissions.HasPermission(Administration.PermissionKeys.Security) && tenantRetriever != null)
        {
            var tenant = tenantRetriever.CurrentTenantInfo.Tenant;
            if (tenant != null)
                query.Where(MyRow.Fields.Tenant == tenant);
        }
    }
}