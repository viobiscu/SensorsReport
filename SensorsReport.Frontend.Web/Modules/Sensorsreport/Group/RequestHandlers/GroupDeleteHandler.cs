using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.DeleteRequest;
using MyResponse = Serenity.Services.DeleteResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Group.GroupRow;


namespace SensorsReport.Frontend.SensorsReport.Group;
public interface IGroupDeleteHandler : IDeleteHandler<MyRow, MyRequest, MyResponse> { }
public class GroupDeleteHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDDeleteHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IGroupDeleteHandler
{
    
}