using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationUsers.NotificationUsersRow;


namespace SensorsReport.Frontend.SensorsReport.NotificationUsers;
public interface INotificationUsersDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationUsersDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationUsersDeleteHandler
{
    
}