# SensorsReport Frontend Web Application

## Overview

The SensorsReport Frontend Web Application is a modern, enterprise-grade ASP.NET Core web application built with the Serenity Platform. It serves as the primary user interface for the SensorsReport cold chain monitoring platform, providing a comprehensive dashboard for managing IoT sensors, alarms, users, and API keys in GXP-compliant environments.

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **UI Framework**: Serenity Platform 8.8.6 (Premium)
- **Frontend**: TypeScript/JavaScript with Preact
- **Database**: SQL Server / PostgreSQL
- **Authentication**: Keycloak (OpenID Connect)
- **Containerization**: Docker
- **Orchestration**: Kubernetes with Flux GitOps

## Key Features

### Core Functionality
- **Multi-tenant Architecture**: Complete tenant isolation with NGSILD-Tenant support
- **API Key Management**: Generate and manage API keys for accessing SensorsReport services
- **User Management**: Comprehensive user administration and role-based access control
- **Dashboard Interface**: Real-time monitoring and management interface
- **Audit Capabilities**: Complete audit trail for GXP compliance

### Authentication & Security
- **Keycloak Integration**: Enterprise-grade SSO with OpenID Connect
- **JWT Token Support**: Secure API authentication
- **Role-Based Access Control**: Granular permission management
- **Two-Factor Authentication**: Enhanced security for sensitive operations
- **Anti-forgery Protection**: CSRF protection across all forms

### Integration Features
- **FIWARE NGSI-LD**: Integration with Orion-LD Context Broker
- **Microservices Communication**: RESTful API integration with backend services
- **Real-time Updates**: Live data updates from sensor networks
- **Multi-language Support**: Internationalization and localization support

## Project Structure

```
SensorsReport.Frontend.Web/
├── Modules/                    # Serenity modules (business logic)
│   ├── Administration/         # System administration
│   ├── Common/                # Shared components
│   ├── Membership/            # User management and authentication
│   └── Sensorsreport/         # Core SensorsReport functionality
│       ├── ApiKey/            # API key management
│       └── User/              # User management
├── Views/                     # Razor views and layouts
├── wwwroot/                   # Static web assets
├── Initialization/            # Application startup configuration
├── Migrations/                # Database migrations
├── Properties/                # Project properties
├── flux/                      # Kubernetes deployment manifests
├── texts/                     # Localization resources
├── Dockerfile                 # Container build configuration
├── package.json              # NPM dependencies
├── tsconfig.json             # TypeScript configuration
└── appsettings.json          # Application configuration
```

## Prerequisites

- .NET 8.0 SDK
- Node.js 20.x or later
- SQL Server or PostgreSQL
- Docker (for containerized deployment)
- Serenity Premium License

## Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd SensorsReport/SensorsReport.Frontend.Web
```

### 2. Install Dependencies
```bash
# Install .NET dependencies
dotnet restore

# Install NPM dependencies
npm install
```

### 3. Database Configuration
Update the connection string in `appsettings.json`:
```json
{
  "Data": {
    "Default": {
      "ConnectionString": "Server=your-server;Database=frontenddb;User ID=your-user;Password=your-password;TrustServerCertificate=True;",
      "ProviderName": "System.Data.SqlClient"
    }
  }
}
```

### 4. Keycloak Configuration
Configure OpenID Connect settings in `appsettings.json`:
```json
{
  "OpenIdSettings": {
    "EnableClient": true,
    "ExternalProviders": {
      "Keycloak": {
        "Issuer": "https://your-keycloak-server/realms/sr",
        "ClientId": "SensorsReport",
        "ClientSecret": "your-client-secret"
      }
    }
  }
}
```

### 5. Build and Run
```bash
# Build the application
dotnet build

# Run the application
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

## Docker Deployment

### Build Docker Image
```bash
docker build -t sensors-report-frontend-web .
```

### Run Container
```bash
docker run -p 8080:8080 \
  -e SR_Data__Default__ConnectionString="your-connection-string" \
  -e SR_OpenIdSettings__ExternalProviders__Keycloak__ClientSecret="your-secret" \
  sensors-report-frontend-web
```

## Kubernetes Deployment

The application includes Flux GitOps manifests for Kubernetes deployment:

```bash
# Apply Kubernetes manifests
kubectl apply -f flux/
```

Key deployment components:
- **ConfigMap**: Application configuration
- **Secret**: Sensitive configuration (Keycloak client secret)
- **Deployment**: Application deployment specification
- **Service**: Kubernetes service for internal communication
- **Ingress**: External access configuration

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `SR_Data__Default__ConnectionString` | Database connection string | See appsettings.json |
| `SR_OpenIdSettings__EnableClient` | Enable OpenID Connect | `true` |
| `SR_OpenIdSettings__ExternalProviders__Keycloak__Issuer` | Keycloak issuer URL | - |
| `SR_OpenIdSettings__ExternalProviders__Keycloak__ClientSecret` | Keycloak client secret | - |

## Development

### TypeScript Development
```bash
# Watch mode for TypeScript compilation
npm run tsbuild:watch
```

### Adding New Modules
1. Create new module in `Modules/` directory
2. Follow Serenity conventions for pages, endpoints, and grids
3. Update navigation in `SensorsreportNavigation.cs`
4. Add localization texts in `texts/` directory

### Database Migrations
```bash
# Add new migration
dotnet run -- migrate create "MigrationName"

# Apply migrations
dotnet run -- migrate up
```

## API Integration

The frontend integrates with various SensorsReport microservices:

- **Alarm API**: Alarm management and monitoring
- **Business Broker API**: Central orchestration
- **Webhook API**: Real-time notifications
- **Email/SMS APIs**: Communication services
- **Audit API**: Compliance and audit logging

## Testing

```bash
# Run TypeScript tests
npm test

# Run .NET tests
dotnet test
```

## Configuration

### Multi-tenancy
Configure tenant support through NGSILD-Tenant headers and service paths.

### Localization
Add new languages by creating text files in the `texts/` directory following Serenity conventions.

### Theming
Customize the application appearance through Serenity's theming system and CSS customizations.

## Troubleshooting

### Common Issues

1. **Serenity License**: Ensure you have a valid Serenity Premium license
2. **Node.js Version**: Use Node.js 20.x or compatible version
3. **Database Connection**: Verify connection string and database accessibility
4. **Keycloak Configuration**: Ensure Keycloak realm and client are properly configured

### Logging
Check application logs for detailed error information. Logs are configured through `nlog.config`.

## Security Considerations

- **HTTPS**: Always use HTTPS in production
- **Secrets Management**: Use proper secret management for sensitive configuration
- **Access Control**: Implement proper role-based access control
- **Audit Logging**: Ensure comprehensive audit logging for compliance

## Support

For technical support and documentation:
- Serenity Platform Documentation: [https://serenity.is](https://serenity.is)
- SensorsReport Platform Documentation: See `SensorsReport_Comprehensive_Documentation.md`

## License

This project is part of the SensorsReport platform. See LICENSE file for details.
