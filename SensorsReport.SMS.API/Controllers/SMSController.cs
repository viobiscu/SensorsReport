using Microsoft.AspNetCore.Mvc;
using SensorsReport.SMS.API.Models;
using SensorsReport.SMS.API.Repositories;
using System.Text.Json;

namespace SensorsReport.SMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[UseTenantHeader]
public class SmsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<SmsModel>), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] ITenantRetriever tenantRetriever, [FromServices] ISmsRepository repository, [FromQuery] string? fromDate, [FromQuery] string? toDate, [FromQuery] int limit = 100, [FromQuery] int offset = 0, [FromQuery] SmsStatusEnum? status = null, [FromQuery] string? countryCode = null)
    {
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));

        var tenantInfo = tenantRetriever.CurrentTenantInfo;

        var entities = await repository.GetAsync(
            tenantInfo.Tenant,
            fromDate,
            toDate,
            limit,
            offset,
            status,
            countryCode
        );

        if (entities == null || entities.Count == 0)
            return NotFound("No SMS records found for the specified criteria.");


        return Ok(entities);
    }

    [HttpGet("{smsId}")]
    [ProducesResponseType(typeof(SmsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Get([FromServices] ITenantRetriever tenantRetriever, [FromServices] ISmsRepository repository, string smsId)
    {
        if (string.IsNullOrWhiteSpace(smsId))
            return BadRequest("SMS ID cannot be null or empty.");

        var tenantInfo = tenantRetriever.CurrentTenantInfo;
        var entity = await repository.GetAsync(smsId, tenantInfo.Tenant);
        if (entity == null)
            return NotFound($"SMS with ID {smsId} not found.");
        return Ok(entity);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SmsModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Post([FromServices] ITenantRetriever tenantRetriever, [FromServices] ISmsRepository repository, [FromBody] SmsModel sms)
    {

        var tenantInfo = tenantRetriever.CurrentTenantInfo;
        if (sms == null)
            return BadRequest("SMS model cannot be null.");

        var createdSms = await repository.CreateAsync(sms, tenantInfo.Tenant);
        return CreatedAtAction(nameof(Get), new { smsId = createdSms.Id }, createdSms);
    }

    [HttpPut("{smsId}")]
    [ProducesResponseType(typeof(SmsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Put([FromServices] ITenantRetriever tenantRetriever, [FromServices] ISmsRepository repository, string smsId, [FromBody] SmsModel sms)
    {
        var tenantInfo = tenantRetriever.CurrentTenantInfo;
        if (string.IsNullOrWhiteSpace(smsId))
            return BadRequest("SMS ID cannot be null or empty.");
        if (sms == null)
            return BadRequest("SMS model cannot be null.");
        sms.Id = smsId; // Ensure the ID is set for the update

        var existingSms = await repository.GetAsync(smsId, tenantInfo.Tenant);
        if (existingSms == null)
            return NotFound($"SMS with ID {smsId} not found.");

        var updatedSms = await repository.UpdateAsync(smsId, sms, tenantInfo.Tenant);
        return Ok(updatedSms);
    }

    [HttpPatch("{smsId}")]
    [PatchRequestBody(typeof(SmsModel))]
    [ProducesResponseType(typeof(SmsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Patch([FromServices] ITenantRetriever tenantRetriever, [FromServices] ISmsRepository repository, string smsId, [FromBody] JsonElement patchDoc)
    {
        var tenantInfo = tenantRetriever.CurrentTenantInfo;
        if (string.IsNullOrWhiteSpace(smsId))
            return BadRequest("SMS ID cannot be null or empty.");
        if (patchDoc.ValueKind != JsonValueKind.Object)
            return BadRequest("Patch document must be a JSON object.");
        var existingSms = await repository.GetAsync(smsId, tenantInfo.Tenant);
        if (existingSms == null)
            return NotFound($"SMS with ID {smsId} not found.");
        var updatedSms = await repository.PatchAsync(smsId, patchDoc, tenantInfo.Tenant);
        if (updatedSms == null)
            return BadRequest("Failed to apply patch document.");
        return Ok(updatedSms);
    }

    [HttpDelete("{smsId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> Delete([FromServices] ITenantRetriever tenantRetriever, [FromServices] ISmsRepository repository, string smsId)
    {
        var tenantInfo = tenantRetriever.CurrentTenantInfo;
        if (string.IsNullOrWhiteSpace(smsId))
            return BadRequest("SMS ID cannot be null or empty.");
        var deleted = await repository.DeleteAsync(smsId, tenantInfo.Tenant);
        if (!deleted)
            return NotFound($"SMS with ID {smsId} not found or could not be deleted.");
        return NoContent();
    }
}
