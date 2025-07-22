using Microsoft.AspNetCore.Mvc;

namespace SensorsReport.NotificationRule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[UseTenantHeader]
public class NotificationRuleController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationRuleModel>), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] INotificationRuleService notificationRuleService, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(notificationRuleService, nameof(notificationRuleService));

        var notifications = await notificationRuleService.GetAsync(
            offset: offset,
            limit: limit
        );

        if (notifications == null || notifications.Count == 0)
            return NotFound();

        return Ok(notifications);
    }

    [HttpGet("{notificationRuleId}")]
    [ProducesResponseType(typeof(NotificationRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] INotificationRuleService notificationRuleService, string notificationRuleId)
    {
        if (string.IsNullOrWhiteSpace(notificationRuleId))
            return BadRequest("Notification Rule ID cannot be null or empty.");

        var notification = await notificationRuleService.GetAsync(notificationRuleId);
        if (notification == null)
            return NotFound();

        return Ok(notification);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationRuleModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Post([FromServices] INotificationRuleService notificationRuleService, [FromBody] NotificationRuleModel notificationRule)
    {
        ArgumentNullException.ThrowIfNull(notificationRuleService, nameof(notificationRuleService));
        if (notificationRule == null)
            return BadRequest("Notification Rule model cannot be null.");

        var createdNotification = await notificationRuleService.PostAsync(notificationRule);
        return CreatedAtAction(nameof(Get), new { notificationRuleId = createdNotification.Id }, createdNotification);
    }

    [HttpPut("{notificationRuleId}")]
    [ProducesResponseType(typeof(NotificationRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Put([FromServices] INotificationRuleService notificationRuleService, string notificationRuleId, [FromBody] NotificationRuleModel notificationRule)
    {
        ArgumentNullException.ThrowIfNull(notificationRuleService, nameof(notificationRuleService));
        if (string.IsNullOrWhiteSpace(notificationRuleId))
            return BadRequest("Notification Rule ID cannot be null or empty.");
        if (notificationRule == null)
            return BadRequest("Notification Rule model cannot be null.");
        var updatedNotification = await notificationRuleService.PutAsync(notificationRuleId, notificationRule);
        return Ok(updatedNotification);
    }


    [HttpPatch("{notificationRuleId}")]
    [PatchRequestBody(typeof(NotificationRuleModel))]
    [ProducesResponseType(typeof(NotificationRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Patch([FromServices] INotificationRuleService notificationRuleService, string notificationRuleId, [FromBody] NotificationRuleModel notificationRule)
    {
        ArgumentNullException.ThrowIfNull(notificationRuleService, nameof(notificationRuleService));
        if (string.IsNullOrWhiteSpace(notificationRuleId))
            return BadRequest("Notification Rule ID cannot be null or empty.");
        if (notificationRule == null)
            return BadRequest("Notification Rule model cannot be null.");

        var updatedNotification = await notificationRuleService.PutAsync(notificationRuleId, notificationRule);
        return Ok(updatedNotification);
    }

    [HttpDelete("{notificationRuleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Delete([FromServices] INotificationRuleService notificationRuleService, string notificationRuleId)
    {
        ArgumentNullException.ThrowIfNull(notificationRuleService, nameof(notificationRuleService));
        if (string.IsNullOrWhiteSpace(notificationRuleId))
            return BadRequest("Notification Rule ID cannot be null or empty.");
        await notificationRuleService.DeleteAsync(notificationRuleId);
        return NoContent();
    }
}

