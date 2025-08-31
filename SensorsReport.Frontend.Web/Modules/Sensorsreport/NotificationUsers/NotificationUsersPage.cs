namespace SensorsReport.Frontend.SensorsReport.NotificationUsers.Pages;

[PageAuthorize(typeof(NotificationUsersRow))]
public class NotificationUsersPage : Controller
{
    [Route("SensorsReport/NotificationUsers")]
    public ActionResult Index()
    {
        return this.GridPage<NotificationUsersRow>(ESM.NotificationUsersPage);
    }
}