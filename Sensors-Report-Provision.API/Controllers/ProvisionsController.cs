using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sensors_Report_Provision.API.Services;

namespace Sensors_Report_Provision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvisionsController(
        IQuantumLeapService quantumLeapService,
        IOrionContextBrokerService orionContextBrokerService,
        ILogger<ProvisionsController> logger,
        IOptions<AppConfig> config) : ControllerBase
    {
        private readonly IQuantumLeapService _quantumLeapService = quantumLeapService
            ?? throw new ArgumentNullException(nameof(quantumLeapService));
        private readonly IOrionContextBrokerService _orionContextBrokerService = orionContextBrokerService
            ?? throw new ArgumentNullException(nameof(orionContextBrokerService));
        private readonly ILogger<ProvisionsController> _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
        private readonly AppConfig _config = config?.Value
            ?? throw new ArgumentNullException(nameof(config));


    }

    public class Provision
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
