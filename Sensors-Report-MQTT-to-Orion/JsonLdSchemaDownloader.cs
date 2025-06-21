using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class JsonLdSchemaDownloader
{
    private readonly HttpClient _httpClient;
    private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();
    public JsonLdSchemaDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool DownloadSchema(string url, string filePath)
    {
        try
        {
            // Fetch the JSON-LD schema from the URL
            HttpResponseMessage response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            // Read the content as a string
            string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Create the directory if it does not exist
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // Write the content to a file
            File.WriteAllText(filePath, content);

            // Check if the file is downloaded
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error downloading the schema:{url} {ex.Message}");
            return false;
        }
    }
}
