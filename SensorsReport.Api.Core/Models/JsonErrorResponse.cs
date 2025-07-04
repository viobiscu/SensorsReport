namespace SensorsReport;

public class JsonErrorResponse
{
    public string[] Errors { get; set; } = Array.Empty<string>();
    public string? DeveloperMessage { get; set; }
}