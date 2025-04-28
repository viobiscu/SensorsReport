using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RulesEngine.Models;
using SensorsReportBusinessBroker.API.Configuration;
using SensorsReportBusinessBroker.API.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add configuration from environment variables
builder.Configuration.AddEnvironmentVariables();

// Configure application settings
var appConfig = new AppConfig();
builder.Configuration.Bind(appConfig);

// Add services to the container
builder.Services.AddSingleton(appConfig);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IOrionService, OrionService>();
builder.Services.AddSingleton<IAuditService, AuditService>();
builder.Services.AddSingleton<RulesEngine.RulesEngine>(sp => {
    var workflow = new Workflow { WorkflowName = "BusinessRules" };
    return new RulesEngine.RulesEngine(new Workflow[] { workflow });
});

// Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

// Configure controllers
app.MapControllers();

app.Run();
