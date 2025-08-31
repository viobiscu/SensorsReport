namespace SensorsReport.Frontend.SensorsReport.NotificationRule.Forms;

[FormScript("SensorsReport.NotificationRuleForm")]
[BasedOnRow(typeof(NotificationRuleRow), CheckNames = true)]
public class NotificationRuleForm
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool? Enable { get; set; }
    public int? ConsecutiveHits { get; set; }
    public int? RepeatAfter { get; set; }
    public bool? NotifyIfClose { get; set; }
    public bool? NotifyIfAcknowledged { get; set; }
    public int? RepeatIfAcknowledged { get; set; }
    public int? NotifyIfTimeOut { get; set; }
    public List<string>? NotificationChannel { get; set; }
}