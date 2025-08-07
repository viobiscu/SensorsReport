using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SensorsReport;
using System.Text.Json;
using SensorsReport.Api.Core.MassTransit;

namespace SensorsReport.Email.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailsController(IEmailRepository emailRepository, ILogger<EmailsController> logger, IEventBus eventBus) : ControllerBase
    {
        private readonly IEmailRepository emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
        private readonly ILogger<EmailsController> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IEventBus eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        [HttpPost]
        [ProducesResponseType(typeof(EmailModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] EmailModel email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            email.Status = EmailStatusEnum.Pending;
            email.RetryCount = 0;

            var createdEmail = await emailRepository.CreateAsync(email);

            if (createdEmail == null)
            {
                logger.LogError("Failed to create email record.");
                return BadRequest("Failed to create email record.");
            }

            logger.LogInformation("Email record created with ID: {Id}", createdEmail.Id);

            logger.LogInformation("Email record with ID: {Id} queued for processing.", createdEmail.Id);
            await emailRepository.UpdateStatusAsync(createdEmail.Id, EmailStatusEnum.Queued);
            await eventBus.PublishAsync(new EmailCreatedEvent
            {
                Id = createdEmail.Id
            });
            logger.LogInformation("Email record with ID: {Id} status updated to Queued.", createdEmail.Id);
            return CreatedAtAction(nameof(Get), new { id = createdEmail.Id }, createdEmail);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmailModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string id)
        {
            var email = await emailRepository.GetAsync(id);
            if (email == null)
                return NotFound();
            return Ok(email);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EmailModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] string? fromDate, [FromQuery] string? toDate, [FromQuery] int limit = 100, [FromQuery] int offset = 0, [FromQuery] EmailStatusEnum? status = null)
        {
            var emails = await emailRepository.GetAllAsync(fromDate, toDate, limit, offset, status);
            return Ok(emails);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmailModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Put(string id, [FromBody] EmailModel emailIn)
        {
            var existingEmail = await emailRepository.GetAsync(id);
            if (existingEmail == null)
                return NotFound();

            var updatedEmail = await emailRepository.UpdateAsync(id, emailIn);
            return Ok(updatedEmail);
        }

        [HttpPatch("{id}")]
        [PatchRequestBody(typeof(EmailModel))]
        [ProducesResponseType(typeof(EmailModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonElement patchDoc)
        {
            var patchedEmail = await emailRepository.PatchAsync(id, patchDoc);
            return Ok(patchedEmail);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(JsonMessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(JsonErrorResponse), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await emailRepository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
