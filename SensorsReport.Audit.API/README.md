# SensorsReport.Audit.API

A REST API service for managing and retrieving audit records within the SensorsReport ecosystem. This service provides HTTP endpoints for storing audit events and querying historical audit data from QuantumLeap time-series database with Keycloak-based authentication.

## Overview

SensorsReport.Audit.API is a .NET 8.0 web API that serves as the primary interface for audit data management in the SensorsReport microservices architecture. It acts as a gateway between client applications and QuantumLeap, providing secure, authenticated access to audit logging and retrieval functionality.

## Features

### Core API Endpoints
- **POST /audit**: Store new audit records in QuantumLeap
- **GET /audit**: Retrieve audit records with flexible filtering
- **GET /audit/health**: Health check endpoint for monitoring
- **GET /audit/version**: Service version information

### Security & Authentication
- **JWT Bearer Authentication**: Keycloak integration for token validation
- **Token Introspection**: Online token validation against Keycloak
- **Secure Configuration**: Environment-based sensitive data management
- **Role-based Access**: Authorization policies for audit data access

### Data Management
- **QuantumLeap Integration**: Direct storage and retrieval from time-series database
- **NGSI-LD Compliance**: Audit records stored in standardized format
- **Flexible Filtering**: Query by date range, user, resource type, and entity ID
- **Pagination Support**: Efficient handling of large result sets

### Operations & Monitoring
- **Swagger/OpenAPI**: Interactive API documentation
- **Structured Logging**: NLog integration with detailed request logging
- **Health Checks**: Kubernetes-ready liveness and readiness probes
- **Containerized Deployment**: Docker support with multi-stage builds

## Technology Stack

- **.NET 8.0**: Web API framework
- **ASP.NET Core**: HTTP service foundation
- **JWT Bearer**: Token-based authentication
- **Keycloak**: Identity and access management
- **QuantumLeap**: Time-series database backend
- **Newtonsoft.Json**: JSON serialization
- **NLog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **Docker**: Containerization
- **Kubernetes**: Orchestration (via Flux)

## Project Structure

```
SensorsReport.Audit.API/
├── Controllers/
│   └── AuditController.cs       # Main API controller
├── Program.cs                   # Application startup and configuration
├── Properties/
│   └── launchSettings.json     # Development settings
├── Dockerfile                   # Container build configuration
├── flux/                        # Kubernetes deployment manifests
│   ├── deployment.yaml         # Kubernetes deployment
│   └── service.yaml            # Kubernetes service
├── nlog.config                 # Logging configuration
└── SensorsReport.Audit.API.csproj
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Access to QuantumLeap instance
- Access to Keycloak instance
- Valid Keycloak client credentials
- Docker (for containerized deployment)

### Local Development

#### 1. Clone and Setup
```bash
cd SensorsReport.Audit.API
dotnet restore
```

#### 2. Configure Environment Variables
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

# Application Configuration
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://localhost:5000"
```

#### 3. Run the Application
```bash
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger`.

### Docker Deployment

#### Build Container
```bash
# From the root SensorsReport directory
docker build -f SensorsReport.Audit.API/Dockerfile -t sensorsreport-audit-api:latest .
```

#### Run Container
```bash
docker run -d \
  --name audit-api \
  -p 8080:80 \
  -e SR_AUDIT_QUANTUMLEAP_HOST="quantum.sensorsreport.net" \
  -e SR_AUDIT_KEYCLOAK_URL="keycloak.sensorsreport.net" \
  -e SR_AUDIT_KEYCLOAK_CLIENTSECRET="your-secret" \
  sensorsreport-audit-api:latest
```

### Kubernetes Deployment

Deploy using Flux manifests:
```bash
kubectl apply -f flux/
```

## API Reference

### Authentication

All endpoints require a valid JWT token in the Authorization header:
```
Authorization: Bearer <jwt-token>
```

### Endpoints

#### Store Audit Record
```http
POST /audit
Content-Type: application/json
Authorization: Bearer <token>

{
  "id": "urn:ngsi-ld:AuditRecord:12345",
  "type": "AuditRecord",
  "actionType": "READ",
  "userId": "user-123",
  "userName": "john.doe",
  "resourceId": "sensor-001",
  "resourceType": "Device",
  "details": "User accessed sensor data",
  "timestamp": "2025-08-18T10:30:00Z",
  "location": "Building A",
  "tenantId": "tenant-789"
}
```

**Response**: `201 Created`
```json
{
  "id": "urn:ngsi-ld:AuditRecord:12345",
  "type": "AuditRecord",
  "actionType": "READ",
  "userId": "user-123",
  "userName": "john.doe",
  "resourceId": "sensor-001",
  "resourceType": "Device",
  "details": "User accessed sensor data",
  "timestamp": "2025-08-18T10:30:00Z",
  "location": "Building A",
  "tenantId": "tenant-789"
}
```

#### Retrieve Audit Records
```http
GET /audit?userId=user-123&resourceType=Device&fromDate=2025-08-01&toDate=2025-08-18&limit=50&offset=0
Authorization: Bearer <token>
```

**Query Parameters:**
- `entityId`: Filter by specific audit record ID
- `entityType`: Filter by entity type (default: "AuditRecord")
- `fromDate`: Start date filter (ISO 8601 format)
- `toDate`: End date filter (ISO 8601 format)
- `userId`: Filter by user ID
- `resourceType`: Filter by resource type
- `limit`: Maximum number of records (default: 100)
- `offset`: Number of records to skip (default: 0)

**Response**: `200 OK`
```json
[
  {
    "id": "urn:ngsi-ld:AuditRecord:12345",
    "type": "AuditRecord",
    "attributes": {
      "actionType": { "value": "READ", "type": "Text" },
      "userId": { "value": "user-123", "type": "Text" },
      "userName": { "value": "john.doe", "type": "Text" },
      "resourceId": { "value": "sensor-001", "type": "Text" },
      "resourceType": { "value": "Device", "type": "Text" },
      "timestamp": { "value": "2025-08-18T10:30:00Z", "type": "DateTime" }
    }
  }
]
```

#### Health Check
```http
GET /audit/health
```

**Response**: `200 OK`
```
OK
```

#### Version Information
```http
GET /audit/version
```

**Response**: `200 OK`
```
1.2.3
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `SR_AUDIT_QUANTUMLEAP_HOST` | QuantumLeap hostname | `quantum.sensorsreport.net` |
| `SR_AUDIT_QUANTUMLEAP_PORT` | QuantumLeap port | `8668` |
| `SR_AUDIT_KEYCLOAK_URL` | Keycloak hostname | `keycloak.sensorsreport.net` |
| `SR_AUDIT_KEYCLOAK_PORT` | Keycloak port | `30100` |
| `SR_AUDIT_KEYCLOAK_REALM` | Keycloak realm | `sr` |
| `SR_AUDIT_KEYCLOAK_CLIENTID` | OAuth2 client ID | `ContextBroker` |
| `SR_AUDIT_KEYCLOAK_CLIENTSECRET` | OAuth2 client secret | Required |
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ASPNETCORE_URLS` | Binding URLs | `http://+:80` |

### Application Settings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Usage Examples

### Storing Audit Events

#### Basic Audit Log
```bash
curl -X POST "http://localhost:5000/audit" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "actionType": "CREATE",
    "userId": "user-456",
    "userName": "jane.smith",
    "resourceId": "alarm-rule-789",
    "resourceType": "AlarmRule",
    "details": "Created new temperature alarm rule",
    "location": "Data Center"
  }'
```

#### Detailed Audit Record
```bash
curl -X POST "http://localhost:5000/audit" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "urn:ngsi-ld:AuditRecord:custom-123",
    "actionType": "DELETE",
    "userId": "admin-001",
    "userName": "administrator",
    "resourceId": "sensor-999",
    "resourceType": "Device",
    "details": "Removed decommissioned sensor",
    "timestamp": "2025-08-18T15:30:00Z",
    "location": "Building B",
    "tenantId": "tenant-abc",
    "ipAddress": "192.168.1.100",
    "userAgent": "SensorsReport-Explorer/1.0"
  }'
```

### Querying Audit Data

#### Recent Activity by User
```bash
curl -X GET "http://localhost:5000/audit?userId=user-123&limit=20" \
  -H "Authorization: Bearer your-jwt-token"
```

#### Activity in Date Range
```bash
curl -X GET "http://localhost:5000/audit?fromDate=2025-08-01T00:00:00Z&toDate=2025-08-18T23:59:59Z&resourceType=Device" \
  -H "Authorization: Bearer your-jwt-token"
```

#### Paginated Results
```bash
curl -X GET "http://localhost:5000/audit?limit=50&offset=100" \
  -H "Authorization: Bearer your-jwt-token"
```

## Integration Examples

### .NET Client
```csharp
public class AuditApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AuditApiClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<bool> LogAuditAsync(string token, AuditRecord record)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var json = JsonConvert.SerializeObject(record);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/audit", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AuditRecord>> GetAuditRecordsAsync(string token, 
        string? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(userId)) queryParams.Add($"userId={userId}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate:yyyy-MM-ddTHH:mm:ssZ}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate:yyyy-MM-ddTHH:mm:ssZ}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var response = await _httpClient.GetAsync($"{_baseUrl}/audit{query}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AuditRecord>>(json) ?? new List<AuditRecord>();
        }

        return new List<AuditRecord>();
    }
}
```

### JavaScript Client
```javascript
class AuditApiClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    async logAudit(token, auditRecord) {
        const response = await fetch(`${this.baseUrl}/audit`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(auditRecord)
        });

        return response.ok;
    }

    async getAuditRecords(token, filters = {}) {
        const params = new URLSearchParams();
        Object.keys(filters).forEach(key => {
            if (filters[key]) params.append(key, filters[key]);
        });

        const url = `${this.baseUrl}/audit${params.toString() ? '?' + params.toString() : ''}`;
        const response = await fetch(url, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            return await response.json();
        }
        return [];
    }
}

// Usage
const auditClient = new AuditApiClient('http://localhost:5000');

// Log audit event
await auditClient.logAudit(token, {
    actionType: 'READ',
    userId: 'user-123',
    resourceId: 'sensor-001',
    resourceType: 'Device',
    details: 'Dashboard access'
});

// Get recent audit records
const records = await auditClient.getAuditRecords(token, {
    userId: 'user-123',
    limit: 20
});
```

## Monitoring and Observability

### Health Checks
The API provides health check endpoints for monitoring:

```bash
# Basic health check
curl http://localhost:5000/audit/health

# Version information
curl http://localhost:5000/audit/version
```

### Logging
Structured logging captures:
- Request/response details
- Authentication events
- QuantumLeap interactions
- Error conditions
- Performance metrics

### Kubernetes Probes
```yaml
livenessProbe:
  httpGet:
    path: /audit/health
    port: 80
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /audit/health
    port: 80
  initialDelaySeconds: 5
  periodSeconds: 5
```

## Error Handling

### HTTP Status Codes
- `200 OK`: Successful retrieval
- `201 Created`: Audit record stored successfully
- `400 Bad Request`: Invalid request format or parameters
- `401 Unauthorized`: Missing or invalid authentication token
- `500 Internal Server Error`: Server-side processing error

### Error Response Format
```json
{
  "error": "Invalid request format",
  "details": "Required field 'actionType' is missing",
  "timestamp": "2025-08-18T10:30:00Z"
}
```

## Security Considerations

1. **JWT Validation**: All tokens are validated against Keycloak
2. **Token Introspection**: Online validation prevents replay attacks
3. **HTTPS**: Use HTTPS in production environments
4. **Rate Limiting**: Consider implementing rate limiting for production
5. **Input Validation**: All input data is validated before processing
6. **Audit Integrity**: Stored audit records are immutable

## Performance Optimization

- **Connection Pooling**: HTTP client reuse for QuantumLeap connections
- **Async Operations**: Non-blocking I/O for all external calls
- **Pagination**: Efficient handling of large result sets
- **Caching**: Authentication token caching to reduce Keycloak calls
- **Logging**: Structured logging minimizes performance impact

## Dependencies

This API depends on:
- **SensorsReport.Audit**: Core audit library for data models and authentication
- **QuantumLeap**: Time-series database for audit storage
- **Keycloak**: Identity and access management

## Related Services

- **SensorsReport.Audit**: Core audit library
- **SensorsReport.Api.Core**: Shared functionality
- **Sensors-Report-Explorer**: Frontend dashboard with audit views
- **QuantumLeap**: Time-series database backend
- **Keycloak**: Authentication service

## Contributing

When contributing to this API:
1. Maintain backward compatibility for existing endpoints
2. Add comprehensive unit and integration tests
3. Update OpenAPI documentation for new endpoints
4. Follow established logging and error handling patterns
5. Test with actual Keycloak and QuantumLeap instances

## License

This project is part of the SensorsReport system for AerOS. See the root LICENSE file for details.

## Support

For issues with the Audit API:
1. Check service health endpoints
2. Verify Keycloak and QuantumLeap connectivity
3. Review application logs for detailed error information
4. Consult the Swagger UI for API documentation
5. Contact the SensorsReport development team
