using System.Text;

namespace SensorsReport;

public static class TemplateHelper
{
    public static string FormatString(string template, Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(template) || values == null || values.Count == 0)
            return template;
        var formattedString = new StringBuilder(template);
        foreach (var kvp in values)
            formattedString.Replace($"{{{kvp.Key}}}", kvp.Value);
        return formattedString.ToString();
    }
}
