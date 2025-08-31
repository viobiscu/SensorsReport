namespace SensorsReport.Frontend.SensorsReport.EmailTemplate.Pages;

[PageAuthorize(typeof(EmailTemplateRow))]
public class EmailTemplatePage : Controller
{
    [Route("SensorsReport/EmailTemplate")]
    public ActionResult Index()
    {
        return this.GridPage<EmailTemplateRow>(ESM.EmailTemplatePage);
    }
}