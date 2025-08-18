# SensorsReport.SMS.API

A REST API service for managing SMS notifications within the SensorsReport ecosystem. This service provides HTTP endpoints for creating, tracking, and managing SMS messages with multi-tenant support, provider management, phone number validation, and automated status updates.

## Overview

SensorsReport.SMS.API is a .NET 8.0 web API that handles SMS notification requests in the SensorsReport microservices architecture. It provides a comprehensive SMS management system with provider integration, multi-tenant isolation, phone number validation using libphonenumber, and real-time status tracking. The service stores SMS records in MongoDB and supports multiple SMS providers with automatic failover capabilities.

## Features

### Core API Endpoints

#### SMS Management
- **GET /api/sms**: List SMS messages with filtering and pagination
- **GET /api/sms/{id}**: Retrieve specific SMS by ID
- **POST /api/sms**: Create new SMS requests
- **PUT /api/sms/{id}**: Update SMS records
- **PATCH /api/sms/{id}**: Partial SMS updates
- **DELETE /api/sms/{id}**: Remove SMS records

#### Provider Management
- **POST /api/provider/register**: Register SMS providers
- **GET /api/provider/{name}**: Get provider by name
- **GET /api/provider/status/{status}**: Get providers by status

### SMS Management Features
- **Status Tracking**: Complete SMS lifecycle monitoring (Pending → Entrusted → Sent/Failed/Expired)
- **Multi-Provider Support**: Support for multiple SMS service providers with load balancing
- **Phone Number Validation**: International phone number validation using libphonenumber-csharp
- **Retry Logic**: Automatic retry mechanism with configurable retry counts
- **TTL Support**: Time-to-live functionality for SMS messages
- **Country Code Support**: International SMS with country code management

### Multi-Tenant Architecture
- **Tenant Isolation**: Complete data isolation per tenant using tenant headers
- **Tenant-Aware Queries**: All operations respect tenant boundaries
- **Tenant Services**: Built-in tenant resolution and validation
- **Secure Access**: JWT-based authentication with tenant context

### Provider Management
- **Provider Registration**: Dynamic registration of SMS service providers
- **Status Monitoring**: Real-time provider health and status tracking
- **Country Code Mapping**: Provider support for specific country codes
- **Automatic Failover**: Load balancing and failover between providers

### Operations & Monitoring
- **Background Tasks**: Automated status updates and maintenance
- **Flexible Filtering**: Query by status, date range, country code, and provider
- **Pagination Support**: Efficient handling of large SMS lists
- **Custom Metadata**: Extensible custom data fields for SMS tracking

## Technology Stack

- **.NET 8.0**: Web API framework
- **ASP.NET Core**: HTTP service foundation
- **MongoDB 3.4.0**: Document database for SMS and provider storage
- **libphonenumber-csharp 9.0.11**: International phone number validation
- **JWT Bearer Authentication**: Token-based security
- **NLog**: Structured logging
- **System.Text.Json**: JSON serialization with extensible data support
- **Docker**: Containerization
- **Kubernetes**: Orchestration (via Flux)

## Project Structure

```
SensorsReport.SMS.API/
├── Controllers/
│   ├── SmsController.cs         # SMS management endpoints
│   └── ProviderController.cs    # Provider management endpoints
├── Models/
│   ├── SmsModel.cs             # SMS data model
│   ├── SmsStatusEnum.cs        # SMS status enumeration
│   ├── ProviderModel.cs        # Provider data model
│   └── ProviderStatusEnum.cs   # Provider status enumeration
├── Repositories/
│   ├── ISmsRepository.cs       # SMS repository interface
│   ├── SmsRepository.cs        # MongoDB SMS implementation
│   ├── IProviderRepository.cs  # Provider repository interface
│   └── ProviderRepository.cs   # MongoDB provider implementation
├── Helpers/
│   └── PhoneNumberHelper.cs    # Phone number validation utilities
├── Tasks/
│   └── UpdateSmsStatusTask.cs  # Background status update service
├── Consumers/                   # Message queue consumers (if any)
├── Program.cs                   # Application startup and configuration
├── Dockerfile                   # Container build configuration
├── flux/                        # Kubernetes deployment manifests
└── nlog.config                 # Logging configuration
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- MongoDB instance
- Valid JWT authentication provider
- SMS service provider credentials (Twilio, AWS SNS, etc.)

### Local Development

#### 1. Clone and Setup
```bash
cd SensorsReport.SMS.API
dotnet restore
```

#### 2. Configure Environment Variables
```bash
# MongoDB Configuration
export SmsMongoDbConnectionOptions__ConnectionString="mongodb://localhost:27017"
export SmsMongoDbConnectionOptions__DatabaseName="sensorsreport_sms"
export ProviderMongoDbConnectionOptions__ConnectionString="mongodb://localhost:27017"
export ProviderMongoDbConnectionOptions__DatabaseName="sensorsreport_providers"

# SMS Configuration
export SmsOptions__ProviderTrustTimeoutInSecond="1800"
export SmsOptions__MaxRetryCount="3"
export SmsOptions__DefaultTtlInMinutes="30"

# JWT Authentication (inherited from Api.Core)
export JWT__Authority="https://your-auth-provider.com"
export JWT__Audience="sensorsreport-api"

# Application Configuration
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://localhost:5000"
```

#### 3. Run the Application
```bash
dotnet run
```

The API will be available at `http://localhost:5000`.

### Docker Deployment

#### Build Container
```bash
# From the root SensorsReport directory
docker build -f SensorsReport.SMS.API/Dockerfile -t sensorsreport-sms-api:latest .
```

#### Run Container
```bash
docker run -d \
  --name sms-api \
  -p 8080:80 \
  -e SmsMongoDbConnectionOptions__ConnectionString="mongodb://mongo:27017" \
  -e SmsMongoDbConnectionOptions__DatabaseName="sensorsreport_sms" \
  sensorsreport-sms-api:latest
```

### Kubernetes Deployment

Deploy using Flux manifests:
```bash
kubectl apply -f flux/
```

## API Reference

### SMS Model

```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j1",
  "phoneNumber": "+1234567890",
  "message": "Alert: Sensor temperature exceeded threshold",
  "timestamp": "2025-08-18T10:30:00Z",
  "tenant": "tenant-123",
  "status": "Sent",
  "statusMessage": "The SMS was successfully sent.",
  "sentAt": "2025-08-18T10:31:15Z",
  "ttl": 30,
  "countryCode": "US",
  "provider": "twilio",
  "trackingId": "SM1234567890abcdef",
  "messageType": "Alert",
  "customData": "{\"alarmId\": \"alarm-123\", \"severity\": \"high\"}",
  "retryCount": 0
}
```

### SMS Status Values

| Status | Description |
|--------|-------------|
| `Pending` | SMS created, waiting to be processed |
| `Entrusted` | SMS handed off to provider for delivery |
| `Sent` | SMS successfully delivered |
| `Failed` | SMS delivery failed |
| `Expired` | SMS expired before delivery |
| `Error` | System error occurred |
| `Unknown` | Status unknown or not available |

### Provider Model

```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j2",
  "name": "twilio",
  "lastSeen": "2025-08-18T10:35:00Z",
  "status": "Active",
  "supportedCountryCodes": ["US", "CA", "GB", "AU"]
}
```

### Authentication

All endpoints require a valid JWT token and tenant header:
```
Authorization: Bearer <jwt-token>
X-Tenant: <tenant-id>
```

### Endpoints

#### Create SMS
```http
POST /api/sms
Content-Type: application/json
Authorization: Bearer <token>
X-Tenant: tenant-123

{
  "phoneNumber": "+1234567890",
  "message": "Your sensor alert: Temperature is 85°C",
  "provider": "twilio",
  "messageType": "Alert",
  "ttl": 30,
  "customData": "{\"sensorId\": \"SR-001\", \"threshold\": 80}"
}
```

**Response**: `201 Created`
```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j1",
  "phoneNumber": "+1234567890",
  "message": "Your sensor alert: Temperature is 85°C",
  "timestamp": "2025-08-18T10:30:00Z",
  "status": "Pending",
  "statusMessage": "The SMS is pending processing.",
  "provider": "twilio",
  "messageType": "Alert",
  "ttl": 30,
  "retryCount": 0
}
```

#### Get SMS by ID
```http
GET /api/sms/64f1a2b3c4d5e6f7g8h9i0j1
Authorization: Bearer <token>
X-Tenant: tenant-123
```

**Response**: `200 OK`
```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j1",
  "phoneNumber": "+1234567890",
  "message": "Your sensor alert: Temperature is 85°C",
  "status": "Sent",
  "statusMessage": "The SMS was successfully sent.",
  "sentAt": "2025-08-18T10:31:15Z",
  "trackingId": "SM1234567890abcdef"
}
```

#### List SMS with Filters
```http
GET /api/sms?status=Sent&countryCode=US&fromDate=2025-08-01&toDate=2025-08-18&limit=50&offset=0
Authorization: Bearer <token>
X-Tenant: tenant-123
```

**Query Parameters:**
- `status`: Filter by SMS status (Pending, Entrusted, Sent, Failed, Expired, Error, Unknown)
- `countryCode`: Filter by country code (US, GB, CA, etc.)
- `fromDate`: Start date filter (ISO 8601 format)
- `toDate`: End date filter (ISO 8601 format)
- `limit`: Maximum number of records (default: 100)
- `offset`: Number of records to skip (default: 0)

**Response**: `200 OK`
```json
[
  {
    "id": "64f1a2b3c4d5e6f7g8h9i0j1",
    "phoneNumber": "+1234567890",
    "message": "Your sensor alert: Temperature is 85°C",
    "status": "Sent",
    "sentAt": "2025-08-18T10:31:15Z"
  }
]
```

#### Update SMS
```http
PUT /api/sms/64f1a2b3c4d5e6f7g8h9i0j1
Content-Type: application/json
Authorization: Bearer <token>
X-Tenant: tenant-123

{
  "phoneNumber": "+1234567890",
  "message": "Updated message content",
  "status": "Failed",
  "retryCount": 1,
  "customData": "{\"error\": \"Provider timeout\"}"
}
```

#### Partial Update (Patch)
```http
PATCH /api/sms/64f1a2b3c4d5e6f7g8h9i0j1
Content-Type: application/json
Authorization: Bearer <token>
X-Tenant: tenant-123

{
  "status": "Sent",
  "sentAt": "2025-08-18T10:31:15Z",
  "trackingId": "SM1234567890abcdef"
}
```

#### Register SMS Provider
```http
POST /api/provider/register
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "twilio",
  "supportedCountryCodes": ["US", "CA", "GB", "AU", "FR", "DE"]
}
```

**Response**: `201 Created`
```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j2",
  "name": "twilio",
  "lastSeen": "2025-08-18T10:35:00Z",
  "status": "Active",
  "supportedCountryCodes": ["US", "CA", "GB", "AU", "FR", "DE"]
}
```

#### Get Provider by Name
```http
GET /api/provider/twilio
Authorization: Bearer <token>
```

## Configuration

### MongoDB Configuration

```json
{
  "SmsMongoDbConnectionOptions": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "sensorsreport_sms"
  },
  "ProviderMongoDbConnectionOptions": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "sensorsreport_providers"
  }
}
```

### SMS Options

```json
{
  "SmsOptions": {
    "ProviderTrustTimeoutInSecond": 1800,
    "MaxRetryCount": 3,
    "DefaultTtlInMinutes": 30
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `SmsMongoDbConnectionOptions__ConnectionString` | MongoDB connection for SMS storage | Required |
| `SmsMongoDbConnectionOptions__DatabaseName` | Database name for SMS collection | Required |
| `ProviderMongoDbConnectionOptions__ConnectionString` | MongoDB connection for providers | Required |
| `ProviderMongoDbConnectionOptions__DatabaseName` | Database name for providers | Required |
| `SmsOptions__ProviderTrustTimeoutInSecond` | Provider trust timeout | `1800` (30 min) |
| `SmsOptions__MaxRetryCount` | Maximum retry attempts | `3` |
| `SmsOptions__DefaultTtlInMinutes` | Default SMS TTL | `30` |

## Usage Examples

### Creating SMS Notifications

#### Simple SMS Alert
```bash
curl -X POST "http://localhost:5000/api/sms" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-Tenant: tenant-123" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+14155552671",
    "message": "ALERT: Sensor SR-001 temperature exceeded 80°C",
    "provider": "twilio",
    "messageType": "Critical Alert"
  }'
```

#### International SMS with Custom Data
```bash
curl -X POST "http://localhost:5000/api/sms" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-Tenant: tenant-123" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+447911123456",
    "message": "Maintenance required for sensor network",
    "countryCode": "GB",
    "provider": "twilio",
    "messageType": "Maintenance",
    "ttl": 60,
    "customData": "{\"severity\": \"medium\", \"location\": \"Building A\"}"
  }'
```

#### Bulk SMS with Different Providers
```bash
# US number via Twilio
curl -X POST "http://localhost:5000/api/sms" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-Tenant: tenant-123" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+12125551234",
    "message": "System maintenance scheduled",
    "provider": "twilio"
  }'

# International number via AWS SNS
curl -X POST "http://localhost:5000/api/sms" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-Tenant: tenant-123" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+33123456789",
    "message": "System maintenance scheduled",
    "provider": "aws-sns"
  }'
```

### Querying SMS Data

#### Check SMS Status
```bash
curl "http://localhost:5000/api/sms/64f1a2b3c4d5e6f7g8h9i0j1" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-Tenant: tenant-123"
```

#### Get Failed SMS Messages
```bash
curl "http://localhost:5000/api/sms?status=Failed&limit=20" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-Tenant: tenant-123"
```

#### Get SMS by Country Code
```bash
curl "http://localhost:5000/api/sms?countryCode=US&fromDate=2025-08-18T00:00:00Z" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-Tenant: tenant-123"
```

### Provider Management

#### Register New Provider
```bash
curl -X POST "http://localhost:5000/api/provider/register" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "aws-sns",
    "supportedCountryCodes": ["US", "CA", "GB", "DE", "FR", "AU", "JP"]
  }'
```

#### Check Provider Status
```bash
curl "http://localhost:5000/api/provider/twilio" \
  -H "Authorization: Bearer your-jwt-token"
```

## Integration Examples

### .NET Client
```csharp
public class SmsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public SmsApiClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<SmsModel?> SendSmsAsync(string token, string tenant, SmsModel sms)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _httpClient.DefaultRequestHeaders.Add("X-Tenant", tenant);

        var json = JsonSerializer.Serialize(sms);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/sms", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SmsModel>(responseJson);
        }

        return null;
    }

    public async Task<SmsModel?> GetSmsStatusAsync(string token, string tenant, string smsId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _httpClient.DefaultRequestHeaders.Add("X-Tenant", tenant);

        var response = await _httpClient.GetAsync($"{_baseUrl}/api/sms/{smsId}");
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SmsModel>(json);
        }

        return null;
    }

    public async Task<List<SmsModel>> GetSmsListAsync(string token, string tenant,
        SmsStatusEnum? status = null, string? countryCode = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int limit = 100, int offset = 0)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _httpClient.DefaultRequestHeaders.Add("X-Tenant", tenant);

        var queryParams = new List<string>();
        if (status.HasValue) queryParams.Add($"status={status}");
        if (!string.IsNullOrEmpty(countryCode)) queryParams.Add($"countryCode={countryCode}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate:yyyy-MM-ddTHH:mm:ssZ}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate:yyyy-MM-ddTHH:mm:ssZ}");
        queryParams.Add($"limit={limit}");
        queryParams.Add($"offset={offset}");

        var query = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/sms?{query}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<SmsModel>>(json) ?? new List<SmsModel>();
        }

        return new List<SmsModel>();
    }

    public async Task<bool> UpdateSmsStatusAsync(string token, string tenant, string smsId, 
        SmsStatusEnum status, string? trackingId = null)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _httpClient.DefaultRequestHeaders.Add("X-Tenant", tenant);

        var updateData = new Dictionary<string, object>
        {
            ["status"] = status.ToString(),
            ["sentAt"] = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(trackingId))
            updateData["trackingId"] = trackingId;

        var json = JsonSerializer.Serialize(updateData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PatchAsync($"{_baseUrl}/api/sms/{smsId}", content);
        return response.IsSuccessStatusCode;
    }
}
```

### JavaScript Client
```javascript
class SmsApiClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    async sendSms(token, tenant, smsData) {
        const response = await fetch(`${this.baseUrl}/api/sms`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'X-Tenant': tenant,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(smsData)
        });

        if (response.ok) {
            return await response.json();
        }
        
        throw new Error(`Failed to send SMS: ${response.statusText}`);
    }

    async getSmsStatus(token, tenant, smsId) {
        const response = await fetch(`${this.baseUrl}/api/sms/${smsId}`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'X-Tenant': tenant
            }
        });
        
        if (response.ok) {
            return await response.json();
        }
        
        return null;
    }

    async getSmsList(token, tenant, filters = {}) {
        const params = new URLSearchParams();
        Object.keys(filters).forEach(key => {
            if (filters[key] !== undefined && filters[key] !== null) {
                params.append(key, filters[key]);
            }
        });

        const url = `${this.baseUrl}/api/sms${params.toString() ? '?' + params.toString() : ''}`;
        const response = await fetch(url, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'X-Tenant': tenant
            }
        });

        if (response.ok) {
            return await response.json();
        }

        return [];
    }

    async registerProvider(token, providerData) {
        const response = await fetch(`${this.baseUrl}/api/provider/register`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(providerData)
        });

        if (response.ok) {
            return await response.json();
        }

        throw new Error(`Failed to register provider: ${response.statusText}`);
    }
}

// Usage examples
const smsClient = new SmsApiClient('http://localhost:5000');

// Send SMS
const sms = await smsClient.sendSms(token, 'tenant-123', {
    phoneNumber: '+1234567890',
    message: 'Alert: Sensor offline',
    provider: 'twilio',
    messageType: 'Alert'
});

// Check status
const status = await smsClient.getSmsStatus(token, 'tenant-123', sms.id);

// Get failed SMS messages
const failedSms = await smsClient.getSmsList(token, 'tenant-123', { 
    status: 'Failed',
    limit: 50 
});

// Register provider
const provider = await smsClient.registerProvider(token, {
    name: 'new-provider',
    supportedCountryCodes: ['US', 'CA']
});
```

## Phone Number Validation

The API uses `libphonenumber-csharp` for international phone number validation:

```csharp
// Phone numbers should be in E.164 format
"+1234567890"  // US number
"+447911123456"  // UK number
"+33123456789"   // French number
"+861234567890"  // Chinese number
```

**Validation Rules:**
- All phone numbers must be in international E.164 format
- Country codes are automatically extracted and validated
- Invalid numbers return `400 Bad Request` with validation details

## Multi-Tenant Architecture

### Tenant Isolation
- All SMS data is isolated by tenant
- Tenant context is provided via `X-Tenant` header
- Repository operations automatically filter by tenant
- Cross-tenant data access is prevented

### Tenant Services
The API integrates with SensorsReport.Api.Core tenant services:
```csharp
services.AddTenantServices();  // Enables tenant resolution
```

## Background Tasks

### SMS Status Updates
The `UpdateSmsStatusBackgroundService` performs:
- Periodic status synchronization with SMS providers
- Retry failed SMS messages within retry limits
- Update expired SMS messages based on TTL
- Provider health monitoring and failover

## Monitoring and Observability

### SMS Analytics
Monitor SMS delivery metrics:
```bash
# Success rate by provider
curl "http://localhost:5000/api/sms?status=Sent&limit=1000" | jq '[.[] | .provider] | group_by(.) | map({provider: .[0], count: length})'

# Failed SMS analysis
curl "http://localhost:5000/api/sms?status=Failed&limit=1000" | jq '[.[] | .countryCode] | group_by(.) | map({country: .[0], failures: length})'
```

### Provider Health
```bash
# Check active providers
curl "http://localhost:5000/api/provider/status/Active"

# Provider performance
curl "http://localhost:5000/api/sms?limit=1000" | jq '[.[] | select(.status == "Sent")] | group_by(.provider) | map({provider: .[0].provider, sent: length})'
```

### Logging
Structured logging captures:
- SMS creation and status updates
- Provider registration and health changes
- Phone number validation results
- Retry attempts and failures
- Performance metrics

## Error Handling

### HTTP Status Codes
- `200 OK`: Successful retrieval or update
- `201 Created`: SMS or provider created successfully
- `204 No Content`: Successful deletion
- `400 Bad Request`: Invalid phone number, SMS data, or parameters
- `401 Unauthorized`: Missing or invalid authentication
- `404 Not Found`: SMS or provider not found
- `500 Internal Server Error`: Server processing error

### Validation Errors
Common validation scenarios:
- **Invalid Phone Number**: Non-E.164 format or invalid country code
- **Missing Required Fields**: Phone number, message, or tenant header
- **Provider Not Found**: Specified provider not registered
- **Tenant Mismatch**: Accessing SMS from different tenant

### Retry Logic
Automatic retry handling:
- Default maximum retry count: 3 (configurable)
- Exponential backoff between retries
- Failed SMS enter error state after max retries
- Manual retry possible via status update

## Performance Considerations

- **Async Operations**: All database and external provider calls are asynchronous
- **Indexing**: MongoDB indexes on tenant, status, timestamp, and country code
- **Pagination**: Default 100 record limit with offset-based pagination
- **Connection Pooling**: MongoDB driver connection pooling
- **Provider Load Balancing**: Automatic distribution across available providers
- **Caching**: Provider status and configuration caching

## Security Considerations

1. **JWT Authentication**: All endpoints require valid JWT tokens
2. **Tenant Isolation**: Complete data separation between tenants
3. **Phone Number Validation**: Prevent injection and fraud attempts
4. **Input Sanitization**: All message content and metadata validated
5. **Rate Limiting**: Consider implementing rate limiting for production
6. **Provider Credentials**: Secure storage of SMS provider API keys
7. **Audit Trail**: Complete SMS lifecycle tracking for compliance

## Dependencies

This API depends on:
- **SensorsReport.Api.Core**: Core shared functionality and tenant services
- **MongoDB**: Document storage for SMS and provider records
- **libphonenumber-csharp**: International phone number validation
- **JWT Authentication Provider**: Token validation and user context

## Related Services

- **SensorsReport.Notification.API**: Coordinates multi-channel notifications
- **SensorsReport.Email.API**: Email notification management
- **SensorsReport.Api.Core**: Core shared functionality
- **SMS Provider Services**: External SMS delivery services (Twilio, AWS SNS, etc.)
- **Sensors-Report-Explorer**: Frontend dashboard for SMS monitoring

## Contributing

When contributing to this API:
1. Maintain backward compatibility for existing endpoints
2. Add comprehensive unit and integration tests
3. Update phone number validation for new country codes
4. Follow established logging and error handling patterns
5. Test with actual SMS providers in development
6. Ensure tenant isolation is maintained in all operations

## License

This project is part of the SensorsReport system for AerOS. See the root LICENSE file for details.

## Support

For issues with the SMS API:
1. Check MongoDB connectivity and database permissions
2. Verify JWT authentication configuration
3. Validate phone number formats (E.164 required)
4. Review provider registration and status
5. Monitor SMS status transitions for stuck messages
6. Check tenant header configuration and isolation
7. Contact the SensorsReport development team
