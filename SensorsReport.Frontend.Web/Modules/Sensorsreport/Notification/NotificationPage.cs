namespace SensorsReport.Frontend.SensorsReport.Notification.Pages;

[PageAuthorize(typeof(NotificationRow))]
public class NotificationPage : Controller
{
    [Route("SensorsReport/Notification")]
    public ActionResult Index()
    {
        return this.GridPage<NotificationRow>(ESM.NotificationPage);
    }
}