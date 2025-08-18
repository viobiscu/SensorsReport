# SensorsReport.Email.API

A REST API service for managing email notifications within the SensorsReport ecosystem. This service provides HTTP endpoints for creating, tracking, and managing email messages with MongoDB storage, message queue integration, and automated retry mechanisms.

## Overview

SensorsReport.Email.API is a .NET 8.0 web API that handles email notification requests in the SensorsReport microservices architecture. It provides a reliable email delivery system with status tracking, retry logic, and integration with message queues for asynchronous processing. The service stores email records in MongoDB and publishes events to trigger email sending by consumer services.

## Features

### Core API Endpoints
- **POST /api/emails**: Create new email requests
- **GET /api/emails/{id}**: Retrieve specific email by ID
- **GET /api/emails**: List emails with filtering and pagination
- **PUT /api/emails/{id}**: Update email records
- **PATCH /api/emails/{id}**: Partial email updates
- **DELETE /api/emails/{id}**: Remove email records

### Email Management
- **Status Tracking**: Complete email lifecycle monitoring (Pending → Queued → Sending → Sent/Failed)
- **Retry Logic**: Automatic retry mechanism with configurable retry counts
- **Error Handling**: Detailed error messages and failure tracking
- **Multi-recipient Support**: Support for To, CC, and BCC recipients

### Data Storage & Processing
- **MongoDB Integration**: Persistent storage for email records and metadata
- **Message Queue Events**: RabbitMQ integration for asynchronous email processing
- **Reconciliation Tasks**: Background services for email status reconciliation
- **Patch Operations**: Flexible partial updates using JSON Patch

### Operations & Monitoring
- **Status Filtering**: Query emails by delivery status
- **Date Range Queries**: Filter emails by creation/sent dates
- **Pagination Support**: Efficient handling of large email lists
- **Validation**: Input validation for email addresses and required fields

## Technology Stack

- **.NET 8.0**: Web API framework
- **ASP.NET Core**: HTTP service foundation
- **MongoDB 3.4.0**: Document database for email storage
- **MassTransit**: Message queue integration (via SensorsReport.Api.Core)
- **NLog**: Structured logging
- **System.Text.Json**: JSON serialization
- **Docker**: Containerization
- **Kubernetes**: Orchestration (via Flux)

## Project Structure

```
SensorsReport.Email.API/
├── Controllers/
│   └── EmailsController.cs      # Main API controller
├── Models/
│   ├── EmailModel.cs           # Email data model
│   └── EmailStatusEnum.cs      # Email status enumeration
├── Repositories/
│   ├── IEmailRepository.cs     # Repository interface
│   └── EmailRepository.cs      # MongoDB repository implementation
├── Consumers/
│   └── CreateEmailConsumer.cs  # Message queue consumer
├── Tasks/
│   └── ReconciliationEmailTask.cs # Background reconciliation service
├── Program.cs                   # Application startup and configuration
├── Dockerfile                   # Container build configuration
├── flux/                        # Kubernetes deployment manifests
├── nlog.config                 # Logging configuration
└── appsettings.json            # Application settings
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- MongoDB instance
- RabbitMQ instance (for message queue integration)
- Access to SensorsReport.Api.Core library

### Local Development

#### 1. Clone and Setup
```bash
cd SensorsReport.Email.API
dotnet restore
```

#### 2. Configure Environment Variables
```bash
# MongoDB Configuration
export EmailMongoDbConnectionOptions__ConnectionString="mongodb://localhost:27017"
export EmailMongoDbConnectionOptions__DatabaseName="sensorsreport_emails"

# Message Queue Configuration (inherited from Api.Core)
export EventBus__Host="localhost"
export EventBus__Username="guest"
export EventBus__Password="guest"

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
docker build -f SensorsReport.Email.API/Dockerfile -t sensorsreport-email-api:latest .
```

#### Run Container
```bash
docker run -d \
  --name email-api \
  -p 8080:80 \
  -e EmailMongoDbConnectionOptions__ConnectionString="mongodb://mongo:27017" \
  -e EmailMongoDbConnectionOptions__DatabaseName="sensorsreport_emails" \
  sensorsreport-email-api:latest
```

### Kubernetes Deployment

Deploy using Flux manifests:
```bash
kubectl apply -f flux/
```

## API Reference

### Email Model

```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j1",
  "toEmail": "recipient@example.com",
  "toName": "John Doe",
  "fromEmail": "sender@example.com",
  "fromName": "SensorsReport System",
  "ccEmail": "cc@example.com",
  "ccName": "CC Recipient",
  "bccEmail": "bcc@example.com",
  "bccName": "BCC Recipient",
  "subject": "Sensor Alert Notification",
  "bodyHtml": "<h1>Alert</h1><p>Sensor temperature exceeded threshold.</p>",
  "status": "Pending",
  "statusMessage": "The email is pending sending.",
  "sentAt": null,
  "retryCount": 0,
  "maxRetryCount": 3,
  "errorMessage": null,
  "createdAt": "2025-08-18T10:30:00Z",
  "lastUpdatedAt": "2025-08-18T10:30:00Z"
}
```

### Email Status Values

| Status | Description |
|--------|-------------|
| `Pending` | Email created, waiting to be queued |
| `Queued` | Email added to message queue for processing |
| `Sending` | Email being processed by consumer |
| `Sent` | Email successfully delivered |
| `Failed` | Email delivery failed |
| `Retry` | Email waiting in retry queue |

### Endpoints

#### Create Email
```http
POST /api/emails
Content-Type: application/json

{
  "toEmail": "user@example.com",
  "toName": "John Doe",
  "fromEmail": "alerts@sensorsreport.net",
  "fromName": "SensorsReport Alerts",
  "subject": "Temperature Alert",
  "bodyHtml": "<h2>Alert</h2><p>Sensor SR-001 temperature is 85°C</p>",
  "maxRetryCount": 3
}
```

**Response**: `201 Created`
```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j1",
  "toEmail": "user@example.com",
  "toName": "John Doe",
  "fromEmail": "alerts@sensorsreport.net",
  "fromName": "SensorsReport Alerts",
  "subject": "Temperature Alert",
  "bodyHtml": "<h2>Alert</h2><p>Sensor SR-001 temperature is 85°C</p>",
  "status": "Queued",
  "statusMessage": "The email has been successfully added to the queue.",
  "retryCount": 0,
  "maxRetryCount": 3,
  "createdAt": "2025-08-18T10:30:00Z",
  "lastUpdatedAt": "2025-08-18T10:30:00Z"
}
```

#### Get Email by ID
```http
GET /api/emails/64f1a2b3c4d5e6f7g8h9i0j1
```

**Response**: `200 OK`
```json
{
  "id": "64f1a2b3c4d5e6f7g8h9i0j1",
  "toEmail": "user@example.com",
  "toName": "John Doe",
  "subject": "Temperature Alert",
  "status": "Sent",
  "statusMessage": "The email was successfully sent.",
  "sentAt": "2025-08-18T10:32:15Z"
}
```

#### List Emails with Filters
```http
GET /api/emails?status=Sent&fromDate=2025-08-01&toDate=2025-08-18&limit=50&offset=0
```

**Query Parameters:**
- `status`: Filter by email status (Pending, Queued, Sending, Sent, Failed, Retry)
- `fromDate`: Start date filter (ISO 8601 format)
- `toDate`: End date filter (ISO 8601 format)
- `limit`: Maximum number of records (default: 100)
- `offset`: Number of records to skip (default: 0)

**Response**: `200 OK`
```json
[
  {
    "id": "64f1a2b3c4d5e6f7g8h9i0j1",
    "toEmail": "user@example.com",
    "subject": "Temperature Alert",
    "status": "Sent",
    "sentAt": "2025-08-18T10:32:15Z"
  }
]
```

#### Update Email
```http
PUT /api/emails/64f1a2b3c4d5e6f7g8h9i0j1
Content-Type: application/json

{
  "toEmail": "updated@example.com",
  "toName": "Updated Name",
  "subject": "Updated Subject",
  "bodyHtml": "<p>Updated content</p>",
  "maxRetryCount": 5
}
```

#### Partial Update (Patch)
```http
PATCH /api/emails/64f1a2b3c4d5e6f7g8h9i0j1
Content-Type: application/json

{
  "maxRetryCount": 5,
  "subject": "Updated Subject"
}
```

#### Delete Email
```http
DELETE /api/emails/64f1a2b3c4d5e6f7g8h9i0j1
```

**Response**: `204 No Content`

## Configuration

### MongoDB Configuration

```json
{
  "EmailMongoDbConnectionOptions": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "sensorsreport_emails"
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `EmailMongoDbConnectionOptions__ConnectionString` | MongoDB connection string | Required |
| `EmailMongoDbConnectionOptions__DatabaseName` | Database name for email storage | Required |
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ASPNETCORE_URLS` | HTTP binding URLs | `http://+:80` |

## Usage Examples

### Creating Email Notifications

#### Simple Email
```bash
curl -X POST "http://localhost:5000/api/emails" \
  -H "Content-Type: application/json" \
  -d '{
    "toEmail": "engineer@company.com",
    "toName": "System Engineer",
    "subject": "Sensor Offline Alert",
    "bodyHtml": "<h3>Alert</h3><p>Sensor SR-005 has gone offline.</p>"
  }'
```

#### Email with CC and BCC
```bash
curl -X POST "http://localhost:5000/api/emails" \
  -H "Content-Type: application/json" \
  -d '{
    "toEmail": "primary@company.com",
    "toName": "Primary Contact",
    "ccEmail": "manager@company.com",
    "ccName": "Manager",
    "bccEmail": "audit@company.com",
    "bccName": "Audit Team",
    "subject": "Critical System Alert",
    "bodyHtml": "<h2>Critical Alert</h2><p>Multiple sensors are reporting anomalies.</p>",
    "maxRetryCount": 5
  }'
```

#### Custom From Address
```bash
curl -X POST "http://localhost:5000/api/emails" \
  -H "Content-Type: application/json" \
  -d '{
    "toEmail": "admin@company.com",
    "toName": "Administrator",
    "fromEmail": "noreply@sensorsreport.net",
    "fromName": "SensorsReport Automated System",
    "subject": "Daily Report",
    "bodyHtml": "<h1>Daily Sensor Report</h1><p>All systems operating normally.</p>"
  }'
```

### Querying Email Status

#### Check Email Status
```bash
curl "http://localhost:5000/api/emails/64f1a2b3c4d5e6f7g8h9i0j1"
```

#### Get Failed Emails
```bash
curl "http://localhost:5000/api/emails?status=Failed&limit=20"
```

#### Get Recent Emails
```bash
curl "http://localhost:5000/api/emails?fromDate=2025-08-18T00:00:00Z&limit=100"
```

## Integration Examples

### .NET Client
```csharp
public class EmailApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public EmailApiClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<EmailModel?> SendEmailAsync(EmailModel email)
    {
        var json = JsonSerializer.Serialize(email);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/emails", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmailModel>(responseJson);
        }

        return null;
    }

    public async Task<EmailModel?> GetEmailStatusAsync(string emailId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/emails/{emailId}");
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmailModel>(json);
        }

        return null;
    }

    public async Task<List<EmailModel>> GetEmailsAsync(EmailStatusEnum? status = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int limit = 100, int offset = 0)
    {
        var queryParams = new List<string>();
        if (status.HasValue) queryParams.Add($"status={status}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate:yyyy-MM-ddTHH:mm:ssZ}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate:yyyy-MM-ddTHH:mm:ssZ}");
        queryParams.Add($"limit={limit}");
        queryParams.Add($"offset={offset}");

        var query = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/emails?{query}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<EmailModel>>(json) ?? new List<EmailModel>();
        }

        return new List<EmailModel>();
    }
}
```

### JavaScript Client
```javascript
class EmailApiClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    async sendEmail(email) {
        const response = await fetch(`${this.baseUrl}/api/emails`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(email)
        });

        if (response.ok) {
            return await response.json();
        }
        
        throw new Error(`Failed to send email: ${response.statusText}`);
    }

    async getEmailStatus(emailId) {
        const response = await fetch(`${this.baseUrl}/api/emails/${emailId}`);
        
        if (response.ok) {
            return await response.json();
        }
        
        return null;
    }

    async getEmails(filters = {}) {
        const params = new URLSearchParams();
        Object.keys(filters).forEach(key => {
            if (filters[key] !== undefined && filters[key] !== null) {
                params.append(key, filters[key]);
            }
        });

        const url = `${this.baseUrl}/api/emails${params.toString() ? '?' + params.toString() : ''}`;
        const response = await fetch(url);

        if (response.ok) {
            return await response.json();
        }

        return [];
    }
}

// Usage example
const emailClient = new EmailApiClient('http://localhost:5000');

// Send email
const email = await emailClient.sendEmail({
    toEmail: 'user@example.com',
    toName: 'John Doe',
    subject: 'Test Email',
    bodyHtml: '<p>This is a test email</p>'
});

// Check status
const status = await emailClient.getEmailStatus(email.id);

// Get failed emails
const failedEmails = await emailClient.getEmails({ status: 'Failed' });
```

## Event-Driven Architecture

### Email Created Event
When an email is created, the service publishes an `EmailCreatedEvent` to the message queue:

```json
{
  "Id": "64f1a2b3c4d5e6f7g8h9i0j1"
}
```

This event is consumed by the SensorsReport.Email.Consumer service for actual email delivery.

### Consumer Integration
The API works in conjunction with email consumer services:

1. **Email API**: Receives email requests, stores in MongoDB, publishes events
2. **Email Consumer**: Processes events, sends emails via SMTP, updates status
3. **Reconciliation Task**: Background service to handle stuck or failed emails

## Monitoring and Observability

### Email Status Tracking
Monitor email delivery success rates:
```bash
# Get email statistics by status
curl "http://localhost:5000/api/emails?status=Sent&limit=1000" | jq length
curl "http://localhost:5000/api/emails?status=Failed&limit=1000" | jq length
```

### Background Tasks
The service includes reconciliation tasks that:
- Identify emails stuck in intermediate states
- Retry failed emails within retry limits
- Update email status for monitoring

### Logging
Structured logging captures:
- Email creation and updates
- Message queue events
- Error conditions and retry attempts
- Performance metrics

## Error Handling

### HTTP Status Codes
- `200 OK`: Successful retrieval or update
- `201 Created`: Email created successfully
- `204 No Content`: Successful deletion
- `400 Bad Request`: Invalid email data or parameters
- `404 Not Found`: Email not found
- `500 Internal Server Error`: Server processing error

### Validation
The API validates:
- Email address formats (To, From, CC, BCC)
- Required fields (ToEmail, ToName, Subject, BodyHtml)
- Field length limits (Subject: 500 chars, Names: 250 chars)
- Enum values for status updates

### Retry Logic
Automatic retry handling:
- Default maximum retry count: 3
- Configurable per email via `maxRetryCount`
- Failed emails enter retry queue
- Exponential backoff in consumer services

## Performance Considerations

- **Async Operations**: All database and message queue operations are asynchronous
- **Indexing**: MongoDB indexes on status, creation date, and tenant for efficient queries
- **Pagination**: Default 100 record limit with offset-based pagination
- **Connection Pooling**: MongoDB driver connection pooling for database efficiency
- **Message Queue**: Event-driven architecture prevents blocking on email delivery

## Security Considerations

1. **Input Validation**: All email addresses and content are validated
2. **HTML Content**: HTML email bodies should be sanitized by clients
3. **Rate Limiting**: Consider implementing rate limiting for production
4. **Tenant Isolation**: Multi-tenant support for data isolation
5. **Audit Trail**: Complete email lifecycle tracking for compliance

## Dependencies

This API depends on:
- **SensorsReport.Api.Core**: Core shared functionality and MassTransit integration
- **MongoDB**: Document storage for email records
- **RabbitMQ**: Message queue for event publishing (via Api.Core)

## Related Services

- **SensorsReport.Email.Consumer**: Processes email events and sends actual emails
- **SensorsReport.Api.Core**: Core shared functionality
- **SensorsReport.Notification.API**: Coordinates multi-channel notifications
- **Sensors-Report-Explorer**: Frontend dashboard for email monitoring

## Contributing

When contributing to this API:
1. Maintain backward compatibility for existing endpoints
2. Add comprehensive unit and integration tests
3. Update model validation attributes for new fields
4. Follow established logging and error handling patterns
5. Test with actual MongoDB and message queue instances

## License

This project is part of the SensorsReport system for AerOS. See the root LICENSE file for details.

## Support

For issues with the Email API:
1. Check MongoDB connectivity and database permissions
2. Verify message queue configuration and connectivity
3. Review application logs for detailed error information
4. Monitor email status transitions for stuck emails
5. Contact the SensorsReport development team
