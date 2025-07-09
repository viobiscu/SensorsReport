using Microsoft.AspNetCore.Mvc;
using SensorsReport;
using System.Text.Json;

namespace Sensors_Report_Email.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailRepository _emailRepository;
        private readonly ILogger<EmailsController> _logger;
        private readonly IQueueService _queueService;

        public EmailsController(IEmailRepository emailRepository, ILogger<EmailsController> logger, IQueueService queueService)
        {
            _emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
        }

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

            var createdEmail = await _emailRepository.CreateAsync(email);

            if (createdEmail == null)
            {
                _logger.LogError("Failed to create email record.");
                return BadRequest("Failed to create email record.");
            }

            _logger.LogInformation("Email record created with ID: {Id}", createdEmail.Id);

            await this._queueService.QueueEmailAsync(createdEmail);
            _logger.LogInformation("Email record with ID: {Id} queued for processing.", createdEmail.Id);
            await _emailRepository.UpdateStatusAsync(createdEmail.Id, EmailStatusEnum.Queued);
            _logger.LogInformation("Email record with ID: {Id} status updated to Queued.", createdEmail.Id);

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
            var email = await _emailRepository.GetAsync(id);
            if (email == null)
                return NotFound();
            return Ok(email);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EmailModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] string? fromDate, [FromQuery] string? toDate, [FromQuery] int limit = 100, [FromQuery] int offset = 0, [FromQuery] EmailStatusEnum? status = null)
        {
            var emails = await _emailRepository.GetAllAsync(fromDate, toDate, limit, offset, status);
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
            var existingEmail = await _emailRepository.GetAsync(id);
            if (existingEmail == null)
                return NotFound();

            var updatedEmail = await _emailRepository.UpdateAsync(id, emailIn);
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
            var patchedEmail = await _emailRepository.PatchAsync(id, patchDoc);
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
            var success = await _emailRepository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }

    public class EmailRecord
    {
        public string Id { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
