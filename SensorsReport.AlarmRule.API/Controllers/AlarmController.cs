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

        var alarms = await alarmService.GetAsync(
            offset: offset,
            limit: limit
        );

        if (alarms == null || alarms.Count == 0)
            return NotFound();

        return Ok(alarms);
    }

    [HttpGet("{alarmRuleId}")]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] IAlarmRuleService alarmService, string alarmRuleId)
    {
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");

        var alarm = await alarmService.GetAsync(alarmRuleId);
        if (alarm == null)
            return NotFound();

        return Ok(alarm);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Post([FromServices] IAlarmRuleService alarmService, [FromBody] AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (alarmRule == null)
            return BadRequest("Alarm Rule model cannot be null.");

        var createdAlarm = await alarmService.PostAsync(alarmRule);
        return CreatedAtAction(nameof(Get), new { alarmRuleId = createdAlarm.Id }, createdAlarm);
    }

    [HttpPut("{alarmRuleId}")]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Put([FromServices] IAlarmRuleService alarmService, string alarmRuleId, [FromBody] AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");
        if (alarmRule == null)
            return BadRequest("Alarm Rule model cannot be null.");
        var updatedAlarm = await alarmService.PutAsync(alarmRuleId, alarmRule);
        return Ok(updatedAlarm);
    }


    [HttpPatch("{alarmRuleId}")]
    [PatchRequestBody(typeof(AlarmRuleModel))]
    [ProducesResponseType(typeof(AlarmRuleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Patch([FromServices] IAlarmRuleService alarmService, string alarmRuleId, [FromBody] AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");
        if (alarmRule == null)
            return BadRequest("Alarm Rule model cannot be null.");

        var updatedAlarm = await alarmService.PutAsync(alarmRuleId, alarmRule);
        return Ok(updatedAlarm);
    }

    [HttpDelete("{alarmRuleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Delete([FromServices] IAlarmRuleService alarmService, string alarmRuleId)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            return BadRequest("Alarm Rule ID cannot be null or empty.");
        await alarmService.DeleteAsync(alarmRuleId);
        return NoContent();
    }
}

