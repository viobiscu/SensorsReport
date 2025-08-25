using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.User.UserRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.User.UserRow;


namespace SensorsReport.Frontend.SensorsReport.User;
public interface IUserListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class UserListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IUserListHandler
{
}