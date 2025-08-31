namespace SensorsReport.Frontend.SensorsReport.NotificationUsers.Forms;

[ColumnsScript("SensorsReport.NotificationUsers")]
[BasedOnRow(typeof(NotificationUsersRow), CheckNames = true)]
public class NotificationUsersColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }

    [EditLink, Width(300), SortOrder(1)]
    public string? Name { get; set; }
    public bool? Enable { get; set; }

}