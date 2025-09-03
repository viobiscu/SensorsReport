using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow;


namespace SensorsReport.Frontend.SensorsReport.SensorHistory;
public interface ISensorHistoryRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class SensorHistoryRetrieveHandler(IRequestContext context, ITenantRetriever tenantRetriever) :
    RetrieveRequestHandler<MyRow, MyRequest, MyResponse>(context), ISensorHistoryRetrieveHandler
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