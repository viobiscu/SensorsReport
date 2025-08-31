namespace SensorsReport.Frontend.SensorsReport.VariableTemplate.Pages;

[PageAuthorize(typeof(VariableTemplateRow))]
public class VariableTemplatePage : Controller
{
    [Route("SensorsReport/VariableTemplate")]
    public ActionResult Index()
    {
        return this.GridPage<VariableTemplateRow>(ESM.VariableTemplatePage);
    }
}