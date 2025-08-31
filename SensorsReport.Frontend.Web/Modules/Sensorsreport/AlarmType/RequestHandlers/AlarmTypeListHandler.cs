using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.AlarmType.AlarmTypeRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmType.AlarmTypeRow;


namespace SensorsReport.Frontend.SensorsReport.AlarmType;
public interface IAlarmTypeListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class AlarmTypeListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmTypeListHandler
{
}