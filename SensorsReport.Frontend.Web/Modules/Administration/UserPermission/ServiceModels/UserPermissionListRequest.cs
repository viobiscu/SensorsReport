namespace SensorsReport.Frontend.Administration;

public class UserPermissionListRequest : ServiceRequest
{
    public int? UserID { get; set; }
    public bool RolePermissions { get; set; }
}