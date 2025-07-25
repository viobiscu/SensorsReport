﻿using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Config;
using NLog.Web;
using SensorsReport;
using SensorsReport.Extensions;
using SensorsReport.OrionLD.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensorsReport;

public static partial class AppConfig
{
    public static ISetupBuilder ConfigureLogger(this ISetupBuilder setupBuilder, string fileName = "nlog.config")
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        if (!File.Exists(configPath))
            File.WriteAllText(configPath, DefaultNlogConfig);
        return setupBuilder.LoadConfigurationFromFile(configPath);
    }

    public static void LogProgramInfo(this Logger log, AppDomain appDomain, string[] args, string[]? logEnvironmentKeys = null)
    {
        foreach (var key in logEnvironmentKeys ?? Array.Empty<string>())
            LogEnvironmentKeys.Add(key);

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        try
        {
            var copyright = assembly
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .OfType<AssemblyCopyrightAttribute>()
                .FirstOrDefault()?.Copyright ?? $"Copyright © 2024-{DateTime.Now.Year}";

            var description = assembly
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault()?.Description ?? "API for sensor reports";

            var title = assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                .OfType<AssemblyTitleAttribute>()
                .FirstOrDefault()?.Title ?? "SensorsReport.API";

            var version = appDomain.GetVersion();

            log.Info($"Application: {title}");
            log.Info($"Version: {version}");
            log.Info($"Description: {description}");
            log.Info($"Copyright: {copyright}\n");
        }
        catch (Exception ex)
        {
            log.Warn($"Error retrieving assembly attributes: {ex.Message}");
            log.Info("Application: SensorsReport.API");
        }

        LogAppsettings();
        LogEnvironmentVariables();
        LogArgs(args);
    }

    public static string GetVersion(this AppDomain appDomain)
    {
        string versionFile = Path.Combine(appDomain.BaseDirectory, "version.txt");
        if (File.Exists(versionFile))
        {
            return File.ReadAllText(versionFile).Trim();
        }

        return "1.0.0-unknown";
    }

    public static string GetValue(string key, string defaultValue = "")
    {
        var args = Environment.GetCommandLineArgs();
        if (args != null && args.Length > 0)
        {
            foreach (var arg in args)
            {
                var splitArg = arg.Split('=');
                if (splitArg.Length == 2 && splitArg[0].Replace("--", "").Equals(key, StringComparison.OrdinalIgnoreCase))
                    return splitArg[1];
            }
        }

        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    public static WebApplicationBuilder GetDefaultWebAppBuilder(string apiVersion = "v1", bool useBearerTokenAuthForSwagger = false, bool useTenantHeader = false)
    {
        var app = GetDefaultWebAppBuilder(c =>
        {
            var applicationName = GetAppName();
            c.SwaggerDoc(apiVersion, new OpenApiInfo { Title = applicationName, Version = apiVersion });
            c.OperationFilter<PatchRequestOperationFilter>();
            c.SchemaFilter<EnumSchemaFilter>();
            c.SchemaFilter<ExcludeTenantSchemaFilter>();
            c.OperationFilter<ExcludeIdSchemaFilter>();

            if (useTenantHeader)
                c.OperationFilter<TenantHeaderOperationFilter>();

            c.UseInlineDefinitionsForEnums();
            if (useBearerTokenAuthForSwagger)
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer [token]'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            }
        });

        if (useTenantHeader)
            app.Services.AddTenantServices();

        return app;
    }

    public static string GetAppName()
    {
        return Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly()?.GetName().Name ?? "SensorsReport";
    }

    public static void LogAppsettings()
    {
        var applicationName = GetAppName();
        var logger = LogManager.GetLogger(applicationName);

        var appsettings = new ConfigurationBuilder().
            SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: false)
            .Build()
            .AsEnumerable()
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? string.Empty);

        if (appsettings.Count == 0)
        {
            logger.Info("============= AppSettings (Empty) =============\n");
            return;
        }

        logger.Info("============= AppSettings (Default Settings) =============");

        foreach (var kvp in appsettings)
        {
            if (kvp.Key == null)
                continue;

            if (string.IsNullOrEmpty(kvp.Value))
                continue;

            logger.LogVariable("Configuration", kvp.Key, MaskIfSensitiveValue(kvp.Key, kvp.Value));
        }
        logger.Info("=================================================\n");
    }

    public static void LogEnvironmentVariables()
    {
        var applicationName = GetAppName();
        var logger = LogManager.GetLogger(applicationName);

        logger.Info("============= Environment variables (Override for AppSettings) =============");
        foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().OrderBy(entry => entry.Key))
        {
            if (env.Key.ToString()?.StartsWith(EnvVariablePrefix) == false && !LogEnvironmentKeys.Contains(env.Key.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                continue;

            var key = env.Key.ToString() ?? string.Empty;
            var value = env.Value?.ToString() ?? string.Empty;
            logger.LogVariable("Environment", key, MaskIfSensitiveValue(key, value));
        }
        logger.Info("=================================================\n");
    }

    public static void LogArgs(string[] args)
    {
        var applicationName = GetAppName();
        var logger = LogManager.GetLogger(applicationName);
        if (args == null || args.Length == 0)
        {
            logger.Info("============= Command line arguments (Empty) =============\n");
            return;
        }

        logger.Info("============= Command line arguments (Override for environment variables) ============");

        foreach (var arg in args)
        {
            var splitArg = arg.Split('=');
            var key = splitArg[0]?.Trim().Replace("--", string.Empty) ?? string.Empty;
            var value = splitArg.Length > 1 ? splitArg[1]?.Trim() ?? string.Empty : "true";

            if (!arg.Contains('=', StringComparison.CurrentCulture))
                value = "true";

            logger.LogVariable("Command line", key, MaskIfSensitiveValue(key, value));
        }
        logger.Info("=================================================\n");
    }

    public static void LogSection(this Logger logger, IConfiguration configuration, string sectionName)
    {
        logger.Info($"============= {sectionName} (Active Config) =============");
        var section = configuration.GetSection(sectionName);
        if (!section.Exists())
        {
            logger.Warn($"Section '{sectionName}' does not exist in the configuration.");
            return;
        }

        var sectionValues = section.AsEnumerable().ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? string.Empty);
        if (sectionValues.Count == 0)
        {
            logger.Warn($"Section '{sectionName}' is empty.");
            return;
        }

        foreach (var kvp in sectionValues)
        {
            if (kvp.Key == null)
                continue;

            if (string.IsNullOrEmpty(kvp.Value))
                continue;

            logger.LogVariable("Configuration", kvp.Key, MaskIfSensitiveValue(kvp.Key, kvp.Value));
        }

        logger.Info("=================================================\n");
    }

    public static string MaskIfSensitiveValue(string key, string value)
    {
        if (SensitiveEnvironmentKeys.Any(s => key.Contains(s, StringComparison.OrdinalIgnoreCase)))
        {
            value = value.Length > 6
                ? $"{value[..3]}*****{value[^3..]}"
                : value[..1] + new string('*', value.Length - 1);
        }

        return value;
    }

    public static WebApplicationBuilder GetDefaultWebAppBuilder(Action<SwaggerGenOptions> swaggerSetupAction)
    {
        var args = Environment.GetCommandLineArgs();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
            WebRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot"),
            EnvironmentName = AppConfig.GetValue("ASPNETCORE_ENVIRONMENT", "Production")
        });

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddEnvironmentVariables(EnvVariablePrefix);


        if (args != null)
            builder.Configuration.AddCommandLine(args);

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<HttpGlobalExceptionFilter>();
            options.Filters.Add(new ShapeDataFilterAttribute());
            options.Filters.Add<NotFoundStringToJsonFilter>();
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AutoConfig(builder.Configuration);
        builder.Services.AddTenantServices();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(swaggerSetupAction);

        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
            logging.RequestHeaders.Add("User-Agent");
            logging.RequestHeaders.Add("NGSILD-Tenant");
            logging.RequestHeaders.Add("Fiware-Service");
            logging.MediaTypeOptions.AddText("application/json");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
        });

        return builder;
    }

    public static void ConfigureAppAndRun(this WebApplication app, Action<WebApplication> configureApp = null!, string apiVersion = "v1")
    {
        app.ConfigureApp(configureApp, apiVersion)
            .Run();
    }

    public static WebApplication ConfigureApp(this WebApplication app, Action<WebApplication> configureApp = null!, string apiVersion = "v1")
    {
        configureApp?.Invoke(app);

        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();

        if (!string.IsNullOrEmpty(apiVersion))
        {
            app.ConfigureSwagger(apiVersion);
        }

        app.UseHttpLogging();
        app.ConfigureDefaultEndpoints();

        app.UseRouting();
        app.MapControllers();
        app.UseStatusCodePages(async context =>
        {
            if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                context.HttpContext.Response.ContentType = "application/json";

                var errorResponse = new JsonMessageResponse
                {
                    Message = "The requested resource was not found."
                };

                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
        });

        return app;
    }

    public static WebApplication ConfigureSwagger(this WebApplication app, string apiVersion = "v1")
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"SensorsReport API {apiVersion}"));

        return app;
    }

    public static WebApplication ConfigureDefaultEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => "OK");
        app.MapGet("/version", () => AppDomain.CurrentDomain.GetVersion());
        return app;
    }

    private static void LogVariable(this Logger log, string type, string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            log.Warn($"{type} variable: key is null or empty");
            return;
        }

        if (SensitiveEnvironmentKeys.Any(s => key.Contains(s, StringComparison.OrdinalIgnoreCase)))
        {
            value = value.Length > 6
                ? $"{value[..3]}*****{value[^3..]}"
                : value[..1] + new string('*', value.Length - 1);
        }

        log.Info($"{type} variable: {key}={value}");
    }

    private const string EnvVariablePrefix = "SR_";

    public static HashSet<string> SensitiveEnvironmentKeys = new()
    {
        "PASSWORD",
        "SECRET",
        "KEY",
        "TOKEN",
        "ConnectionString"
    };

    private static HashSet<string> LogEnvironmentKeys = [
        "ASPNETCORE_ENVIRONMENT",
        "ASPNETCORE_URLS",
        "ASPNETCORE_HTTPS_PORT",
        "DOTNET_RUNNING_IN_CONTAINER",
        "DOTNET_VERSION",
        "DOTNET_ROOT",
        "HOSTTYPE",
        "LANG"
    ];

    private const string DefaultNlogConfig = @"""
<?xml version=""1.0"" encoding=""utf-8"" ?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
      autoReload=""true""
      internalLogLevel=""Info"">

  <targets>
    <target xsi:type=""ColoredConsole""
            name=""logconsole""
            layout=""${longdate} ${level:uppercase=true} ${logger:shortName=true} ${message} ${exception:format=tostring}"">
      <highlight-row condition=""level == LogLevel.Trace"" foregroundColor=""DarkGray"" />
      <highlight-row condition=""level == LogLevel.Debug"" foregroundColor=""Gray"" />
      <highlight-row condition=""level == LogLevel.Info"" foregroundColor=""White"" />
      <highlight-row condition=""level == LogLevel.Warn"" foregroundColor=""Yellow"" />
      <highlight-row condition=""level == LogLevel.Error"" foregroundColor=""Red"" />
      <highlight-row condition=""level == LogLevel.Fatal"" foregroundColor=""DarkRed"" />
    </target>
  </targets>

  <rules>
    <!-- Microsoft framework logs -->
    <logger name=""Microsoft.*"" maxlevel=""Warn"" final=""true"" />

    <!-- System framework logs -->
    <logger name=""System.*"" maxlevel=""Warn"" final=""true"" />

    <!-- ASP.NET Core hosting logs -->
    <logger name=""Microsoft.AspNetCore.Hosting.*"" minlevel=""Error"" final=""true"" />

    <!-- Entity Framework logs -->
    <logger name=""Microsoft.EntityFrameworkCore.*"" maxlevel=""Warn"" final=""true"" />

    <!-- All other logs -->
    <logger name=""*"" minlevel=""Trace"" writeTo=""logconsole"" />
  </rules>
</nlog>";
}
