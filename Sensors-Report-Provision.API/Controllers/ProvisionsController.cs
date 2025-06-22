using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Sensors_Report_Provision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProvisionsController : ControllerBase
    {
        // In-memory store for demonstration
        private static readonly ConcurrentDictionary<string, Provision> _provisions = new();

        // GET /api/Provisions
        [HttpGet]
        public IActionResult Get([FromQuery] string? q = null)
        {
            // Simulate Quantum Leap style query handling
            var result = string.IsNullOrEmpty(q)
                ? _provisions.Values
                : _provisions.Values.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
            return Ok(result);
        }

        // POST /api/Provisions
        [HttpPost]
        public IActionResult Post([FromBody] Provision provision)
        {
            if (string.IsNullOrWhiteSpace(provision.Id))
                provision.Id = Guid.NewGuid().ToString();
            _provisions[provision.Id] = provision;
            return CreatedAtAction(nameof(GetById), new { ProvisionsId = provision.Id }, provision);
        }

        // GET /api/Provisions/{ProvisionsId}
        [HttpGet("{ProvisionsId}")]
        public IActionResult GetById(string ProvisionsId)
        {
            if (_provisions.TryGetValue(ProvisionsId, out var provision))
                return Ok(provision);
            return NotFound();
        }
    }

    public class Provision
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        // Add more fields as needed for your domain
    }
}
