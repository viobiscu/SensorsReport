using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.User.UserRow;


namespace SensorsReport.Frontend.SensorsReport.User;
public interface IUserDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class UserDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IUserDeleteHandler
{
}