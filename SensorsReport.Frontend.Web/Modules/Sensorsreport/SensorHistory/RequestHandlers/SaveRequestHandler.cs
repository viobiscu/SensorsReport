using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow;

namespace SensorsReport.Frontend.SensorsReport.SensorHistory;
public interface ISensorHistorySaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class SensorHistorySaveHandler(IRequestContext context) :
    SaveRequestHandler<MyRow, MyRequest, MyResponse>(context), ISensorHistorySaveHandler
{
}