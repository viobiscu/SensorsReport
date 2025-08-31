namespace SensorsReport.Frontend.SensorsReport.Notification.Forms;

[ColumnsScript("SensorsReport.Notification")]
[BasedOnRow(typeof(NotificationRow), CheckNames = true)]
public class NotificationColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }

    [EditLink, Width(300), SortOrder(1)]
    public string? Name { get; set; }
    public bool? Enable { get; set; }
    public string? NotificationRule { get; set; }
    public string? NotificationUser { get; set; }
    public List<RelationModel<string>>? SMS { get; set; }
    public List<RelationModel<string>>? Email { get; set; }
    public List<RelationModel<string>>? Monitors { get; set; }

}