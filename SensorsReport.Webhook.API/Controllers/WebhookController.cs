using Microsoft.AspNetCore.Mvc;
using SensorsReport.Api.Core.MassTransit;
using System.Text;
using System.Text.Json;

namespace SensorsReport.Webhook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[UseTenantHeader]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(ILogger<WebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> ReceiveWebhook(
        [FromServices] ITenantRetriever tenantRetriever,
        [FromServices] IEventBus eventBus,
        [FromBody] SubscriptionEventModel payload,
        [FromQuery] string? subscriptionId = null)
    {
        await LogCompleteHttpRequestAsync(payload);

        _logger.LogInformation("Webhook notification received: {Payload}", payload.Id);

        payload.Tenant = tenantRetriever.CurrentTenantInfo;

        if (string.IsNullOrEmpty(payload.Id) ||
            string.IsNullOrEmpty(payload.Type) ||
            string.IsNullOrEmpty(payload.SubscriptionId) ||
            payload.Data == null || payload.Data.Length == 0)
            return BadRequest("Invalid webhook notification format");

        if (!payload.Type.Equals("Notification", StringComparison.OrdinalIgnoreCase))
            return BadRequest($"Invalid notification type: {payload.Type}");

        if (string.IsNullOrEmpty(subscriptionId) || !
            payload.SubscriptionId.Equals(subscriptionId, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Query parameter subscriptionId does not match payload subscriptionId");

        await eventBus.PublishAsync(new SensorDataChangedEvent
        {
            Id = payload.Id,
            Type = payload.Type,
            SubscriptionId = payload.SubscriptionId,
            Tenant = payload.Tenant,
            Data = payload.Data
        });
        return NoContent();
    }

    private async Task LogCompleteHttpRequestAsync(SubscriptionEventModel payload)
    {
        var requestDetails = new StringBuilder();
        requestDetails.AppendLine("=== COMPLETE HTTP REQUEST DUMP ===");

        // Request line
        requestDetails.AppendLine($"Method: {Request.Method}");
        requestDetails.AppendLine($"URL: {Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}");
        requestDetails.AppendLine($"Protocol: {Request.Protocol}");

        // Headers
        requestDetails.AppendLine("\n--- HEADERS ---");
        foreach (var header in Request.Headers)
        {
            requestDetails.AppendLine($"{header.Key}: {string.Join(", ", header.Value!)}");
        }

        // Query parameters
        if (Request.Query.Any())
        {
            requestDetails.AppendLine("\n--- QUERY PARAMETERS ---");
            foreach (var query in Request.Query)
            {
                requestDetails.AppendLine($"{query.Key}: {string.Join(", ", query.Value!)}");
            }
        }

        // Body/Payload
        requestDetails.AppendLine("\n--- BODY/PAYLOAD ---");
        requestDetails.AppendLine($"Content-Type: {Request.ContentType}");
        requestDetails.AppendLine($"Content-Length: {Request.ContentLength}");

        // Pretty print JSON payload
        try
        {
            var prettyJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            requestDetails.AppendLine($"Payload (Pretty JSON):\n{prettyJson}");
        }
        catch
        {
            requestDetails.AppendLine($"Payload (Raw): {JsonSerializer.Serialize(payload)}");
        }

        // Connection info
        requestDetails.AppendLine("\n--- CONNECTION INFO ---");
        requestDetails.AppendLine($"Remote IP: {Request.HttpContext.Connection.RemoteIpAddress}");
        requestDetails.AppendLine($"Remote Port: {Request.HttpContext.Connection.RemotePort}");
        requestDetails.AppendLine($"Local IP: {Request.HttpContext.Connection.LocalIpAddress}");
        requestDetails.AppendLine($"Local Port: {Request.HttpContext.Connection.LocalPort}");

        // Additional context
        if (Request.HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            requestDetails.AppendLine($"User: {Request.HttpContext.User.Identity.Name}");
        }

        requestDetails.AppendLine("=== END HTTP REQUEST DUMP ===");

        _logger.LogInformation(requestDetails.ToString());
        await Task.CompletedTask;
    }

}
