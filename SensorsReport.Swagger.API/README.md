# SensorsReport.Swagger.API

## Overview

The SensorsReport.Swagger.API is a microservice that provides a unified API gateway and Swagger documentation interface for the entire SensorsReport ecosystem. Built on Ocelot API Gateway, it aggregates multiple microservices into a single entry point and provides comprehensive API documentation through SwaggerUI, with dynamic service discovery via Kubernetes integration.

## Features

- **API Gateway**: Unified entry point for all SensorsReport microservices
- **Swagger Aggregation**: Combined Swagger documentation from all services
- **Kubernetes Service Discovery**: Automatic discovery of services in Kubernetes clusters
- **Dynamic Routing**: Intelligent request routing to appropriate microservices
- **Multi-tenant Support**: Tenant header propagation across services
- **Health Monitoring**: Gateway health checks and service availability monitoring
- **Load Balancing**: Built-in load balancing for downstream services
- **Authentication Integration**: Centralized authentication and authorization

## Technology Stack

- **.NET 8.0**: Modern web API framework
- **Ocelot**: API Gateway framework for .NET
- **SwaggerForOcelot**: Swagger documentation aggregation
- **Kubernetes Client**: Service discovery and configuration management
- **NLog**: Structured logging
- **Docker**: Containerized deployment

## Project Structure

```
SensorsReport.Swagger.API/
├── KubernetesOcelotConfigurationProvider.cs  # Kubernetes integration logic
├── Program.cs                                # Application entry point
├── SensorsReport.Swagger.API.csproj         # Project dependencies
├── Dockerfile                               # Container configuration
├── nlog.config                             # Logging configuration
├── Properties/
│   └── launchSettings.json                 # Development settings
└── README.md                               # This file
```

## Setup and Installation

### Prerequisites

- .NET 8.0 SDK
- Docker (for containerized deployment)
- Kubernetes cluster (for production deployment)
- Access to SensorsReport microservices

### Local Development

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd SensorsReport/SensorsReport.Swagger.API
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure Kubernetes (optional for development)**:
   ```bash
   # For local development, ensure kubectl is configured
   kubectl config current-context
   ```

4. **Build the project**:
   ```bash
   dotnet build
   ```

5. **Run the application**:
   ```bash
   dotnet run
   ```

The API Gateway will be available at `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger`.

### Docker Deployment

1. **Build the Docker image**:
   ```bash
   docker build -t sensorsreport-swagger-api .
   ```

2. **Run the container**:
   ```bash
   docker run -p 80:80 sensorsreport-swagger-api
   ```

### Kubernetes Deployment

The service is designed for Kubernetes deployment with automatic service discovery:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sensorsreport-swagger-api
spec:
  replicas: 2
  selector:
    matchLabels:
      app: sensorsreport-swagger-api
  template:
    metadata:
      labels:
        app: sensorsreport-swagger-api
    spec:
      containers:
      - name: api
        image: sensorsreport-swagger-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
```

## Configuration

### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production | No |
| `ASPNETCORE_URLS` | Binding URLs | http://+:80 | No |
| `SwaggerForOcelot__PathToSwaggerGenerator` | Swagger generation path | /swagger/docs | No |

### Ocelot Configuration

The service uses dynamic configuration from Kubernetes services. In development, you can provide a static `ocelot.json`:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "sensorsreport-alarm-api",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/alarm/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

### Kubernetes Service Discovery

The service automatically discovers SensorsReport services in Kubernetes using service labels and annotations.

## API Reference

### Base URL
- Development: `http://localhost:5000`
- Production: `http://<your-domain>`

### Swagger Documentation
- **Swagger UI**: `/swagger`
- **OpenAPI JSON**: `/swagger/docs`

### Gateway Routes

The API Gateway routes requests to the following services:

| Route Pattern | Target Service | Description |
|---------------|---------------|-------------|
| `/alarm/*` | SensorsReport.Alarm.API | Alarm management |
| `/alarmrule/*` | SensorsReport.AlarmRule.API | Alarm rule configuration |
| `/audit/*` | SensorsReport.Audit.API | Audit logging |
| `/broker/*` | SensorsReport.Business.Broker.API | Business logic broker |
| `/email/*` | SensorsReport.Email.API | Email notifications |
| `/logrule/*` | SensorsReport.LogRule.API | Log rule management |
| `/notification/*` | SensorsReport.NotificationRule.API | Notification rules |
| `/provision/*` | SensorsReport.Provision.API | Data provisioning |
| `/sms/*` | SensorsReport.SMS.API | SMS notifications |
| `/webhook/*` | SensorsReport.Webhook.API | Webhook management |

### Health Checks

- **Gateway Health**: `GET /health`
- **Service Discovery**: `GET /health/discovery`

## Usage Examples

### Access Swagger Documentation

```bash
# Open Swagger UI in browser
curl http://localhost:5000/swagger

# Get OpenAPI specification
curl http://localhost:5000/swagger/docs
```

### Route API Calls

```bash
# Create an alarm through the gateway
curl -X POST "http://localhost:5000/alarm/api/alarms" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"message": "Temperature threshold exceeded"}'

# Get audit logs through the gateway
curl -X GET "http://localhost:5000/audit/api/audits" \
  -H "Authorization: Bearer <token>"
```

### Multi-tenant Requests

```bash
# Request with tenant header
curl -X GET "http://localhost:5000/alarm/api/alarms" \
  -H "Authorization: Bearer <token>" \
  -H "Tenant: production-tenant"
```

## Integration

### Service Discovery
- **Kubernetes**: Automatic discovery of services with proper labels
- **Load Balancing**: Round-robin and other balancing strategies
- **Health Checks**: Continuous monitoring of downstream services

### Authentication
- **JWT Propagation**: Forwards authentication headers to downstream services
- **Centralized Auth**: Single point for authentication validation
- **Role-based Access**: Support for role-based routing and access control

### Monitoring Integration
- **Logging**: Centralized logging for all gateway operations
- **Metrics**: Performance metrics for routing and service health
- **Tracing**: Distributed tracing support for request flows

## Monitoring and Health Checks

### Gateway Metrics
- Request count and latency per route
- Success/error rates for downstream services
- Service discovery status
- Configuration reload events

### Logging
The service uses NLog for comprehensive logging:
- **Information**: Route configurations and service discovery
- **Warning**: Service unavailability and configuration issues
- **Error**: Routing failures and downstream service errors

### Health Endpoints
- `/health`: Overall gateway health
- `/health/ready`: Readiness probe for Kubernetes
- `/health/live`: Liveness probe for Kubernetes

## Error Handling

### Common Error Scenarios

1. **Service Unavailable**
   - Status: 503 Service Unavailable
   - Resolution: Check downstream service health

2. **Route Not Found**
   - Status: 404 Not Found
   - Resolution: Verify route configuration and service discovery

3. **Authentication Failures**
   - Status: 401 Unauthorized
   - Resolution: Check JWT token validity and propagation

4. **Timeout Errors**
   - Status: 504 Gateway Timeout
   - Resolution: Adjust timeout settings and check service performance

### Circuit Breaker
- Automatic circuit breaking for failing services
- Configurable failure thresholds
- Graceful degradation strategies

## Performance Considerations

### Caching
- Route configuration caching
- Service discovery result caching
- Response caching for static content

### Load Balancing
- Multiple load balancing algorithms
- Health-based routing
- Geographic routing support

### Optimization Tips
- Configure appropriate timeout values
- Use health checks to avoid routing to unhealthy services
- Monitor and tune circuit breaker settings
- Optimize service discovery refresh intervals

## Security

### Gateway Security
- Request/response header manipulation
- Rate limiting and throttling
- IP whitelisting/blacklisting
- SSL/TLS termination

### Service Communication
- Secure inter-service communication
- Certificate-based authentication
- Network policies in Kubernetes
- Service mesh integration support

### Data Protection
- Header sanitization
- Request/response filtering
- Audit logging for security events

## Dependencies

### NuGet Packages
- `Ocelot` (24.0.0): API Gateway framework
- `Ocelot.Provider.Kubernetes` (24.0.0): Kubernetes service discovery
- `MMLib.SwaggerForOcelot` (9.0.0): Swagger aggregation
- `KubernetesClient` (17.0.4): Kubernetes API client

### Internal Dependencies
- `SensorsReport.Api.Core`: Core functionality and utilities

### External Dependencies
- Kubernetes API Server
- SensorsReport microservices
- Authentication providers
- Monitoring systems

## Related Services

All SensorsReport microservices integrate through this gateway:
- **SensorsReport.Alarm.API**: Alarm management
- **SensorsReport.AlarmRule.API**: Alarm rule configuration
- **SensorsReport.Audit.API**: Audit logging
- **SensorsReport.Business.Broker.API**: Business logic broker
- **SensorsReport.Email.API**: Email notifications
- **SensorsReport.LogRule.API**: Log rule management
- **SensorsReport.NotificationRule.API**: Notification rules
- **SensorsReport.Provision.API**: Data provisioning
- **SensorsReport.SMS.API**: SMS notifications
- **SensorsReport.Webhook.API**: Webhook management

## Contributing

### Development Guidelines

1. **Route Configuration**: Follow RESTful patterns for new routes
2. **Documentation**: Ensure new services are included in Swagger aggregation
3. **Testing**: Test gateway routing and service discovery
4. **Monitoring**: Add appropriate logging and metrics

### Adding New Services

1. **Service Registration**: Ensure proper Kubernetes service labels
2. **Route Configuration**: Add routes to dynamic configuration
3. **Health Checks**: Implement health endpoints in downstream services
4. **Documentation**: Update Swagger aggregation configuration

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Update route configurations
4. Test with downstream services
5. Submit a pull request

## Troubleshooting

### Common Issues

1. **Service Discovery Failures**
   - Check Kubernetes service labels and annotations
   - Verify namespace and RBAC permissions
   - Monitor service discovery logs

2. **Route Configuration Errors**
   - Validate JSON configuration syntax
   - Check route priorities and conflicts
   - Review gateway logs for configuration errors

3. **Downstream Service Connectivity**
   - Verify service endpoints and health
   - Check network policies and firewall rules
   - Monitor service-to-service communication

### Debug Mode
Enable debug logging by setting `ASPNETCORE_ENVIRONMENT=Development`.

### Kubernetes Debugging
```bash
# Check service discovery
kubectl get services -l app=sensorsreport

# View gateway logs
kubectl logs -l app=sensorsreport-swagger-api

# Check service endpoints
kubectl get endpoints
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Support

### Documentation
- **Ocelot Documentation**: https://ocelot.readthedocs.io/
- **Swagger Integration**: SwaggerUI available at `/swagger`
- **Kubernetes Integration**: See Kubernetes client documentation

### Getting Help
- Check the troubleshooting section above
- Review gateway and service logs
- Verify service discovery configuration
- Contact the development team for complex routing issues

### Reporting Issues
When reporting issues, please include:
- Gateway configuration and routes
- Service discovery logs
- Downstream service availability
- Steps to reproduce routing problems
- Expected vs actual routing behavior
