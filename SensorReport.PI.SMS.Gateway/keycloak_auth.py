#!/usr/bin/env python3
"""
Keycloak Authentication Manager for SensorReport.PI.SMS.Gateway

This module handles authentication with Keycloak server, including:
- Initial login and token retrieval
- Automatic token renewal before expiration
- Robust error handling and retry mechanisms
- Thread-safe token management

Author: SensorReport Team
Version: 1.0.0
Compatible with: Raspberry PI 4, Python 3.9+
"""

import requests
import time
import threading
import logging
from datetime import datetime, timedelta
from typing import Optional, Dict, Any
import json


class KeycloakAuthManager:
    """
    Manages Keycloak authentication and token lifecycle.
    
    This class provides robust authentication management with automatic
    token renewal, error handling, and thread-safe operations.
    """
    
    def __init__(self, keycloak_url: str, realm: str, client_id: str, 
                 username: str, password: str, client_secret: Optional[str] = None):
        """
        Initialize the Keycloak Authentication Manager.
        
        Args:
            keycloak_url (str): Base URL of the Keycloak server
            realm (str): Keycloak realm name
            client_id (str): Client ID for authentication
            username (str): Username for authentication
            password (str): Password for authentication
            client_secret (str, optional): Client secret if required
        """
        self.keycloak_url = keycloak_url.rstrip('/')
        self.realm = realm
        self.client_id = client_id
        self.username = username
        self.password = password
        self.client_secret = client_secret
        
        # Token management
        self._access_token: Optional[str] = None
        self._refresh_token: Optional[str] = None
        self._token_expires_at: Optional[datetime] = None
        self._token_lock = threading.Lock()
        
        # Configuration
        self.renewal_buffer_seconds = 60  # Renew token 60 seconds before expiry
        self.max_retry_attempts = 3
        self.retry_delay_seconds = 5
        
        # Setup logging
        self.logger = logging.getLogger(__name__)
        
        # URLs - Support both legacy and modern Keycloak URL structures
        self.token_url = self._build_token_url()
        self.logout_url = self._build_logout_url()
    
    def _build_token_url(self) -> str:
        """Build the token URL supporting both legacy and modern Keycloak versions."""
        # Try modern format first (Keycloak 18+)
        modern_url = f"{self.keycloak_url}/realms/{self.realm}/protocol/openid-connect/token"
        
        # Fallback to legacy format (Keycloak < 18)
        legacy_url = f"{self.keycloak_url}/auth/realms/{self.realm}/protocol/openid-connect/token"
        
        # For now, we'll try to detect which one to use by testing the URL structure
        # If the base URL already contains /auth, use legacy format
        if '/auth' in self.keycloak_url:
            return f"{self.keycloak_url}/realms/{self.realm}/protocol/openid-connect/token"
        else:
            # Default to modern format, will fallback during login if needed
            return modern_url
    
    def _build_logout_url(self) -> str:
        """Build the logout URL supporting both legacy and modern Keycloak versions."""
        # Try modern format first (Keycloak 18+)
        if '/auth' in self.keycloak_url:
            return f"{self.keycloak_url}/realms/{self.realm}/protocol/openid-connect/logout"
        else:
            return f"{self.keycloak_url}/realms/{self.realm}/protocol/openid-connect/logout"
        
    def login(self) -> bool:
        """
        Perform initial login to Keycloak and retrieve tokens.
        
        Returns:
            bool: True if login successful, False otherwise
        """
        # Try both modern and legacy URL formats
        urls_to_try = [
            self.token_url,  # Primary URL
            f"{self.keycloak_url}/auth/realms/{self.realm}/protocol/openid-connect/token",  # Legacy fallback
            f"{self.keycloak_url}/realms/{self.realm}/protocol/openid-connect/token"  # Modern fallback
        ]
        
        # Remove duplicates while preserving order
        urls_to_try = list(dict.fromkeys(urls_to_try))
        
        for attempt, url in enumerate(urls_to_try):
            try:
                self.logger.info(f"Attempting to login to Keycloak (attempt {attempt + 1})...")
                self.logger.debug(f"Using URL: {url}")
                
                # Prepare login data
                data = {
                    'grant_type': 'password',
                    'client_id': self.client_id,
                    'username': self.username,
                    'password': self.password,
                    'scope': 'openid profile email'
                }
                
                # Add client secret if provided
                if self.client_secret:
                    data['client_secret'] = self.client_secret
                
                headers = {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'User-Agent': 'SensorReport-PI-SMS-Gateway/1.0.0'
                }
                
                # Make the authentication request
                response = requests.post(
                    url,
                    data=data,
                    headers=headers,
                    timeout=30,
                    verify=True
                )
                
                if response.status_code == 200:
                    token_data = response.json()
                    
                    with self._token_lock:
                        self._access_token = token_data.get('access_token')
                        self._refresh_token = token_data.get('refresh_token')
                        
                        # Calculate expiration time
                        expires_in = token_data.get('expires_in', 3600)
                        self._token_expires_at = datetime.now() + timedelta(seconds=expires_in)
                    
                    # Update URLs to use the successful format
                    self.token_url = url
                    self.logout_url = url.replace('/token', '/logout')
                    
                    self.logger.info(f"Login successful. Token expires at: {self._token_expires_at}")
                    self.logger.info(f"Using Keycloak URL format: {url}")
                    return True
                else:
                    self.logger.warning(f"Login attempt {attempt + 1} failed. Status: {response.status_code}, Response: {response.text}")
                    if attempt == len(urls_to_try) - 1:  # Last attempt
                        self.logger.error(f"All login attempts failed. Final status: {response.status_code}")
                    
            except requests.exceptions.RequestException as e:
                self.logger.warning(f"Network error during login attempt {attempt + 1}: {e}")
                if attempt == len(urls_to_try) - 1:  # Last attempt
                    self.logger.error(f"All login attempts failed due to network errors")
            except Exception as e:
                self.logger.warning(f"Unexpected error during login attempt {attempt + 1}: {e}")
                if attempt == len(urls_to_try) - 1:  # Last attempt
                    self.logger.error(f"All login attempts failed due to unexpected errors")
        
        return False
    
    def refresh_token(self) -> bool:
        """
        Refresh the access token using the refresh token.
        
        Returns:
            bool: True if refresh successful, False otherwise
        """
        try:
            if not self._refresh_token:
                self.logger.warning("No refresh token available, performing full login")
                return self.login()
            
            self.logger.info("Refreshing access token...")
            
            data = {
                'grant_type': 'refresh_token',
                'client_id': self.client_id,
                'refresh_token': self._refresh_token
            }
            
            if self.client_secret:
                data['client_secret'] = self.client_secret
            
            headers = {
                'Content-Type': 'application/x-www-form-urlencoded',
                'User-Agent': 'SensorReport-PI-SMS-Gateway/1.0.0'
            }
            
            response = requests.post(
                self.token_url,
                data=data,
                headers=headers,
                timeout=30,
                verify=True
            )
            
            if response.status_code == 200:
                token_data = response.json()
                
                with self._token_lock:
                    self._access_token = token_data.get('access_token')
                    
                    # Update refresh token if provided
                    new_refresh_token = token_data.get('refresh_token')
                    if new_refresh_token:
                        self._refresh_token = new_refresh_token
                    
                    # Calculate new expiration time
                    expires_in = token_data.get('expires_in', 3600)
                    self._token_expires_at = datetime.now() + timedelta(seconds=expires_in)
                
                self.logger.info(f"Token refreshed successfully. New expiration: {self._token_expires_at}")
                return True
            else:
                self.logger.error(f"Token refresh failed. Status: {response.status_code}, Response: {response.text}")
                # If refresh fails, try full login
                return self.login()
                
        except requests.exceptions.RequestException as e:
            self.logger.error(f"Network error during token refresh: {e}")
            return False
        except Exception as e:
            self.logger.error(f"Unexpected error during token refresh: {e}")
            return False
    
    def get_access_token(self) -> Optional[str]:
        """
        Get the current access token, refreshing if necessary.
        
        Returns:
            str: Current valid access token, or None if unavailable
        """
        with self._token_lock:
            # Check if we need to refresh the token
            if self.is_token_expired():
                self.logger.info("Token is expired or about to expire, refreshing...")
                if not self.refresh_token():
                    self.logger.error("Failed to refresh token")
                    return None
            
            return self._access_token
    
    def is_token_expired(self) -> bool:
        """
        Check if the current token is expired or about to expire.
        
        Returns:
            bool: True if token is expired or about to expire, False otherwise
        """
        if not self._access_token or not self._token_expires_at:
            return True
        
        # Consider token expired if it expires within the buffer time
        buffer_time = datetime.now() + timedelta(seconds=self.renewal_buffer_seconds)
        return self._token_expires_at <= buffer_time
    
    def check_and_renew_token(self) -> bool:
        """
        Check token status and renew if necessary.
        
        This method is thread-safe and can be called periodically.
        
        Returns:
            bool: True if token is valid (either current or successfully renewed)
        """
        try:
            if self.is_token_expired():
                self.logger.info("Token needs renewal")
                
                # Attempt renewal with retries
                for attempt in range(self.max_retry_attempts):
                    if self.refresh_token():
                        return True
                    
                    if attempt < self.max_retry_attempts - 1:
                        self.logger.warning(f"Token renewal attempt {attempt + 1} failed, retrying in {self.retry_delay_seconds} seconds...")
                        time.sleep(self.retry_delay_seconds)
                
                self.logger.error(f"Failed to renew token after {self.max_retry_attempts} attempts")
                return False
            
            return True  # Token is still valid
            
        except Exception as e:
            self.logger.error(f"Error during token check and renewal: {e}")
            return False
    
    def get_token_info(self) -> Dict[str, Any]:
        """
        Get information about the current token status.
        
        Returns:
            dict: Token information including validity and expiration
        """
        with self._token_lock:
            return {
                'has_token': self._access_token is not None,
                'expires_at': self._token_expires_at.isoformat() if self._token_expires_at else None,
                'is_expired': self.is_token_expired(),
                'seconds_until_expiry': (
                    (self._token_expires_at - datetime.now()).total_seconds()
                    if self._token_expires_at else None
                )
            }
    
    def logout(self) -> bool:
        """
        Logout from Keycloak and invalidate tokens.
        
        Returns:
            bool: True if logout successful, False otherwise
        """
        try:
            if not self._refresh_token:
                self.logger.info("No active session to logout from")
                return True
            
            self.logger.info("Logging out from Keycloak...")
            
            data = {
                'client_id': self.client_id,
                'refresh_token': self._refresh_token
            }
            
            if self.client_secret:
                data['client_secret'] = self.client_secret
            
            headers = {
                'Content-Type': 'application/x-www-form-urlencoded',
                'User-Agent': 'SensorReport-PI-SMS-Gateway/1.0.0'
            }
            
            response = requests.post(
                self.logout_url,
                data=data,
                headers=headers,
                timeout=30,
                verify=True
            )
            
            # Clear tokens regardless of response
            with self._token_lock:
                self._access_token = None
                self._refresh_token = None
                self._token_expires_at = None
            
            if response.status_code in [200, 204]:
                self.logger.info("Logout successful")
                return True
            else:
                self.logger.warning(f"Logout response: {response.status_code}, but tokens cleared locally")
                return True  # Still consider successful since tokens are cleared
                
        except requests.exceptions.RequestException as e:
            self.logger.error(f"Network error during logout: {e}")
            # Clear tokens locally even if logout request failed
            with self._token_lock:
                self._access_token = None
                self._refresh_token = None
                self._token_expires_at = None
            return False
        except Exception as e:
            self.logger.error(f"Unexpected error during logout: {e}")
            return False
    
    def get_authorization_header(self) -> Optional[Dict[str, str]]:
        """
        Get authorization header for API requests.
        
        Returns:
            dict: Authorization header with Bearer token, or None if no valid token
        """
        token = self.get_access_token()
        if token:
            return {'Authorization': f'Bearer {token}'}
        return None
    
    def __del__(self):
        """Cleanup when the object is destroyed."""
        try:
            self.logout()
        except:
            pass  # Ignore errors during cleanup
