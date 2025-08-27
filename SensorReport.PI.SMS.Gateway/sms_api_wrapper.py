#!/usr/bin/env python3
"""
SMS API Wrapper for SensorReport.PI.SMS.Gateway

This module provides a Python wrapper for the SensorsReport.SMS.API SMS endpoints.
It handles authentication, request formatting, and response parsing for SMS management operations.

Features:
- SMS creation and management
- Status tracking and updates
- Message querying and filtering
- Phone number validation
- Error handling and retry logic
- Keycloak authentication integration
- Multi-tenant support

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
import re


class SmsStatusEnum(Enum):
    """SMS status enumeration matching the API."""
    PENDING = "Pending"
    ENTRUSTED = "Entrusted"
    SENT = "Sent"
    FAILED = "Failed"
    EXPIRED = "Expired"
    ERROR = "Error"
    UNKNOWN = "Unknown"


class SmsModel:
    """SMS data model matching the API structure."""
    
    def __init__(self, phone_number: str, message: str, provider: str = "",
                 id: Optional[str] = None, timestamp: Optional[datetime] = None,
                 status: SmsStatusEnum = SmsStatusEnum.PENDING, sent_at: Optional[datetime] = None,
                 ttl: Optional[int] = 5, country_code: Optional[str] = None,
                 tracking_id: Optional[str] = None, message_type: Optional[str] = None,
                 custom_data: Optional[str] = None, retry_count: int = 0,
                 other_fields: Optional[Dict[str, Any]] = None):
        """
        Initialize SMS model.
        
        Args:
            phone_number (str): Recipient phone number
            message (str): SMS message content
            provider (str): SMS provider name
            id (str, optional): SMS ID (set by API)
            timestamp (datetime, optional): Creation timestamp
            status (SmsStatusEnum): SMS status
            sent_at (datetime, optional): Sent timestamp
            ttl (int, optional): Time to live in minutes
            country_code (str, optional): Country code
            tracking_id (str, optional): External tracking ID
            message_type (str, optional): Message type/category
            custom_data (str, optional): Custom JSON data
            retry_count (int): Number of retry attempts
            other_fields (dict, optional): Additional fields
        """
        self.id = id
        self.phone_number = phone_number
        self.message = message
        self.timestamp = timestamp or datetime.utcnow()
        self.status = status
        self.sent_at = sent_at
        self.ttl = ttl
        self.country_code = country_code
        self.provider = provider
        self.tracking_id = tracking_id
        self.message_type = message_type
        self.custom_data = custom_data
        self.retry_count = retry_count
        self.other_fields = other_fields or {}
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for API requests."""
        data = {
            'phoneNumber': self.phone_number,
            'message': self.message,
            'provider': self.provider,
            'status': self.status.value,
            'retryCount': self.retry_count
        }
        
        if self.id:
            data['id'] = self.id
        if self.timestamp:
            data['timestamp'] = self.timestamp.isoformat() + 'Z'
        if self.sent_at:
            data['sentAt'] = self.sent_at.isoformat() + 'Z'
        if self.ttl is not None:
            data['ttl'] = self.ttl
        if self.country_code:
            data['countryCode'] = self.country_code
        if self.tracking_id:
            data['trackingId'] = self.tracking_id
        if self.message_type:
            data['messageType'] = self.message_type
        if self.custom_data:
            data['customData'] = self.custom_data
        
        # Add other fields
        data.update(self.other_fields)
        
        return data
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'SmsModel':
        """Create instance from API response data."""
        # Parse timestamps
        timestamp = None
        if data.get('timestamp'):
            timestamp = datetime.fromisoformat(data['timestamp'].replace('Z', '+00:00'))
        
        sent_at = None
        if data.get('sentAt'):
            sent_at = datetime.fromisoformat(data['sentAt'].replace('Z', '+00:00'))
        
        # Parse status
        status = SmsStatusEnum.UNKNOWN
        if data.get('status'):
            try:
                status = SmsStatusEnum(data['status'])
            except ValueError:
                status = SmsStatusEnum.UNKNOWN
        
        # Extract other fields
        known_fields = {
            'id', 'phoneNumber', 'message', 'timestamp', 'status', 'sentAt',
            'ttl', 'countryCode', 'provider', 'trackingId', 'messageType',
            'customData', 'retryCount', 'tenant', 'statusMessage'
        }
        other_fields = {k: v for k, v in data.items() if k not in known_fields}
        
        return cls(
            id=data.get('id'),
            phone_number=data.get('phoneNumber', ''),
            message=data.get('message', ''),
            provider=data.get('provider', ''),
            timestamp=timestamp,
            status=status,
            sent_at=sent_at,
            ttl=data.get('ttl'),
            country_code=data.get('countryCode'),
            tracking_id=data.get('trackingId'),
            message_type=data.get('messageType'),
            custom_data=data.get('customData'),
            retry_count=data.get('retryCount', 0),
            other_fields=other_fields
        )
    
    def is_expired(self) -> bool:
        """Check if the SMS has expired based on TTL."""
        if not self.ttl or not self.timestamp:
            return False
        
        expiry_time = self.timestamp + timedelta(minutes=self.ttl)
        return datetime.utcnow() > expiry_time
    
    def get_age_minutes(self) -> float:
        """Get the age of the SMS in minutes."""
        if not self.timestamp:
            return 0.0
        
        age = datetime.utcnow() - self.timestamp
        return age.total_seconds() / 60.0
    
    def __str__(self) -> str:
        return f"SMS(id={self.id}, phone={self.phone_number}, status={self.status.value}, provider={self.provider})"


class SMSAPIWrapper:
    """
    Wrapper class for SensorsReport.SMS.API SMS endpoints.
    
    This class provides a Python interface to the SMS management API,
    handling authentication, request formatting, and response parsing.
    """
    
    def __init__(self, base_url: str, auth_manager, tenant_id: Optional[str] = None):
        """
        Initialize the SMS API wrapper.
        
        Args:
            base_url (str): Base URL of the SMS API service
            auth_manager: Keycloak authentication manager instance
            tenant_id (str, optional): Tenant ID for multi-tenant support
        """
        self.base_url = base_url.rstrip('/')
        self.auth_manager = auth_manager
        self.tenant_id = tenant_id
        
        # API endpoints
        self.sms_endpoint = f"{self.base_url}/api/sms"
        
        # Configuration
        self.timeout = 30
        self.max_retries = 3
        self.retry_delay = 2
        
        # Setup logging
        self.logger = logging.getLogger(f"{__name__}.SMSAPIWrapper")
    
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
    
    def create_sms(self, sms: SmsModel) -> Optional[SmsModel]:
        """
        Create a new SMS message.
        
        Args:
            sms (SmsModel): SMS message to create
            
        Returns:
            SmsModel: Created SMS with ID or None on failure
        """
        try:
            self.logger.info(f"Creating SMS to {sms.phone_number}")
            
            data = sms.to_dict()
            response_data = self._make_request('POST', self.sms_endpoint, data=data)
            
            if response_data:
                created_sms = SmsModel.from_dict(response_data)
                self.logger.info(f"Successfully created SMS: {created_sms.id}")
                return created_sms
            else:
                self.logger.error("Failed to create SMS - no response data")
                return None
                
        except Exception as e:
            self.logger.error(f"Error creating SMS: {e}")
            return None
    
    def get_sms_by_id(self, sms_id: str) -> Optional[SmsModel]:
        """
        Get SMS message by ID.
        
        Args:
            sms_id (str): SMS message ID
            
        Returns:
            SmsModel: SMS message or None if not found
        """
        try:
            self.logger.debug(f"Getting SMS by ID: {sms_id}")
            
            url = f"{self.sms_endpoint}/{sms_id}"
            response_data = self._make_request('GET', url)
            
            if response_data:
                sms = SmsModel.from_dict(response_data)
                self.logger.debug(f"Found SMS: {sms}")
                return sms
            else:
                self.logger.debug(f"SMS not found: {sms_id}")
                return None
                
        except Exception as e:
            self.logger.error(f"Error getting SMS {sms_id}: {e}")
            return None
    
    def get_sms_list(self, from_date: Optional[str] = None, to_date: Optional[str] = None,
                     limit: int = 100, offset: int = 0, status: Optional[SmsStatusEnum] = None,
                     country_code: Optional[str] = None) -> List[SmsModel]:
        """
        Get list of SMS messages with filtering.
        
        Args:
            from_date (str, optional): Start date (ISO format)
            to_date (str, optional): End date (ISO format)
            limit (int): Maximum number of results
            offset (int): Result offset for pagination
            status (SmsStatusEnum, optional): Filter by status
            country_code (str, optional): Filter by country code
            
        Returns:
            List[SmsModel]: List of SMS messages
        """
        try:
            self.logger.debug(f"Getting SMS list with filters")
            
            params = {
                'limit': limit,
                'offset': offset
            }
            
            if from_date:
                params['fromDate'] = from_date
            if to_date:
                params['toDate'] = to_date
            if status:
                params['status'] = status.value
            if country_code:
                params['countryCode'] = country_code
            
            response_data = self._make_request('GET', self.sms_endpoint, params=params)
            
            if response_data:
                if isinstance(response_data, list):
                    sms_list = [SmsModel.from_dict(sms_data) for sms_data in response_data]
                else:
                    sms_list = [SmsModel.from_dict(response_data)]
                
                self.logger.debug(f"Retrieved {len(sms_list)} SMS messages")
                return sms_list
            else:
                self.logger.debug("No SMS messages found")
                return []
                
        except Exception as e:
            self.logger.error(f"Error getting SMS list: {e}")
            return []
    
    def update_sms(self, sms_id: str, sms: SmsModel) -> Optional[SmsModel]:
        """
        Update SMS message.
        
        Args:
            sms_id (str): SMS message ID
            sms (SmsModel): Updated SMS data
            
        Returns:
            SmsModel: Updated SMS or None on failure
        """
        try:
            self.logger.info(f"Updating SMS: {sms_id}")
            
            url = f"{self.sms_endpoint}/{sms_id}"
            data = sms.to_dict()
            
            response_data = self._make_request('PUT', url, data=data)
            
            if response_data:
                updated_sms = SmsModel.from_dict(response_data)
                self.logger.info(f"Successfully updated SMS: {updated_sms.id}")
                return updated_sms
            else:
                self.logger.error("Failed to update SMS - no response data")
                return None
                
        except Exception as e:
            self.logger.error(f"Error updating SMS {sms_id}: {e}")
            return None
    
    def update_sms_status(self, sms_id: str, status: SmsStatusEnum, 
                         status_message: Optional[str] = None,
                         sent_at: Optional[datetime] = None) -> Optional[SmsModel]:
        """
        Update SMS status and related fields.
        
        Args:
            sms_id (str): SMS message ID
            status (SmsStatusEnum): New status
            status_message (str, optional): Status message
            sent_at (datetime, optional): Sent timestamp
            
        Returns:
            SmsModel: Updated SMS or None on failure
        """
        try:
            self.logger.info(f"Updating SMS status: {sms_id} -> {status.value}")
            
            # Get current SMS to preserve other fields
            current_sms = self.get_sms_by_id(sms_id)
            if not current_sms:
                self.logger.error(f"SMS not found for status update: {sms_id}")
                return None
            
            # Update status and related fields
            current_sms.status = status
            if sent_at:
                current_sms.sent_at = sent_at
            
            # Use PATCH for partial update
            url = f"{self.sms_endpoint}/{sms_id}"
            data = {
                'status': status.value
            }
            
            if sent_at:
                data['sentAt'] = sent_at.isoformat() + 'Z'
            
            response_data = self._make_request('PATCH', url, data=data)
            
            if response_data:
                updated_sms = SmsModel.from_dict(response_data)
                self.logger.info(f"Successfully updated SMS status: {updated_sms.id}")
                return updated_sms
            else:
                self.logger.error("Failed to update SMS status - no response data")
                return None
                
        except Exception as e:
            self.logger.error(f"Error updating SMS status {sms_id}: {e}")
            return None
    
    def delete_sms(self, sms_id: str) -> bool:
        """
        Delete SMS message.
        
        Args:
            sms_id (str): SMS message ID
            
        Returns:
            bool: True if deleted successfully, False otherwise
        """
        try:
            self.logger.info(f"Deleting SMS: {sms_id}")
            
            url = f"{self.sms_endpoint}/{sms_id}"
            response_data = self._make_request('DELETE', url)
            
            # DELETE typically returns 204 No Content or 200 OK
            self.logger.info(f"Successfully deleted SMS: {sms_id}")
            return True
                
        except Exception as e:
            self.logger.error(f"Error deleting SMS {sms_id}: {e}")
            return False
    
    def get_pending_sms(self, limit: int = 100, country_code: Optional[str] = None) -> List[SmsModel]:
        """
        Get pending SMS messages.
        
        Args:
            limit (int): Maximum number of results
            country_code (str, optional): Filter by country code
            
        Returns:
            List[SmsModel]: List of pending SMS messages
        """
        return self.get_sms_list(
            status=SmsStatusEnum.PENDING,
            limit=limit,
            country_code=country_code
        )
    
    def get_failed_sms(self, limit: int = 100) -> List[SmsModel]:
        """
        Get failed SMS messages.
        
        Args:
            limit (int): Maximum number of results
            
        Returns:
            List[SmsModel]: List of failed SMS messages
        """
        return self.get_sms_list(
            status=SmsStatusEnum.FAILED,
            limit=limit
        )
    
    def mark_sms_as_sent(self, sms_id: str, sent_at: Optional[datetime] = None) -> Optional[SmsModel]:
        """
        Mark SMS as sent.
        
        Args:
            sms_id (str): SMS message ID
            sent_at (datetime, optional): Sent timestamp (defaults to now)
            
        Returns:
            SmsModel: Updated SMS or None on failure
        """
        sent_timestamp = sent_at or datetime.utcnow()
        return self.update_sms_status(sms_id, SmsStatusEnum.SENT, sent_at=sent_timestamp)
    
    def mark_sms_as_failed(self, sms_id: str, error_message: Optional[str] = None) -> Optional[SmsModel]:
        """
        Mark SMS as failed.
        
        Args:
            sms_id (str): SMS message ID
            error_message (str, optional): Error message
            
        Returns:
            SmsModel: Updated SMS or None on failure
        """
        return self.update_sms_status(sms_id, SmsStatusEnum.FAILED, status_message=error_message)
    
    def mark_sms_as_entrusted(self, sms_id: str) -> Optional[SmsModel]:
        """
        Mark SMS as entrusted to provider.
        
        Args:
            sms_id (str): SMS message ID
            
        Returns:
            SmsModel: Updated SMS or None on failure
        """
        return self.update_sms_status(sms_id, SmsStatusEnum.ENTRUSTED)
    
    def increment_retry_count(self, sms_id: str) -> Optional[SmsModel]:
        """
        Increment retry count for SMS.
        
        Args:
            sms_id (str): SMS message ID
            
        Returns:
            SmsModel: Updated SMS or None on failure
        """
        try:
            current_sms = self.get_sms_by_id(sms_id)
            if not current_sms:
                return None
            
            current_sms.retry_count += 1
            return self.update_sms(sms_id, current_sms)
            
        except Exception as e:
            self.logger.error(f"Error incrementing retry count for SMS {sms_id}: {e}")
            return None
    
    def validate_phone_number(self, phone_number: str, country_code: Optional[str] = None) -> bool:
        """
        Basic phone number validation.
        
        Args:
            phone_number (str): Phone number to validate
            country_code (str, optional): Country code for validation
            
        Returns:
            bool: True if phone number appears valid
        """
        # Basic validation - remove spaces and special characters
        cleaned = re.sub(r'[\s\-\(\)]', '', phone_number)
        
        # Check if it starts with + or country code
        if cleaned.startswith('+'):
            return len(cleaned) >= 8 and cleaned[1:].isdigit()
        
        # Check if it's all digits and reasonable length
        return cleaned.isdigit() and 7 <= len(cleaned) <= 15
    
    def create_simple_sms(self, phone_number: str, message: str, provider: str = "",
                         country_code: Optional[str] = None) -> Optional[SmsModel]:
        """
        Create a simple SMS message with minimal configuration.
        
        Args:
            phone_number (str): Recipient phone number
            message (str): SMS message content
            provider (str): SMS provider name
            country_code (str, optional): Country code
            
        Returns:
            SmsModel: Created SMS or None on failure
        """
        if not self.validate_phone_number(phone_number, country_code):
            self.logger.error(f"Invalid phone number: {phone_number}")
            return None
        
        sms = SmsModel(
            phone_number=phone_number,
            message=message,
            provider=provider,
            country_code=country_code
        )
        
        return self.create_sms(sms)
    
    def get_sms_statistics(self) -> Dict[str, Any]:
        """
        Get SMS statistics.
        
        Returns:
            dict: SMS statistics including counts by status
        """
        try:
            stats = {
                'total': 0,
                'by_status': {},
                'timestamp': datetime.utcnow().isoformat()
            }
            
            # Get counts for each status
            for status in SmsStatusEnum:
                sms_list = self.get_sms_list(status=status, limit=1000)  # Adjust as needed
                count = len(sms_list)
                stats['by_status'][status.value] = count
                stats['total'] += count
            
            return stats
            
        except Exception as e:
            self.logger.error(f"Error getting SMS statistics: {e}")
            return {
                'error': str(e),
                'timestamp': datetime.utcnow().isoformat()
            }
