using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmType.AlarmTypeRow;


namespace SensorsReport.Frontend.SensorsReport.AlarmType;
public interface IAlarmTypeDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmTypeDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmTypeDeleteHandler
{
    
}