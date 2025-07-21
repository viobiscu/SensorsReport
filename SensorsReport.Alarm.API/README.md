# SensorsReport Alarm API

## Overview

The SensorsReport Alarm API is a microservice that manages alarm functionality within the SensorsReport platform. This API handles alarm rules, alarm events, and alarm-related operations for the IoT sensor monitoring system.

## Architecture

This API is part of the SensorsReport microservices architecture and follows the platform's standard patterns:

- **Framework**: .NET 8.0 Web API
- **Authentication**: Handled by KrakenD API Gateway (no local auth required)
- **Logging**: NLog with centralized configuration
- **Multi-tenancy**: Supports tenant isolation via `NGSILD-Tenant` header
- **Data Format**: NGSI-LD compliant entities
- **Deployment**: Kubernetes with Docker containers

## API Endpoints

Based on the platform specification, the Alarm API implements the following endpoints:

### Alarm Management
- `GET /v1/alarms` - Get all alarms for the current tenant
- `GET /v1/alarms/{alarmId}` - Get a specific alarm by ID
- `PATCH /v1/alarms/{alarmId}` - Update an alarm
- `DELETE /v1/alarms/{alarmId}` - Delete an alarm

### Alarm Rules Administration
- `GET /v1/admin/alarmrules` - Get all alarm rules
- `GET /v1/admin/alarmrules/{alarmRuleId}` - Get a specific alarm rule
- `POST /v1/admin/alarmrules` - Create a new alarm rule
- `PATCH /v1/admin/alarmrules/{alarmRuleId}` - Update an alarm rule
- `DELETE /v1/admin/alarmrules/{alarmRuleId}` - Delete an alarm rule

### Health & Status
- `GET /version` - API version information
- `GET /health` - Health check endpoint

## Data Models

### Alarm Entity
Alarms are NGSI-LD entities that represent triggered alarm conditions:

```json
{
  "id": "urn:ngsi-ld:Alarm:alarm-001",
  "type": "Alarm",
  "device": {
    "type": "Relationship",
    "object": "urn:ngsi-ld:Device:device-001"
  },
  "alarmRuleBy": {
    "type": "Relationship", 
    "object": "urn:ngsi-ld:AlarmRule:rule-001"
  },
  "severity": {
    "type": "Property",
    "value": "high"
  },
  "status": {
    "type": "Property",
    "value": "active"
  },
  "triggeredAt": {
    "type": "Property",
    "value": "2025-07-13T10:30:00Z"
  }
}
```

### Alarm Rule Entity
Alarm rules define the conditions and thresholds that trigger alarms:

```json
{
  "id": "urn:ngsi-ld:AlarmRule:rule-001",
  "type": "AlarmRule",
  "name": {
    "type": "Property",
    "value": "Temperature High Alert"
  },
  "unit": {
    "type": "Property",
    "value": "°C"
  },
  "low": {
    "type": "Property",
    "value": -10.0
  },
  "prelow": {
    "type": "Property", 
    "value": 0.0
  },
  "prehigh": {
    "type": "Property",
    "value": 35.0
  },
  "high": {
    "type": "Property",
    "value": 45.0
  }
}
```

## Multi-tenancy

The API supports multi-tenant operation through:

- **Tenant Header**: `NGSILD-Tenant` - Identifies the tenant for data isolation
- **Path Header**: `NGSILD-Path` - Additional location/path context
- **Data Isolation**: All alarm data is isolated per tenant

## Authentication & Authorization

- **No Local Authentication**: Authentication is handled by KrakenD API Gateway
- **Token Validation**: KrakenD validates JWT tokens from Keycloak
- **Header Forwarding**: Only necessary claims/headers are forwarded to the API
- **Internal Trust**: All internal pod-to-pod communication is trusted

## Configuration

### Application Settings

The API uses the standard SensorsReport configuration pattern:

```json
{
  "AppConfiguration": {
    "ConnectionString": "MongoDB connection string",
    "DatabaseName": "SensorsReport",
    "AlarmCollectionName": "alarms",
    "AlarmRuleCollectionName": "alarmrules"
  }
}
```

### Environment Variables

Standard environment variables are supported:
- `ASPNETCORE_ENVIRONMENT` - Development/Production environment
- `NGSILD_TENANT` - Default tenant (if header not provided)

## Development

### Prerequisites

- .NET 8.0 SDK
- Docker (for containerization)
- Access to SensorsReport.Api.Core library

### Local Development

1. **Clone Repository**:
   ```bash
   git clone https://github.com/viobiscu/SensorsReport
   cd SensorsReport/Sensors.Report.Alarm.API
   ```

2. **Build Project**:
   ```bash
   dotnet build
   ```

3. **Run Locally**:
   ```bash
   dotnet run
   ```

4. **Test Endpoints**:
   ```bash
   curl http://localhost:5000/version
   curl http://localhost:5000/health
   ```

### Docker Build

```bash
# Build image
docker build -t viobiscu/sensors-report-alarm-api:latest .

# Run container
docker run -p 8080:8080 viobiscu/sensors-report-alarm-api:latest
```

## Deployment

### Kubernetes Deployment

The API is deployed to Kubernetes using the standard SensorsReport deployment pattern:

1. **Build & Push Image**:
   ```bash
   ./increment_version.sh
   ```

2. **Deploy to Cluster**:
   ```bash
   kubectl apply -f kubernetes.yaml
   ```

3. **Verify Deployment**:
   ```bash
   kubectl get pods -l app=sensors-report-alarm-api
   ```

### Cluster Testing

Test from within the cluster using the test pod:

```bash
# Access test pod shell in k9s (press 's' on test-pod)
curl http://sensors-report-alarm-api/api/version
curl http://sensors-report-alarm-api/api/health
```

### External Testing

Test through the API Gateway:

```bash
# Requires valid Keycloak token
curl -H "Authorization: Bearer <token>" \
     https://api.sensorsreport.net/alarm/version
```

## Monitoring & Logging

### Logging

- **Framework**: NLog with structured logging
- **Log Levels**: Info, Warning, Error, Debug
- **Tenant Context**: All logs include tenant information
- **Request Tracing**: Each request is tracked with correlation IDs

### Health Monitoring

- **Health Endpoint**: `/health` - Returns service health status
- **Version Endpoint**: `/version` - Returns API version and build info
- **AerOS Integration**: Health checks are monitored by AerOS orchestration

## Integration

### Context Broker Integration

- **Orion-LD**: Integrates with FIWARE Orion-LD Context Broker
- **Entity Storage**: Alarm entities are stored in the context broker
- **Subscriptions**: Can subscribe to device changes to trigger alarms

### Quantum Leap Integration

- **Historical Data**: Alarm events are archived to Quantum Leap
- **Time Series**: Historical alarm data for reporting and analysis

### Notification Services

- **Email Integration**: Triggered alarms can send email notifications
- **SMS Integration**: Critical alarms can trigger SMS alerts
- **Webhook Integration**: Custom webhook notifications for alarm events

## Business Rules

The Alarm API implements business logic for:

1. **Threshold Monitoring**: Continuously monitors sensor values against alarm rules
2. **Alarm Triggering**: Triggers alarms when thresholds are breached
3. **Alarm States**: Manages alarm lifecycle (active, acknowledged, resolved)
4. **Escalation**: Handles alarm escalation based on severity and duration
5. **Notification Routing**: Routes alarm notifications based on rules

## Security Considerations

- **No Direct Internet Access**: Only accessible through KrakenD API Gateway
- **Tenant Isolation**: Strict data isolation between tenants
- **Input Validation**: All API inputs are validated
- **Rate Limiting**: Handled at the API Gateway level
- **Audit Logging**: All alarm operations are logged for audit purposes

## Troubleshooting

### Common Issues

1. **Connection Errors**: Verify MongoDB connection string
2. **Tenant Issues**: Ensure `NGSILD-Tenant` header is present
3. **Authentication**: Verify KrakenD configuration and Keycloak tokens

### Debug Commands

```bash
# Check pod status
kubectl get pods -l app=sensors-report-alarm-api

# View logs
kubectl logs -f deployment/sensors-report-alarm-api

# Test internal connectivity
kubectl exec -it test-pod -- curl http://sensors-report-alarm-api/api/health
```

## Contributing

1. Follow the standard SensorsReport development patterns
2. Implement proper error handling and logging
3. Include unit tests for new functionality
4. Update API documentation for new endpoints
5. Ensure multi-tenant compliance

## Related Services

- **Business Broker API**: Handles business rule evaluation
- **Email API**: Sends alarm notification emails  
- **SMS API**: Sends alarm notification SMS messages
- **Workflow API**: Manages alarm workflows and escalation
- **Audit API**: Logs all alarm-related activities

---

**Version**: 1.0.0  
**Last Updated**: July 13, 2025  
**Maintainer**: SensorsReport Development Team
