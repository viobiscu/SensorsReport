#!/bin/bash
# SensorReport.PI.SMS.Gateway Installation Script for Raspberry PI
# This script sets up the SMS Gateway service on Raspberry PI

set -e

echo "=== SensorReport.PI.SMS.Gateway Setup ==="
echo "Setting up SMS Gateway for Raspberry PI..."

# Check if running on Raspberry PI
if ! grep -q "Raspberry Pi" /proc/device-tree/model 2>/dev/null; then
    echo "Warning: This script is designed for Raspberry PI"
    read -p "Continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Update system packages
echo "Updating system packages..."
sudo apt update
sudo apt upgrade -y

# Install system dependencies for SMS modem
echo "Installing system dependencies..."
sudo apt install -y \
    python3-pip \
    python3-venv \
    python3-dev \
    build-essential \
    libgammu-dev \
    gammu \
    usb-modeswitch \
    usb-modeswitch-data \
    minicom \
    screen

# Create virtual environment
echo "Creating Python virtual environment..."
python3 -m venv venv
source venv/bin/activate

# Upgrade pip
pip install --upgrade pip

# Install Python dependencies
echo "Installing Python dependencies..."
pip install -r requirements.txt

# Create service user (optional, for production)
if ! id "smsgateway" &>/dev/null; then
    echo "Creating service user..."
    sudo useradd -r -s /bin/false -d /opt/sms-gateway smsgateway
fi

# Set up directories
echo "Setting up directories..."
sudo mkdir -p /opt/sms-gateway
sudo mkdir -p /var/log/sms-gateway
sudo mkdir -p /etc/sms-gateway

# Copy configuration file
sudo cp config.ini /etc/sms-gateway/config.ini
sudo chown smsgateway:smsgateway /etc/sms-gateway/config.ini
sudo chmod 600 /etc/sms-gateway/config.ini

# Set up log directory permissions
sudo chown smsgateway:smsgateway /var/log/sms-gateway
sudo chmod 755 /var/log/sms-gateway

# Create systemd service file
echo "Creating systemd service..."
sudo tee /etc/systemd/system/sms-gateway.service > /dev/null <<EOF
[Unit]
Description=SensorReport SMS Gateway
Documentation=file:///opt/sms-gateway/README.md
After=network.target
Wants=network.target

[Service]
Type=simple
User=smsgateway
Group=smsgateway
WorkingDirectory=/opt/sms-gateway
Environment=PATH=/opt/sms-gateway/venv/bin
ExecStart=/opt/sms-gateway/venv/bin/python /opt/sms-gateway/sms_gateway.py
ExecReload=/bin/kill -HUP \$MAINPID
Restart=on-failure
RestartSec=5
StandardOutput=journal
StandardError=journal
SyslogIdentifier=sms-gateway

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/log/sms-gateway /etc/sms-gateway

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd
sudo systemctl daemon-reload

# Enable USB permissions for modem access
echo "Setting up USB modem permissions..."
sudo tee /etc/udev/rules.d/99-sms-modem.rules > /dev/null <<EOF
# USB modem rules for SMS Gateway
SUBSYSTEM=="tty", ATTRS{idVendor}=="*", ATTRS{idProduct}=="*", GROUP="dialout", MODE="0660"
SUBSYSTEM=="usb", ATTRS{idVendor}=="*", ATTRS{idProduct}=="*", GROUP="dialout", MODE="0660"
EOF

# Add service user to dialout group for modem access
sudo usermod -a -G dialout smsgateway

# Reload udev rules
sudo udevadm control --reload-rules
sudo udevadm trigger

echo ""
echo "=== Installation Complete ==="
echo ""
echo "Next steps:"
echo "1. Edit the configuration file: sudo nano /etc/sms-gateway/config.ini"
echo "2. Configure your Keycloak credentials and SMS service URL"
echo "3. Connect your SMS modem and identify the device (usually /dev/ttyUSB0)"
echo "4. Test the configuration: sudo -u smsgateway python3 /opt/sms-gateway/sms_gateway.py"
echo "5. Enable and start the service:"
echo "   sudo systemctl enable sms-gateway"
echo "   sudo systemctl start sms-gateway"
echo "6. Check service status: sudo systemctl status sms-gateway"
echo "7. View logs: sudo journalctl -u sms-gateway -f"
echo ""
echo "For troubleshooting:"
echo "- Check modem detection: lsusb"
echo "- Check serial devices: ls -la /dev/ttyUSB*"
echo "- Test modem communication: minicom -D /dev/ttyUSB0"
echo ""
echo "Configuration file location: /etc/sms-gateway/config.ini"
echo "Log file location: /var/log/sms-gateway/"
echo ""
