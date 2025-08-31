using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.NotificationUsers.NotificationUsersRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationUsers.NotificationUsersRow;

namespace SensorsReport.Frontend.SensorsReport.NotificationUsers;
public interface INotificationUsersSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationUsersSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationUsersSaveHandler
{
}