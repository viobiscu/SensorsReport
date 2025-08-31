using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.Alarm.AlarmRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Alarm.AlarmRow;

namespace SensorsReport.Frontend.SensorsReport.Alarm;
public interface IAlarmSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmSaveHandler
{
}