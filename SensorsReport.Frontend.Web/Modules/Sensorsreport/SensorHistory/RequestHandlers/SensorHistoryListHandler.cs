using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow;


namespace SensorsReport.Frontend.SensorsReport.SensorHistory;
public interface ISensorHistoryListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class SensorHistoryListHandler(IRequestContext context) :
    ListRequestHandler<MyRow, MyRequest, MyResponse>(context), ISensorHistoryListHandler
{
}