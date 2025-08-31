using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.SensorsReport.Group.GroupRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Group.GroupRow;


namespace SensorsReport.Frontend.SensorsReport.Group;
public interface IGroupRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class GroupRetrieveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDRetrieveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IGroupRetrieveHandler
{
}