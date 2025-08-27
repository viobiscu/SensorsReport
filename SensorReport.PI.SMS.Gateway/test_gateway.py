#!/usr/bin/env python3
"""
Test script for SensorReport.PI.SMS.Gateway

This script provides basic testing functionality for the SMS Gateway
components, including configuration loading and authentication testing.

Usage: python3 test_gateway.py
"""

import sys
import os
import logging
from pathlib import Path

# Add current directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from sms_gateway import SMSGatewayConfig, RobustLogger
from keycloak_auth import KeycloakAuthManager


def test_configuration():
    """Test configuration loading."""
    print("=" * 50)
    print("Testing Configuration Loading")
    print("=" * 50)
    
    try:
        config = SMSGatewayConfig("config.ini")
        print("✓ Configuration loaded successfully")
        
        # Test reading values
        modem_timeout = config.getint('MODEM', 'ModemTimeOutInSec')
        modem_nr = config.get('MODEM', 'ModemNr')
        keycloak_url = config.get('KEYCLOAK', 'KeycloakURL')
        
        print(f"  Modem Timeout: {modem_timeout}s")
        print(f"  Modem Number: {modem_nr}")
        print(f"  Keycloak URL: {keycloak_url}")
        
        return True
        
    except Exception as e:
        print(f"✗ Configuration test failed: {e}")
        return False


def test_logging():
    """Test logging system."""
    print("\n" + "=" * 50)
    print("Testing Logging System")
    print("=" * 50)
    
    try:
        config = SMSGatewayConfig("config.ini")
        robust_logger = RobustLogger(config)
        
        logger = logging.getLogger('test_logger')
        logger.info("Test log message - INFO level")
        logger.warning("Test log message - WARNING level")
        logger.error("Test log message - ERROR level")
        
        print("✓ Logging system initialized successfully")
        print("  Check log file for test messages")
        
        return True
        
    except Exception as e:
        print(f"✗ Logging test failed: {e}")
        return False


def test_authentication():
    """Test Keycloak authentication (requires network and valid credentials)."""
    print("\n" + "=" * 50)
    print("Testing Keycloak Authentication")
    print("=" * 50)
    
    try:
        config = SMSGatewayConfig("config.ini")
        
        # Get Keycloak configuration
        keycloak_url = config.get('KEYCLOAK', 'KeycloakURL')
        realm = config.get('KEYCLOAK', 'KeycloakRealm', 'sr')
        client_id = config.get('KEYCLOAK', 'KeycloakClientId', 'sms-gateway')
        username = config.get('KEYCLOAK', 'KeycloakUser')
        password = config.get('KEYCLOAK', 'KeycloakPassword')
        client_secret = config.get('KEYCLOAK', 'KeycloakClientSecret', '')
        
        print(f"  Testing authentication to: {keycloak_url}")
        print(f"  Realm: {realm}")
        print(f"  Client ID: {client_id}")
        print(f"  Username: {username}")
        
        # Create authentication manager
        auth_manager = KeycloakAuthManager(
            keycloak_url=keycloak_url,
            realm=realm,
            client_id=client_id,
            username=username,
            password=password,
            client_secret=client_secret if client_secret else None
        )
        
        # Test login
        print("  Attempting login...")
        if auth_manager.login():
            print("✓ Login successful")
            
            # Get token info
            token_info = auth_manager.get_token_info()
            print(f"  Token expires at: {token_info.get('expires_at')}")
            print(f"  Seconds until expiry: {token_info.get('seconds_until_expiry'):.0f}")
            
            # Test token check
            if auth_manager.check_and_renew_token():
                print("✓ Token check/renewal successful")
            else:
                print("✗ Token check/renewal failed")
            
            # Test logout
            if auth_manager.logout():
                print("✓ Logout successful")
            else:
                print("✗ Logout failed")
            
            return True
        else:
            print("✗ Login failed")
            return False
            
    except Exception as e:
        print(f"✗ Authentication test failed: {e}")
        return False


def test_dependencies():
    """Test if all required dependencies are available."""
    print("\n" + "=" * 50)
    print("Testing Dependencies")
    print("=" * 50)
    
    dependencies = [
        ('requests', 'HTTP client library'),
        ('configparser', 'Configuration management'),
        ('serial', 'Serial communication (pyserial)'),
        ('logging.handlers', 'Logging handlers'),
        ('threading', 'Threading support'),
        ('datetime', 'Date/time handling'),
        ('json', 'JSON processing'),
        ('pathlib', 'Path utilities')
    ]
    
    failed = []
    
    for module, description in dependencies:
        try:
            __import__(module)
            print(f"✓ {module:<20} - {description}")
        except ImportError:
            print(f"✗ {module:<20} - {description} (MISSING)")
            failed.append(module)
    
    if failed:
        print(f"\nMissing dependencies: {', '.join(failed)}")
        print("Run: pip install -r requirements.txt")
        return False
    else:
        print("\n✓ All dependencies available")
        return True


def test_api_wrappers():
    """Test API wrapper functionality."""
    print("\n" + "=" * 50)
    print("Testing API Wrappers")
    print("=" * 50)
    
    try:
        # Test importing the wrapper classes
        from sms_api_wrapper import SMSAPIWrapper, SmsModel, SmsStatusEnum
        from sms_provider_wrapper import SMSProviderAPIWrapper, ProviderModel, ProviderStatusEnum
        
        print("✓ SMS API wrapper imported successfully")
        print("✓ Provider API wrapper imported successfully")
        
        # Test model creation
        sms = SmsModel(
            phone_number="+40726369867",
            message="Test message",
            provider="test-provider"
        )
        print(f"✓ SMS model created: {sms}")
        
        provider = ProviderModel(
            name="test-provider",
            supported_country_codes=["RO", "US"]
        )
        print(f"✓ Provider model created: {provider}")
        
        # Test enum values
        print(f"✓ SMS statuses available: {[s.value for s in SmsStatusEnum]}")
        print(f"✓ Provider statuses available: {[s.value for s in ProviderStatusEnum]}")
        
        return True
        
    except Exception as e:
        print(f"✗ API wrapper test failed: {e}")
        return False


def test_keycloak_connectivity():
    """Test basic connectivity to Keycloak server."""
    print("\n" + "=" * 50)
    print("Testing Keycloak Connectivity")
    print("=" * 50)
    
    try:
        import requests
        config = SMSGatewayConfig("config.ini")
        
        keycloak_url = config.get('KEYCLOAK', 'KeycloakURL')
        realm = config.get('KEYCLOAK', 'KeycloakRealm', 'sr')
        
        print(f"  Testing connectivity to: {keycloak_url}")
        
        # Test different URL formats to find the correct one
        test_urls = [
            f"{keycloak_url}/realms/{realm}/.well-known/openid_configuration",
            f"{keycloak_url}/auth/realms/{realm}/.well-known/openid_configuration",
            f"{keycloak_url}/realms/{realm}",
            f"{keycloak_url}/auth/realms/{realm}"
        ]
        
        working_url = None
        for url in test_urls:
            try:
                print(f"  Trying: {url}")
                response = requests.get(url, timeout=10, verify=True)
                if response.status_code == 200:
                    print(f"✓ Successfully connected to: {url}")
                    working_url = url
                    
                    # If it's the well-known config, parse the token endpoint
                    if 'well-known' in url:
                        try:
                            config_data = response.json()
                            token_endpoint = config_data.get('token_endpoint')
                            if token_endpoint:
                                print(f"  Token endpoint from discovery: {token_endpoint}")
                        except:
                            pass
                    break
                else:
                    print(f"  Status {response.status_code}: {url}")
            except requests.exceptions.RequestException as e:
                print(f"  Connection failed: {url} - {e}")
        
        if working_url:
            print(f"✓ Keycloak server is accessible")
            return True
        else:
            print("✗ Unable to connect to Keycloak server")
            return False
            
    except Exception as e:
        print(f"✗ Connectivity test failed: {e}")
        return False


def main():
    """Run all tests."""
    print("SensorReport.PI.SMS.Gateway - Test Suite")
    print("Version: 1.0.0")
    print("Platform: Raspberry PI")
    
    # Change to script directory
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    
    tests = [
        ("Dependencies", test_dependencies),
        ("Configuration", test_configuration),
        ("Logging", test_logging),
        ("Authentication", test_authentication)
    ]
    
    results = []
    
    for test_name, test_func in tests:
        try:
            result = test_func()
            results.append((test_name, result))
        except KeyboardInterrupt:
            print(f"\n\nTest interrupted during: {test_name}")
            break
        except Exception as e:
            print(f"✗ Unexpected error in {test_name}: {e}")
            results.append((test_name, False))
    
    # Summary
    print("\n" + "=" * 50)
    print("Test Summary")
    print("=" * 50)
    
    passed = 0
    total = len(results)
    
    for test_name, result in results:
        status = "PASS" if result else "FAIL"
        print(f"  {test_name:<20} : {status}")
        if result:
            passed += 1
    
    print(f"\nResults: {passed}/{total} tests passed")
    
    if passed == total:
        print("✓ All tests passed!")
        return 0
    else:
        print("✗ Some tests failed!")
        return 1


if __name__ == "__main__":
    sys.exit(main())
