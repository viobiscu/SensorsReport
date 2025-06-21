using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Sensors_Report_SMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SMSController : ControllerBase
    {
        // In-memory store for demonstration
        private static readonly List<SMSRecord> SMSs = new List<SMSRecord>();

        // GET /api/SMS?fromDate=...&toDate=...&limit=...&offset=...
        [HttpGet]
        [Authorize]
        public IActionResult Get([FromQuery] string? fromDate, [FromQuery] string? toDate, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var query = SMSs.AsQueryable();
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

        // POST /api/SMS
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] SMSRecord sms)
        {
            sms.Id = Guid.NewGuid().ToString();
            sms.Timestamp = DateTime.UtcNow;
            SMSs.Add(sms);
            return CreatedAtAction(nameof(GetById), new { SMSId = sms.Id }, sms);
        }

        // GET /api/SMS/{SMSId}
        [HttpGet("{SMSId}")]
        [Authorize]
        public IActionResult GetById(string SMSId)
        {
            var sms = SMSs.FirstOrDefault(e => e.Id == SMSId);
            if (sms == null)
                return NotFound();
            return Ok(sms);
        }
    }

    public class SMSRecord
    {
        public string Id { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
