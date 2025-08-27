
namespace SensorsReport.Frontend;

public interface IDirectoryService
{
    AppServices.DirectoryEntry Validate(string username, string password);
}