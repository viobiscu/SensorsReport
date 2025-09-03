using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow;


namespace SensorsReport.Frontend.SensorsReport.SensorHistory;
public interface ISensorHistoryRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class SensorHistoryRetrieveHandler(IRequestContext context) :
    RetrieveRequestHandler<MyRow, MyRequest, MyResponse>(context), ISensorHistoryRetrieveHandler
{
}