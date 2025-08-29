# SensorsReport

A comprehensive IoT sensor data management platform built for AerOS, providing real-time monitoring, alerting, data processing, and visualization capabilities through a distributed microservices architecture.

## 🚀 Overview

SensorsReport is an enterprise-grade IoT platform designed to handle sensor data collection, processing, analysis, and visualization at scale. The platform leverages modern cloud-native technologies and microservices architecture to provide robust, scalable, and maintainable solutions for IoT sensor management.

### Key Capabilities

- **Real-time Data Processing**: MQTT-based sensor data ingestion and processing
- **Rule-based Alerting**: Configurable alarm rules and notification systems
- **Multi-channel Notifications**: Email, SMS, and webhook-based alert delivery
- **SMS Gateway Integration**: Dedicated Raspberry PI SMS gateway with cellular modem support
- **Data Visualization**: Interactive web dashboard for sensor data exploration
- **Audit & Compliance**: Comprehensive activity logging and audit trails
- **API Gateway**: Centralized API management and documentation
- **Cloud-native Deployment**: Kubernetes-ready with GitOps workflows

## 🏗️ Architecture

The SensorsReport platform follows a microservices architecture pattern with the following key components:

### Core Libraries
- **[SensorsReport.Api.Core](./SensorsReport.Api.Core/README.md)** - Shared API infrastructure and common utilities
- **[SensorsReport.Audit](./SensorsReport.Audit/README.md)** - Audit logging and compliance framework

### API Services
- **[SensorsReport.Alarm.API](./SensorsReport.Alarm.API/README.md)** - Alarm management and processing
- **[SensorsReport.AlarmRule.API](./SensorsReport.AlarmRule.API/README.md)** - Alarm rule configuration and management
- **[SensorsReport.Audit.API](./SensorsReport.Audit.API/README.md)** - Audit data access and reporting
- **[SensorsReport.Business.Broker.API](./SensorsReport.Business.Broker.API/README.md)** - Business logic orchestration and data brokering
- **[SensorsReport.Email.API](./SensorsReport.Email.API/README.md)** - Email notification service
- **[SensorsReport.LogRule.API](./SensorsReport.LogRule.API/README.md)** - Log rule management and configuration
- **[SensorsReport.NotificationRule.API](./SensorsReport.NotificationRule.API/README.md)** - Notification rule configuration
- **[SensorsReport.Provision.API](./SensorsReport.Provision.API/README.md)** - Device and sensor provisioning
- **[SensorsReport.SMS.API](./SensorsReport.SMS.API/README.md)** - SMS notification service
- **[SensorsReport.Swagger.API](./SensorsReport.Swagger.API/README.md)** - API documentation and testing interface
- **[SensorsReport.Webhook.API](./SensorsReport.Webhook.API/README.md)** - Webhook notification service
- **[Sensors-Report-Workflow.API](./Sensors-Report-Workflow.API/README.md)** - Workflow orchestration and automation

### Gateway Services
- **[SensorReport.PI.SMS.Gateway](./SensorReport.PI.SMS.Gateway/README.md)** - ✅ **Operational** Raspberry PI SMS Gateway with Keycloak authentication

### Consumer Services
- **[SensorsReport.AlarmRule.Consumer](./SensorsReport.AlarmRule.Consumer/README.md)** - Alarm rule event processing
- **[SensorsReport.Email.Consumer](./SensorsReport.Email.Consumer/README.md)** - Email notification processing
- **[SensorsReport.LogRule.Consumer](./SensorsReport.LogRule.Consumer/README.md)** - Log rule event processing
- **[SensorsReport.NotificationRule.Consumer](./SensorsReport.NotificationRule.Consumer/README.md)** - Notification rule event processing

### Data Processing Services
- **[Sensors-Report-MQTT-to-Orion](./Sensors-Report-MQTT-to-Orion/README.md)** - MQTT to FIWARE Orion Context Broker integration

### User Interface
- **[SensorsReport.Frontend.Web](./SensorsReport.Frontend.Web/README.md)** - Enterprise-grade ASP.NET Core web application with Serenity Platform for comprehensive system administration
- **[Sensors-Report-Explorer](./Sensors-Report-Explorer/README.md)** - Web-based dashboard for data visualization and system management

## 🛠️ Technology Stack

### Backend Technologies
- **.NET 8.0** - Primary application framework
- **C#** - Programming language
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM and data access
- **MassTransit** - Message bus and service bus abstraction
- **RabbitMQ** - Message broker
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **Python 3.9+** - SMS Gateway and data processing services

### Frontend Technologies
- **Serenity Platform** - Enterprise web application framework with TypeScript/Preact
- **ASP.NET Core** - Modern web framework for enterprise applications
- **TypeScript/JavaScript** - Type-safe frontend development
- **Preact** - Lightweight React-compatible framework
- **Python/Flask** - Backend services for web dashboard
- **HTML5/CSS3/JavaScript** - Frontend technologies
- **Bootstrap** - UI framework
- **Chart.js** - Data visualization

### Infrastructure & DevOps
- **Docker** - Containerization
- **Kubernetes** - Container orchestration
- **GitOps/Flux** - Continuous deployment
- **KrakenD** - API Gateway
- **Nginx** - Web server and reverse proxy
- **Keycloak** - Identity and access management
- **Raspberry PI** - Edge computing for SMS gateway

### Data & Messaging
- **FIWARE Orion Context Broker** - Context data management
- **QuantumLeap** - Time series data storage
- **MongoDB** - Document database
- **PostgreSQL** - Relational database
- **MQTT** - IoT messaging protocol
- **SMS/Cellular** - SMS gateway communication via cellular modems

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0 SDK** or higher
- **Docker** and **Docker Compose**
- **Visual Studio 2022** or **VS Code** (recommended)
- **Git** for version control
- **Kubernetes cluster** (for production deployment)
- **Python 3.9+** (for SMS Gateway and data processing services)
- **Raspberry PI 4** (for SMS Gateway deployment)

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd SensorsReport
   ```

2. **Build the solution**:
   ```bash
   dotnet build SensorsReport.sln
   ```

3. **Run individual services**:
   ```bash
   # Start a specific API service
   cd SensorsReport.Provision.API
   dotnet run
   ```

4. **Use the interactive build tool**:
   ```bash
   chmod +x build.sh
   ./build.sh
   ```

5. **Deploy SMS Gateway** (optional, for SMS notifications):
   ```bash
   # On Raspberry PI device
   cd SensorReport.PI.SMS.Gateway
   chmod +x install.sh
   sudo ./install.sh
   ```

### Development Environment Setup

1. **Install dependencies**:
   ```bash
   dotnet restore
   ```

2. **Configure environment variables** (see individual service README files for specific configurations)

3. **Start required infrastructure services**:
   ```bash
   docker-compose up -d rabbitmq mongodb postgresql
   ```

4. **Run services in development mode**:
   ```bash
   # Use the build script for interactive selection
   ./build.sh
   ```

## 📦 Project Structure

```
SensorsReport/
├── 📁 Core Libraries
│   ├── SensorsReport.Api.Core/          # Shared API infrastructure
│   └── SensorsReport.Audit/             # Audit framework
├── 📁 API Services
│   ├── SensorsReport.Alarm.API/         # Alarm management
│   ├── SensorsReport.AlarmRule.API/     # Alarm rule configuration
│   ├── SensorsReport.Audit.API/         # Audit data access
│   ├── SensorsReport.Business.Broker.API/ # Business logic orchestration
│   ├── SensorsReport.Email.API/         # Email notifications
│   ├── SensorsReport.LogRule.API/       # Log rule management
│   ├── SensorsReport.NotificationRule.API/ # Notification rules
│   ├── SensorsReport.Provision.API/     # Device provisioning
│   ├── SensorsReport.SMS.API/           # SMS notifications
│   ├── SensorsReport.Swagger.API/       # API documentation
│   ├── SensorsReport.Webhook.API/       # Webhook notifications
│   └── Sensors-Report-Workflow.API/     # Workflow orchestration
├── 📁 Consumer Services
│   ├── SensorsReport.AlarmRule.Consumer/    # Alarm rule processing
│   ├── SensorsReport.Email.Consumer/        # Email processing
│   ├── SensorsReport.LogRule.Consumer/      # Log rule processing
│   └── SensorsReport.NotificationRule.Consumer/ # Notification processing
├── 📁 Data Processing
│   └── Sensors-Report-MQTT-to-Orion/    # MQTT to Orion integration
├── 📁 Gateway Services
│   └── SensorReport.PI.SMS.Gateway/     # Raspberry PI SMS Gateway (✅ Operational)
├── 📁 User Interface
│   ├── SensorsReport.Frontend.Web/      # Enterprise web application (Serenity Platform)
│   └── Sensors-Report-Explorer/         # Web dashboard
├── 📁 Infrastructure
│   ├── flux/                            # GitOps deployment configurations
│   ├── krakend/                         # API Gateway configuration
│   ├── misc-files/                      # Utility scripts and tools
│   └── rest-client/                     # API testing configurations
├── 📄 Configuration Files
│   ├── SensorsReport.sln                # Visual Studio solution
│   ├── global.json                      # .NET SDK configuration
│   ├── Directory.Build.targets          # MSBuild targets
│   ├── nlog.config                      # Logging configuration
│   └── build.sh                         # Interactive build script
└── 📄 Documentation
    ├── README.md                        # This file
    ├── LICENSE                          # License information
    └── INTERACTIVE-BUILD.md             # Build system documentation
```

## 🔧 Configuration

### Global Configuration

The platform uses several global configuration files:

- **`global.json`** - .NET SDK version specification (8.0.408)
- **`Directory.Build.targets`** - Shared MSBuild properties and targets
- **`nlog.config`** - Centralized logging configuration
- **`SensorsReport.sln`** - Visual Studio solution file

### Environment-Specific Configuration

Each service can be configured using:
- **appsettings.json** - Application settings
- **Environment variables** - Runtime configuration
- **Kubernetes ConfigMaps** - Deployment-specific settings
- **Docker environment files** - Container configuration

## 🚀 Deployment

### Local Development

Use the interactive build script for local development:

```bash
./build.sh
```

This provides options to:
- Build individual projects
- Run services locally
- Execute tests
- Generate deployment manifests

### Container Deployment

Each service includes Dockerfiles for containerization:

```bash
# Build all services
docker-compose build

# Run the platform
docker-compose up -d
```

### Kubernetes Deployment

The platform supports GitOps deployment with Flux:

```bash
# Deploy using Flux
kubectl apply -f flux/kustomization.yaml

# Monitor deployment
flux get kustomizations
```

### API Gateway

KrakenD serves as the API gateway:

```bash
# Deploy API gateway
kubectl apply -f krakend/krakend-deployment.yaml
kubectl apply -f krakend/krakend-service.yaml
```

## 📊 Monitoring and Observability

### Logging

- **Structured logging** with Serilog
- **Centralized log aggregation** 
- **Log correlation** across services
- **Performance metrics** and tracing

### Health Checks

Each service exposes health endpoints:
- `/health` - Basic health status
- `/health/ready` - Readiness for traffic
- `/health/live` - Liveness indicator

### Metrics

- **Application metrics** via built-in .NET metrics
- **Custom business metrics** for sensor data processing
- **Infrastructure metrics** from Kubernetes

## 🔐 Security

### Authentication & Authorization

- **Keycloak** integration for identity management
- **OAuth 2.0/OpenID Connect** protocols
- **Role-based access control** (RBAC)
- **Service-to-service authentication**

### Security Best Practices

- **HTTPS/TLS** encryption in transit
- **Secrets management** with Kubernetes secrets
- **Network policies** for service isolation
- **Input validation** and sanitization
- **OWASP security guidelines** compliance

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test SensorsReport.Provision.API.Tests
```

### Test Types

- **Unit tests** - Individual component testing
- **Integration tests** - Service interaction testing
- **API tests** - REST client configurations in `rest-client/`
- **End-to-end tests** - Complete workflow validation

## 📚 API Documentation

### Swagger/OpenAPI

Access interactive API documentation:
- **Local development**: `http://localhost:5000/swagger`
- **Production**: via the Swagger API service

### REST Client

Use the provided REST client configurations in `rest-client/` for:
- **API testing** and validation
- **Integration examples**
- **Development workflows**

## 🤝 Contributing

### Development Workflow

1. **Fork** the repository
2. **Create** a feature branch
3. **Implement** changes with tests
4. **Run** the build script to validate
5. **Submit** a pull request

### Coding Standards

- Follow **.NET coding conventions**
- Use **consistent naming patterns**
- Include **comprehensive documentation**
- Write **meaningful tests**
- Follow **microservices best practices**

### Pull Request Guidelines

- Include **clear descriptions** of changes
- Ensure **all tests pass**
- Update **relevant documentation**
- Follow **semantic versioning** for releases

## 🔗 Service Integration Patterns

### Message-based Communication

Services communicate via:
- **RabbitMQ** for reliable messaging
- **MassTransit** for service bus abstraction
- **Event-driven architecture** patterns

### Data Consistency

- **Eventual consistency** model
- **Saga patterns** for distributed transactions
- **Event sourcing** for audit trails

### Service Discovery

- **Kubernetes services** for internal communication
- **DNS-based discovery** within cluster
- **Load balancing** via Kubernetes

## 📈 Performance Considerations

### Scalability

- **Horizontal scaling** via Kubernetes
- **Auto-scaling** based on metrics
- **Resource optimization** per service

### Caching

- **Application-level caching** with Redis
- **Database connection pooling**
- **Static asset caching**

### Optimization

- **Asynchronous processing** patterns
- **Bulk operations** for data processing
- **Connection multiplexing**

## 🛠️ Troubleshooting

### Common Issues

1. **Service connectivity** - Check network policies and DNS
2. **Authentication failures** - Verify Keycloak configuration
3. **Message processing** - Monitor RabbitMQ queues
4. **Database connections** - Check connection strings and pools
5. **SMS Gateway issues** - See [SMS Gateway README](./SensorReport.PI.SMS.Gateway/README.md) for specific troubleshooting

### Debugging Tools

- **Application logs** via Serilog
- **Kubernetes logs** with `kubectl logs`
- **Health check endpoints** for service status
- **Swagger UI** for API testing

### Support Channels

- **Documentation** - Individual service README files
- **Issue tracking** - Project repository issues
- **Development team** - Internal support channels

## 📋 Service Dependencies

### External Services

- **FIWARE Orion Context Broker** - Context data management
- **QuantumLeap** - Time series data storage
- **Keycloak** - Identity and access management
- **RabbitMQ** - Message broker
- **MongoDB/PostgreSQL** - Data storage
- **Cellular Network** - SMS gateway connectivity

### Inter-service Dependencies

- **Business Broker API** ← Core orchestration hub
- **Rule APIs** → **Consumer Services** - Event processing
- **Notification APIs** ← **Consumer Services** - Alert delivery
- **SMS Gateway** ← **SMS API** - Physical SMS delivery via Raspberry PI
- **Audit API** ← All services - Activity logging
- **Provision API** → **Explorer** - Device management

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:

- **📖 Documentation**: Refer to individual service README files
- **🐛 Issues**: Create issues in the project repository  
- **💬 Discussions**: Use project discussion forums
- **📧 Contact**: Reach out to the development team

## 🗺️ Roadmap

### Current Version
- ✅ Core microservices architecture
- ✅ MQTT data ingestion
- ✅ Rule-based alerting
- ✅ Multi-channel notifications
- ✅ **SMS Gateway with Raspberry PI integration**
- ✅ **Keycloak authentication for SMS Gateway**
- ✅ Web dashboard
- ✅ Kubernetes deployment

### Upcoming Features
- 🔄 Enhanced data analytics
- 🔄 Machine learning integration
- 🔄 Mobile application
- 🔄 Advanced visualization tools
- 🔄 Multi-tenant support

## 📊 Architecture Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   IoT Sensors   │───▶│  MQTT Broker    │───▶│ MQTT-to-Orion   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                       │
                                                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  Frontend.Web   │◀───│  API Gateway    │◀───│ Context Broker  │
│ (Admin Portal)  │    │   (KrakenD)     │    │    (Orion)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │                       │
┌─────────────────┐             │                       ▼
│    Explorer     │◀────────────┤              ┌─────────────────┐
│   Dashboard     │             │              │   QuantumLeap   │
└─────────────────┘             ▼              │  (Time Series)  │
                       ┌─────────────────┐     └─────────────────┘
                       │  Business Logic │
                       │     Services    │
                       └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │ Rule Processing │───▶│  Notifications  │
                       │    Services     │    │    Services     │
                       └─────────────────┘    └─────────────────┘
                                                       │
                                                       ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │   SMS Gateway   │◀───│    SMS API      │
                       │ (Raspberry PI)  │    │    Service      │
                       └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │ Cellular Modem  │
                       │  SMS Delivery   │
                       └─────────────────┘
```

### SMS Gateway Architecture Detail

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Keycloak      │◀───│  SMS Gateway    │───▶│  SMS API        │
│ Authentication  │    │ (Raspberry PI)  │    │   Service       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │ USB Cellular    │
                       │     Modem       │
                       └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │   SMS Network   │
                       │    Delivery     │
                       └─────────────────┘
```

---

**SensorsReport** - Empowering IoT sensor data management for the future of connected systems.