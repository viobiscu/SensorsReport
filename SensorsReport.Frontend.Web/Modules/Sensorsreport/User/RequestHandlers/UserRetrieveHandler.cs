using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.User.UserRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.User.UserRow;


namespace SensorsReport.Frontend.SensorsReport.User;
public interface IUserRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class UserRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IUserRetrieveHandler
{
}