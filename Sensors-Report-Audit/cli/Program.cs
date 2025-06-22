using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Parse args
        string token = null, location = null, action = null, details = null, configPath = "config.json";
        foreach (var arg in args)
        {
            if (arg.StartsWith("--token")) token = GetValue(args, "--token");
            if (arg.StartsWith("--location")) location = GetValue(args, "--location");
            if (arg.StartsWith("--action")) action = GetValue(args, "--action");
            if (arg.StartsWith("--details")) details = GetValue(args, "--details");
            if (arg.StartsWith("--config")) configPath = GetValue(args, "--config");
        }
        if (token == null || location == null || action == null)
        {
            Console.WriteLine("Usage: --token <JWT> --location <Location> --action <Action> [--details <Details>] [--config <config.json>]");
            return 1;
        }

        // Read config
        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Config file not found: {configPath}");
            return 1;
        }
        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
        if (config == null || string.IsNullOrEmpty(config.auditApiUrl))
        {
            Console.WriteLine("Invalid config file.");
            return 1;
        }

        // Parse TenantId from JWT
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        string tenantId = jwt.Payload.TryGetValue("TenantId", out var tid) ? tid.ToString() : "Unknown";

        // Prepare audit log payload
        var payload = new
        {
            TenantId = tenantId,
            Location = location,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        // Send to Audit API
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsync(
            config.auditApiUrl,
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        );
        var respContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine(respContent);

        return response.IsSuccessStatusCode ? 0 : 1;
    }

    static string GetValue(string[] args, string key)
    {
        for (int i = 0; i < args.Length; i++)
            if (args[i] == key && i + 1 < args.Length)
                return args[i + 1];
        return null;
    }

    class Config
    {
        public string auditApiUrl { get; set; }
        public string businessBrokerApiUrl { get; set; }
    }
}
