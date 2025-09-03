using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.SensorHistory.SensorHistoryRow;


namespace SensorsReport.Frontend.SensorsReport.SensorHistory;
public interface ISensorHistoryDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }

public class SensorHistoryDeleteHandler(IRequestContext context) :
    DeleteRequestHandler<MyRow, MyRequest, MyResponse>(context), ISensorHistoryDeleteHandler
{
}