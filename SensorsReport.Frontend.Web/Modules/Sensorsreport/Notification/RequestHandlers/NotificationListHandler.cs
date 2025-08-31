using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.Notification.NotificationRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Notification.NotificationRow;


namespace SensorsReport.Frontend.SensorsReport.Notification;
public interface INotificationListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class NotificationListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationListHandler
{
}