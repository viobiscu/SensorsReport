namespace SensorsReport.Frontend.SensorsReport.Group.Forms;

[ColumnsScript("SensorsReport.Group")]
[BasedOnRow(typeof(GroupRow), CheckNames = true)]
public class GroupColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }

    [EditLink, Width(300), SortOrder(1)]
    public string? Name { get; set; }
    public List<string>? Users { get; set; }
}