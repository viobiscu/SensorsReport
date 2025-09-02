using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.Sensor.SensorRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Sensor.SensorRow;


namespace SensorsReport.Frontend.SensorsReport.Sensor;
public interface ISensorRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class SensorRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISensorRetrieveHandler
{
}