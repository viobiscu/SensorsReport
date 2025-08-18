# SensorsReport.Audit

A .NET 8.0 library and CLI tool for auditing and logging user activities within the SensorsReport ecosystem. This library provides a standardized way to capture, authenticate, and store audit events in QuantumLeap time-series database with Keycloak authentication integration.

## Overview

SensorsReport.Audit enables applications within the SensorsReport microservices architecture to log audit events for compliance, security, and monitoring purposes. It provides both a programmatic API and a command-line interface for recording user actions, system events, and data access patterns.

## Features

### Core Functionality
- **Audit Logging**: Standardized audit event capture and storage
- **QuantumLeap Integration**: Direct integration with QuantumLeap time-series database
- **Keycloak Authentication**: JWT token validation and service authentication
- **NGSI-LD Compliance**: Audit records stored in NGSI-LD format
- **Flexible Configuration**: Environment-based or programmatic configuration

### Authentication & Security
- **JWT Token Validation**: Validates user tokens against Keycloak
- **Service Authentication**: Client credentials flow for service-to-service auth
- **Token Introspection**: Validates and extracts user information from tokens
- **Secure Configuration**: Environment variable-based sensitive data handling

### Data Models
- **AuditRecord**: Structured audit event representation
- **QuantumLeapEntity**: NGSI-LD compliant entity format
- **AttributeValue**: Flexible attribute system for audit metadata
- **User Context**: Extracted user information from JWT tokens

## Technology Stack

- **.NET 8.0**: Target framework
- **QuantumLeap**: Time-series database for audit storage
- **Keycloak**: Identity and access management
- **JWT Bearer**: Token-based authentication
- **Newtonsoft.Json**: JSON serialization
- **NGSI-LD**: Context information management standard

## Project Structure

```
SensorsReport.Audit/
├── SensorsReportAudit.cs        # Main audit library class
├── AuditConfig.cs               # Configuration management
├── Auth/
│   └── KeycloakAuthService.cs   # Keycloak authentication service
├── Models/
│   └── QuantumLeapModels.cs     # Data models for audit records
├── cli/
│   ├── Program.cs               # Command-line interface
│   └── config.json              # CLI configuration template
└── nlog.config                  # Logging configuration
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Access to QuantumLeap instance
- Access to Keycloak instance
- Valid Keycloak client credentials

### Installation

#### As a Library
Add the project reference to your application:

```xml
<ProjectReference Include="../SensorsReport.Audit/SensorsReport.Audit.csproj" />
```

#### As a CLI Tool
Build and run the command-line interface:

```bash
cd SensorsReport.Audit/cli
dotnet run -- --token <JWT_TOKEN> --location <LOCATION> --action <ACTION>
```

## Configuration

### Environment Variables

Set the following environment variables for automatic configuration:

```bash
# QuantumLeap Configuration
export SR_AUDIT_QUANTUMLEAP_HOST="quantum.sensorsreport.net"
export SR_AUDIT_QUANTUMLEAP_PORT="8668"

# Keycloak Configuration
export SR_AUDIT_KEYCLOAK_URL="keycloak.sensorsreport.net"
export SR_AUDIT_KEYCLOAK_PORT="30100"
export SR_AUDIT_KEYCLOAK_REALM="sr"
export SR_AUDIT_KEYCLOAK_CLIENTID="ContextBroker"
export SR_AUDIT_KEYCLOAK_CLIENTSECRET="your-client-secret"
```

### Programmatic Configuration

```csharp
var config = new AuditConfig
{
    QuantumLeapHost = "quantum.sensorsreport.net",
    QuantumLeapPort = "8668",
    KeycloakUrl = "keycloak.sensorsreport.net",
    KeycloakPort = "30100",
    KeycloakRealm = "sr",
    KeycloakClientId = "ContextBroker",
    KeycloakClientSecret = "your-client-secret"
};
```

## Usage

### Library Usage

#### Basic Audit Logging
```csharp
using SensorsReportAudit;
using Microsoft.Extensions.Logging;

// Initialize with environment configuration
var logger = LoggerFactory.Create(builder => builder.AddConsole())
    .CreateLogger<SensorsReportAudit>();

var auditService = new SensorsReportAudit(logger);

// Log an audit event
await auditService.LogAuditAsync(
    userToken: "jwt-token-here",
    actionType: "READ",
    resourceId: "sensor-001",
    resourceType: "Device",
    details: "User accessed sensor data"
);
```

#### Custom Configuration
```csharp
var config = new AuditConfig
{
    QuantumLeapHost = "custom-quantum-host",
    QuantumLeapPort = "8668",
    // ... other settings
};

var auditService = new SensorsReportAudit(config, logger);
```

#### Advanced Audit Record
```csharp
var auditRecord = new AuditRecord
{
    ActionType = "UPDATE",
    UserId = "user-123",
    UserName = "john.doe",
    ResourceId = "alarm-rule-456",
    ResourceType = "AlarmRule",
    Details = "Modified temperature threshold",
    Timestamp = DateTime.UtcNow,
    Location = "Building A",
    TenantId = "tenant-789"
};

await auditService.LogAuditRecordAsync("jwt-token", auditRecord);
```

### CLI Usage

#### Basic Command
```bash
dotnet run -- \
    --token "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..." \
    --location "Building A" \
    --action "READ" \
    --details "User viewed sensor dashboard"
```

#### With Custom Configuration
```bash
dotnet run -- \
    --token "jwt-token-here" \
    --location "Data Center" \
    --action "DELETE" \
    --details "Removed old sensor data" \
    --config "/path/to/config.json"
```

## API Reference

### SensorsReportAudit Class

#### Constructor
```csharp
// Default configuration from environment
SensorsReportAudit(ILogger<SensorsReportAudit>? logger = null)

// Custom configuration
SensorsReportAudit(AuditConfig config, ILogger<SensorsReportAudit>? logger = null)
```

#### Methods
```csharp
// Log audit event with basic parameters
Task LogAuditAsync(string userToken, string actionType, string resourceId, 
    string resourceType, string? details = null)

// Log detailed audit record
Task LogAuditRecordAsync(string userToken, AuditRecord auditRecord)
```

### AuditConfig Class

#### Properties
- `QuantumLeapHost`: QuantumLeap server hostname
- `QuantumLeapPort`: QuantumLeap server port
- `KeycloakUrl`: Keycloak server URL
- `KeycloakRealm`: Keycloak realm name
- `KeycloakClientId`: OAuth2 client identifier
- `KeycloakClientSecret`: OAuth2 client secret

#### Methods
```csharp
// Create configuration from environment variables
static AuditConfig FromEnvironment()
```

## Error Handling

The library provides comprehensive error handling and logging:

```csharp
try
{
    await auditService.LogAuditAsync(token, "READ", "sensor-001", "Device");
}
catch (HttpRequestException ex)
{
    // Handle network/API errors
    logger.LogError(ex, "Failed to send audit log to QuantumLeap");
}
catch (UnauthorizedAccessException ex)
{
    // Handle authentication errors
    logger.LogError(ex, "Invalid or expired token");
}
```

## Integration Examples

### ASP.NET Core Middleware
```csharp
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SensorsReportAudit _auditService;

    public AuditMiddleware(RequestDelegate next, SensorsReportAudit auditService)
    {
        _next = next;
        _auditService = auditService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Substring("Bearer ".Length);
        
        await _next(context);
        
        if (!string.IsNullOrEmpty(token))
        {
            await _auditService.LogAuditAsync(
                token,
                context.Request.Method,
                context.Request.Path,
                "API",
                $"Status: {context.Response.StatusCode}"
            );
        }
    }
}
```

### Dependency Injection
```csharp
// Program.cs
builder.Services.AddSingleton<SensorsReportAudit>();

// Controller
[ApiController]
public class SensorsController : ControllerBase
{
    private readonly SensorsReportAudit _auditService;

    public SensorsController(SensorsReportAudit auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSensor(string id)
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Substring("Bearer ".Length);
        
        // ... business logic ...
        
        await _auditService.LogAuditAsync(token, "READ", id, "Sensor");
        return Ok(result);
    }
}
```

## Monitoring and Troubleshooting

### Logging
The library uses structured logging with NLog. Configure logging levels in `nlog.config`:

```xml
<rules>
    <logger name="SensorsReportAudit.*" minlevel="Info" writeTo="console" />
</rules>
```

### Health Checks
Monitor the health of dependent services:
- **QuantumLeap**: Verify connectivity to time-series database
- **Keycloak**: Ensure authentication service availability
- **Network**: Check firewall and DNS resolution

## Performance Considerations

- **Async Operations**: All audit operations are asynchronous
- **Batching**: Consider batching multiple audit events for high-throughput scenarios
- **Caching**: Authentication tokens are cached to reduce Keycloak calls
- **Error Resilience**: Failed audit attempts are logged but don't block application flow

## Security Best Practices

1. **Environment Variables**: Store sensitive configuration in environment variables
2. **Token Validation**: Always validate JWT tokens before processing
3. **Least Privilege**: Use dedicated service accounts with minimal permissions
4. **Audit Integrity**: Ensure audit logs cannot be modified after creation
5. **Network Security**: Use HTTPS for all external communications

## Contributing

This library is part of the SensorsReport audit infrastructure. Changes should:
- Maintain backward compatibility
- Include comprehensive unit tests
- Follow established logging patterns
- Update documentation for new features

## Related Projects

- **SensorsReport.Audit.API**: REST API for audit data access
- **SensorsReport.Api.Core**: Core shared functionality
- **QuantumLeap**: Time-series database backend
- **Keycloak**: Identity and access management

## Support

For issues related to audit logging:
1. Check QuantumLeap and Keycloak connectivity
2. Verify environment variable configuration
3. Review application logs for detailed error messages
4. Contact the SensorsReport development team

## License

This project is part of the SensorsReport system for AerOS. See the root LICENSE file for details.
