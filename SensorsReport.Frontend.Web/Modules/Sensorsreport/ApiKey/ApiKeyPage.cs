namespace SensorsReport.Frontend.SensorsReport.ApiKey.Pages;

[PageAuthorize(typeof(ApiKeyRow))]
public class ApiKeyPage : Controller
{
    [Route("SensorsReport/ApiKey")]
    public ActionResult Index()
    {
        return this.GridPage<ApiKeyRow>(ESM.ApiKeyPage);
    }
}