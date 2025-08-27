# SensorReport.PI.SMS.Gateway

A robust SMS Gateway service designed to run on Raspberry PI devices, providing SMS messaging capabilities for the SensorReport ecosystem with Keycloak authentication integration.

## Overview

This SMS Gateway service enables the SensorReport platform to send SMS notifications through a dedicated Raspberry PI device equipped with a cellular modem. The service features automatic Keycloak authentication, token management, robust error handling, and comprehensive logging.

## Features

- **🔐 Keycloak Authentication**: Automatic login and token renewal
- **📱 SMS Modem Support**: Compatible with USB cellular modems
- **🔄 Automatic Token Renewal**: Prevents authentication expiration
- **📊 Robust Logging**: Comprehensive logging with rotation
- **🔧 Configuration Management**: INI-based configuration system
- **🐧 Raspberry PI Optimized**: Designed for ARM architecture
- **🔄 Service Integration**: Seamless SensorReport ecosystem integration
- **⚡ Error Recovery**: Automatic recovery from common failures
- **📈 System Monitoring**: Health checks and status reporting

## Hardware Requirements

### Raspberry PI
- **Raspberry PI 4** (recommended) or Raspberry PI 3B+
- **Minimum 2GB RAM** (4GB recommended)
- **MicroSD Card**: 32GB or larger (Class 10)
- **Power Supply**: Official Raspberry PI power adapter

### USB Cellular Modem
- **Supported Modems**: Most USB cellular modems with AT command support
- **Examples**: 
  - Huawei E3372, E3531, E8372
  - ZTE MF823, MF831
  - Quectel EC25, BG96
  - SIMCom SIM7600, SIM800

### SIM Card
- **Active cellular plan** with SMS capabilities
- **Adequate credit** for outgoing SMS messages

## Software Requirements

- **Raspberry PI OS**: Bullseye or newer (64-bit recommended)
- **Python**: 3.9 or higher
- **Internet Connection**: For Keycloak authentication and service API calls

## Installation

### Automatic Installation (Recommended)

1. **Download and prepare**:
   ```bash
   # Clone or download the project files to your Raspberry PI
   cd /home/pi
   git clone <repository-url>
   cd SensorReport.PI.SMS.Gateway
   ```

2. **Run the installation script**:
   ```bash
   chmod +x install.sh
   sudo ./install.sh
   ```

3. **Configure the service**:
   ```bash
   sudo nano /etc/sms-gateway/config.ini
   ```

### Manual Installation

1. **Update system packages**:
   ```bash
   sudo apt update && sudo apt upgrade -y
   ```

2. **Install system dependencies**:
   ```bash
   sudo apt install -y python3-pip python3-venv python3-dev build-essential \
                       libgammu-dev gammu usb-modeswitch usb-modeswitch-data \
                       minicom screen
   ```

3. **Create virtual environment**:
   ```bash
   python3 -m venv venv
   source venv/bin/activate
   pip install --upgrade pip
   pip install -r requirements.txt
   ```

4. **Setup service user**:
   ```bash
   sudo useradd -r -s /bin/false -d /opt/sms-gateway smsgateway
   sudo mkdir -p /opt/sms-gateway /var/log/sms-gateway /etc/sms-gateway
   ```

## Configuration

### Configuration File: `config.ini`

```ini
[MODEM]
ModemTimeOutInSec = 30
ModemNr = +40726369867
Country = RO

[SERVICE]
ServiceSMSURL = https://api.sensorsreport.net/sms
SleepTimeInSec = 10

[KEYCLOAK]
KeycloakUser = viobiscu
KeycloakPassword = viobiscu
KeycloakURL = https://keycloak.sensorsreport.net
KeycloakRealm = sr
KeycloakClientId = sms-gateway
KeycloakClientSecret = 

[LOGGING]
LogLevel = INFO
LogFile = /var/log/sms-gateway/sms_gateway.log
MaxLogFileSize = 10485760
LogBackupCount = 5
```

### Configuration Parameters

| Section | Parameter | Description | Example |
|---------|-----------|-------------|---------|
| **MODEM** | ModemTimeOutInSec | Modem command timeout | `30` |
| | ModemNr | Gateway phone number | `+40726369867` |
| | Country | Country code | `RO` |
| **SERVICE** | ServiceSMSURL | SMS API endpoint | `https://api.sensorsreport.net/sms` |
| | SleepTimeInSec | Poll interval | `10` |
| **KEYCLOAK** | KeycloakUser | Username | `viobiscu` |
| | KeycloakPassword | Password | `viobiscu` |
| | KeycloakURL | Keycloak server URL | `https://keycloak.sensorsreport.net` |
| | KeycloakRealm | Realm name | `sr` |
| | KeycloakClientId | Client ID | `sms-gateway` |
| **LOGGING** | LogLevel | Log verbosity | `INFO` |
| | LogFile | Log file path | `/var/log/sms-gateway/sms_gateway.log` |

## Usage

### Running as a Service (Recommended)

1. **Enable the service**:
   ```bash
   sudo systemctl enable sms-gateway
   ```

2. **Start the service**:
   ```bash
   sudo systemctl start sms-gateway
   ```

3. **Check service status**:
   ```bash
   sudo systemctl status sms-gateway
   ```

4. **View logs**:
   ```bash
   sudo journalctl -u sms-gateway -f
   ```

### Manual Execution

1. **Activate virtual environment**:
   ```bash
   source venv/bin/activate
   ```

2. **Run the gateway**:
   ```bash
   python3 sms_gateway.py
   ```

3. **Stop with Ctrl+C** for graceful shutdown

## Architecture

### Core Components

1. **SMSGateway** (`sms_gateway.py`): Main application class
2. **KeycloakAuthManager** (`keycloak_auth.py`): Authentication management
3. **SMSAPIWrapper** (`sms_api_wrapper.py`): SMS API client wrapper
4. **SMSProviderAPIWrapper** (`sms_provider_wrapper.py`): Provider API client wrapper
5. **SMSGatewayConfig**: Configuration management
6. **RobustLogger**: Logging system

### Authentication Flow

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  SMS Gateway    │───▶│    Keycloak     │───▶│  Access Token   │
│   Application   │    │     Server      │    │   (JWT)         │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                       │
         ▼                        ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Token Monitor   │    │ Refresh Token   │    │ API Requests    │
│   Background    │───▶│   Automatic     │───▶│  with Bearer    │
│    Thread       │    │    Renewal      │    │     Token       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Service Integration

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ SensorReport    │───▶│  SMS Gateway    │───▶│   Cellular      │
│   Services      │    │   (Raspberry)   │    │     Modem       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                       │
         ▼                        ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Queue     │    │ Message Queue   │    │   SMS Network   │
│   Management    │    │   Processing    │    │   Delivery      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Modem Setup

### Detecting the Modem

1. **Check USB devices**:
   ```bash
   lsusb
   ```

2. **Check serial devices**:
   ```bash
   ls -la /dev/ttyUSB*
   ```

3. **Test modem communication**:
   ```bash
   minicom -D /dev/ttyUSB0
   ```

### Common AT Commands

- `ATI` - Modem information
- `AT+CPIN?` - Check SIM status
- `AT+CSQ` - Signal quality
- `AT+CMGF=1` - Text mode
- `AT+CMGS="+1234567890"` - Send SMS

### Troubleshooting Modem Issues

1. **Modem not detected**:
   ```bash
   sudo usb_modeswitch -v 12d1 -p 1f01 -M '55534243123456780000000000000a11062000000000000100000000000000'
   ```

2. **Permission issues**:
   ```bash
   sudo usermod -a -G dialout $USER
   sudo chmod 666 /dev/ttyUSB0
   ```

3. **Check modem status**:
   ```bash
   gammu identify
   ```

## Monitoring and Logging

### Log Files

- **Application logs**: `/var/log/sms-gateway/sms_gateway.log`
- **System logs**: `sudo journalctl -u sms-gateway`
- **Modem logs**: Check application logs for modem communication

### Log Levels

- **DEBUG**: Detailed debugging information
- **INFO**: General operational messages
- **WARNING**: Warning conditions
- **ERROR**: Error conditions
- **CRITICAL**: Critical failures

### Health Monitoring

The application logs:
- Authentication status and token renewals
- Modem communication status
- SMS processing statistics
- Error conditions and recovery attempts

## Security Considerations

### Authentication Security

- **Token-based authentication** with automatic renewal
- **Secure credential storage** in configuration files
- **HTTPS communication** with Keycloak and APIs
- **Certificate validation** for all HTTPS connections

### System Security

- **Dedicated service user** with minimal privileges
- **Restricted file permissions** on configuration files
- **Process isolation** with systemd security features
- **Network access control** through firewall rules

### Modem Security

- **SIM PIN protection** (configure in modem)
- **Limited AT command access**
- **Monitoring for unauthorized access**

## Performance Optimization

### Resource Usage

- **Memory efficient**: Minimal memory footprint
- **CPU optimized**: Low CPU usage during idle
- **Network efficient**: Optimized API polling
- **Storage conscious**: Log rotation and cleanup

### Raspberry PI Optimization

- **ARM architecture** optimized dependencies
- **GPIO access** if needed for hardware control
- **SD card longevity** through reduced writes
- **Thermal management** awareness

## Troubleshooting

### Common Issues

1. **Authentication failures**:
   - Verify Keycloak credentials
   - Check network connectivity
   - Validate Keycloak server URL

2. **Modem not responding**:
   - Check USB connection
   - Verify device permissions
   - Test with minicom

3. **Service not starting**:
   - Check systemd logs
   - Verify file permissions
   - Validate configuration

4. **Token renewal failures**:
   - Check Keycloak server status
   - Verify network connectivity
   - Review authentication logs

### Diagnostic Commands

```bash
# Check service status
sudo systemctl status sms-gateway

# View recent logs
sudo journalctl -u sms-gateway --since "1 hour ago"

# Check configuration
sudo -u smsgateway cat /etc/sms-gateway/config.ini

# Test modem
sudo minicom -D /dev/ttyUSB0

# Check USB devices
lsusb | grep -i modem

# Monitor resources
htop
```

### Log Analysis

```bash
# Filter authentication logs
sudo journalctl -u sms-gateway | grep -i "auth\|token\|login"

# Filter modem logs
sudo journalctl -u sms-gateway | grep -i "modem\|sms\|serial"

# Filter error logs
sudo journalctl -u sms-gateway | grep -i "error\|fail\|exception"
```

## API Integration

### SMS Service API Wrappers

The gateway includes comprehensive Python wrapper classes for the SensorsReport.SMS.API:

#### SMSAPIWrapper (`sms_api_wrapper.py`)
- **SMS Management**: Create, update, delete SMS messages
- **Status Tracking**: Update SMS status (Pending → Entrusted → Sent/Failed)
- **Querying**: List SMS messages with filtering and pagination
- **Validation**: Phone number validation and formatting

#### SMSProviderAPIWrapper (`sms_provider_wrapper.py`)
- **Provider Registration**: Register SMS gateway as a provider
- **Message Polling**: Poll for pending SMS messages to process
- **Status Monitoring**: Track provider health and availability
- **Country Code Support**: Handle country-specific message routing

### API Usage Examples

```python
# Initialize API wrappers
from sms_api_wrapper import SMSAPIWrapper, SmsModel
from sms_provider_wrapper import SMSProviderAPIWrapper, ProviderModel

# Create wrappers with authentication
sms_api = SMSAPIWrapper(service_url, auth_manager)
provider_api = SMSProviderAPIWrapper(service_url, auth_manager)

# Register as a provider
provider = ProviderModel(
    name="RaspberryPI-Gateway-001",
    supported_country_codes=["RO", "US", "GB"]
)
provider_api.register_provider(provider)

# Poll for SMS messages
sms_message = provider_api.get_next_sms_for_provider("RaspberryPI-Gateway-001")

# Update SMS status
sms_api.mark_sms_as_entrusted(sms_message['id'])
sms_api.mark_sms_as_sent(sms_message['id'])
```

### API Endpoint Integration

The gateway integrates with the following API endpoints:

```python
# SMS Management Endpoints
POST /api/sms                    # Create SMS
GET /api/sms/{id}               # Get SMS by ID
PUT /api/sms/{id}               # Update SMS
PATCH /api/sms/{id}             # Partial update
GET /api/sms                    # List SMS with filtering

# Provider Management Endpoints
POST /api/provider/register     # Register provider
GET /api/provider/{name}        # Get provider info
GET /api/provider/next/{name}   # Get next SMS to process
```

### Authentication Headers

All API requests include proper authentication:

```python
headers = {
    'Authorization': f'Bearer {access_token}',
    'Content-Type': 'application/json',
    'User-Agent': 'SensorReport-PI-SMS-Gateway/1.0.0',
    'X-Tenant-Id': tenant_id  # If multi-tenant
}
```

## Development

### Project Structure

```
SensorReport.PI.SMS.Gateway/
├── sms_gateway.py              # Main application
├── keycloak_auth.py            # Authentication manager
├── sms_api_wrapper.py          # SMS API wrapper class
├── sms_provider_wrapper.py     # Provider API wrapper class
├── config.ini                  # Configuration file
├── requirements.txt            # Python dependencies
├── install.sh                  # Installation script
├── test_gateway.py             # Testing utility
├── example_api_usage.py        # API wrapper examples
└── README.md                   # This documentation
```

### Adding Features

1. **SMS Processing**: Extend `_process_sms_queue()` method
2. **Modem Control**: Add modem-specific communication
3. **API Integration**: Enhance service API calls
4. **Monitoring**: Add health check endpoints

### Testing

```bash
# Run comprehensive tests
python3 test_gateway.py

# Test API wrapper functionality
python3 example_api_usage.py

# Test individual components
python -c "from sms_api_wrapper import SMSAPIWrapper; print('SMS API wrapper loaded')"
python -c "from sms_provider_wrapper import SMSProviderAPIWrapper; print('Provider API wrapper loaded')"
python -c "from keycloak_auth import KeycloakAuthManager; print('Auth module loaded')"
```

## Dependencies

### Python Packages

- **requests**: HTTP client for API communication
- **configparser**: Configuration file management
- **pyserial**: Serial communication with modem
- **python-gammu**: SMS modem integration
- **PyJWT**: JWT token handling
- **cryptography**: Security and encryption

### System Packages

- **gammu**: SMS handling library
- **usb-modeswitch**: USB modem initialization
- **minicom**: Serial terminal for testing

## Contributing

1. Fork the repository
2. Create a feature branch
3. Implement changes with tests
4. Update documentation
5. Submit a pull request

### Development Guidelines

- Follow PEP 8 coding standards
- Include comprehensive error handling
- Add logging for all operations
- Test on actual Raspberry PI hardware
- Document configuration changes

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:

- **Documentation**: This README and inline code comments
- **Issues**: Create issues in the project repository
- **Community**: SensorReport development forums
- **Contact**: Development team support channels

## Changelog

### Version 1.0.0
- Initial release
- Keycloak authentication integration
- Basic SMS gateway functionality
- Raspberry PI optimization
- Systemd service integration
- Comprehensive logging system

---

**SensorReport.PI.SMS.Gateway** - Reliable SMS messaging for IoT sensor networks on Raspberry PI.
