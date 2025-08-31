namespace SensorsReport.Frontend.SensorsReport.LogRule.Pages;

[PageAuthorize(typeof(LogRuleRow))]
public class LogRulePage : Controller
{
    [Route("SensorsReport/LogRule")]
    public ActionResult Index()
    {
        return this.GridPage<LogRuleRow>(ESM.LogRulePage);
    }
}