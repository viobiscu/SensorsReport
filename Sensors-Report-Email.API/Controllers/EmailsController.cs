using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Sensors_Report_Email.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailsController : ControllerBase
    {
        // In-memory store for demonstration
        private static readonly List<EmailRecord> Emails = new List<EmailRecord>();

        // GET /api/Emails?fromDate=...&toDate=...&limit=...&offset=...
        [HttpGet]
        [Authorize]
        public IActionResult Get([FromQuery] string? fromDate, [FromQuery] string? toDate, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var query = Emails.AsQueryable();
            // Simulate Quantum Leap-style query params
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
                query = query.Where(e => e.Timestamp >= from);
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
                query = query.Where(e => e.Timestamp <= to);
            if (offset.HasValue)
                query = query.Skip(offset.Value);
            if (limit.HasValue)
                query = query.Take(limit.Value);
            return Ok(query.ToList());
        }

        // POST /api/Emails
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] EmailRecord email)
        {
            email.Id = Guid.NewGuid().ToString();
            email.Timestamp = DateTime.UtcNow;
            Emails.Add(email);
            return CreatedAtAction(nameof(GetById), new { EmailId = email.Id }, email);
        }

        // GET /api/Emails/{EmailId}
        [HttpGet("{EmailId}")]
        [Authorize]
        public IActionResult GetById(string EmailId)
        {
            var email = Emails.FirstOrDefault(e => e.Id == EmailId);
            if (email == null)
                return NotFound();
            return Ok(email);
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
