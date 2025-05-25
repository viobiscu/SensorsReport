using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;

var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configPath);
var logger = NLog.LogManager.GetCurrentClassLogger();

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
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sensors-Report-Workflow API", Version = "v1" });
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

    await app.RunAsync();
}
catch (System.IO.IOException ioEx)
{
    logger.Error(ioEx, $"Failed to bind to the configured address. The port may already be in use. {ioEx.Message}");
    Console.Error.WriteLine("ERROR: Failed to start server. The configured port may already be in use.\n" + ioEx.Message);
    Environment.Exit(1);
}
catch (Exception ex)
{
    logger.Error(ex, "Application startup failed");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
