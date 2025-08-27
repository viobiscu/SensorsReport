#!/usr/bin/env python3
"""
SMS Provider API Wrapper for SensorReport.PI.SMS.Gateway

This module provides a Python wrapper for the SensorsReport.SMS.API Provider endpoints.
It handles authentication, request formatting, and response parsing for provider management operations.

Features:
- Provider registration and management
- Status monitoring and updates
- Country code mapping
- Message queue processing
- Error handling and retry logic
- Keycloak authentication integration

Author: SensorReport Team
Version: 1.0.0
Compatible with: SensorsReport.SMS.API, Raspberry PI
"""

import requests
import logging
import time
from datetime import datetime, timedelta
from typing import Optional, Dict, List, Any, Union
from enum import Enum
import json


class ProviderStatusEnum(Enum):
    """Provider status enumeration matching the API."""
    UNAVAILABLE = "Unavailable"
    ACTIVE = "Active"


class ProviderModel:
    """Provider data model matching the API structure."""
    
    def __init__(self, name: str, supported_country_codes: List[str] = None, 
                 id: Optional[str] = None, last_seen: Optional[datetime] = None,
                 status: ProviderStatusEnum = ProviderStatusEnum.ACTIVE):
        """
        Initialize Provider model.
        
        Args:
            name (str): Provider name
            supported_country_codes (List[str]): List of supported country codes
            id (str, optional): Provider ID (set by API)
            last_seen (datetime, optional): Last seen timestamp
            status (ProviderStatusEnum): Provider status
        """
        self.id = id
        self.name = name
        self.supported_country_codes = supported_country_codes or []
        self.last_seen = last_seen or datetime.utcnow()
        self.status = status
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for API requests."""
        data = {
            'name': self.name,
            'supportedCountryCodes': self.supported_country_codes
        }
        
        if self.id:
            data['id'] = self.id
            
        return data
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'ProviderModel':
        """Create instance from API response data."""
        return cls(
            id=data.get('id'),
            name=data.get('name', ''),
            supported_country_codes=data.get('supportedCountryCodes', []),
            last_seen=datetime.fromisoformat(data['lastSeen'].replace('Z', '+00:00')) if data.get('lastSeen') else None,
            status=ProviderStatusEnum(data.get('status', 'Active'))
        )
    
    def __str__(self) -> str:
        return f"Provider(name={self.name}, status={self.status.value}, countries={len(self.supported_country_codes)})"


class SMSProviderAPIWrapper:
    """
    Wrapper class for SensorsReport.SMS.API Provider endpoints.
    
    This class provides a Python interface to the Provider management API,
    handling authentication, request formatting, and response parsing.
    """
    
    def __init__(self, base_url: str, auth_manager, tenant_id: Optional[str] = None):
        """
        Initialize the Provider API wrapper.
        
        Args:
            base_url (str): Base URL of the SMS API service
            auth_manager: Keycloak authentication manager instance
            tenant_id (str, optional): Tenant ID for multi-tenant support
        """
        self.base_url = base_url.rstrip('/')
        self.auth_manager = auth_manager
        self.tenant_id = tenant_id
        
        # API endpoints
        self.provider_endpoint = f"{self.base_url}/api/provider"
        
        # Configuration
        self.timeout = 30
        self.max_retries = 3
        self.retry_delay = 2
        
        # Setup logging
        self.logger = logging.getLogger(f"{__name__}.SMSProviderAPIWrapper")
    
    def _get_headers(self) -> Dict[str, str]:
        """Get request headers with authentication and tenant information."""
        headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'User-Agent': 'SensorReport-PI-SMS-Gateway/1.0.0'
        }
        
        # Add authentication header
        auth_header = self.auth_manager.get_authorization_header()
        if auth_header:
            headers.update(auth_header)
        
        # Add tenant header if specified
        if self.tenant_id:
            headers['X-Tenant-Id'] = self.tenant_id
        
        return headers
    
    def _make_request(self, method: str, url: str, data: Optional[Dict] = None,
                     params: Optional[Dict] = None) -> Optional[Dict[str, Any]]:
        """
        Make HTTP request with retry logic and error handling.
        
        Args:
            method (str): HTTP method (GET, POST, PUT, etc.)
            url (str): Request URL
            data (dict, optional): Request body data
            params (dict, optional): Query parameters
            
        Returns:
            dict: Response data or None on failure
        """
        for attempt in range(self.max_retries):
            try:
                headers = self._get_headers()
                
                self.logger.debug(f"Making {method} request to {url} (attempt {attempt + 1})")
                
                kwargs = {
                    'headers': headers,
                    'timeout': self.timeout,
                    'verify': True
                }
                
                if data:
                    kwargs['json'] = data
                if params:
                    kwargs['params'] = params
                
                response = requests.request(method, url, **kwargs)
                
                # Log response details
                self.logger.debug(f"Response: {response.status_code} - {response.reason}")
                
                if response.status_code in [200, 201]:
                    return response.json() if response.content else {}
                elif response.status_code == 404:
                    self.logger.warning(f"Resource not found: {url}")
                    return None
                elif response.status_code in [400, 401, 403]:
                    error_msg = f"Client error {response.status_code}: {response.text}"
                    self.logger.error(error_msg)
                    raise ValueError(error_msg)
                else:
                    error_msg = f"Server error {response.status_code}: {response.text}"
                    self.logger.error(error_msg)
                    
                    if attempt < self.max_retries - 1:
                        self.logger.info(f"Retrying in {self.retry_delay} seconds...")
                        time.sleep(self.retry_delay)
                        continue
                    else:
                        raise Exception(error_msg)
                        
            except requests.exceptions.Timeout:
                self.logger.error(f"Request timeout for {url}")
                if attempt < self.max_retries - 1:
                    time.sleep(self.retry_delay)
                    continue
                raise
                
            except requests.exceptions.ConnectionError as e:
                self.logger.error(f"Connection error for {url}: {e}")
                if attempt < self.max_retries - 1:
                    time.sleep(self.retry_delay)
                    continue
                raise
                
            except Exception as e:
                self.logger.error(f"Unexpected error in request to {url}: {e}")
                raise
        
        return None
    
    def register_provider(self, provider: ProviderModel) -> Optional[ProviderModel]:
        """
        Register a new SMS provider.
        
        Args:
            provider (ProviderModel): Provider information to register
            
        Returns:
            ProviderModel: Created provider or None on failure
        """
        try:
            self.logger.info(f"Registering provider: {provider.name}")
            
            url = f"{self.provider_endpoint}/register"
            data = provider.to_dict()
            
            response_data = self._make_request('POST', url, data=data)
            
            if response_data:
                created_provider = ProviderModel.from_dict(response_data)
                self.logger.info(f"Successfully registered provider: {created_provider}")
                return created_provider
            else:
                self.logger.error("Failed to register provider - no response data")
                return None
                
        except Exception as e:
            self.logger.error(f"Error registering provider {provider.name}: {e}")
            return None
    
    def get_provider_by_name(self, provider_name: str) -> Optional[ProviderModel]:
        """
        Get provider information by name.
        
        Args:
            provider_name (str): Name of the provider
            
        Returns:
            ProviderModel: Provider information or None if not found
        """
        try:
            self.logger.debug(f"Getting provider by name: {provider_name}")
            
            url = f"{self.provider_endpoint}/{provider_name}"
            response_data = self._make_request('GET', url)
            
            if response_data:
                provider = ProviderModel.from_dict(response_data)
                self.logger.debug(f"Found provider: {provider}")
                return provider
            else:
                self.logger.debug(f"Provider not found: {provider_name}")
                return None
                
        except Exception as e:
            self.logger.error(f"Error getting provider {provider_name}: {e}")
            return None
    
    def get_providers_by_status(self, status: ProviderStatusEnum) -> Optional[List[ProviderModel]]:
        """
        Get providers by status.
        
        Args:
            status (ProviderStatusEnum): Provider status to filter by
            
        Returns:
            List[ProviderModel]: List of providers with the specified status
        """
        try:
            self.logger.debug(f"Getting providers by status: {status.value}")
            
            url = f"{self.provider_endpoint}/status/{status.value}"
            response_data = self._make_request('GET', url)
            
            if response_data:
                if isinstance(response_data, list):
                    providers = [ProviderModel.from_dict(p) for p in response_data]
                else:
                    providers = [ProviderModel.from_dict(response_data)]
                
                self.logger.debug(f"Found {len(providers)} providers with status {status.value}")
                return providers
            else:
                self.logger.debug(f"No providers found with status: {status.value}")
                return []
                
        except Exception as e:
            self.logger.error(f"Error getting providers by status {status.value}: {e}")
            return None
    
    def get_next_sms_for_provider(self, provider_name: str, country_code: Optional[str] = None) -> Optional[Dict[str, Any]]:
        """
        Get the next SMS message for a provider to process.
        
        Args:
            provider_name (str): Name of the provider
            country_code (str, optional): Specific country code to filter by
            
        Returns:
            dict: SMS message data or None if no messages available
        """
        try:
            self.logger.debug(f"Getting next SMS for provider: {provider_name}")
            
            if country_code:
                url = f"{self.provider_endpoint}/next/{provider_name}/{country_code}"
            else:
                url = f"{self.provider_endpoint}/next/{provider_name}"
            
            response_data = self._make_request('GET', url)
            
            if response_data:
                self.logger.debug(f"Retrieved SMS message for provider {provider_name}")
                return response_data
            else:
                self.logger.debug(f"No SMS messages available for provider {provider_name}")
                return None
                
        except Exception as e:
            self.logger.error(f"Error getting next SMS for provider {provider_name}: {e}")
            return None
    
    def get_active_providers(self) -> List[ProviderModel]:
        """
        Get all active providers.
        
        Returns:
            List[ProviderModel]: List of active providers
        """
        return self.get_providers_by_status(ProviderStatusEnum.ACTIVE) or []
    
    def get_unavailable_providers(self) -> List[ProviderModel]:
        """
        Get all unavailable providers.
        
        Returns:
            List[ProviderModel]: List of unavailable providers
        """
        return self.get_providers_by_status(ProviderStatusEnum.UNAVAILABLE) or []
    
    def check_provider_exists(self, provider_name: str) -> bool:
        """
        Check if a provider exists.
        
        Args:
            provider_name (str): Name of the provider
            
        Returns:
            bool: True if provider exists, False otherwise
        """
        provider = self.get_provider_by_name(provider_name)
        return provider is not None
    
    def get_provider_supported_countries(self, provider_name: str) -> List[str]:
        """
        Get supported country codes for a provider.
        
        Args:
            provider_name (str): Name of the provider
            
        Returns:
            List[str]: List of supported country codes
        """
        provider = self.get_provider_by_name(provider_name)
        return provider.supported_country_codes if provider else []
    
    def poll_for_sms(self, provider_name: str, country_codes: Optional[List[str]] = None,
                     poll_interval: int = 10, max_polls: int = 0):
        """
        Poll for SMS messages for a provider.
        
        Args:
            provider_name (str): Name of the provider
            country_codes (List[str], optional): List of country codes to poll for
            poll_interval (int): Interval between polls in seconds
            max_polls (int): Maximum number of polls (0 = infinite)
            
        Yields:
            dict: SMS message data
        """
        self.logger.info(f"Starting SMS polling for provider: {provider_name}")
        
        poll_count = 0
        country_codes = country_codes or [None]  # None means all countries
        
        while max_polls == 0 or poll_count < max_polls:
            try:
                messages_found = False
                
                for country_code in country_codes:
                    sms_data = self.get_next_sms_for_provider(provider_name, country_code)
                    if sms_data:
                        messages_found = True
                        yield sms_data
                
                if not messages_found:
                    self.logger.debug(f"No messages available, sleeping for {poll_interval} seconds")
                    time.sleep(poll_interval)
                
                poll_count += 1
                
            except KeyboardInterrupt:
                self.logger.info("Polling interrupted by user")
                break
            except Exception as e:
                self.logger.error(f"Error during polling: {e}")
                time.sleep(poll_interval)
    
    def get_api_status(self) -> Dict[str, Any]:
        """
        Get API status information.
        
        Returns:
            dict: API status information
        """
        try:
            # Try to get an active provider as a health check
            providers = self.get_active_providers()
            
            return {
                'api_accessible': True,
                'active_providers': len(providers),
                'timestamp': datetime.utcnow().isoformat(),
                'base_url': self.base_url
            }
            
        except Exception as e:
            self.logger.error(f"Error checking API status: {e}")
            return {
                'api_accessible': False,
                'error': str(e),
                'timestamp': datetime.utcnow().isoformat(),
                'base_url': self.base_url
            }
