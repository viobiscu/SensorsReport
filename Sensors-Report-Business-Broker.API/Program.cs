using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NLog;
using NLog.Web;
using RulesEngine.Models;
using SensorsReportBusinessBroker.API.Configuration;
using SensorsReportBusinessBroker.API.Services;
using System.Reflection;
using System.Text.Json;

// Initialize NLog
var logger = LogManager.GetCurrentClassLogger();

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
    
    logger.Info("Initializing application...");

    var builder = WebApplication.CreateBuilder(args);
    
    // Log command line arguments
    LogCommandLine(logger, args);

    // Configure NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    logger.Info("NLog configured in host builder");

    // Add configuration from environment variables
    builder.Configuration.AddEnvironmentVariables();
    logger.Info("Environment variables configuration added");

    // Configure application settings
    var appConfig = new AppConfig();
    builder.Configuration.Bind(appConfig);
    logger.Info("Application configuration bound");

    // Add services to the container
    builder.Services.AddSingleton(appConfig);
    builder.Services.AddHttpClient();
    builder.Services.AddSingleton<IOrionService, OrionService>();
    builder.Services.AddSingleton<IAuditService, AuditService>();
    builder.Services.AddSingleton<RulesEngine.RulesEngine>(sp => {
        // Create a workflow with at least one default rule to pass validation
        var workflow = new Workflow 
        { 
            WorkflowName = "BusinessRules",
            Rules = new Rule[]
            {
                new Rule
                {
                    RuleName = "DefaultRule",
                    RuleExpressionType = RuleExpressionType.LambdaExpression,
                    Expression = "true",
                    Enabled = true,
                    SuccessEvent = "Success",
                    ErrorMessage = "Error"
                }
            }
        };
        return new RulesEngine.RulesEngine(new Workflow[] { workflow });
    });
    logger.Info("Services registered");

    // Configure health checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy());
    logger.Info("Health checks configured");

    // Add controllers
    builder.Services.AddControllers()
        .AddJsonOptions(options => {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });
    logger.Info("Controllers added");

    // Configure Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    logger.Info("Swagger configured");

    var app = builder.Build();
    logger.Info("Application built");

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        logger.Info("Development environment: Swagger UI enabled");
    }

    // Map health check endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    });
    logger.Info("Health check endpoint mapped");

    // Configure controllers
    app.MapControllers();
    logger.Info("Controllers mapped");

    // Log the URLs the application is listening on
    var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "http://+:80";
    logger.Info($"Application listening on: {urls}");

    logger.Info("Application started");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal NLog threads before application exit
    logger.Info("Application shutting down");
    LogManager.Shutdown();
}

/// <summary>
/// Logs assembly and version information
/// </summary>
static void LogProgramInfo(Logger logger)
{
    var assembly = Assembly.GetExecutingAssembly();
    
    // Get version with better fallback handling
    string version;
    try
    {
        // Get version from assembly
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
        // Get other assembly attributes
        var copyright = assembly
            .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
            .OfType<AssemblyCopyrightAttribute>()
            .FirstOrDefault()?.Copyright ?? "Copyright © 2024-2025";
            
        var description = assembly
            .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
            .OfType<AssemblyDescriptionAttribute>()
            .FirstOrDefault()?.Description ?? "Business Rules Broker API for SensorsReport";
            
        var title = assembly
            .GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
            .OfType<AssemblyTitleAttribute>()
            .FirstOrDefault()?.Title ?? "SensorsReportBusinessBroker.API";
        
        logger.Info($"Application: {title}");
        logger.Info($"Description: {description}");
        logger.Info($"Copyright: {copyright}");
    }
    catch (Exception ex)
    {
        logger.Warn($"Error retrieving assembly attributes: {ex.Message}");
        logger.Info($"Application: SensorsReportBusinessBroker.API");
    }
}

/// <summary>
/// Logs command line arguments and relevant environment variables
/// </summary>
static void LogCommandLine(Logger logger, string[] args)
{
    logger.Trace("Command line arguments:");
    foreach (var arg in args)
    {
        logger.Trace(arg);
    }
    
    // Log environment variables
    logger.Trace("Environment variables starting with SR_BB_:");
    foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().OrderBy(entry => entry.Key))
    {
        if (env.Key.ToString()?.StartsWith("SR_BB_") == true)
        {
            logger.Trace($"{env.Key}={env.Value}");
        }
    }
}
