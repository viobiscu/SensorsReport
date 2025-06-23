using Microsoft.AspNetCore.Authentication.JwtBearer;
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

string version = string.Empty;

try
{
    logger.Info("Application starting...");
    // Log version from version.txt
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
    builder.Configuration.Sources.Clear();
    builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
    if (args != null)
    {
        builder.Configuration.AddCommandLine(args);
    }

    // Load configuration from environment variables
    var auditConfig = new AuditConfig
    {
        QuantumLeapHost = GetConfigValue("SR_AUDIT_QUANTUMLEAP_HOST", "quantum.sensorsreport.net"),
        QuantumLeapPort = GetConfigValue("SR_AUDIT_QUANTUMLEAP_PORT", "8668"),
        KeycloakUrl = GetConfigValue("SR_AUDIT_KEYCLOAK_URL", "keycloak.sensorsreport.net"),
        KeycloakPort = GetConfigValue("SR_AUDIT_KEYCLOAK_PORT", "30100"),
        KeycloakRealm = GetConfigValue("SR_AUDIT_KEYCLOAK_REALM", "sr"), // Fixed spelling from RELM to REALM
        KeycloakClientId = GetConfigValue("SR_AUDIT_KEYCLOAK_CLIENTID", "ContextBroker"),
        KeycloakClientSecret = GetConfigValue("SR_AUDIT_KEYCLOAK_CLIENTSECRET", "AELYK4tusYazvIDIvw0meQZiSnGMnVJP")
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
    app.MapGet("/audit/health", () => "OK");
    app.MapGet("/audit/version", () => version);

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
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
    if (args == null || args.Length == 0)
    {
        return Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }

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
        log.Info($"Version: {version}");
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