using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using MyRequest = Serenity.Services.SaveRequest<SensorsReport.Frontend.SensorsReport.Group.GroupRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SensorsReport.Frontend.SensorsReport.Group.GroupRow;

namespace SensorsReport.Frontend.SensorsReport.Group;
public interface IGroupSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class GroupSaveHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDSaveHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), IGroupSaveHandler
{
}