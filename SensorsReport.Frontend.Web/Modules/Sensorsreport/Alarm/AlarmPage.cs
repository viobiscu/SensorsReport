namespace SensorsReport.Frontend.SensorsReport.Alarm.Pages;

[PageAuthorize(typeof(AlarmRow))]
public class AlarmPage : Controller
{
    [Route("SensorsReport/Alarm")]
    public ActionResult Index()
    {
        return this.GridPage<AlarmRow>(ESM.AlarmPage);
    }
}