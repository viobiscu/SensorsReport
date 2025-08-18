# Sensors Report Explorer

A comprehensive web application for exploring and visualizing sensor data within the SensorsReport ecosystem. This application provides both frontend and backend components for data visualization, analysis, and management.

## Overview

The Sensors Report Explorer is a full-stack web application that serves as the primary interface for users to interact with sensor data. It features a Python Flask backend for data processing and API services, combined with a modern frontend for data visualization and user interaction.

## Features

- **Data Visualization**: Interactive charts and graphs for sensor data
- **Authentication**: Keycloak integration for secure access
- **Real-time Updates**: Live data monitoring and updates
- **Data Export**: Export functionality for sensor data
- **Multi-environment Support**: Development and production configurations
- **Responsive Design**: Mobile-friendly user interface
- **API Integration**: Seamless integration with SensorsReport microservices

## Technology Stack

### Backend
- **Python 3.9+**: Core runtime
- **Flask**: Web framework
- **Requests**: HTTP client library
- **python-dotenv**: Environment variable management

### Frontend
- **HTML5/CSS3**: Modern web standards
- **JavaScript**: Interactive functionality
- **Bootstrap**: Responsive UI framework
- **Font Awesome**: Icon library

### Infrastructure
- **Docker**: Containerization
- **Nginx**: Web server and reverse proxy
- **Keycloak**: Authentication and authorization

## Project Structure

```
Sensors-Report-Explorer/
├── backend-sr-explorer.py      # Flask backend application
├── endpoints.py                # API endpoint definitions
├── keycloak_auth.py           # Keycloak authentication module
├── keycloak_auth_check.py     # Authentication verification
├── index.html                 # Main frontend application
├── favicon.ico                # Application icon
├── nginx.conf                 # Nginx configuration
├── requirements.txt           # Python dependencies
├── Dockerfile.backend         # Backend container definition
├── Dockerfile.frontend        # Frontend container definition
├── docker-entrypoint.sh       # Container startup script
├── set-frontend-version.sh    # Version management script
├── css/                       # Stylesheets
├── js/                        # JavaScript modules
├── images/                    # Static images
└── test_data/                # Test data files
```

## Setup Instructions

### Prerequisites

- Python 3.9 or higher
- Docker and Docker Compose
- Access to Keycloak server
- Network access to SensorsReport APIs

### Local Development

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd Sensors-Report-Explorer
   ```

2. **Install Python dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

3. **Configure environment variables**:
   ```bash
   # Create .env file with required configurations
   KEYCLOAK_SERVER_URL=https://your-keycloak-server.com
   KEYCLOAK_REALM=sr
   KEYCLOAK_CLIENT_ID=ContextBroker
   KEYCLOAK_CLIENT_SECRET=your-client-secret
   ```

4. **Run the backend**:
   ```bash
   python backend-sr-explorer.py
   ```

5. **Access the application**:
   - Open browser to `http://localhost:5000`

### Docker Deployment

1. **Build the containers**:
   ```bash
   docker build -f Dockerfile.backend -t sensors-explorer-backend .
   docker build -f Dockerfile.frontend -t sensors-explorer-frontend .
   ```

2. **Run with Docker Compose** (if available):
   ```bash
   docker-compose up -d
   ```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `KEYCLOAK_SERVER_URL` | Keycloak server URL | - |
| `KEYCLOAK_REALM` | Keycloak realm name | `sr` |
| `KEYCLOAK_CLIENT_ID` | Keycloak client ID | `ContextBroker` |
| `KEYCLOAK_CLIENT_SECRET` | Keycloak client secret | - |
| `API_BASE_URL` | Base URL for SensorsReport APIs | - |
| `FLASK_ENV` | Flask environment | `production` |
| `FLASK_DEBUG` | Enable Flask debugging | `false` |

### Keycloak Configuration

Ensure your Keycloak server is configured with:

1. **Client Configuration**:
   - Client ID: `ContextBroker`
   - Client type: `confidential`
   - Valid redirect URIs matching your application URLs
   - Proper client secret configuration

2. **Realm Settings**:
   - Realm name: `sr`
   - Proper user roles and permissions
   - Token expiration settings

## API Reference

### Authentication Endpoints

- `POST /auth/login` - Initiate authentication flow
- `POST /auth/logout` - Logout and clear session
- `GET /auth/callback` - Handle OAuth callback
- `POST /auth/refresh` - Refresh authentication token

### Data Endpoints

- `GET /api/sensors` - Retrieve sensor information
- `GET /api/data/{sensor_id}` - Get sensor data
- `GET /api/export/{format}` - Export data in specified format
- `POST /api/query` - Execute custom data queries

## Usage Examples

### Basic Authentication Flow

```javascript
// Check authentication status
fetch('/auth/check')
  .then(response => response.json())
  .then(data => {
    if (data.authenticated) {
      // User is authenticated
      loadDashboard();
    } else {
      // Redirect to login
      window.location.href = '/auth/login';
    }
  });
```

### Data Visualization

```javascript
// Load sensor data
fetch('/api/sensors')
  .then(response => response.json())
  .then(sensors => {
    sensors.forEach(sensor => {
      renderSensorChart(sensor);
    });
  });
```

## Integration with SensorsReport Services

The Explorer integrates with various SensorsReport microservices:

- **Audit API**: User activity tracking
- **Business Broker API**: Data aggregation and processing
- **Provision API**: Device and sensor management
- **Notification APIs**: Real-time alerts and notifications

## Monitoring and Logging

### Application Logs

Logs are generated for:
- Authentication events
- API requests and responses
- Error conditions
- Performance metrics

### Health Checks

- `GET /health` - Application health status
- `GET /ready` - Readiness probe for Kubernetes

## Error Handling

The application implements comprehensive error handling:

- **Authentication Errors**: Proper OAuth error handling
- **API Errors**: Graceful degradation for service unavailability
- **Network Errors**: Retry mechanisms and user feedback
- **Validation Errors**: Input validation and sanitization

## Security Considerations

- **Authentication**: OAuth 2.0 with Keycloak
- **Authorization**: Role-based access control
- **HTTPS**: Enforce secure connections
- **CSRF Protection**: Cross-site request forgery prevention
- **Input Validation**: Sanitize all user inputs
- **Content Security Policy**: Prevent XSS attacks

## Performance Optimization

- **Caching**: Browser and server-side caching
- **Compression**: Gzip compression for static assets
- **CDN**: Content delivery network for static resources
- **Lazy Loading**: Progressive loading of data
- **Minification**: Compressed CSS and JavaScript

## Dependencies

### Python Dependencies (requirements.txt)

```
Flask>=2.0.0
requests>=2.25.0
python-dotenv>=0.19.0
flask-cors>=3.0.10
PyJWT>=2.0.0
```

### Frontend Dependencies

- Bootstrap 5.x
- Font Awesome 6.x
- Chart.js (for data visualization)
- jQuery (for DOM manipulation)

## Related Services

This service integrates with:

- **SensorsReport.Audit.API**: Activity logging
- **SensorsReport.Business.Broker.API**: Data processing
- **SensorsReport.Provision.API**: Device management
- **Keycloak**: Authentication service
- **QuantumLeap**: Time series data storage

## Testing the Keycloak Authentication Flow

### Prerequisites

Make sure your Keycloak server is properly configured with:

1. The correct client ID (`ContextBroker`)
2. Client type set to "confidential" (since we're using client_secret)
3. Valid redirect URIs that match your application URLs
4. Proper client secret configuration matching what's in the code

### Test Plan

Follow these steps to test the Keycloak authentication flow:

1. **Clear Existing Tokens**:
   - Open your browser developer tools (F12)
   - Go to Application → Local Storage
   - Clear the following keys if they exist:
     - javaToken
     - refreshToken
     - tokenExpiresAt
     - auth_state

2. **Access the Application**:
   - Load the application in your browser
   - You should be automatically redirected to the Keycloak login page
   - The URL should start with `https://www.sensorsreport.net/auth/realms/sr/protocol/openid-connect/auth`

3. **Authenticate**:
   - Enter valid credentials in the Keycloak login page
   - After successful authentication, you should be redirected back to the application
   - Check the developer console for "[Auth Flow]" logs showing the authentication process

4. **Verify Token**:
   - Once back in the application, check that you're properly authenticated
   - The user information should be displayed correctly
   - Click the "Show Token" button (if available) to verify the token contents

5. **Test Token Refresh**:
   - To force a token refresh, you can modify the expiration time in local storage to a time near the current time
   - The application should automatically refresh the token without requiring re-login

6. **Test Logout**:
   - Click the logout button
   - You should be redirected to the Keycloak logout page
   - After logout, you should be redirected back to the application
   - The application should now show you're not authenticated

## Troubleshooting

If you encounter any issues:

1. **Check Browser Console**: Look for any error messages, especially those prefixed with "[Auth Flow]"

2. **Inspect Network Requests**: In your browser's developer tools, check the network tab for requests to:
   - `/auth/realms/sr/protocol/openid-connect/auth`
   - `/auth/realms/sr/protocol/openid-connect/token`
   - `/auth/realms/sr/protocol/openid-connect/logout`

3. **Validate Keycloak Configuration**:
   - Ensure the client ID, client secret, and redirect URIs match in both Keycloak and the application
   - Check that your Keycloak server is accessible

4. **Clear Local Storage**: If you experience persistent issues, try clearing all local storage and restarting the process

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow PEP 8 for Python code style
- Use meaningful variable and function names
- Include docstrings for all functions and classes
- Write unit tests for new functionality
- Update documentation as needed

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:

- **Documentation**: Check this README and inline code comments
- **Issues**: Create an issue in the project repository
- **Contact**: Reach out to the development team

## Changelog

### Version 1.0.0
- Initial release with basic functionality
- Keycloak authentication integration
- Data visualization capabilities
- Docker containerization support