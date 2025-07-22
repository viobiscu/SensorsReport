using Microsoft.AspNetCore.Mvc;

namespace SensorsReport.AlarmRule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[UseTenantHeader]
public class AlarmRuleController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AlarmRuleModel>), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] IAlarmRuleService alarmService, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));

        var alarmRules = await alarmService.GetAsync(
            offset: offset,
            limit: limit
        );

        if (alarmRules == null || alarmRules.Count == 0)
            return NotFound();

        return Ok(alarmRules);
    }

    [HttpGet("{alarmRuleId}")]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] IAlarmRuleService alarmRuleService, string alarmRuleId)
    {
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");

        var alarmRule = await alarmRuleService.GetAsync(alarmRuleId);
        if (alarmRule == null)
            return NotFound();

        return Ok(alarmRule);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Post([FromServices] IAlarmRuleService alarmRuleService, [FromBody] AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(alarmRuleService, nameof(alarmRuleService));
        if (alarmRule == null)
            return BadRequest("Alarm Rule model cannot be null.");

        var createdAlarmRule = await alarmRuleService.PostAsync(alarmRule);
        return CreatedAtAction(nameof(Get), new { alarmRuleId = createdAlarmRule.Id }, createdAlarmRule);
    }

    [HttpPut("{alarmRuleId}")]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Put([FromServices] IAlarmRuleService alarmRuleService, string alarmRuleId, [FromBody] AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(alarmRuleService, nameof(alarmRuleService));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");
        if (alarmRule == null)
            return BadRequest("Alarm Rule model cannot be null.");
        var updatedAlarmRule = await alarmRuleService.PutAsync(alarmRuleId, alarmRule);
        return Ok(updatedAlarmRule);
    }


    [HttpPatch("{alarmRuleId}")]
    [PatchRequestBody(typeof(AlarmRuleModel))]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Patch([FromServices] IAlarmRuleService alarmRuleService, string alarmRuleId, [FromBody] AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(alarmRuleService, nameof(alarmRuleService));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");
        if (alarmRule == null)
            return BadRequest("Alarm Rule model cannot be null.");

        var updatedAlarmRule = await alarmRuleService.PutAsync(alarmRuleId, alarmRule);
        return Ok(updatedAlarmRule);
    }

    [HttpDelete("{alarmRuleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Delete([FromServices] IAlarmRuleService alarmRuleService, string alarmRuleId)
    {
        ArgumentNullException.ThrowIfNull(alarmRuleService, nameof(alarmRuleService));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");
        await alarmRuleService.DeleteAsync(alarmRuleId);
        return NoContent();
    }
}

