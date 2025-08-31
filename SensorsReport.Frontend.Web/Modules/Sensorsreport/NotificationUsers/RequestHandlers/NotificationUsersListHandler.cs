using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.NotificationUsers.NotificationUsersRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationUsers.NotificationUsersRow;


namespace SensorsReport.Frontend.SensorsReport.NotificationUsers;
public interface INotificationUsersListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class NotificationUsersListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationUsersListHandler
{
}