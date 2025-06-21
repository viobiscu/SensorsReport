using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using SensorsReportAudit;
using SensorsReportAudit.Auth;
using System.Net.Http.Headers;
using System.Reflection;

// Configure NLog
var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
LogManager.Setup().LoadConfigurationFromFile(configPath);
var logger = LogManager.GetCurrentClassLogger();

try
{
    logger.Info("Application starting...");
    // Log version from version.txt
    string version = "unknown";
    try
    {
        string versionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
        if (File.Exists(versionFile))
        {
            version = File.ReadAllText(versionFile).Trim();
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

    // Process command-line arguments and environment variables
    ProcessArguments(args);
    LogEnvironmentVariables(logger);

    // Create builder with options that disable file monitoring
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        WebRootPath = "wwwroot",
        ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
        EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    });

    // Disable file provider watcher
    builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false);
        config.AddEnvironmentVariables();
        if (args != null)
        {
            config.AddCommandLine(args);
        }
    });

    // Load configuration from environment variables
    var auditConfig = new AuditConfig
    {
        QuantumLeapHost = GetConfigValue("SR_AUDIT_QUANTUMLEAP_HOST", "quantum.sensorsreport.net"),
        QuantumLeapPort = GetConfigValue("SR_AUDIT_QUANTUMLEAP_PORT", "8668"),
    };

    // Add services to the container
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

    // Add HttpClient for QuantumLeap API calls with authorization
    builder.Services.AddHttpClient("QuantumLeapClient", client =>
    {
        client.BaseAddress = new Uri(auditConfig.QuantumLeapBaseUrl);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddHttpMessageHandler(serviceProvider =>
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var authLogger = loggerFactory.CreateLogger<KeycloakAuthService>();
        var authService = new KeycloakAuthService(auditConfig, authLogger);
        return new AuthenticationDelegatingHandler(authService);
    });

    builder.Services.AddSingleton<HttpClient>(sp =>
    {
        return sp.GetRequiredService<IHttpClientFactory>().CreateClient("QuantumLeapClient");
    });

    // Register configuration
    builder.Services.AddSingleton(auditConfig);

    // Configure Keycloak authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = $"{auditConfig.KeycloakBaseUrl}/realms/{auditConfig.KeycloakRealm}";
        options.RequireHttpsMetadata = false; // Set to true in production with HTTPS
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        // Enable token introspection for online validation
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                if (token != null)
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var authLogger = loggerFactory.CreateLogger<KeycloakAuthService>();
                    var authService = new KeycloakAuthService(auditConfig, authLogger);

                    var isValid = await authService.ValidateTokenAsync(token.RawData);
                    if (!isValid)
                    {
                        context.Fail("Token validation failed");
                    }
                }
            }
        };
    });

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SensorsReportAudit API", Version = "v1" });

        // Configure Swagger to use JWT Bearer Authentication
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
    });

    // Configure NLog for ASP.NET Core
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SensorsReportAudit API v1"));

    // Health/liveness probe endpoint
    //app.MapGet("/health", () => "OK");

    // Add version endpoint
    app.MapGet("/audit/version", () =>
    {
        // Return the integer version from version.txt
        string versionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
        string version = "unknown";
        if (File.Exists(versionFile))
        {
            version = File.ReadAllText(versionFile).Trim();
        }
        return Results.Ok(version);
    });

    // Add health endpoint
    app.MapGet("/audit/health", () => Results.Ok("Service is healthy"));

    // Log all incoming HTTP requests and their response status code
    app.Use(async (context, next) =>
    {
        var reqLogger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogger");
        await next();
        reqLogger.LogInformation("HTTP {Method} {Path} responded {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
    });

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Log the IP and port number(s) the service is listening on at startup
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var addresses = app.Urls;
        if (addresses != null && addresses.Any())
        {
            foreach (var address in addresses)
            {
                logger.Info($"Sensors-Report-Audit.API is listening on: {address}");
            }
        }
        else
        {
            logger.Info("No explicit server addresses found. Default Kestrel settings will be used.");
        }
    });

    app.Run();

    // After app.Run();, log the listening addresses using the built-in Kestrel feature
    // This will log the IP and port(s) the service is listening on
    var serverAddressesFeature = app.Services.GetService<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
    if (serverAddressesFeature != null && serverAddressesFeature.Addresses.Any())
    {
        foreach (var address in serverAddressesFeature.Addresses)
        {
            logger.Info($"Sensors-Report-Audit.API is listening on: {address}");
        }
    }
    else
    {
        logger.Info("No explicit server addresses found. Default Kestrel settings will be used.");
    }
}
catch (Exception ex)
{
    logger.Error(ex, "Application startup failed");
    throw;
}
finally
{
    LogManager.Shutdown();
}

// Helper method to get configuration value from environment or command line
string GetConfigValue(string key, string defaultValue = "")
{
    // First check if the value was set in the command-line args
    foreach (var arg in args)
    {
        var splitArg = arg.Split('=');
        if (splitArg.Length == 2 && splitArg[0].Replace("--", "").Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return splitArg[1];
        }
    }

    // Then check environment variables
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}

// Process command-line arguments
void ProcessArguments(string[] arguments)
{
    foreach (var arg in arguments)
    {
        var splitArg = arg.Split('=');
        if (splitArg.Length != 2) continue;

        // Log the argument but don't show sensitive values
        if (splitArg[0].Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
            splitArg[0].Contains("PASSWORD", StringComparison.OrdinalIgnoreCase))
        {
            logger.Info($"Command-line argument: {splitArg[0]}=*****");
        }
        else
        {
            logger.Info($"Command-line argument: {splitArg[0]}={splitArg[1]}");
        }
    }
}

// Log environment variables
void LogEnvironmentVariables(Logger log)
{
    log.Info("Environment variables starting with SR_AUDIT_:");
    foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().OrderBy(entry => entry.Key))
    {
        if (env.Key.ToString()?.StartsWith("SR_AUDIT_") == true)
        {
            var key = env.Key.ToString();
            var value = env.Value?.ToString();

            // Don't log sensitive values
            if (key?.Contains("SECRET", StringComparison.OrdinalIgnoreCase) == true ||
                key?.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase) == true)
            {
                log.Info($"{key}=*****");
            }
            else
            {
                log.Info($"{key}={value}");
            }
        }
    }
}

// Log program information
void LogProgramInfo(Logger log)
{
    var assembly = Assembly.GetExecutingAssembly();

    // Get version with better fallback handling
    string version;
    try
    {
        version = assembly.GetName().Version?.ToString() ?? "1.0.0.0";
    }
    catch (Exception ex)
    {
        log.Warn($"Error retrieving version: {ex.Message}");
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
            .FirstOrDefault()?.Description ?? "API for audit logging system for sensor reports";

        var title = assembly
            .GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
            .OfType<AssemblyTitleAttribute>()
            .FirstOrDefault()?.Title ?? "SensorsReportAudit.API";

        log.Info($"Application: {title}");
        log.Info($"Description: {description}");
        log.Info($"Copyright: {copyright}");
    }
    catch (Exception ex)
    {
        log.Warn($"Error retrieving assembly attributes: {ex.Message}");
        log.Info("Application: SensorsReportAudit.API");
    }
}

// Custom delegating handler for authentication
public class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly KeycloakAuthService _authService;

    public AuthenticationDelegatingHandler(KeycloakAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authService.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}

public class StartupAddressLogger : IHostedService
{
    private readonly ILogger<StartupAddressLogger> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IWebHost _webHost;

    public StartupAddressLogger(ILogger<StartupAddressLogger> logger, IHostApplicationLifetime lifetime, IWebHost webHost)
    {
        _logger = logger;
        _lifetime = lifetime;
        _webHost = webHost;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(() =>
        {
            var addresses = _webHost.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()?.Addresses;
            if (addresses != null && addresses.Any())
            {
                foreach (var address in addresses)
                {
                    _logger.LogInformation($"Sensors-Report-Audit.API is listening on: {address}");
                }
            }
            else
            {
                _logger.LogInformation("No explicit server addresses found. Default Kestrel settings will be used.");
            }
        });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}