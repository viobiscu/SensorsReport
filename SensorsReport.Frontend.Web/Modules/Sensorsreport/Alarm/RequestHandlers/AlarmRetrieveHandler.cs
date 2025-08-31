using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.Alarm.AlarmRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Alarm.AlarmRow;


namespace SensorsReport.Frontend.SensorsReport.Alarm;
public interface IAlarmRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmRetrieveHandler
{
}