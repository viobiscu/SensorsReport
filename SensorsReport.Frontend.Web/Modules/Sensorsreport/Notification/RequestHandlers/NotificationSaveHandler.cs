using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.Notification.NotificationRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Notification.NotificationRow;

namespace SensorsReport.Frontend.SensorsReport.Notification;
public interface INotificationSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationSaveHandler
{
}