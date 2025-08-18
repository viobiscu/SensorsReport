# SensorsReport.LogRule.API

A REST API microservice for managing log rules within the SensorsReport ecosystem. This service provides comprehensive CRUD operations for log rules and integrates with OrionLD context broker for persistent storage and multi-tenant support.

## Features

- **CRUD Operations**: Complete Create, Read, Update, Delete operations for log rules
- **Multi-Tenant Support**: Tenant-aware operations using header-based tenant identification
- **OrionLD Integration**: Seamless integration with OrionLD context broker for data persistence
- **RESTful API**: Standard HTTP methods with proper response codes and error handling
- **JSON Response Format**: Structured JSON responses with comprehensive error information
- **Pagination Support**: Configurable offset and limit parameters for list operations
- **Entity Validation**: Input validation with detailed error messages
- **Logging**: Comprehensive logging using NLog for monitoring and debugging
- **Health Checks**: Built-in health monitoring and status endpoints
- **OpenAPI Documentation**: Auto-generated API documentation via Swagger/OpenAPI

## Technology Stack

- **.NET 8.0**: Latest LTS version of .NET
- **ASP.NET Core**: Web API framework
- **OrionLD**: Context broker integration for data persistence
- **NLog**: Structured logging framework
- **Docker**: Containerization support
- **Kubernetes**: Deployment orchestration via Flux

## Project Structure

```
SensorsReport.LogRule.API/
├── Controllers/
│   └── LogRuleController.cs          # Main API controller with CRUD endpoints
├── Services/
│   └── LogRuleService.cs             # Business logic and OrionLD integration
├── Properties/
├── flux/                             # Kubernetes deployment configurations
├── Program.cs                        # Application startup and configuration
├── appsettings.json                  # Application configuration
├── Dockerfile                        # Container configuration
├── nlog.config                       # Logging configuration
└── SensorsReport.LogRule.API.csproj  # Project file
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker (for containerized deployment)
- Access to OrionLD context broker
- Valid tenant configuration

### Installation

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd SensorsReport/SensorsReport.LogRule.API
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

### Docker Deployment

1. **Build Docker image**:
   ```bash
   docker build -t sensorsreport-logrule-api .
   ```

2. **Run container**:
   ```bash
   docker run -p 8080:8080 sensorsreport-logrule-api
   ```

## Configuration

### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` | No |
| `ASPNETCORE_URLS` | Binding URLs | `http://+:8080` | No |
| `OrionLD__BaseUrl` | OrionLD context broker URL | - | Yes |
| `OrionLD__Timeout` | Request timeout in seconds | `30` | No |
| `Logging__LogLevel__Default` | Default log level | `Information` | No |

### Application Settings

Configure the application through `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "OrionLD": {
    "BaseUrl": "http://orionld:1026",
    "Timeout": 30
  },
  "AllowedHosts": "*"
}
```

## API Reference

### Base URL
```
http://localhost:8080/api/logrule
```

### Endpoints

#### Get All Log Rules
```http
GET /api/logrule?limit=100&offset=0
```

**Parameters:**
- `limit` (optional): Maximum number of records to return (default: 100)
- `offset` (optional): Number of records to skip (default: 0)

**Response:**
```json
[
  {
    "id": "log-rule-123",
    "type": "LogRule",
    "name": "Critical Error Rule",
    "description": "Rule for handling critical errors",
    "dateCreated": "2024-01-01T00:00:00Z",
    "dateModified": "2024-01-01T00:00:00Z"
  }
]
```

#### Get Log Rule by ID
```http
GET /api/logrule/{logRuleId}
```

**Response:**
```json
{
  "id": "log-rule-123",
  "type": "LogRule",
  "name": "Critical Error Rule",
  "description": "Rule for handling critical errors",
  "dateCreated": "2024-01-01T00:00:00Z",
  "dateModified": "2024-01-01T00:00:00Z"
}
```

#### Create New Log Rule
```http
POST /api/logrule
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "New Log Rule",
  "description": "Description of the log rule",
  "logLevel": "Error",
  "pattern": "*.error.*",
  "actions": ["notify", "store"]
}
```

#### Update Log Rule
```http
PUT /api/logrule/{logRuleId}
Content-Type: application/json
```

#### Partial Update Log Rule
```http
PATCH /api/logrule/{logRuleId}
Content-Type: application/json
```

#### Delete Log Rule
```http
DELETE /api/logrule/{logRuleId}
```

### Response Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 404 | Not Found |
| 500 | Internal Server Error |

### Error Response Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Log Rule ID cannot be null or empty.",
    "details": "The provided log rule identifier is invalid."
  }
}
```

## Usage Examples

### Creating a Log Rule

```bash
curl -X POST http://localhost:8080/api/logrule \
  -H "Content-Type: application/json" \
  -H "X-Tenant: tenant-123" \
  -d '{
    "name": "Security Alert Rule",
    "description": "Rule for handling security alerts",
    "logLevel": "Warning",
    "pattern": "security.*",
    "actions": ["alert", "audit"]
  }'
```

### Retrieving Log Rules

```bash
curl -X GET "http://localhost:8080/api/logrule?limit=50&offset=0" \
  -H "X-Tenant: tenant-123"
```

### Updating a Log Rule

```bash
curl -X PUT http://localhost:8080/api/logrule/log-rule-123 \
  -H "Content-Type: application/json" \
  -H "X-Tenant: tenant-123" \
  -d '{
    "name": "Updated Security Alert Rule",
    "description": "Updated rule for handling security alerts"
  }'
```

## Integration

### OrionLD Context Broker

The service integrates with OrionLD context broker for:
- **Entity Storage**: Log rules stored as NGSI-LD entities
- **Query Operations**: Advanced filtering and search capabilities
- **Multi-Tenancy**: Tenant-based data isolation
- **Event Notification**: Change notifications for rule updates

### Related Services

- **SensorsReport.LogRule.Consumer**: Processes log rule events and applies rules to incoming logs
- **SensorsReport.Audit.API**: Tracks log rule changes and access patterns
- **SensorsReport.Business.Broker.API**: Manages business rule orchestration
- **SensorsReport.Notification.API**: Handles notifications triggered by log rules

## Monitoring

### Health Checks

The service exposes health check endpoints:
- `/health`: Overall health status
- `/health/ready`: Readiness probe
- `/health/live`: Liveness probe

### Metrics

Key metrics to monitor:
- Request rate and response times
- Error rates by endpoint
- OrionLD connectivity status
- Memory and CPU usage
- Active tenant connections

### Logging

Comprehensive logging includes:
- Request/response logging
- Error and exception tracking
- Performance metrics
- Audit trail for rule changes
- OrionLD integration status

## Error Handling

### Validation Errors

The service validates:
- Required fields presence
- Data type compliance
- Business rule constraints
- Tenant access permissions

### Integration Errors

Handles OrionLD integration issues:
- Connection timeouts
- Authentication failures
- Data consistency errors
- Network connectivity issues

### Error Recovery

- Automatic retry mechanisms for transient failures
- Circuit breaker pattern for OrionLD connections
- Graceful degradation during service outages
- Comprehensive error logging and alerting

## Performance

### Optimization Features

- **Async Operations**: All I/O operations are asynchronous
- **Connection Pooling**: Efficient OrionLD connection management
- **Response Caching**: Caching for frequently accessed rules
- **Pagination**: Efficient handling of large datasets
- **Resource Cleanup**: Proper disposal of resources

### Performance Targets

- Response time: < 200ms for simple operations
- Throughput: > 1000 requests/second
- Availability: 99.9% uptime
- Concurrent users: Support for 100+ simultaneous connections

## Security

### Authentication & Authorization

- **Tenant-Based Access**: Multi-tenant data isolation
- **Header Validation**: Secure tenant identification
- **Input Sanitization**: Protection against injection attacks
- **Rate Limiting**: Prevention of abuse and DoS attacks

### Data Protection

- **Encryption in Transit**: HTTPS/TLS encryption
- **Input Validation**: Comprehensive request validation
- **Error Handling**: Secure error responses without sensitive data exposure
- **Audit Logging**: Complete audit trail for compliance

## Dependencies

### Core Dependencies

- **SensorsReport.Api.Core**: Shared API infrastructure and utilities
- **SensorsReport.OrionLD**: OrionLD context broker integration
- **Microsoft.AspNetCore**: Web API framework
- **NLog**: Logging framework

### External Services

- **OrionLD Context Broker**: Primary data storage and querying
- **Authentication Service**: User and tenant authentication
- **Monitoring System**: Health checks and metrics collection

## Related Services

### Upstream Services
- **SensorsReport.Business.Broker.API**: Rule orchestration and management
- **SensorsReport.Audit.API**: Change tracking and compliance

### Downstream Services
- **SensorsReport.LogRule.Consumer**: Event processing and rule execution
- **SensorsReport.Notification.API**: Alert and notification delivery
- **SensorsReport.Email.API**: Email notification integration

### Data Flow
1. Log rules created/updated via API
2. Changes propagated to LogRule Consumer
3. Consumer applies rules to incoming log data
4. Actions triggered based on rule matches
5. Notifications sent via appropriate channels

## Contributing

### Development Guidelines

1. **Code Style**: Follow .NET coding conventions
2. **Testing**: Include unit tests for new features
3. **Documentation**: Update API documentation for changes
4. **Logging**: Add appropriate logging for new functionality
5. **Error Handling**: Implement comprehensive error handling

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Implement changes with tests
4. Update documentation
5. Submit pull request with detailed description

### Testing

```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter Category=Integration

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

### Documentation
- API documentation available at `/swagger` when running in development mode
- Technical documentation in the `/docs` folder

### Contact
- **Issue Tracking**: GitHub Issues
- **Email Support**: support@sensorsreport.com
- **Documentation**: See project wiki for detailed guides

### Troubleshooting

Common issues and solutions:

1. **OrionLD Connection Issues**
   - Verify OrionLD service is running
   - Check network connectivity
   - Validate configuration settings

2. **Tenant Header Missing**
   - Ensure X-Tenant header is included in requests
   - Verify tenant configuration

3. **Validation Errors**
   - Check request payload format
   - Verify required fields are provided
   - Validate data types and constraints
