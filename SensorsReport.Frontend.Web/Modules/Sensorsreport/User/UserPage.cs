namespace SensorsReport.Frontend.SensorsReport.User.Pages;

[PageAuthorize(typeof(UserRow))]
public class UserPage : Controller
{
    [Route("SensorsReport/User")]
    public ActionResult Index()
    {
        return this.GridPage<UserRow>(ESM.Modules.Sensorsreport.User.UserPage);
    }
}