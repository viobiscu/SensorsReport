namespace SensorsReport.Frontend.SensorsReport.SmsTemplate.Pages;

[PageAuthorize(typeof(SmsTemplateRow))]
public class SmsTemplatePage : Controller
{
    [Route("SensorsReport/SmsTemplate")]
    public ActionResult Index()
    {
        return this.GridPage<SmsTemplateRow>(ESM.SmsTemplatePage);
    }
}