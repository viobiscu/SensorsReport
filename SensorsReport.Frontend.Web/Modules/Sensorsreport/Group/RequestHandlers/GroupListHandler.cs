using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.Group.GroupRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Group.GroupRow;


namespace SensorsReport.Frontend.SensorsReport.Group;
public interface IGroupListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class GroupListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IGroupListHandler
{
}