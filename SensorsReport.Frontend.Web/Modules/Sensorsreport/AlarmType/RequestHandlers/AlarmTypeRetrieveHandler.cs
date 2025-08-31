using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.AlarmType.AlarmTypeRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmType.AlarmTypeRow;


namespace SensorsReport.Frontend.SensorsReport.AlarmType;
public interface IAlarmTypeRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmTypeRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmTypeRetrieveHandler
{
}