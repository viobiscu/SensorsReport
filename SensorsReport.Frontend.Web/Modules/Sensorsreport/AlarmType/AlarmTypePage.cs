namespace SensorsReport.Frontend.SensorsReport.AlarmType.Pages;

[PageAuthorize(typeof(AlarmTypeRow))]
public class AlarmTypePage : Controller
{
    [Route("SensorsReport/AlarmType")]
    public ActionResult Index()
    {
        return this.GridPage<AlarmTypeRow>(ESM.AlarmTypePage);
    }
}