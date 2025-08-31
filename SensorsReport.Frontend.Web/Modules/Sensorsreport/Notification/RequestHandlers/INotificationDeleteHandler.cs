using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Notification.NotificationRow;


namespace SensorsReport.Frontend.SensorsReport.Notification;
public interface INotificationDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationDeleteHandler
{
    
}