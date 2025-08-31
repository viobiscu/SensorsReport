namespace SensorsReport.Frontend.SensorsReport.Group.Forms;

[FormScript("SensorsReport.GroupForm")]
[BasedOnRow(typeof(GroupRow), CheckNames = true)]
public class GroupForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<string>? Users { get; set; }
}