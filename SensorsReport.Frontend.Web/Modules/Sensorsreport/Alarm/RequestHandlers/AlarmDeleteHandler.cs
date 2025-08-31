using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Alarm.AlarmRow;


namespace SensorsReport.Frontend.SensorsReport.Alarm;
public interface IAlarmDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmDeleteHandler
{
    
}