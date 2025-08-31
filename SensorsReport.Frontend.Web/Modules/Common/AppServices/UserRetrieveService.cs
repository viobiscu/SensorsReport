using MyRow = SensorsReport.Frontend.Administration.UserRow;

namespace SensorsReport.Frontend.AppServices;
public class UserRetrieveService(ITwoLevelCache cache, ISqlConnections sqlConnections, IHttpContextAccessor httpContextAccessor)
    : BaseUserRetrieveService<MyRow>(cache, sqlConnections)
{
    protected override UserDefinition ToUserDefinition(MyRow user)
    {
        var tenant = httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(s => s.Type == "organization")?.Value;
        return new()
        {
            UserId = user.UserId.Value,
            Username = user.Username,
            Email = user.Email,
            MobilePhoneNumber = user.MobilePhoneNumber,
            UserImage = user.UserImage,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive.Value,
            Source = user.Source,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            UpdateDate = user.UpdateDate,
            LastDirectoryUpdate = user.LastDirectoryUpdate,
            TwoFactorData = user.TwoFactorData,
            Tenant = tenant
        };
    }
}