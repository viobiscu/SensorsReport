namespace SensorsReport.Frontend.SensorsReport.SensorHistory.Pages;

[PageAuthorize(typeof(SensorHistoryRow))]
public class SensorHistoryPage : Controller
{
    [Route("SensorsReport/SensorHistory")]
    public ActionResult Index()
    {
        return this.GridPage<SensorHistoryRow>(ESM.SensorHistoryPage);
    }
}