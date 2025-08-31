namespace SensorsReport.Frontend.SensorsReport.AlarmRule.Pages;

[PageAuthorize(typeof(AlarmRuleRow))]
public class AlarmRulePage : Controller
{
    [Route("SensorsReport/AlarmRule")]
    public ActionResult Index()
    {
        return this.GridPage<AlarmRuleRow>(ESM.AlarmRulePage);
    }
}