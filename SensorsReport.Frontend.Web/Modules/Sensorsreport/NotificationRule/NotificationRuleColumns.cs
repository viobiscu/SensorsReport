namespace SensorsReport.Frontend.SensorsReport.NotificationRule.Forms;

[ColumnsScript("SensorsReport.NotificationRule")]
[BasedOnRow(typeof(NotificationRuleRow), CheckNames = true)]
public class NotificationRuleColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }

    [EditLink, Width(300), SortOrder(1)]
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