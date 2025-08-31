#!/usr/bin/env python3
"""
SensorReport.PI.SMS.Gateway - Main Application

A robust SMS Gateway service designed to run on Raspberry PI devices.
This service integrates with the SensorReport ecosystem to provide
SMS messaging capabilities with Keycloak authentication.

Features:
- Keycloak authentication with automatic token renewal
- Robust configuration management
- Comprehensive logging
- SMS modem integration
- Error handling and recovery
- Raspberry PI optimized

Author: SensorReport Team
Version: 1.0.0
Compatible with: Raspberry PI 4, Python 3.9+
"""

import os
import sys
import time
import signal
import logging
import threading
import configparser
from datetime import datetime
from typing import Optional, Dict, Any
from pathlib import Path

# Import our custom modules
from keycloak_auth import KeycloakAuthManager


class SMSGatewayConfig:
    """Configuration manager for SMS Gateway."""
    
    def __init__(self, config_file: str = "config.ini"):
        """
        Initialize configuration manager.
        
        Args:
            config_file (str): Path to configuration file
        """
        self.config_file = config_file
        self.config = configparser.ConfigParser()
        self.load_config()
    
    def load_config(self) -> None:
        """Load configuration from file."""
        if not os.path.exists(self.config_file):
            raise FileNotFoundError(f"Configuration file not found: {self.config_file}")
        
        self.config.read(self.config_file)
        
        # Validate required sections
        required_sections = ['MODEM', 'SERVICE', 'KEYCLOAK', 'LOGGING']
        for section in required_sections:
            if not self.config.has_section(section):
                raise ValueError(f"Missing required configuration section: {section}")
    
    def get(self, section: str, key: str, fallback: Any = None) -> str:
        """Get configuration value."""
        return self.config.get(section, key, fallback=fallback)
    
    def getint(self, section: str, key: str, fallback: int = 0) -> int:
        """Get configuration value as integer."""
        return self.config.getint(section, key, fallback=fallback)
    
    def getfloat(self, section: str, key: str, fallback: float = 0.0) -> float:
        """Get configuration value as float."""
        return self.config.getfloat(section, key, fallback=fallback)
    
    def getboolean(self, section: str, key: str, fallback: bool = False) -> bool:
        """Get configuration value as boolean."""
        return self.config.getboolean(section, key, fallback=fallback)


class RobustLogger:
    """Robust logging implementation for Raspberry PI."""
    
    def __init__(self, config: SMSGatewayConfig):
        """
        Initialize robust logger.
        
        Args:
            config (SMSGatewayConfig): Configuration manager
        """
        self.config = config
        self.setup_logging()
    
    def setup_logging(self) -> None:
        """Setup comprehensive logging configuration."""
        # Get logging configuration
        log_level = self.config.get('LOGGING', 'LogLevel', 'INFO')
        log_file = self.config.get('LOGGING', 'LogFile', 'sms_gateway.log')
        max_file_size = self.config.getint('LOGGING', 'MaxLogFileSize', 10485760)  # 10MB
        backup_count = self.config.getint('LOGGING', 'LogBackupCount', 5)
        
        # Create logs directory if it doesn't exist
        log_dir = Path(log_file).parent
        log_dir.mkdir(parents=True, exist_ok=True)
        
        # Configure root logger
        root_logger = logging.getLogger()
        root_logger.setLevel(getattr(logging, log_level.upper(), logging.INFO))
        
        # Remove existing handlers
        for handler in root_logger.handlers[:]:
            root_logger.removeHandler(handler)
        
        # Create formatters
        detailed_formatter = logging.Formatter(
            '%(asctime)s - %(name)s - %(levelname)s - %(funcName)s:%(lineno)d - %(message)s'
        )
        
        console_formatter = logging.Formatter(
            '%(asctime)s - %(levelname)s - %(message)s'
        )
        
        # File handler with rotation
        from logging.handlers import RotatingFileHandler
        file_handler = RotatingFileHandler(
            log_file,
            maxBytes=max_file_size,
            backupCount=backup_count,
            encoding='utf-8'
        )
        file_handler.setFormatter(detailed_formatter)
        file_handler.setLevel(logging.DEBUG)
        root_logger.addHandler(file_handler)
        
        # Console handler
        console_handler = logging.StreamHandler(sys.stdout)
        console_handler.setFormatter(console_formatter)
        console_handler.setLevel(getattr(logging, log_level.upper(), logging.INFO))
        root_logger.addHandler(console_handler)
        
        # Create application logger
        self.logger = logging.getLogger('sms_gateway')
        self.logger.info("Logging system initialized")
        self.logger.info(f"Log level: {log_level}")
        self.logger.info(f"Log file: {log_file}")


class SMSGateway:
    """Main SMS Gateway application class."""
    
    def __init__(self):
        """Initialize SMS Gateway application."""
        self.config = None
        self.logger = None
        self.auth_manager = None
        self.running = False
        self.main_thread = None
        self.token_check_thread = None
        
        # Setup signal handlers for graceful shutdown
        signal.signal(signal.SIGINT, self._signal_handler)
        signal.signal(signal.SIGTERM, self._signal_handler)
        
    def initialize(self) -> bool:
        """
        Initialize the SMS Gateway application.
        
        Returns:
            bool: True if initialization successful, False otherwise
        """
        try:
            print("Initializing SensorReport.PI.SMS.Gateway...")
            
            # Load configuration
            self.config = SMSGatewayConfig()
            print("✓ Configuration loaded successfully")
            
            # Setup logging
            robust_logger = RobustLogger(self.config)
            self.logger = logging.getLogger('sms_gateway')
            self.logger.info("=== SensorReport.PI.SMS.Gateway Starting ===")
            self.logger.info("Initialization started")
            
            # Initialize Keycloak authentication
            self._initialize_auth()
            
            # Validate configuration
            self._validate_configuration()
            
            self.logger.info("✓ SMS Gateway initialization completed successfully")
            return True
            
        except Exception as e:
            error_msg = f"Failed to initialize SMS Gateway: {e}"
            if self.logger:
                self.logger.error(error_msg)
            else:
                print(f"ERROR: {error_msg}")
            return False
    
    def _initialize_auth(self) -> None:
        """Initialize Keycloak authentication manager."""
        try:
            self.logger.info("Initializing Keycloak authentication...")
            
            # Get Keycloak configuration
            keycloak_url = self.config.get('KEYCLOAK', 'KeycloakURL')
            realm = self.config.get('KEYCLOAK', 'KeycloakRealm', 'sr')
            client_id = self.config.get('KEYCLOAK', 'KeycloakClientId', 'sms-gateway')
            username = self.config.get('KEYCLOAK', 'KeycloakUser')
            password = self.config.get('KEYCLOAK', 'KeycloakPassword')
            client_secret = self.config.get('KEYCLOAK', 'KeycloakClientSecret', '')
            
            # Validate required parameters
            if not all([keycloak_url, username, password]):
                raise ValueError("Missing required Keycloak configuration parameters")
            
            # Create authentication manager
            self.auth_manager = KeycloakAuthManager(
                keycloak_url=keycloak_url,
                realm=realm,
                client_id=client_id,
                username=username,
                password=password,
                client_secret=client_secret if client_secret else None
            )
            
            # Perform initial login
            if self.auth_manager.login():
                self.logger.info("✓ Keycloak authentication initialized successfully")
                
                # Log token information
                token_info = self.auth_manager.get_token_info()
                self.logger.info(f"Token expires at: {token_info.get('expires_at')}")
            else:
                raise Exception("Failed to authenticate with Keycloak")
                
        except Exception as e:
            self.logger.error(f"Failed to initialize Keycloak authentication: {e}")
            raise
    
    def _validate_configuration(self) -> None:
        """Validate all configuration parameters."""
        self.logger.info("Validating configuration...")
        
        # Validate modem configuration
        modem_timeout = self.config.getint('MODEM', 'ModemTimeOutInSec', 30)
        modem_nr = self.config.get('MODEM', 'ModemNr')
        country = self.config.get('MODEM', 'Country')
        
        if not modem_nr or not country:
            raise ValueError("Missing modem configuration (ModemNr or Country)")
        
        if modem_timeout <= 0:
            raise ValueError("ModemTimeOutInSec must be positive")
        
        # Validate service configuration
        service_url = self.config.get('SERVICE', 'ServiceSMSURL')
        sleep_time = self.config.getint('SERVICE', 'SleepTimeInSec', 10)
        
        if not service_url:
            raise ValueError("Missing ServiceSMSURL configuration")
        
        if sleep_time <= 0:
            raise ValueError("SleepTimeInSec must be positive")
        
        self.logger.info("✓ Configuration validation completed")
        self.logger.info(f"Modem Number: {modem_nr}")
        self.logger.info(f"Country: {country}")
        self.logger.info(f"Service URL: {service_url}")
        self.logger.info(f"Sleep Time: {sleep_time}s")
    
    def _signal_handler(self, signum, frame):
        """Handle shutdown signals gracefully."""
        signal_name = signal.Signals(signum).name
        if self.logger:
            self.logger.info(f"Received signal {signal_name}, initiating graceful shutdown...")
        else:
            print(f"Received signal {signal_name}, shutting down...")
        self.stop()
    
    def _token_monitor_loop(self) -> None:
        """Background thread to monitor and renew tokens."""
        self.logger.info("Token monitor thread started")
        
        sleep_time = self.config.getint('SERVICE', 'SleepTimeInSec', 10)
        
        while self.running:
            try:
                # Check and renew token if necessary
                if self.auth_manager:
                    token_info = self.auth_manager.get_token_info()
                    
                    if token_info.get('seconds_until_expiry'):
                        seconds_left = token_info['seconds_until_expiry']
                        if seconds_left < 300:  # Log if less than 5 minutes left
                            self.logger.info(f"Token expires in {seconds_left:.0f} seconds")
                    
                    if not self.auth_manager.check_and_renew_token():
                        self.logger.error("Failed to maintain valid authentication token")
                
                # Sleep before next check
                time.sleep(sleep_time)
                
            except Exception as e:
                self.logger.error(f"Error in token monitor loop: {e}")
                time.sleep(sleep_time)
    
    def _main_loop(self) -> None:
        """Main application loop."""
        self.logger.info("Main application loop started")
        
        sleep_time = self.config.getint('SERVICE', 'SleepTimeInSec', 10)
        service_url = self.config.get('SERVICE', 'ServiceSMSURL')
        
        loop_count = 0
        
        while self.running:
            try:
                loop_count += 1
                
                # Log periodic status
                if loop_count % 60 == 0:  # Every 10 minutes at 10s intervals
                    self.logger.info(f"SMS Gateway running - Loop #{loop_count}")
                    
                    if self.auth_manager:
                        token_info = self.auth_manager.get_token_info()
                        self.logger.info(f"Token status: Valid={not token_info.get('is_expired', True)}")
                
                # TODO: Implement SMS processing logic here
                # This is where you would:
                # 1. Check for pending SMS messages from the service
                # 2. Send SMS via modem
                # 3. Report status back to service
                
                # Example placeholder for SMS processing
                self._process_sms_queue()
                
                # Sleep before next iteration
                time.sleep(sleep_time)
                
            except Exception as e:
                self.logger.error(f"Error in main loop: {e}")
                time.sleep(sleep_time)
    
    def _process_sms_queue(self) -> None:
        """Process pending SMS messages using the API wrappers."""
        try:
            # Initialize API wrappers if not already done
            if not hasattr(self, 'sms_wrapper') or not hasattr(self, 'provider_wrapper'):
                self._initialize_api_wrappers()
            
            # Get provider name from configuration
            provider_name = self._get_provider_name()
            
            # Check for SMS messages to process
            sms_message = self.provider_wrapper.get_next_sms_for_provider(
                provider_name, 
                self.config.get('MODEM', 'Country', 'RO')
            )
            
            if sms_message:
                self.logger.info(f"Processing SMS message: {sms_message.get('id')}")
                
                # Mark SMS as entrusted to this provider
                sms_id = sms_message.get('id')
                self.sms_wrapper.mark_sms_as_entrusted(sms_id)
                
                # TODO: Send SMS via modem
                success = self._send_sms_via_modem(sms_message)
                
                if success:
                    # Mark as sent
                    self.sms_wrapper.mark_sms_as_sent(sms_id)
                    self.logger.info(f"SMS {sms_id} sent successfully")
                else:
                    # Mark as failed
                    self.sms_wrapper.mark_sms_as_failed(sms_id, "Failed to send via modem")
                    self.logger.error(f"Failed to send SMS {sms_id}")
            
        except Exception as e:
            self.logger.error(f"Error processing SMS queue: {e}")
    
    def _initialize_api_wrappers(self) -> None:
        """Initialize SMS and Provider API wrappers."""
        try:
            from sms_api_wrapper import SMSAPIWrapper
            from sms_provider_wrapper import SMSProviderAPIWrapper
            
            service_url = self.config.get('SERVICE', 'ServiceSMSURL')
            
            self.sms_wrapper = SMSAPIWrapper(service_url, self.auth_manager)
            self.provider_wrapper = SMSProviderAPIWrapper(service_url, self.auth_manager)
            
            # Register this provider if it doesn't exist
            self._ensure_provider_registered()
            
            self.logger.info("✓ API wrappers initialized successfully")
            
        except Exception as e:
            self.logger.error(f"Failed to initialize API wrappers: {e}")
            raise
    
    def _get_provider_name(self) -> str:
        """Get provider name for this gateway instance."""
        # Use modem number as part of provider name for uniqueness
        modem_nr = self.config.get('MODEM', 'ModemNr', '+40726369867')
        # Clean the phone number for use in provider name
        clean_number = modem_nr.replace('+', '').replace('-', '').replace(' ', '')
        return f"RaspberryPI-Gateway-{clean_number}"
    
    def _ensure_provider_registered(self) -> None:
        """Ensure this provider is registered in the system."""
        try:
            from sms_provider_wrapper import ProviderModel
            
            provider_name = self._get_provider_name()
            
            # Check if provider exists
            if not self.provider_wrapper.check_provider_exists(provider_name):
                self.logger.info(f"Registering provider: {provider_name}")
                
                # Get supported country codes from configuration
                country = self.config.get('MODEM', 'Country', 'RO')
                supported_countries = [country]  # Add more if needed
                
                provider = ProviderModel(
                    name=provider_name,
                    supported_country_codes=supported_countries
                )
                
                registered = self.provider_wrapper.register_provider(provider)
                if registered:
                    self.logger.info(f"✓ Provider registered: {registered.name}")
                else:
                    self.logger.error("Failed to register provider")
            else:
                self.logger.info(f"✓ Provider already registered: {provider_name}")
                
        except Exception as e:
            self.logger.error(f"Error ensuring provider registration: {e}")
    
    def _send_sms_via_modem(self, sms_message: dict) -> bool:
        """
        Send SMS message via cellular modem.
        
        Args:
            sms_message (dict): SMS message data from API
            
        Returns:
            bool: True if sent successfully, False otherwise
        """
        try:
            phone_number = sms_message.get('phoneNumber')
            message = sms_message.get('message')
            
            self.logger.info(f"Sending SMS to {phone_number}: {message[:50]}...")
            
            # TODO: Implement actual modem communication
            # This is a placeholder for the actual SMS sending logic
            # In a real implementation, this would:
            # 1. Open serial connection to modem
            # 2. Send AT commands to send SMS
            # 3. Handle responses and errors
            # 4. Return success/failure status
            
            # Simulate SMS sending (remove in production)
            import time
            time.sleep(0.5)  # Simulate processing time
            
            # For now, always return success (implement actual logic)
            self.logger.info(f"✓ SMS sent successfully (simulated)")
            return True
            
        except Exception as e:
            self.logger.error(f"Error sending SMS via modem: {e}")
            return False
    
    def start(self) -> None:
        """Start the SMS Gateway service."""
        if self.running:
            self.logger.warning("SMS Gateway is already running")
            return
        
        try:
            self.logger.info("Starting SMS Gateway service...")
            self.running = True
            
            # Start token monitor thread
            self.token_check_thread = threading.Thread(
                target=self._token_monitor_loop,
                name="TokenMonitor",
                daemon=True
            )
            self.token_check_thread.start()
            self.logger.info("✓ Token monitor thread started")
            
            # Start main application thread
            self.main_thread = threading.Thread(
                target=self._main_loop,
                name="MainLoop",
                daemon=False
            )
            self.main_thread.start()
            self.logger.info("✓ Main application thread started")
            
            self.logger.info("=== SMS Gateway service started successfully ===")
            
        except Exception as e:
            self.logger.error(f"Failed to start SMS Gateway: {e}")
            self.stop()
            raise
    
    def stop(self) -> None:
        """Stop the SMS Gateway service gracefully."""
        if not self.running:
            return
        
        try:
            self.logger.info("Stopping SMS Gateway service...")
            self.running = False
            
            # Wait for threads to complete
            if self.main_thread and self.main_thread.is_alive():
                self.logger.info("Waiting for main thread to complete...")
                self.main_thread.join(timeout=10)
            
            if self.token_check_thread and self.token_check_thread.is_alive():
                self.logger.info("Waiting for token monitor thread to complete...")
                self.token_check_thread.join(timeout=5)
            
            # Logout from Keycloak
            if self.auth_manager:
                self.logger.info("Logging out from Keycloak...")
                self.auth_manager.logout()
            
            self.logger.info("=== SMS Gateway service stopped ===")
            
        except Exception as e:
            if self.logger:
                self.logger.error(f"Error during shutdown: {e}")
            else:
                print(f"Error during shutdown: {e}")
    
    def run(self) -> None:
        """Run the SMS Gateway service (blocking)."""
        try:
            self.start()
            
            # Keep main thread alive
            if self.main_thread:
                self.main_thread.join()
                
        except KeyboardInterrupt:
            self.logger.info("Keyboard interrupt received")
        except Exception as e:
            self.logger.error(f"Unexpected error: {e}")
        finally:
            self.stop()


def main():
    """Main entry point for the SMS Gateway application."""
    print("=== SensorReport.PI.SMS.Gateway ===")
    print("Version: 1.0.0")
    print("Platform: Raspberry PI")
    print("Python Version:", sys.version)
    print("=" * 40)
    
    gateway = SMSGateway()
    
    try:
        # Initialize the gateway
        if not gateway.initialize():
            print("Failed to initialize SMS Gateway")
            sys.exit(1)
        
        # Run the gateway
        gateway.run()
        
    except KeyboardInterrupt:
        print("\nKeyboard interrupt received, shutting down...")
    except Exception as e:
        print(f"Fatal error: {e}")
        sys.exit(1)
    finally:
        print("SMS Gateway shutdown complete")


if __name__ == "__main__":
    main()
