using System.Reflection;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Config;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensorsReport;

public static partial class AppConfig
{
    public static void ConfigureLogger(this ISetupBuilder setupBuilder, string fileName = "nlog.config")
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        if (!File.Exists(configPath))
            File.WriteAllText(configPath, DefaultNlogConfig);
        setupBuilder.LoadConfigurationFromFile(configPath);
    }

    public static Logger GetLogger(string loggerName = null)
    {
        if (string.IsNullOrEmpty(loggerName))
            loggerName = Assembly.GetExecutingAssembly().GetName().Name ?? "SensorsReport";

        LogManager.Setup().ConfigureLogger();
        return LogManager.GetLogger(loggerName);
    }

    public static void LogProgramInfo(this Logger log, AppDomain appDomain, string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();

        try
        {
            var copyright = assembly
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .OfType<AssemblyCopyrightAttribute>()
                .FirstOrDefault()?.Copyright ?? "Copyright © 2024-2025";

            var description = assembly
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault()?.Description ?? "API for audit logging system for sensor reports";

            var title = assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                .OfType<AssemblyTitleAttribute>()
                .FirstOrDefault()?.Title ?? "SensorsReportAudit.API";

            var version = appDomain.GetVersion();

            log.Info($"Application: {title}");
            log.Info($"Version: {version}");
            log.Info($"Description: {description}");
            log.Info($"Copyright: {copyright}");
        }
        catch (Exception ex)
        {
            log.Warn($"Error retrieving assembly attributes: {ex.Message}");
            log.Info("Application: SensorsReportAudit.API");
        }

        foreach (var arg in args ?? [])
        {
            var splitArg = arg.Split('=');


            var key = splitArg[0]?.Trim().Replace("--", string.Empty) ?? string.Empty;
            var value = splitArg.Length > 1 ? splitArg[1]?.Trim() ?? string.Empty : string.Empty;

            if (string.IsNullOrEmpty(value))
                value = "true";

            if (!arg.Contains('=', StringComparison.CurrentCulture))
                value = "true";

            log.LogVariable("Command line", key, value);
        }

        log.Info("Environment variables starting:");
        foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().OrderBy(entry => entry.Key))
        {
            var key = env.Key.ToString() ?? string.Empty;
            var value = env.Value?.ToString() ?? string.Empty;
            log.LogVariable("Environment", key, value);
        }
    }

    public static List<string> SensitiveEnvironmentKeys = new()
    {
        "PASSWORD",
        "SECRET",
        "KEY",
        "TOKEN"
    };

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

    public static WebApplicationBuilder GetDefaultWebAppBuilder(string apiVersion = "v1", bool useBearerTokenAuthForSwagger = false)
    {
        return GetDefaultWebAppBuilder(c =>
        {
            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
            c.SwaggerDoc(apiVersion, new OpenApiInfo { Title = applicationName, Version = apiVersion });

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

        if (args != null)
            builder.Configuration.AddCommandLine(args);

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(swaggerSetupAction);

        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        return builder;
    }

    public static void ConfigureAppAndRun(this WebApplication app, string apiVersion = "v1")
    {
        app.ConfigureApp()
            .ConfigureSwagger(apiVersion)
            .ConfigureDefaultEndpoints()
            .Run();
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseRouting();
        app.MapControllers();

        return app;
    }

    public static WebApplication ConfigureSwagger(this WebApplication app, string apiVersion = "v1")
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"SensorsReportAudit API {apiVersion}"));

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