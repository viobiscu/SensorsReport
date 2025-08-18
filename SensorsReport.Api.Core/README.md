# SensorsReport.Api.Core

A core library for the SensorsReport system, providing shared functionality, models, and services for sensor data management and processing within the AerOS ecosystem.

## Overview

SensorsReport.Api.Core is a foundational .NET 8.0 library that serves as the backbone for the SensorsReport microservices architecture. It provides common functionality including data models, configuration management, HTTP services, and message queue integration for handling sensor data, alarms, notifications, and workflows.

## Features

### Core Models
- **Entity Models**: Base models for representing sensor entities and their properties
- **Alarm Management**: Models for alarm definitions, rules, and notifications
- **User & Tenant Management**: Multi-tenant support with user and group models
- **Subscription Models**: Event subscription and notification management
- **Property Models**: Flexible property system for sensor data attributes

### Services
- **BaseHttpService**: Common HTTP client functionality
- **OrionLD Integration**: NGSI-LD context broker integration
- **QuantumLeap Integration**: Time-series data storage and retrieval
- **Tenant Services**: Multi-tenant data isolation and management

### Configuration Management
- **Event Bus Options**: RabbitMQ/MassTransit configuration
- **Database Options**: MongoDB connection management
- **OrionLD Options**: Context broker configuration
- **SMTP Options**: Email service configuration
- **QuantumLeap Options**: Time-series database configuration

### Infrastructure Components
- **Health Checks**: RabbitMQ health monitoring
- **Message Queue**: MassTransit with RabbitMQ integration
- **Logging**: NLog integration with structured logging
- **Swagger/OpenAPI**: API documentation support
- **JSON Serialization**: Newtonsoft.Json and System.Text.Json support

## Technology Stack

- **.NET 8.0**: Target framework
- **ASP.NET Core 8.0**: Web framework foundation
- **MassTransit 8.5.1**: Message queue abstraction with RabbitMQ
- **NLog**: Structured logging
- **Newtonsoft.Json**: JSON serialization
- **Swagger/OpenAPI**: API documentation

## Project Structure

```
SensorsReport.Api.Core/
├── Models/                    # Data models and DTOs
│   ├── EntityModels.cs       # Core entity representations
│   ├── AlarmModel.cs         # Alarm definitions
│   ├── NotificationRuleModel.cs
│   └── ...
├── Services/                 # Service layer implementations
│   ├── BaseHttpService.cs    # Common HTTP functionality
│   ├── OrionLD/             # Context broker services
│   ├── QuantumLeap/         # Time-series services
│   └── Tenants/             # Multi-tenant services
├── Config/                   # Configuration classes
│   ├── EventBusOptions.cs    # Message queue configuration
│   ├── OrionLdOptions.cs     # Context broker settings
│   └── ...
├── Attributes/               # Custom attributes
├── Converters/              # JSON converters
├── Filters/                 # Request/response filters
├── Helpers/                 # Utility classes
├── MassTransit/             # Message queue configuration
├── Responses/               # API response models
└── AppConfigExtensions.cs   # Application configuration extensions
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Access to RabbitMQ instance
- Access to OrionLD context broker
- Access to QuantumLeap time-series database

### Installation

This is a library project that is referenced by other SensorsReport microservices. Add it as a project reference:

```xml
<ProjectReference Include="../SensorsReport.Api.Core/SensorsReport.Api.Core.csproj" />
```

### Configuration

The library provides extension methods for configuring services in your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure core services
builder.Services.AddSensorsReportCore(builder.Configuration);

// Configure logging
builder.Services.ConfigureLogger();

// Configure health checks
builder.Services.AddHealthChecks()
    .AddRabbitMQ();

var app = builder.Build();
```

### Environment Variables

Configure the following environment variables or appsettings.json:

```json
{
  "EventBus": {
    "Host": "rabbitmq-host",
    "Username": "username",
    "Password": "password"
  },
  "OrionLD": {
    "BaseUrl": "http://orion-ld:1026"
  },
  "QuantumLeap": {
    "BaseUrl": "http://quantumleap:8668"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "sensorsreport"
  }
}
```

## Usage

### Entity Models
```csharp
var entity = new EntityModel
{
    Id = "sensor-001",
    Type = "Device",
    Properties = new Dictionary<string, JsonElement>
    {
        ["temperature"] = JsonSerializer.SerializeToElement(25.5),
        ["location"] = JsonSerializer.SerializeToElement("Building A")
    }
};
```

### HTTP Services
```csharp
public class MySensorService : BaseHttpService
{
    public MySensorService(HttpClient httpClient, ILogger<MySensorService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<EntityModel?> GetSensorAsync(string id)
    {
        return await GetAsync<EntityModel>($"/sensors/{id}");
    }
}
```

## Dependencies

This project serves as a dependency for the following SensorsReport microservices:
- SensorsReport.Alarm.API
- SensorsReport.AlarmRule.API
- SensorsReport.AlarmRule.Consumer
- SensorsReport.Audit.API
- SensorsReport.Email.API
- SensorsReport.Notification.API
- And other SensorsReport services

## Contributing

This is a core shared library. Changes should be carefully considered as they impact all dependent services. Ensure backward compatibility and comprehensive testing.

## License

This project is part of the SensorsReport system for AerOS. See the root LICENSE file for details.

## Related Projects

- **SensorsReport.Alarm.API**: Alarm management service
- **SensorsReport.Workflow.API**: Workflow orchestration service
- **Sensors-Report-Explorer**: Frontend dashboard
- **Sensors-Report-MQTT-to-Orion**: MQTT data ingestion service

## Support

For issues and questions related to this core library, please refer to the main SensorsReport project documentation or contact the development team.
