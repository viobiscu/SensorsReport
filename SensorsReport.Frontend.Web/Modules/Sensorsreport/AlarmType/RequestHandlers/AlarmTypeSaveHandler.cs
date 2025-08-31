using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.AlarmType.AlarmTypeRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.AlarmType.AlarmTypeRow;

namespace SensorsReport.Frontend.SensorsReport.AlarmType;
public interface IAlarmTypeSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class AlarmTypeSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IAlarmTypeSaveHandler
{
}