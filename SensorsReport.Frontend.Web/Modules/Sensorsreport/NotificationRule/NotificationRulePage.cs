namespace SensorsReport.Frontend.SensorsReport.NotificationRule.Pages;

[PageAuthorize(typeof(NotificationRuleRow))]
public class NotificationRulePage : Controller
{
    [Route("SensorsReport/NotificationRule")]
    public ActionResult Index()
    {
        return this.GridPage<NotificationRuleRow>(ESM.NotificationRulePage);
    }
}