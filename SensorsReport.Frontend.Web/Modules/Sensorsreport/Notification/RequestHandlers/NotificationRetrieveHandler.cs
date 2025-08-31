using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.Notification.NotificationRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Notification.NotificationRow;


namespace SensorsReport.Frontend.SensorsReport.Notification;
public interface INotificationRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationRetrieveHandler
{
}