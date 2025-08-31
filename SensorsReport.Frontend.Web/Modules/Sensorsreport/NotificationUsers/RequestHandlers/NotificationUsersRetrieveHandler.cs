using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.NotificationUsers.NotificationUsersRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.NotificationUsers.NotificationUsersRow;


namespace SensorsReport.Frontend.SensorsReport.NotificationUsers;
public interface INotificationUsersRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class NotificationUsersRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), INotificationUsersRetrieveHandler
{
}