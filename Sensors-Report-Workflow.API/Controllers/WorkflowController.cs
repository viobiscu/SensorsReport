using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Sensors_Report_Workflow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        // In-memory store for demonstration
        private static readonly ConcurrentDictionary<string, Workflow> _workflows = new();

        // GET /api/Workflow
        [HttpGet]
        public IActionResult Get([FromQuery] string? q = null)
        {
            // Simulate Quantum Leap style query handling
            var result = string.IsNullOrEmpty(q)
                ? _workflows.Values
                : _workflows.Values.Where(w => w.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
            return Ok(result);
        }

        // POST /api/Workflow
        [HttpPost]
        public IActionResult Post([FromBody] Workflow workflow)
        {
            if (string.IsNullOrWhiteSpace(workflow.Id))
                workflow.Id = Guid.NewGuid().ToString();
            _workflows[workflow.Id] = workflow;
            return CreatedAtAction(nameof(GetById), new { WorkflowId = workflow.Id }, workflow);
        }

        // GET /api/Workflow/{WorkflowId}
        [HttpGet("{WorkflowId}")]
        public IActionResult GetById(string WorkflowId)
        {
            if (_workflows.TryGetValue(WorkflowId, out var workflow))
                return Ok(workflow);
            return NotFound();
        }
    }

    public class Workflow
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        // Add more fields as needed for your domain
    }
}
