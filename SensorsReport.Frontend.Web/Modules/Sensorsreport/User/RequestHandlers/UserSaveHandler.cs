using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.User.UserRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.User.UserRow;

namespace SensorsReport.Frontend.SensorsReport.User;
public interface IUserSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class UserSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IUserSaveHandler
{
}