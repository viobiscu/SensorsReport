using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Sensor.SensorRow;


namespace SensorsReport.Frontend.SensorsReport.Sensor;
public interface ISensorDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class SensorDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISensorDeleteHandler
{
    
}