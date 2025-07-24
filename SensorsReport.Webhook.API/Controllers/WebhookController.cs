using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SensorsReport.Webhook.API.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        [FromServices] INotifyRuleQueueService notifyRuleQueueService,
        [FromServices] ITenantRetriever tenantRetriever,
        [FromBody] JsonElement payload,
        [FromQuery] string? subscriptionId = null)
    {
        await LogCompleteHttpRequestAsync(payload);

        _logger.LogInformation("Webhook notification received: {Payload}", JsonSerializer.Serialize(payload));

        var tenant = tenantRetriever.CurrentTenantInfo;
        if (!payload.TryGetProperty("id", out var notificationId) ||
            !payload.TryGetProperty("type", out var typeElement) ||
            !payload.TryGetProperty("subscriptionId", out var subIdElement) ||
            !payload.TryGetProperty("data", out var dataElement))
        {
            return BadRequest("Invalid webhook notification format");
        }

        string type = typeElement.GetString()!;
        if (type != "Notification")
        {
            return BadRequest($"Unknown notification type: {type}");
        }

        if (string.IsNullOrEmpty(subscriptionId) || subIdElement.GetString() != subscriptionId)
        {
            return BadRequest("Query parameter subscriptionId does not match payload subscriptionId");
        }

        await notifyRuleQueueService.EnqueueNotificationAsync(dataElement, tenant, notificationId.GetString()!);
        _logger.LogInformation("Webhook notification enqueued successfully for subscriptionId: {SubscriptionId}", subscriptionId);
        return NoContent();
    }

    private async Task LogCompleteHttpRequestAsync(JsonElement payload)
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
