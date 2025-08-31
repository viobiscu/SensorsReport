namespace SensorsReport.Frontend.SensorsReport.NotificationUsers.Forms;

[FormScript("SensorsReport.NotificationUsersForm")]
[BasedOnRow(typeof(NotificationUsersRow), CheckNames = true)]
public class NotificationUsersForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool? Enable { get; set; }
    public List<RelationModel<string>>? Users { get; set; }
    public List<RelationModel<string>>? Groups { get; set; }
    public List<string>? Notification { get; set; }
}