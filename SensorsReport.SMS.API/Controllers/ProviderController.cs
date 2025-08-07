using Microsoft.AspNetCore.Mvc;
using SensorsReport.SMS.API.Models;
using SensorsReport.SMS.API.Repositories;

namespace SensorsReport.SMS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProviderController : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ProviderModel), 201)]
    [ProducesResponseType(typeof(JsonErrorResponse), 400)]
    [ProducesResponseType(typeof(JsonErrorResponse), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> RegisterProvider([FromServices] IProviderRepository providerRepository, [FromBody] ProviderModel provider)
    {
        ArgumentNullException.ThrowIfNull(providerRepository, nameof(providerRepository));
        if (provider == null)
            return BadRequest("Provider model cannot be null.");

        var createdProvider = await providerRepository.CreateAsync(provider);
        return CreatedAtAction(nameof(GetByName), new { providerName = createdProvider.Name }, createdProvider);
    }

    [HttpGet("{providerName}")]
    [ProducesResponseType(typeof(ProviderModel), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), 404)]
    [ProducesResponseType(typeof(JsonErrorResponse), 400)]
    [ProducesResponseType(typeof(JsonErrorResponse), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetByName([FromServices] IProviderRepository providerRepository, string providerName)
    {
        ArgumentNullException.ThrowIfNull(providerRepository, nameof(providerRepository));
        if (string.IsNullOrWhiteSpace(providerName))
            return BadRequest("Provider Name cannot be null or empty.");

        var provider = await providerRepository.GetByNameAsync(providerName);
        if (provider == null)
            return NotFound($"Provider with Name '{providerName}' not found.");

        return Ok(provider);
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(ProviderModel), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), 404)]
    [ProducesResponseType(typeof(JsonErrorResponse), 400)]
    [ProducesResponseType(typeof(JsonErrorResponse), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetProviderStatus([FromServices] IProviderRepository providerRepository, ProviderStatusEnum status)
    {
        ArgumentNullException.ThrowIfNull(providerRepository, nameof(providerRepository));
        if (!Enum.IsDefined(typeof(ProviderStatusEnum), status))
            return BadRequest($"Invalid provider status '{status}'.");
        var provider = await providerRepository.GetByStatusAsync(status);
        if (provider == null)
            return NotFound($"Provider with status '{status}' not found.");

        return Ok(provider);
    }

    [HttpGet("next/{provider}")]
    [ProducesResponseType(typeof(SmsModel), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), 404)]
    [ProducesResponseType(typeof(JsonErrorResponse), 400)]
    [ProducesResponseType(typeof(JsonErrorResponse), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetNextProvider([FromServices] ISmsRepository repository, [FromServices] IProviderRepository providerRepository, string provider)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));
        ArgumentNullException.ThrowIfNull(providerRepository, nameof(providerRepository));
        if (string.IsNullOrWhiteSpace(provider))
            return BadRequest("Provider cannot be null or empty.");

        await providerRepository.UpdateLastSeenAsync(provider);

        var providerSupportedCountryCodes = (await providerRepository.GetByNameAsync(provider))?.SupportedCountryCodes ?? [];

        var entity = await repository.GetNextAsync(
            provider,
            [..providerSupportedCountryCodes]
        );

        if (entity == null)
            return NotFound($"No SMS records found with provider '{provider}'.");

        return Ok(entity);
    }

    [HttpGet("next/{provider}/{CountryCode}")]
    [ProducesResponseType(typeof(SmsModel), 200)]
    [ProducesResponseType(typeof(JsonMessageResponse), 404)]
    [ProducesResponseType(typeof(JsonErrorResponse), 400)]
    [ProducesResponseType(typeof(JsonErrorResponse), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetNextCountryCode([FromServices] ISmsRepository repository, [FromServices] IProviderRepository providerRepository, string provider, string countryCode)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));
        ArgumentNullException.ThrowIfNull(providerRepository, nameof(providerRepository));
        if (string.IsNullOrWhiteSpace(countryCode))
            return BadRequest("Provider cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(provider))
            return BadRequest("Provider cannot be null or empty.");

        await providerRepository.UpdateLastSeenAsync(provider);

        var entity = await repository.GetNextAsync(
            provider,
            [countryCode]
        );

        if (entity == null)
            return NotFound($"No SMS records found with country code '{countryCode}'.");

        return Ok(entity);
    }
}
