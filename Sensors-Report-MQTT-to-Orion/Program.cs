using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using OrionldClient;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

public static class ConfigProgram
{
    public static string? OrionUrl;
    public static string? Jsonld;
    public static string? MqttBroker;
    public static string? MqttTopic ;
    public static string? MqttPort ;
    public static string? MqttHost;
    public static string? MqttUser;
    public static string? MqttPassword;
    public static string? MqttClientId ;
    public static string? MqttCleanSession ;
    public static string? MqttKeepAlivePeriod ;
}

public static class Program
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static OrionldTenantCache tenantCache = new OrionldTenantCache();
    static void Main(string[] args)
    {
        try
        {
            // Explicitly set the path for nlog.config file
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
            LogManager.LoadConfiguration(configPath);
            
            logger.Info("Application starting...");
            // Log version from version.txt
            try
            {
                string versionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
                if (File.Exists(versionFile))
                {
                    string version = File.ReadAllText(versionFile).Trim();
                    logger.Info($"Build Version: {version}");
                }
                else
                {
                    logger.Warn("version.txt not found");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Could not read version.txt: {ex.Message}");
            }
            LogProgramInfo(logger);
            logger.Debug("init main run 3");
            ProcessArguments(args);
            LogCommandLine(logger, args);

            if (string.IsNullOrEmpty(ConfigProgram.OrionUrl) ||
                string.IsNullOrEmpty(ConfigProgram.MqttHost) ||
                string.IsNullOrEmpty(ConfigProgram.MqttTopic) ||
                string.IsNullOrEmpty(ConfigProgram.MqttPort))
            {
                ShowHelp();
            }
            CheckForJsonld();
            CheckOrionBroker();

            logger.Debug("program is initializing");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error in Main: {ex.Message}");
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                logging.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                logger.Info("NLog configured in host builder");
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService(provider =>
                    new MqttSubscriber(
                        ConfigProgram.MqttHost,
                        string.IsNullOrEmpty(ConfigProgram.MqttPort) ? 0 : int.Parse(ConfigProgram.MqttPort),
                        ConfigProgram.MqttTopic));
                logger.Info("MqttSubscriber service registered");
            });

    static void ProcessArguments(string[] args)
    {
        // check for an environment variable for the MqttCleanSession
        if (string.IsNullOrEmpty(ConfigProgram.MqttCleanSession))
        {
            ConfigProgram.MqttCleanSession = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTCLEANSESSION");
        }
        // check for an environment variable for the MqttClientId
        if (string.IsNullOrEmpty(ConfigProgram.MqttClientId))
        {
            ConfigProgram.MqttClientId = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTCLIENTID");
        }
        // check for an environment variable for the MqttHost
        if (string.IsNullOrEmpty(ConfigProgram.MqttHost))
        {
            ConfigProgram.MqttHost = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTHOST");
        }
        // check for an environment variable for the MqttPassword
        if (string.IsNullOrEmpty(ConfigProgram.MqttPassword))
        {
            ConfigProgram.MqttPassword = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTPASSWORD");
        }
        // check for an environment variable for the MqttPort
        if (string.IsNullOrEmpty(ConfigProgram.MqttPort))
        {
            ConfigProgram.MqttPort = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTPORT");
        }
        // check for an environment variable for the MqttTopic
        if (string.IsNullOrEmpty(ConfigProgram.MqttTopic))
        {
            ConfigProgram.MqttTopic = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTTOPIC");
        }
        // check for an environment variable for the MqttUser
        if (string.IsNullOrEmpty(ConfigProgram.MqttUser))
        {
            ConfigProgram.MqttUser = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTUSER");
        }
        // check for an environment variable for the OrionUrl
        if (string.IsNullOrEmpty(ConfigProgram.OrionUrl))
        {
            ConfigProgram.OrionUrl = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_ORIONURL");
        }
        // check for an environment variable for the Jsonld
        if (string.IsNullOrEmpty(ConfigProgram.Jsonld))
        {
            ConfigProgram.Jsonld = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_JSONLD");
        }
        // check for an environment variable for the MqttKeepAlivePeriod
        if (string.IsNullOrEmpty(ConfigProgram.MqttKeepAlivePeriod))
        {
            ConfigProgram.MqttKeepAlivePeriod = Environment.GetEnvironmentVariable("SENSORSREPORTMQTT_TO_ORION_MQTTKEEPALIVEPERIOD");
        }


        foreach (var arg in args)
        {
            var splitArg = arg.Split('=');
            if (splitArg.Length != 2) continue;

            switch (splitArg[0].ToLower())
            {
                case "--orionurl":
                case "-o":
                    ConfigProgram.OrionUrl = splitArg[1];
                    break;
                case "--jsonld":
                case "-j":
                    ConfigProgram.Jsonld = splitArg[1];
                    break;
                case "--mqtttopic":
                case "-t":
                    ConfigProgram.MqttTopic = splitArg[1];
                    break;
                case "--mqttport":
                case "-p":
                    ConfigProgram.MqttPort = splitArg[1];

                    break;
                case "--mqtthost":
                case "-h":
                    ConfigProgram.MqttHost = splitArg[1];

                    break;
                case "--mqttuser":
                case "-u":
                    ConfigProgram.MqttUser = splitArg[1];
                    break;
                case "--mqttpassword":
                case "-pw":
                    ConfigProgram.MqttPassword = splitArg[1];
                    break;
                case "--mqttclientid":
                case "-cid":
                    ConfigProgram.MqttClientId = splitArg[1];
                    break;
                case "--mqttcleansession":
                case "-cs":
                    ConfigProgram.MqttCleanSession = splitArg[1];

                    break;
                case "--mqttkeepaliveperiod":
                    ConfigProgram.MqttKeepAlivePeriod = splitArg[1];
                    break;
                case "--help":
                    //case "-h":
                    ShowHelp();
                    return;
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage: Program [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --orionurl=<OrionUrl>          The URL of the Orion LD server");
        Console.WriteLine("  --mqtttopic=<MqttTopic>        The MQTT topic to subscribe to");
        Console.WriteLine("  --mqttport=<MqttPort>          The MQTT broker port");
        Console.WriteLine("  --mqtthost=<MqttHost>          The MQTT broker host");
        Console.WriteLine("  --mqttuser=<MqttUser>          The MQTT broker username");
        Console.WriteLine("  --mqttpassword=<MqttPassword>  The MQTT broker password");
        Console.WriteLine("  --mqttclientid=<MqttClientId>  The MQTT client ID");
        Console.WriteLine("  --mqttcleansession=<MqttCleanSession>  The MQTT clean session flag");
        Console.WriteLine("  --mqttkeepaliveperiod=<MqttKeepAlivePeriod>  The MQTT keep alive period");
        Console.WriteLine("  --help, -h                     Show this help message");
    }

    static void LogProgramInfo(Logger logger)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Get version with better fallback handling
        string version;
        try
        {
            // Get version from assembly - this will now read from the csproj properties
            version = assembly.GetName().Version?.ToString() ?? "1.0.0.0";
            
            // Get the informational version (typically used for SemVer)
            var infoVersion = assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .OfType<AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()?.InformationalVersion;
                
            if (!string.IsNullOrEmpty(infoVersion))
            {
                logger.Debug($"Informational version: {infoVersion}");
            }
        }
        catch (Exception ex)
        {
            logger.Warn($"Error retrieving version: {ex.Message}");
            version = "1.0.0.0";
        }
        
        try
        {
            // Get other assembly attributes using the GetCustomAttributes method for better compatibility
            var copyright = assembly
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .OfType<AssemblyCopyrightAttribute>()
                .FirstOrDefault()?.Copyright ?? "Copyright © 2024-2025";
                
            var description = assembly
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault()?.Description ?? "MQTT to Orion-LD Context Broker bridge";
                
            var title = assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                .OfType<AssemblyTitleAttribute>()
                .FirstOrDefault()?.Title ?? "SensorsReportMQTT-toOrion";
            
            logger.Info($"Application: {title}");
            logger.Info($"Description: {description}");
            logger.Info($"Copyright: {copyright}");
        }
        catch (Exception ex)
        {
            logger.Warn($"Error retrieving assembly attributes: {ex.Message}");
            logger.Info($"Application: SensorsReportMQTT-toOrion");
        }
    }

    //log the command line arguments
    static void LogCommandLine(Logger logger, string[] args)
    {
        logger.Trace("Command line arguments:");
        foreach (var arg in args)
        {
            logger.Trace(arg);
        }
        //log the environment variables
        logger.Trace("Environment variables starting with SENSORSREPORTMQTT_TO_ORION_:");
        foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().OrderBy(entry => entry.Key))
        {
            if (env.Key.ToString()?.StartsWith("SENSORSREPORTMQTT_TO_ORION_") == true)
            {
            logger.Trace($"{env.Key}={env.Value}");
            }
        }
    }

    static void CheckForJsonld()
    {
        logger.Trace("Checking for JSON-LD schema...");

        // Get current execution path
        string? currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (currentPath == null)
        {
            logger.Error("Failed to get the current path.");
            return;
        }

        string jsonldDir = Path.Combine(currentPath, "jsonld");
        string jsonld = Path.Combine(jsonldDir, "synchro.jsonld");
        string jsonldNew = jsonld + ".new";

        // If no URL is specified, check if local file exists
        if (string.IsNullOrEmpty(ConfigProgram.Jsonld))
        {
            if (File.Exists(jsonld))
                logger.Debug($"Using existing JSON-LD: {jsonld}");
            else
                logger.Warn($"JSON-LD file not found: {jsonld}");
            return;
        }

        // Ensure directory exists
        Directory.CreateDirectory(jsonldDir);

        // Download schema
        using HttpClient httpClient = new HttpClient();
        var schemaDownloader = new JsonLdSchemaDownloader(httpClient);

        if (!schemaDownloader.DownloadSchema(ConfigProgram.Jsonld, jsonldNew))
        {
            logger.Warn($"Failed to download: {jsonldNew}");
            return;
        }

        logger.Trace($"JSON-LD downloaded: {jsonldNew}");

        // Update file if needed
        try
        {
            if (File.Exists(jsonld))
            {
                if (File.ReadAllText(jsonld) != File.ReadAllText(jsonldNew))
                {
                    File.Delete(jsonld);
                    File.Move(jsonldNew, jsonld);
                    logger.Debug($"JSON-LD updated: {jsonld}");
                }
                else
                {
                    File.Delete(jsonldNew);
                    logger.Debug($"JSON-LD already exists and is up to date");
                }
            }
            else
            {
                File.Move(jsonldNew, jsonld);
                logger.Debug($"JSON-LD created: {jsonld}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error managing JSON-LD files: {ex.Message}");
        }
    }

    static void CheckOrionBroker()
    {
        logger.Trace("Checking Orion broker...");
        try
        {
            //check if ConfigProgram.OrionUrl is not null
            if (string.IsNullOrEmpty(ConfigProgram.OrionUrl))
            {
                logger.Error("Orion URL is not set.");
                return;
            }
            var orionLDClient = new Orionld();
            var response = orionLDClient.GetVersionAsync().GetAwaiter().GetResult();
            if (response == null)
            {
                logger.Error($"Orion-LD is offline: {ConfigProgram.OrionUrl}");
                //return;
            }
            else
            {
                logger.Debug($"Orion-LD is online: {ConfigProgram.OrionUrl}");
                //log the response
                logger.Trace(JsonConvert.SerializeObject(response, Formatting.Indented));
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error while checking Orion-LD status: {ex.Message}");
        }
    }
}
