#!/usr/bin/env python3
"""
Example usage of SMS API Wrappers for SensorReport.PI.SMS.Gateway

This script demonstrates how to use the SMS and Provider API wrapper classes
to interact with the SensorsReport.SMS.API service.

Author: SensorReport Team
Version: 1.0.0
"""

import sys
import os
import logging
from datetime import datetime, timedelta

# Add current directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from keycloak_auth import KeycloakAuthManager
from sms_api_wrapper import SMSAPIWrapper, SmsModel, SmsStatusEnum
from sms_provider_wrapper import SMSProviderAPIWrapper, ProviderModel, ProviderStatusEnum
from sms_gateway import SMSGatewayConfig


def setup_logging():
    """Setup basic logging for examples."""
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )


def create_auth_manager(config: SMSGatewayConfig) -> KeycloakAuthManager:
    """Create and authenticate Keycloak manager."""
    print("Setting up Keycloak authentication...")
    
    auth_manager = KeycloakAuthManager(
        keycloak_url=config.get('KEYCLOAK', 'KeycloakURL'),
        realm=config.get('KEYCLOAK', 'KeycloakRealm', 'sr'),
        client_id=config.get('KEYCLOAK', 'KeycloakClientId', 'sms-gateway'),
        username=config.get('KEYCLOAK', 'KeycloakUser'),
        password=config.get('KEYCLOAK', 'KeycloakPassword'),
        client_secret=config.get('KEYCLOAK', 'KeycloakClientSecret', '') or None
    )
    
    if auth_manager.login():
        print("✓ Authentication successful")
        return auth_manager
    else:
        raise Exception("Failed to authenticate with Keycloak")


def example_provider_operations(config: SMSGatewayConfig, auth_manager: KeycloakAuthManager):
    """Demonstrate provider API operations."""
    print("\n" + "=" * 50)
    print("Provider API Operations Example")
    print("=" * 50)
    
    # Initialize provider wrapper
    service_url = config.get('SERVICE', 'ServiceSMSURL', 'https://api.sensorsreport.net/sms')
    provider_wrapper = SMSProviderAPIWrapper(service_url, auth_manager)
    
    try:
        # Example 1: Register a new provider
        print("\n1. Registering a new provider...")
        provider = ProviderModel(
            name="RaspberryPI-Gateway-001",
            supported_country_codes=["RO", "US", "GB", "DE"]
        )
        
        registered_provider = provider_wrapper.register_provider(provider)
        if registered_provider:
            print(f"✓ Provider registered: {registered_provider}")
        else:
            print("✗ Failed to register provider")
        
        # Example 2: Get provider by name
        print("\n2. Getting provider by name...")
        retrieved_provider = provider_wrapper.get_provider_by_name("RaspberryPI-Gateway-001")
        if retrieved_provider:
            print(f"✓ Provider found: {retrieved_provider}")
        else:
            print("✗ Provider not found")
        
        # Example 3: Get active providers
        print("\n3. Getting active providers...")
        active_providers = provider_wrapper.get_active_providers()
        print(f"✓ Found {len(active_providers)} active providers")
        for provider in active_providers:
            print(f"  - {provider}")
        
        # Example 4: Check for SMS messages
        print("\n4. Checking for SMS messages...")
        sms_message = provider_wrapper.get_next_sms_for_provider("RaspberryPI-Gateway-001")
        if sms_message:
            print(f"✓ SMS message available: {sms_message}")
        else:
            print("✓ No SMS messages available")
        
        # Example 5: Get API status
        print("\n5. Getting API status...")
        status = provider_wrapper.get_api_status()
        print(f"✓ API Status: {status}")
        
    except Exception as e:
        print(f"✗ Error in provider operations: {e}")


def example_sms_operations(config: SMSGatewayConfig, auth_manager: KeycloakAuthManager):
    """Demonstrate SMS API operations."""
    print("\n" + "=" * 50)
    print("SMS API Operations Example")
    print("=" * 50)
    
    # Initialize SMS wrapper
    service_url = config.get('SERVICE', 'ServiceSMSURL', 'https://api.sensorsreport.net/sms')
    sms_wrapper = SMSAPIWrapper(service_url, auth_manager)
    
    try:
        # Example 1: Create a simple SMS
        print("\n1. Creating a simple SMS...")
        phone_number = config.get('MODEM', 'ModemNr', '+40726369867')
        
        created_sms = sms_wrapper.create_simple_sms(
            phone_number=phone_number,
            message="Test SMS from Raspberry PI Gateway",
            provider="RaspberryPI-Gateway-001",
            country_code="RO"
        )
        
        if created_sms:
            print(f"✓ SMS created: {created_sms}")
            sms_id = created_sms.id
        else:
            print("✗ Failed to create SMS")
            return
        
        # Example 2: Get SMS by ID
        print("\n2. Getting SMS by ID...")
        retrieved_sms = sms_wrapper.get_sms_by_id(sms_id)
        if retrieved_sms:
            print(f"✓ SMS retrieved: {retrieved_sms}")
        else:
            print("✗ SMS not found")
        
        # Example 3: Update SMS status
        print("\n3. Updating SMS status to Entrusted...")
        updated_sms = sms_wrapper.mark_sms_as_entrusted(sms_id)
        if updated_sms:
            print(f"✓ SMS status updated: {updated_sms.status.value}")
        else:
            print("✗ Failed to update SMS status")
        
        # Example 4: Mark SMS as sent
        print("\n4. Marking SMS as sent...")
        sent_sms = sms_wrapper.mark_sms_as_sent(sms_id)
        if sent_sms:
            print(f"✓ SMS marked as sent: {sent_sms.sent_at}")
        else:
            print("✗ Failed to mark SMS as sent")
        
        # Example 5: Get pending SMS messages
        print("\n5. Getting pending SMS messages...")
        pending_sms = sms_wrapper.get_pending_sms(limit=10)
        print(f"✓ Found {len(pending_sms)} pending SMS messages")
        for sms in pending_sms[:3]:  # Show first 3
            print(f"  - {sms}")
        
        # Example 6: Get SMS statistics
        print("\n6. Getting SMS statistics...")
        stats = sms_wrapper.get_sms_statistics()
        print(f"✓ SMS Statistics:")
        print(f"  Total SMS: {stats.get('total', 0)}")
        for status, count in stats.get('by_status', {}).items():
            print(f"  {status}: {count}")
        
    except Exception as e:
        print(f"✗ Error in SMS operations: {e}")


def example_integrated_workflow(config: SMSGatewayConfig, auth_manager: KeycloakAuthManager):
    """Demonstrate integrated SMS processing workflow."""
    print("\n" + "=" * 50)
    print("Integrated SMS Processing Workflow")
    print("=" * 50)
    
    service_url = config.get('SERVICE', 'ServiceSMSURL', 'https://api.sensorsreport.net/sms')
    provider_wrapper = SMSProviderAPIWrapper(service_url, auth_manager)
    sms_wrapper = SMSAPIWrapper(service_url, auth_manager)
    
    try:
        provider_name = "RaspberryPI-Gateway-001"
        
        # Step 1: Check if provider exists
        print(f"\n1. Checking if provider '{provider_name}' exists...")
        if not provider_wrapper.check_provider_exists(provider_name):
            print(f"✗ Provider '{provider_name}' not found, registering...")
            
            # Register the provider
            provider = ProviderModel(
                name=provider_name,
                supported_country_codes=["RO", "US", "GB"]
            )
            
            registered = provider_wrapper.register_provider(provider)
            if registered:
                print(f"✓ Provider registered: {registered.name}")
            else:
                print("✗ Failed to register provider")
                return
        else:
            print(f"✓ Provider '{provider_name}' exists")
        
        # Step 2: Create a test SMS
        print("\n2. Creating test SMS...")
        test_sms = sms_wrapper.create_simple_sms(
            phone_number="+40726369867",
            message="Integrated workflow test from SMS Gateway",
            provider=provider_name,
            country_code="RO"
        )
        
        if not test_sms:
            print("✗ Failed to create test SMS")
            return
        
        print(f"✓ Test SMS created: {test_sms.id}")
        
        # Step 3: Simulate provider polling for messages
        print("\n3. Simulating provider polling...")
        sms_message = provider_wrapper.get_next_sms_for_provider(provider_name, "RO")
        
        if sms_message:
            print(f"✓ SMS message retrieved for processing:")
            print(f"  Phone: {sms_message.get('phoneNumber')}")
            print(f"  Message: {sms_message.get('message')}")
            print(f"  Status: {sms_message.get('status')}")
            
            # Step 4: Mark as entrusted and then sent
            print("\n4. Processing SMS message...")
            sms_id = sms_message.get('id')
            
            # Mark as entrusted
            sms_wrapper.mark_sms_as_entrusted(sms_id)
            print("✓ SMS marked as entrusted")
            
            # Simulate processing time
            import time
            time.sleep(1)
            
            # Mark as sent
            sms_wrapper.mark_sms_as_sent(sms_id)
            print("✓ SMS marked as sent")
            
        else:
            print("✓ No SMS messages available for processing")
        
        print("\n✓ Integrated workflow completed successfully!")
        
    except Exception as e:
        print(f"✗ Error in integrated workflow: {e}")


def main():
    """Main example function."""
    print("SensorReport.PI.SMS.Gateway - API Wrapper Examples")
    print("Version: 1.0.0")
    print("=" * 60)
    
    setup_logging()
    
    try:
        # Load configuration
        config = SMSGatewayConfig("config.ini")
        print("✓ Configuration loaded")
        
        # Setup authentication
        auth_manager = create_auth_manager(config)
        
        # Run examples
        example_provider_operations(config, auth_manager)
        example_sms_operations(config, auth_manager)
        example_integrated_workflow(config, auth_manager)
        
        print("\n" + "=" * 60)
        print("All examples completed successfully!")
        
    except FileNotFoundError:
        print("✗ Configuration file 'config.ini' not found")
        print("Please ensure the configuration file exists and is properly configured")
    except Exception as e:
        print(f"✗ Error running examples: {e}")
        return 1
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
