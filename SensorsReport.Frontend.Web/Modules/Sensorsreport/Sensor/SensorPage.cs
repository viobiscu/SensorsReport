namespace SensorsReport.Frontend.SensorsReport.Sensor.Pages;

[PageAuthorize(typeof(SensorRow))]
public class SensorPage : Controller
{
    [Route("SensorsReport/Sensor")]
    public ActionResult Index()
    {
        return this.GridPage<SensorRow>(ESM.SensorPage);
    }
}