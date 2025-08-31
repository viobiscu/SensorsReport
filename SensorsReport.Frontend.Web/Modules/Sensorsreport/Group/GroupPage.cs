namespace SensorsReport.Frontend.SensorsReport.Group.Pages;

[PageAuthorize(typeof(GroupRow))]
public class GroupPage : Controller
{
    [Route("SensorsReport/Group")]
    public ActionResult Index()
    {
        return this.GridPage<GroupRow>(ESM.GroupPage);
    }
}