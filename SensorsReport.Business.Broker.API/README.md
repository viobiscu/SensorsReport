# SensorsReport.Business.Broker.API

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-supported-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](../LICENSE)

## Overview

The SensorsReport.Business.Broker.API is a microservice that acts as a business rule broker for the SensorsReport IoT platform. It receives FIWARE NGSI-LD notifications from the Orion Context Broker, applies business rules using a rules engine, and orchestrates actions based on the evaluated rules. This service enables dynamic business logic processing for IoT data without requiring code changes.

## Features

- **FIWARE NGSI-LD Integration**: Receives and processes notifications from Orion Context Broker
- **Dynamic Business Rules**: Uses RulesEngine for configurable business logic evaluation
- **Multi-tenant Support**: Handles tenant-specific rules and data isolation
- **Audit Logging**: Comprehensive audit trail for all business rule executions
- **Health Monitoring**: Built-in health checks for service monitoring
- **Orion Integration**: Bidirectional communication with FIWARE Orion Context Broker
- **Keycloak Authentication**: Integrated security with Keycloak for API access

## Technology Stack

- **.NET 8.0**: Core framework for high-performance web APIs
- **ASP.NET Core**: Web framework for RESTful API development
- **RulesEngine**: Business rules engine for dynamic rule evaluation
- **NLog**: Structured logging framework
- **Docker**: Containerization for deployment
- **Kubernetes**: Orchestration with Flux GitOps
- **FIWARE NGSI-LD**: Standard for context information management

## Project Structure

```
SensorsReport.Business.Broker.API/
├── Configuration/
│   └── AppConfig.cs                    # Application configuration settings
├── Controllers/
│   └── NotificationController.cs       # Main API controller for notifications
├── Models/
│   └── Models.cs                       # Data models and DTOs
├── Services/
│   ├── OrionService.cs                 # Orion Context Broker integration
│   └── AuditService.cs                 # Audit logging service
├── Properties/
├── flux/                               # Kubernetes deployment manifests
├── Dockerfile                          # Container build configuration
├── Program.cs                          # Application entry point and DI setup
├── appsettings.json                    # Default configuration
├── nlog.config                         # Logging configuration
└── kubernetes.yaml                     # Kubernetes deployment
```

## Setup and Installation

### Prerequisites

- .NET 8.0 SDK
- Docker (for containerized deployment)
- Access to FIWARE Orion Context Broker
- Access to SensorsReport Audit API
- Keycloak instance (for authentication)

### Local Development

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd SensorsReport/SensorsReport.Business.Broker.API
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure environment variables** (see Configuration section)

4. **Run the application**:
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5000` (or configured port).

### Docker Deployment

1. **Build the Docker image**:
   ```bash
   docker build -t sensorsreport-business-broker-api .
   ```

2. **Run the container**:
   ```bash
   docker run -p 80:80 \
     -e SR_BB_ORION_HOST=your-orion-host \
     -e SR_BB_AUDIT_URL=your-audit-url \
     sensorsreport-business-broker-api
   ```

## Configuration

The service uses environment variables for configuration:

### Required Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `SR_BB_ORION_HOST` | Orion Context Broker host | `orion.sensorsreport.net` |
| `SR_BB_ORION_PORT` | Orion Context Broker port | `31026` |
| `SR_BB_AUDIT_URL` | Audit API host | `localhost` |
| `SR_BB_AUDIT_PORT` | Audit API port | `80` |
| `SR_BB_KEYCLOAK_RELM` | Keycloak realm | `sr` |
| `SR_BB_KEYCLOAK_CLIENTID` | Keycloak client ID | `ContextBroker` |
| `SR_BB_KEYCLOAK_CLIENTSECRET` | Keycloak client secret | (default provided) |

### Example Environment Configuration

```bash
# Orion Context Broker
export SR_BB_ORION_HOST=orion.example.com
export SR_BB_ORION_PORT=1026

# Audit Service
export SR_BB_AUDIT_URL=audit-api.example.com
export SR_BB_AUDIT_PORT=80

# Keycloak
export SR_BB_KEYCLOAK_RELM=sensorsreport
export SR_BB_KEYCLOAK_CLIENTID=business-broker
export SR_BB_KEYCLOAK_CLIENTSECRET=your-secret-key
```

## API Reference

### Main Endpoint

#### POST /v1/notify
Receives FIWARE NGSI-LD notifications and processes them through business rules.

**Headers:**
- `Content-Type: application/ld+json`
- `NGSILD-Tenant: <tenant-id>` (for multi-tenancy)
- `Fiware-ServicePath: <service-path>` (for location context)

**Request Body:**
```json
{
  "id": "notification-id",
  "type": "Notification",
  "subscriptionId": "subscription-id",
  "notifiedAt": "2024-01-01T12:00:00Z",
  "data": [
    {
      "id": "entity-id",
      "type": "SensorData",
      "temperature": {
        "type": "Property",
        "value": 25.5
      }
    }
  ]
}
```

**Response:**
- `200 OK`: Notification processed successfully
- `400 Bad Request`: Invalid notification format
- `500 Internal Server Error`: Processing error

### Health Check

#### GET /health
Returns the health status of the service.

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.001"
}
```

## Business Rules Engine

The service uses the RulesEngine library to evaluate business rules dynamically:

### Rule Configuration
Rules are retrieved from the Orion Context Broker as `BusinessRule` entities:

```json
{
  "id": "temperature-alert-rule",
  "type": "BusinessRule",
  "ruleName": {
    "type": "Property",
    "value": "HighTemperatureAlert"
  },
  "ruleContent": {
    "type": "Property",
    "value": "input1.temperature > 30"
  },
  "targetEntityType": {
    "type": "Property",
    "value": "SensorData"
  },
  "enabled": {
    "type": "Property",
    "value": true
  },
  "priority": {
    "type": "Property",
    "value": 1
  }
}
```

### Rule Execution
1. Notification received from Orion
2. Business rules retrieved for tenant/entity type
3. Rules evaluated against notification data
4. Actions triggered based on rule results
5. Audit log created for rule execution

## Usage Examples

### Processing Temperature Alerts

When a temperature sensor sends data exceeding a threshold:

1. **Orion sends notification**:
   ```json
   {
     "id": "temp-notification",
     "type": "Notification",
     "data": [{
       "id": "sensor-001",
       "type": "TemperatureSensor",
       "temperature": {
         "type": "Property",
         "value": 35.2
       }
     }]
   }
   ```

2. **Business rule evaluates**:
   - Rule: `input1.temperature > 30`
   - Result: `true` (temperature is 35.2)

3. **Action triggered**:
   - Create alarm entity in Orion
   - Send notification to alarm service
   - Log audit record

### Multi-tenant Rule Processing

Different tenants can have different business rules for the same entity types:

```bash
# Tenant A notification
curl -X POST http://localhost/v1/notify \
  -H "Content-Type: application/ld+json" \
  -H "NGSILD-Tenant: tenant-a" \
  -d '{"data": [{"temperature": 25}]}'

# Tenant B notification  
curl -X POST http://localhost/v1/notify \
  -H "Content-Type: application/ld+json" \
  -H "NGSILD-Tenant: tenant-b" \
  -d '{"data": [{"temperature": 25}]}'
```

Each tenant's rules are evaluated independently.

## Integration

### FIWARE Orion Context Broker

The service integrates with Orion as both a notification consumer and entity manager:

- **Receives notifications** via subscription webhooks
- **Queries business rules** from Orion entities
- **Updates entities** when rules trigger actions

### SensorsReport Audit API

All business rule executions are logged:

```json
{
  "entityId": "sensor-001",
  "action": "BusinessRuleEvaluation",
  "ruleName": "HighTemperatureAlert",
  "result": "Success",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### Message Flow

```
[Sensor] → [Orion] → [Business Broker] → [Rules Engine]
                            ↓
[Audit API] ← [Action Services] ← [Rule Results]
```

## Monitoring and Observability

### Health Checks
- **Endpoint**: `/health`
- **Monitoring**: Kubernetes liveness/readiness probes
- **Status**: Service health and dependencies

### Logging
- **Framework**: NLog with structured logging
- **Levels**: Info, Warning, Error, Debug
- **Format**: JSON with correlation IDs
- **Destinations**: Console, file, external systems

### Metrics
Key metrics to monitor:
- Notification processing rate
- Rule evaluation time
- Rule execution success/failure rate
- Memory and CPU usage
- HTTP request/response metrics

## Error Handling

### Notification Processing Errors
- Invalid notification format → 400 Bad Request
- Rule evaluation failure → Logged and continue
- Orion communication error → Retry with backoff
- Audit logging failure → Warning logged

### Resilience Patterns
- **Circuit Breaker**: For external service calls
- **Retry Policy**: For transient failures
- **Timeout Handling**: For long-running operations
- **Graceful Degradation**: Continue with limited functionality

## Performance

### Optimization Strategies
- **Asynchronous Processing**: Non-blocking notification handling
- **Rule Caching**: Cache frequently used business rules
- **Connection Pooling**: HTTP client connection management
- **Memory Management**: Efficient object lifecycle

### Scalability
- **Horizontal Scaling**: Multiple service instances
- **Load Balancing**: Kubernetes service distribution
- **Resource Limits**: Configured CPU/memory constraints
- **Auto-scaling**: Based on CPU/memory usage

## Security

### Authentication
- **Keycloak Integration**: OAuth 2.0/OpenID Connect
- **Client Credentials**: Service-to-service authentication
- **Token Validation**: JWT token verification

### Authorization
- **Tenant Isolation**: Rules evaluated per tenant
- **Service Path**: Location-based access control
- **API Security**: Protected endpoints

### Data Protection
- **Encryption**: TLS for all communications
- **Audit Trail**: Complete operation logging
- **Secure Configuration**: Environment-based secrets

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="RulesEngine" Version="5.0.6" />
```

### External Services
- **FIWARE Orion Context Broker**: Entity and notification management
- **SensorsReport Audit API**: Audit logging
- **Keycloak**: Authentication and authorization

## Related Services

- **SensorsReport.Alarm.API**: Processes alarm-related rule results
- **SensorsReport.Audit.API**: Logs all business rule executions
- **SensorsReport.NotificationRule.API**: Manages notification rules
- **SensorsReport.Email.API**: Sends email notifications from rules
- **SensorsReport.SMS.API**: Sends SMS notifications from rules

## Deployment

### Kubernetes Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: business-broker-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: business-broker-api
  template:
    spec:
      containers:
      - name: business-broker-api
        image: sensorsreport/business-broker-api:latest
        ports:
        - containerPort: 80
        env:
        - name: SR_BB_ORION_HOST
          value: "orion.default.svc.cluster.local"
```

### GitOps with Flux
The service uses Flux for GitOps deployment:
- **Source**: Git repository monitoring
- **Kustomization**: Environment-specific configurations
- **Auto-deployment**: Automated rollouts and rollbacks

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow .NET coding standards
- Write unit tests for new features
- Update documentation for API changes
- Ensure Docker builds succeed
- Test with real FIWARE Orion notifications

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## Support

For support and questions:

- **Issues**: GitHub Issues for bug reports and feature requests
- **Documentation**: Additional docs in the `/docs` directory
- **Community**: SensorsReport community channels

## Changelog

### Version 1.0.0
- Initial release with business rules engine
- FIWARE NGSI-LD notification processing
- Multi-tenant rule evaluation
- Orion Context Broker integration
- Audit logging integration
- Kubernetes deployment support
