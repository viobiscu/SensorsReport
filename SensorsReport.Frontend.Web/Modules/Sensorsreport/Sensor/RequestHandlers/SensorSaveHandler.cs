using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.Sensor.SensorRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Sensor.SensorRow;

namespace SensorsReport.Frontend.SensorsReport.Sensor;
public interface ISensorSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class SensorSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISensorSaveHandler
{
}