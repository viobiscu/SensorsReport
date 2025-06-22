using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
LogManager.Setup().LoadConfigurationFromFile(configPath);
var logger = LogManager.GetCurrentClassLogger();

try
{
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

    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // JWT/Keycloak authentication setup
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/sr";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SensorsReportSMS API", Version = "v1" });
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

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
