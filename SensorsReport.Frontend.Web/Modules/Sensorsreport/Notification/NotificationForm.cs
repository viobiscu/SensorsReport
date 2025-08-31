namespace SensorsReport.Frontend.SensorsReport.Notification.Forms;

[FormScript("SensorsReport.NotificationForm")]
[BasedOnRow(typeof(NotificationRow), CheckNames = true)]
public class NotificationForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool? Enable { get; set; }
    public string? NotificationRule { get; set; }
    public string? NotificationUser { get; set; }
    public List<RelationModel<string>>? SMS { get; set; }
    public List<RelationModel<string>>? Email { get; set; }
    public List<RelationModel<string>>? Monitors { get; set; }
}