using Microsoft.AspNetCore.Mvc;

namespace SensorsReport.LogRule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[UseTenantHeader]
public class LogRuleController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<LogRuleModel>), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] ILogRuleService logRuleService, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(logRuleService, nameof(logRuleService));

        var logRules = await logRuleService.GetAsync(
            offset: offset,
            limit: limit
        );

        if (logRules == null || logRules.Count == 0)
            return NotFound();

        return Ok(logRules);
    }

    [HttpGet("{logRuleId}")]
    [ProducesResponseType(typeof(LogRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] ILogRuleService logRuleService, string logRuleId)
    {
        if (string.IsNullOrWhiteSpace(logRuleId))
            return BadRequest("Log Rule ID cannot be null or empty.");

        var logRule = await logRuleService.GetAsync(logRuleId);
        if (logRule == null)
            return NotFound();

        return Ok(logRule);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LogRuleModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Post([FromServices] ILogRuleService logRuleService, [FromBody] LogRuleModel logRule)
    {
        ArgumentNullException.ThrowIfNull(logRuleService, nameof(logRuleService));
        if (logRule == null)
            return BadRequest("Log Rule model cannot be null.");

        var createdLog = await logRuleService.PostAsync(logRule);
        return CreatedAtAction(nameof(Get), new { logRuleId = createdLog.Id }, createdLog);
    }

    [HttpPut("{logRuleId}")]
    [ProducesResponseType(typeof(LogRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Put([FromServices] ILogRuleService logRuleService, string logRuleId, [FromBody] LogRuleModel logRule)
    {
        ArgumentNullException.ThrowIfNull(logRuleService, nameof(logRuleService));
        if (string.IsNullOrWhiteSpace(logRuleId))
            return BadRequest("Log Rule ID cannot be null or empty.");
        if (logRule == null)
            return BadRequest("Log Rule model cannot be null.");
        var updatedLog = await logRuleService.PutAsync(logRuleId, logRule);
        return Ok(updatedLog);
    }


    [HttpPatch("{logRuleId}")]
    [PatchRequestBody(typeof(LogRuleModel))]
    [ProducesResponseType(typeof(LogRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Patch([FromServices] ILogRuleService logRuleService, string logRuleId, [FromBody] LogRuleModel logRule)
    {
        ArgumentNullException.ThrowIfNull(logRuleService, nameof(logRuleService));
        if (string.IsNullOrWhiteSpace(logRuleId))
            return BadRequest("Log Rule ID cannot be null or empty.");
        if (logRule == null)
            return BadRequest("Log Rule model cannot be null.");

        var updatedLog = await logRuleService.PutAsync(logRuleId, logRule);
        return Ok(updatedLog);
    }

    [HttpDelete("{logRuleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Delete([FromServices] ILogRuleService logRuleService, string logRuleId)
    {
        ArgumentNullException.ThrowIfNull(logRuleService, nameof(logRuleService));
        if (string.IsNullOrWhiteSpace(logRuleId))
            return BadRequest("Log Rule ID cannot be null or empty.");
        await logRuleService.DeleteAsync(logRuleId);
        return NoContent();
    }
}

