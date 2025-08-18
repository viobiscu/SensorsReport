# SensorsReport.Webhook.API

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-supported-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](../LICENSE)

## Overview

The SensorsReport.Webhook.API is a microservice that serves as the central webhook endpoint for receiving NGSI-LD subscription notifications from the Orion-LD Context Broker. This service acts as a bridge between external webhook notifications and the internal SensorsReport event system, automatically processing incoming data changes and publishing them to the event bus for consumption by other microservices.

## Features

### Core Functionality
- **Webhook Reception**: Receives NGSI-LD subscription notifications via HTTP POST
- **Event Publishing**: Publishes sensor data change events to the internal event bus
- **Multi-tenant Support**: Automatic tenant context handling for all webhook notifications
- **Request Logging**: Comprehensive HTTP request logging for debugging and auditing
- **Subscription Management**: Automatic registration of alarm subscriptions for all tenants

### Subscription Management
- **Auto-Registration**: Automatically creates alarm subscriptions for all tenants
- **Background Processing**: Continuous monitoring and subscription updates
- **Status Validation**: Ensures subscriptions remain active and properly configured
- **Multi-tenant Isolation**: Separate subscription management per tenant

### Integration Features
- **Event Bus Integration**: Seamless integration with SensorsReport event system
- **Orion-LD Native**: Direct integration with FIWARE Orion-LD Context Broker
- **Tenant Awareness**: Automatic tenant detection and context propagation
- **Structured Logging**: Comprehensive logging with NLog integration

## Technology Stack

- **.NET 8.0**: Core framework for high-performance web APIs
- **ASP.NET Core**: Web framework for webhook endpoint development
- **FIWARE Orion-LD**: NGSI-LD context broker for subscription notifications
- **MassTransit**: Event bus for internal message publishing
- **NLog**: Structured logging framework
- **Docker**: Containerization for deployment
- **Kubernetes**: Orchestration with Flux GitOps

## Project Structure

```
SensorsReport.Webhook.API/
├── Controllers/
│   └── WebhookController.cs           # Main webhook endpoint controller
├── Tasks/
│   └── AutoRegisterAlarmSubscriptions.cs  # Background subscription management
├── Properties/                        # Assembly properties
├── flux/                             # Kubernetes deployment manifests
├── Dockerfile                        # Container build configuration
├── Program.cs                        # Application entry point and DI setup
├── appsettings.json                  # Default configuration
├── nlog.config                       # Logging configuration
└── README.md                         # This documentation
```

## API Reference

### Base URL
- **Local Development**: `http://localhost:5000/api/webhook`
- **Production**: `https://your-domain/api/webhook`

### Authentication
The webhook endpoint is typically accessed by Orion-LD and does not require authentication. However, it supports tenant identification via headers.

### Endpoints

#### POST /api/webhook
Receives NGSI-LD subscription notifications and processes sensor data changes.

**Query Parameters:**
- `subscriptionId` (string, optional): Subscription ID for validation

**Headers:**
- `Content-Type: application/json` (required)
- `NGSILD-Tenant: <tenant-id>` (optional, auto-detected from payload)

**Request Body:**
```json
{
  "id": "urn:ngsi-ld:Notification:001",
  "type": "Notification",
  "subscriptionId": "urn:ngsi-ld:Subscription:Alarm",
  "notifiedAt": "2024-01-01T10:00:00.000Z",
  "data": [
    {
      "id": "urn:ngsi-ld:Alarm:001",
      "type": "Alarm",
      "severity": {
        "type": "Property",
        "value": "high"
      },
      "message": {
        "type": "Property",
        "value": "Temperature threshold exceeded"
      },
      "timestamp": {
        "type": "Property",
        "value": "2024-01-01T10:00:00.000Z"
      }
    }
  ]
}
```

**Response:**
- `204 No Content`: Notification processed successfully
- `400 Bad Request`: Invalid notification format or validation failure
- `500 Internal Server Error`: Server error during processing

**Status Codes:**
- `204 No Content`: Webhook processed successfully
- `400 Bad Request`: Invalid payload or subscription ID mismatch
- `500 Internal Server Error`: Server error

### Webhook Validation
The service validates incoming webhooks for:
- **Required Fields**: `id`, `type`, `subscriptionId`, and `data` presence
- **Notification Type**: Must be "Notification"
- **Subscription Matching**: Query parameter must match payload subscription ID
- **Data Integrity**: Ensures data array is not empty

## Data Models

### SubscriptionEventModel (Incoming Webhook)
```json
{
  "id": "urn:ngsi-ld:Notification:12345",
  "type": "Notification",
  "subscriptionId": "urn:ngsi-ld:Subscription:Alarm",
  "notifiedAt": "2024-01-01T10:00:00.000Z",
  "data": [
    {
      "id": "urn:ngsi-ld:Alarm:sensor001-high-temp",
      "type": "Alarm",
      "severity": {
        "type": "Property",
        "value": "critical"
      },
      "message": {
        "type": "Property",
        "value": "Temperature sensor reading 95°C exceeds critical threshold of 85°C"
      },
      "deviceId": {
        "type": "Relationship",
        "object": "urn:ngsi-ld:Device:sensor001"
      },
      "timestamp": {
        "type": "Property",
        "value": "2024-01-01T10:00:00.000Z"
      }
    }
  ]
}
```

### SensorDataChangedEvent (Published Event)
```json
{
  "id": "urn:ngsi-ld:Notification:12345",
  "type": "Notification",
  "subscriptionId": "urn:ngsi-ld:Subscription:Alarm",
  "tenant": {
    "id": "manufacturing",
    "name": "Manufacturing Division"
  },
  "data": [
    {
      "id": "urn:ngsi-ld:Alarm:sensor001-high-temp",
      "type": "Alarm",
      "severity": {
        "type": "Property",
        "value": "critical"
      }
    }
  ]
}
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Access to FIWARE Orion-LD Context Broker
- Event bus infrastructure (RabbitMQ/Azure Service Bus)
- Docker (for containerized deployment)

### Local Development

#### 1. Clone and Setup
```bash
cd SensorsReport.Webhook.API
dotnet restore
```

#### 2. Configure Environment Variables
```bash
# Orion-LD Context Broker Configuration
export ORION_LD_HOST="localhost"
export ORION_LD_PORT="1026"

# Event Bus Configuration (example for RabbitMQ)
export RABBITMQ_HOST="localhost"
export RABBITMQ_USERNAME="guest"
export RABBITMQ_PASSWORD="guest"

# Application Configuration
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://localhost:5000"
```

#### 3. Run the Application
```bash
dotnet run
```

The webhook endpoint will be available at `http://localhost:5000/api/webhook`.

#### 4. Test Webhook Reception
```bash
# Simulate an Orion-LD notification
curl -X POST http://localhost:5000/api/webhook?subscriptionId=urn:ngsi-ld:Subscription:Alarm \
  -H "Content-Type: application/json" \
  -H "NGSILD-Tenant: test-tenant" \
  -d '{
    "id": "test-notification-001",
    "type": "Notification",
    "subscriptionId": "urn:ngsi-ld:Subscription:Alarm",
    "data": [
      {
        "id": "urn:ngsi-ld:Alarm:test-001",
        "type": "Alarm",
        "severity": {"type": "Property", "value": "high"}
      }
    ]
  }'
```

### Docker Deployment

#### Build Container
```bash
# From the root SensorsReport directory
docker build -f SensorsReport.Webhook.API/Dockerfile -t sensorsreport-webhook-api:latest .
```

#### Run Container
```bash
docker run -d \
  --name webhook-api \
  -p 80:80 \
  -e ORION_LD_HOST="orion.example.com" \
  -e ORION_LD_PORT="1026" \
  sensorsreport-webhook-api:latest
```

### Kubernetes Deployment

Deploy using Flux manifests:
```bash
kubectl apply -f flux/
```

## Configuration

### Orion-LD Integration
The service integrates with FIWARE Orion-LD Context Broker for subscription management:

```json
{
  "OrionLD": {
    "Host": "localhost",
    "Port": 1026,
    "UseHttps": false,
    "BasePath": "/ngsi-ld/v1"
  }
}
```

### Event Bus Configuration
```json
{
  "EventBus": {
    "ConnectionString": "amqp://guest:guest@localhost:5672",
    "ExchangeName": "sensorsreport",
    "QueueName": "webhook-events"
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ORION_LD_HOST` | Orion-LD Context Broker host | `localhost` |
| `ORION_LD_PORT` | Orion-LD Context Broker port | `1026` |
| `ORION_LD_USE_HTTPS` | Use HTTPS for Orion-LD connection | `false` |
| `EVENT_BUS_CONNECTION` | Event bus connection string | - |
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ASPNETCORE_URLS` | Application URLs | `http://*:80` |

## Usage Examples

### Orion-LD Subscription Configuration

To receive webhooks, configure subscriptions in Orion-LD:

```bash
# Create alarm subscription pointing to webhook endpoint
curl -X POST http://orion:1026/ngsi-ld/v1/subscriptions \
  -H "Content-Type: application/ld+json" \
  -H "NGSILD-Tenant: production" \
  -d '{
    "id": "urn:ngsi-ld:Subscription:Alarm",
    "type": "Subscription",
    "entities": [
      {
        "type": "Alarm"
      }
    ],
    "notification": {
      "endpoint": {
        "uri": "http://sensors-report-webhook-api/api/webhook?subscriptionId=urn:ngsi-ld:Subscription:Alarm",
        "accept": "application/json"
      }
    },
    "@context": "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context-v1.6.jsonld"
  }'
```

### Webhook Processing Flow

1. **Orion-LD Notification**: Context broker sends notification when entities change
2. **Webhook Reception**: Service receives POST request with notification data
3. **Validation**: Validates notification format and subscription ID
4. **Tenant Detection**: Extracts tenant information from headers or payload
5. **Event Publishing**: Publishes `SensorDataChangedEvent` to event bus
6. **Response**: Returns 204 No Content on successful processing

### Event Consumption Example

Other services can consume the published events:

```csharp
public class AlarmEventHandler : IConsumer<SensorDataChangedEvent>
{
    public async Task Consume(ConsumeContext<SensorDataChangedEvent> context)
    {
        var sensorEvent = context.Message;
        
        foreach (var entity in sensorEvent.Data)
        {
            if (entity.Type == "Alarm")
            {
                // Process alarm notification
                await ProcessAlarmNotification(entity, sensorEvent.Tenant);
            }
        }
    }
}
```

## Multi-tenancy

The webhook API provides complete tenant isolation:

### Tenant Detection
- **Header-based**: Uses `NGSILD-Tenant` header when available
- **Payload-based**: Extracts tenant from notification payload
- **Automatic Propagation**: Includes tenant context in published events

### Subscription Management
- **Per-tenant Subscriptions**: Automatic subscription creation for each tenant
- **Isolated Processing**: Each tenant's notifications processed separately
- **Background Management**: Continuous subscription status monitoring

```bash
# Example: Different tenants receive separate notifications
# Manufacturing tenant
curl -X POST http://webhook-api/api/webhook \
  -H "NGSILD-Tenant: manufacturing" \
  -d '{"id": "mfg-notification", "type": "Notification", ...}'

# Healthcare tenant  
curl -X POST http://webhook-api/api/webhook \
  -H "NGSILD-Tenant: healthcare" \
  -d '{"id": "health-notification", "type": "Notification", ...}'
```

## Background Services

### AutoRegisterAlarmSubscriptions

A background service that automatically manages alarm subscriptions:

#### Functionality
- **Tenant Discovery**: Queries all tenants from Orion-LD
- **Subscription Creation**: Creates alarm subscriptions for each tenant
- **Subscription Updates**: Updates existing subscriptions if needed
- **Error Handling**: Robust error handling for tenant processing failures

#### Subscription Template
```json
{
  "id": "urn:ngsi-ld:Subscription:Alarm",
  "type": "Subscription",
  "entities": [
    {
      "type": "Alarm"
    }
  ],
  "notification": {
    "endpoint": {
      "uri": "http://sensors-report-webhook-api/api/webhook",
      "accept": "application/json"
    }
  },
  "status": "active",
  "isActive": true
}
```

#### Monitoring
- **Execution Logging**: Detailed logs of subscription management activities
- **Error Tracking**: Logs failures with tenant-specific context
- **Progress Reporting**: Tracks processing of large tenant lists

## Integration

### Event Bus Integration
The service integrates with the SensorsReport event system:

```mermaid
graph TD
    A[Orion-LD Context Broker] -->|Webhook Notification| B[Webhook API]
    B -->|Validate & Process| C[Event Publishing]
    C -->|SensorDataChangedEvent| D[Event Bus]
    D -->|Consume Events| E[Alarm Consumer]
    D -->|Consume Events| F[Notification Consumer]
    D -->|Consume Events| G[Log Consumer]
```

### SensorsReport Ecosystem
- **Alarm Consumers**: Process alarm-related notifications
- **Notification Services**: Handle notification routing and delivery
- **Log Services**: Audit and log webhook activities
- **Business Broker**: Orchestrates business rule evaluation

### Orion-LD Context Broker
- **Subscription Management**: Creates and maintains NGSI-LD subscriptions
- **Notification Reception**: Receives entity change notifications
- **Tenant Isolation**: Manages tenant-specific subscriptions

## Monitoring and Observability

### Comprehensive Request Logging
The service provides detailed HTTP request logging:

```csharp
// Example log output
=== COMPLETE HTTP REQUEST DUMP ===
Method: POST
URL: http://localhost:5000/api/webhook?subscriptionId=urn:ngsi-ld:Subscription:Alarm
Protocol: HTTP/1.1

--- HEADERS ---
Content-Type: application/json
NGSILD-Tenant: manufacturing
User-Agent: Orion-LD/2.0.0

--- BODY/PAYLOAD ---
{
  "id": "urn:ngsi-ld:Notification:001",
  "type": "Notification",
  "data": [...]
}
=== END HTTP REQUEST DUMP ===
```

### Health Monitoring
- **Service Health**: Built-in health check endpoints
- **Orion-LD Connectivity**: Monitor context broker connectivity
- **Event Bus Status**: Track event publishing success rates
- **Subscription Status**: Monitor automatic subscription management

### Performance Metrics
Monitor these key metrics:
- **Webhook Processing Rate**: Notifications processed per second
- **Response Times**: Average webhook processing latency
- **Event Publishing Success**: Percentage of successfully published events
- **Subscription Management**: Background service execution status

### Logging Categories
- **Information**: Successful webhook processing and subscription management
- **Warning**: Validation failures and non-critical issues
- **Error**: Processing failures and system errors

## Error Handling

### Webhook Validation Errors
```json
{
  "error": "Invalid webhook notification format",
  "details": "Required fields missing: id, type, subscriptionId, or data"
}
```

### Subscription ID Validation
```json
{
  "error": "Query parameter subscriptionId does not match payload subscriptionId",
  "expected": "urn:ngsi-ld:Subscription:Alarm",
  "received": "urn:ngsi-ld:Subscription:Invalid"
}
```

### Event Publishing Failures
- **Retry Logic**: Automatic retry for transient failures
- **Dead Letter Queues**: Failed events routed to dead letter queues
- **Error Logging**: Comprehensive error logging with context

### Background Service Errors
- **Tenant Processing**: Individual tenant failures don't stop overall processing
- **Subscription Management**: Robust error handling for subscription operations
- **Recovery Mechanisms**: Automatic recovery from temporary failures

## Performance Considerations

### Webhook Processing
- **Async Operations**: All webhook processing is asynchronous
- **Concurrent Processing**: Support for concurrent webhook requests
- **Efficient Validation**: Fast validation with early returns
- **Memory Management**: Efficient handling of large notification payloads

### Event Publishing
- **Batch Publishing**: Efficient event bus integration
- **Connection Pooling**: Optimized event bus connections
- **Message Persistence**: Reliable event delivery guarantees

### Background Services
- **Batch Processing**: Efficient tenant discovery and processing
- **Rate Limiting**: Controlled subscription management to avoid overwhelming Orion-LD
- **Resource Management**: Proper scoping and disposal of services

## Security Considerations

### Webhook Security
- **Input Validation**: Comprehensive validation of all webhook payloads
- **Content-Type Validation**: Ensures proper JSON content
- **Size Limits**: Protection against oversized payloads
- **Rate Limiting**: Protection against webhook flooding (typically handled at gateway level)

### Tenant Security
- **Tenant Isolation**: Complete separation of tenant-specific notifications
- **Context Validation**: Ensures tenant context is properly maintained
- **Data Protection**: Secure handling of sensitive notification data

### Event Publishing Security
- **Message Encryption**: Support for encrypted event bus communication
- **Access Control**: Proper authentication for event bus connections
- **Audit Trail**: Complete logging of all webhook and event activities

## Troubleshooting

### Common Issues

#### Webhooks Not Received
```bash
# Check Orion-LD subscription configuration
curl -H "NGSILD-Tenant: mytenant" \
     http://orion:1026/ngsi-ld/v1/subscriptions/urn:ngsi-ld:Subscription:Alarm

# Verify webhook endpoint accessibility
curl -X POST http://webhook-api/api/webhook \
  -H "Content-Type: application/json" \
  -d '{"id": "test", "type": "Notification", "subscriptionId": "test", "data": [{}]}'
```

#### Event Publishing Failures
```bash
# Check event bus connectivity
kubectl logs -f deployment/webhook-api | grep "Event publishing"

# Verify event bus configuration
kubectl describe configmap webhook-api-config
```

#### Subscription Management Issues
```bash
# Check background service logs
kubectl logs -f deployment/webhook-api | grep "AutoRegisterAlarmSubscriptions"

# Verify tenant discovery
curl -H "NGSILD-Tenant: default" \
     http://orion:1026/ngsi-ld/v1/entities?type=APIKey
```

### Debug Commands
```bash
# Check service health
curl http://webhook-api/health

# View detailed logs
kubectl logs -f deployment/webhook-api --tail=100

# Test webhook processing
curl -v -X POST http://webhook-api/api/webhook?subscriptionId=test \
  -H "Content-Type: application/json" \
  -d '{"id": "test", "type": "Notification", "subscriptionId": "test", "data": [{}]}'
```

## Dependencies

### External Dependencies
- **FIWARE Orion-LD**: NGSI-LD Context Broker for subscription notifications
- **Event Bus**: Message broker (RabbitMQ, Azure Service Bus, etc.)
- **MongoDB**: Database backend for Orion-LD (indirect dependency)

### .NET Dependencies
No additional NuGet packages beyond the base .NET 8.0 framework.

### Runtime Dependencies
- **.NET 8.0 Runtime**: Core runtime environment
- **SensorsReport.Api.Core**: Shared core functionality and event bus integration
- **SensorsReport.OrionLD.Extensions**: Orion-LD integration extensions

## Related Services

- **SensorsReport.Alarm.API**: Manages alarm entities that trigger webhook notifications
- **SensorsReport.AlarmRule.Consumer**: Processes alarm rule events from webhooks
- **SensorsReport.NotificationRule.Consumer**: Handles notification routing from webhook events
- **SensorsReport.LogRule.Consumer**: Processes log rule events from webhooks
- **SensorsReport.Audit.API**: Logs webhook reception and processing activities
- **SensorsReport.Business.Broker.API**: Orchestrates business logic based on webhook events

## Contributing

When contributing to this service:

1. **Follow Webhook Standards**: Ensure compatibility with NGSI-LD notification format
2. **Maintain Event Schema**: Keep published event schema backward compatible
3. **Add Comprehensive Tests**: Include webhook processing and event publishing tests
4. **Update Documentation**: Keep webhook endpoint documentation current
5. **Test Multi-tenancy**: Verify tenant isolation works correctly
6. **Monitor Performance**: Ensure webhook processing remains performant

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/webhook-enhancement`)
3. Make your changes with tests
4. Test with actual Orion-LD notifications
5. Verify event publishing functionality
6. Update documentation
7. Submit a pull request

## License

This project is part of the SensorsReport system for AerOS. See the root LICENSE file for details.

## Support

For issues with the Webhook API:

1. **Check Orion-LD Integration**: Verify context broker connectivity and subscriptions
2. **Verify Event Bus**: Ensure event bus connectivity and configuration
3. **Test Webhook Reception**: Use curl to test webhook endpoint directly
4. **Review Background Services**: Check subscription management logs
5. **Monitor Event Publishing**: Verify events are being published to event bus
6. **Contact Support**: Reach out to the SensorsReport development team

## Changelog

### Version 1.0.0
- Initial release with webhook reception capabilities
- FIWARE NGSI-LD subscription notification processing
- Event bus integration for internal message publishing
- Multi-tenant support with automatic tenant detection
- Background subscription management service
- Comprehensive request logging and debugging features
- Docker and Kubernetes deployment support
