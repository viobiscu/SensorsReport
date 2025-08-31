using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.Alarm.AlarmRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Alarm.AlarmRow;


namespace SensorsReport.Frontend.SensorsReport.Alarm;
public interface IAlarmListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class AlarmListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmListHandler
{
}