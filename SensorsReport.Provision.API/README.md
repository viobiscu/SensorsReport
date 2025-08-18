# SensorsReport.Provision.API

## Overview

The SensorsReport.Provision.API is a microservice that provides data provisioning capabilities within the SensorsReport ecosystem. This service enables copying and migrating entities and subscriptions between different tenants in the Orion-LD context broker, facilitating multi-tenant data management and tenant provisioning workflows.

## Features

- **Bulk Provisioning**: Copy all entities and subscriptions from the default tenant to a target tenant
- **Selective Entity Provisioning**: Provision specific entities by ID with their related entities
- **Selective Subscription Provisioning**: Provision specific subscriptions by ID
- **Relationship Handling**: Automatically resolve and copy related entities and subscriptions
- **Duplicate Prevention**: Skip entities and subscriptions that already exist in the target tenant
- **Comprehensive Logging**: Detailed logging for provisioning operations and error tracking
- **Multi-tenant Support**: Support for copying data between different Orion-LD tenants

## Technology Stack

- **.NET 8.0**: Modern web API framework
- **ASP.NET Core**: RESTful API development
- **Orion-LD Integration**: Context broker interaction through SensorsReport.Api.Core
- **JWT Bearer Authentication**: Secure API access
- **NLog**: Structured logging
- **System.Text.Json**: JSON serialization
- **Docker**: Containerized deployment

## Project Structure

```
SensorsReport.Provision.API/
├── Controllers/
│   └── ProvisionsController.cs       # Main provisioning API endpoints
├── Properties/
│   └── launchSettings.json           # Development launch settings
├── Program.cs                        # Application entry point and configuration
├── SensorsReport.Provision.API.csproj # Project dependencies
├── appsettings.json                  # Application configuration
├── Dockerfile                        # Container configuration
├── nlog.config                       # Logging configuration
└── README.md                         # This file
```

## Setup and Installation

### Prerequisites

- .NET 8.0 SDK
- Docker (for containerized deployment)
- Access to Orion-LD context broker
- Valid JWT authentication setup

### Local Development

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd SensorsReport/SensorsReport.Provision.API
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

The API will be available at `http://localhost:5000` (or the port specified in launchSettings.json).

### Docker Deployment

1. **Build the Docker image**:
   ```bash
   docker build -t sensorsreport-provision-api .
   ```

2. **Run the container**:
   ```bash
   docker run -p 80:80 sensorsreport-provision-api
   ```

## Configuration

### Environment Variables

The service uses the following configuration options:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production | No |
| `ASPNETCORE_URLS` | Binding URLs | http://+:80 | No |
| `OrionLd__BaseUrl` | Orion-LD broker base URL | - | Yes |
| `OrionLd__DefaultTenant` | Default tenant name | - | Yes |
| `OrionLd__ApiKey` | API key for broker access | - | No |

### appsettings.json

```json
{
  "OrionLd": {
    "BaseUrl": "http://orion-ld:1026",
    "DefaultTenant": "default",
    "ApiKey": "your-api-key"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## API Reference

### Base URL
- Development: `http://localhost:5000/api`
- Production: `http://<your-domain>/api`

### Authentication
All endpoints require JWT Bearer authentication.

### Endpoints

#### POST /api/provisions
Performs bulk provisioning of all entities and subscriptions from the default tenant to the target tenant.

**Request Body:**
```json
{
  "targetTenant": "tenant-name"
}
```

**Response:**
- `200 OK`: Provisioning completed successfully
- `400 Bad Request`: Invalid request or missing target tenant
- `500 Internal Server Error`: Server error during provisioning

**Example:**
```bash
curl -X POST "http://localhost:5000/api/provisions" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"targetTenant": "new-tenant"}'
```

#### POST /api/provisions/entity/{entityId}
Provisions a specific entity and its related entities to the target tenant.

**Parameters:**
- `entityId` (path): The ID of the entity to provision

**Request Body:**
```json
{
  "targetTenant": "tenant-name"
}
```

**Response:**
- `200 OK`: Entity provisioned successfully
- `400 Bad Request`: Invalid request or missing target tenant
- `404 Not Found`: Entity not found in source broker
- `500 Internal Server Error`: Server error during provisioning

**Example:**
```bash
curl -X POST "http://localhost:5000/api/provisions/entity/urn:ngsi-ld:Device:001" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"targetTenant": "new-tenant"}'
```

#### POST /api/provisions/subscription/{subscriptionId}
Provisions a specific subscription to the target tenant.

**Parameters:**
- `subscriptionId` (path): The ID of the subscription to provision

**Request Body:**
```json
{
  "targetTenant": "tenant-name"
}
```

**Response:**
- `200 OK`: Subscription provisioned successfully
- `400 Bad Request`: Invalid request or missing target tenant
- `404 Not Found`: Subscription not found in source broker
- `500 Internal Server Error`: Server error during provisioning

**Example:**
```bash
curl -X POST "http://localhost:5000/api/provisions/subscription/urn:ngsi-ld:subscription:001" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"targetTenant": "new-tenant"}'
```

## Usage Examples

### Complete Tenant Provisioning

```bash
# Provision all entities and subscriptions to a new tenant
curl -X POST "http://localhost:5000/api/provisions" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"targetTenant": "production-tenant"}'
```

### Selective Entity Provisioning

```bash
# Provision a specific device and its relationships
curl -X POST "http://localhost:5000/api/provisions/entity/urn:ngsi-ld:Device:sensor001" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"targetTenant": "testing-tenant"}'
```

### Subscription Migration

```bash
# Copy a specific subscription to another tenant
curl -X POST "http://localhost:5000/api/provisions/subscription/urn:ngsi-ld:subscription:alerts" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"targetTenant": "backup-tenant"}'
```

## Integration

### Orion-LD Context Broker
The service integrates with Orion-LD context brokers to:
- Retrieve entities and subscriptions from the source tenant
- Create entities and subscriptions in the target tenant
- Manage tenant-specific contexts and namespaces

### Related Services
- **SensorsReport.Api.Core**: Provides Orion-LD service integration
- **Authentication Services**: JWT token validation and authorization
- **Monitoring Services**: Logging and health monitoring integration

## Monitoring and Health Checks

### Logging
The service uses NLog for comprehensive logging:
- **Information**: Provisioning progress and status updates
- **Warning**: Skipped entities/subscriptions and non-critical issues
- **Error**: Failed operations and exceptions

### Health Endpoints
- Check service status and dependencies
- Monitor provisioning operation status
- Validate Orion-LD broker connectivity

### Metrics
- Total entities provisioned
- Total subscriptions provisioned
- Provisioning operation duration
- Failed operation count

## Error Handling

### Common Error Scenarios

1. **Missing Target Tenant**
   - Status: 400 Bad Request
   - Resolution: Provide valid target tenant in request body

2. **Entity Not Found**
   - Status: 404 Not Found
   - Resolution: Verify entity ID exists in source broker

3. **Broker Connectivity Issues**
   - Status: 500 Internal Server Error
   - Resolution: Check Orion-LD broker configuration and connectivity

4. **Authentication Failures**
   - Status: 401 Unauthorized
   - Resolution: Provide valid JWT bearer token

### Retry Logic
- Automatic retry for transient failures
- Exponential backoff for broker communication
- Graceful handling of rate limits

## Performance Considerations

### Batch Processing
- Processes entities and subscriptions in batches of 100
- Configurable batch size for optimal performance
- Memory-efficient pagination through large datasets

### Parallel Processing
- Concurrent entity relationship resolution
- Asynchronous broker operations
- Non-blocking I/O for improved throughput

### Optimization Tips
- Use specific entity provisioning for small datasets
- Monitor memory usage during bulk operations
- Consider broker capacity when setting batch sizes

## Security

### Authentication
- JWT Bearer token validation
- Integration with identity providers
- Role-based access control support

### Authorization
- Tenant-specific access controls
- Operation-level permissions
- Audit trail for provisioning operations

### Data Protection
- Secure broker communication
- Tenant data isolation
- Encrypted configuration secrets

## Dependencies

### NuGet Packages
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.0)
- `System.IdentityModel.Tokens.Jwt` (7.3.1)
- `IdentityModel` (6.2.0)

### Internal Dependencies
- `SensorsReport.Api.Core`: Core API functionality and Orion-LD integration

### External Dependencies
- Orion-LD Context Broker
- JWT Authentication Provider
- Logging Infrastructure

## Related Services

- **SensorsReport.Api.Core**: Core API functionality
- **Orion-LD Context Broker**: Data storage and retrieval
- **Authentication Service**: JWT token validation
- **Other SensorsReport APIs**: Integrated ecosystem services

## Contributing

### Development Guidelines

1. **Code Style**: Follow C# coding conventions and use EditorConfig
2. **Testing**: Add unit tests for new functionality
3. **Documentation**: Update API documentation for new endpoints
4. **Logging**: Add appropriate logging for debugging and monitoring

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Make your changes with tests
4. Update documentation
5. Submit a pull request

### Code Review Checklist

- [ ] Code follows established patterns
- [ ] Proper error handling implemented
- [ ] Logging added for important operations
- [ ] Tests cover new functionality
- [ ] Documentation updated

## Troubleshooting

### Common Issues

1. **Provisioning Timeouts**
   - Check Orion-LD broker performance
   - Reduce batch size for large datasets
   - Monitor network connectivity

2. **Memory Issues**
   - Monitor application memory usage
   - Use selective provisioning for large datasets
   - Configure appropriate container limits

3. **Authentication Errors**
   - Verify JWT token validity
   - Check token permissions
   - Validate authentication configuration

### Debug Mode
Enable debug logging by setting `ASPNETCORE_ENVIRONMENT=Development`.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Support

### Documentation
- API Reference: Available via Swagger when running in development mode
- Integration Guide: See SensorsReport ecosystem documentation

### Getting Help
- Check the troubleshooting section above
- Review application logs for error details
- Contact the development team for complex issues

### Reporting Issues
When reporting issues, please include:
- Error messages and stack traces
- Configuration details (without sensitive data)
- Steps to reproduce the problem
- Expected vs actual behavior
