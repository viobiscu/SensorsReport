namespace SensorsReport.Frontend.SensorsReport.User.Forms;

[ColumnsScript("SensorsReport.User")]
[BasedOnRow(typeof(UserRow), CheckNames = true)]
public class UserColumns
{
    [EditLink, DisplayName("Id"), AlignRight]
    public string? Id { get; set; }
    [EditLink]
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Mobile { get; set; }
    public string? Language { get; set; }
}