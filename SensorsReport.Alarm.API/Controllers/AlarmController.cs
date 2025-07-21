using Microsoft.AspNetCore.Mvc;

namespace SensorsReport.Alarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[UseTenantHeader]
public class AlarmController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AlarmModel>), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] IAlarmService alarmService, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
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

    [HttpGet("{alarmId}")]
    [ProducesResponseType(typeof(AlarmModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] IAlarmService alarmService, string alarmId)
    {
        if (string.IsNullOrWhiteSpace(alarmId))
            return BadRequest("Alarm ID cannot be null or empty.");

        var alarm = await alarmService.GetAsync(alarmId);
        if (alarm == null)
            return NotFound();

        return Ok(alarm);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AlarmModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Post([FromServices] IAlarmService alarmService, [FromBody] AlarmModel alarm)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (alarm == null)
            return BadRequest("Alarm model cannot be null.");

        var createdAlarm = await alarmService.PostAsync(alarm);
        return CreatedAtAction(nameof(Get), new { alarmId = createdAlarm.Id }, createdAlarm);
    }

    [HttpPut("{alarmId}")]
    [ProducesResponseType(typeof(AlarmModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Put([FromServices] IAlarmService alarmService, string alarmId, [FromBody] AlarmModel alarm)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (string.IsNullOrWhiteSpace(alarmId))
            return BadRequest("Alarm ID cannot be null or empty.");
        if (alarm == null)
            return BadRequest("Alarm model cannot be null.");
        var updatedAlarm = await alarmService.PutAsync(alarmId, alarm);
        return Ok(updatedAlarm);
    }


    [HttpPatch("{alarmId}")]
    [PatchRequestBody(typeof(AlarmModel))]
    [ProducesResponseType(typeof(AlarmModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Patch([FromServices] IAlarmService alarmService, string alarmId, [FromBody] AlarmModel alarm)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (string.IsNullOrWhiteSpace(alarmId))
            return BadRequest("Alarm ID cannot be null or empty.");
        if (alarm == null)
            return BadRequest("Alarm model cannot be null.");

        var updatedAlarm = await alarmService.PutAsync(alarmId, alarm);
        return Ok(updatedAlarm);
    }

    [HttpDelete("{alarmId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Delete([FromServices] IAlarmService alarmService, string alarmId)
    {
        ArgumentNullException.ThrowIfNull(alarmService, nameof(alarmService));
        if (string.IsNullOrWhiteSpace(alarmId))
            return BadRequest("Alarm ID cannot be null or empty.");
        await alarmService.DeleteAsync(alarmId);
        return NoContent();
    }
}

