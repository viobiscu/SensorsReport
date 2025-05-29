import os
import requests
from flask import Flask, request, redirect, jsonify, session, make_response, send_from_directory, Response, g
from flask_cors import CORS
from urllib.parse import urlencode
import logging
import json
import base64
import uuid
import time
import sys
from queue import Queue
import jwt

# =====================
# Global Configuration
# =====================

# Environment variables (with defaults)
LOG_LEVEL = os.environ.get('LOG_LEVEL', 'DEBUG').upper()
SECRET_KEY = os.environ.get('SECRET_KEY', 'dev-secret-key')  # Change in production
CORS_ORIGINS = os.environ.get('CORS_ORIGINS', '*')
KEYCLOAK_AUTH_URL = os.environ.get('KEYCLOAK_AUTH_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/auth')
KEYCLOAK_TOKEN_URL = os.environ.get('KEYCLOAK_TOKEN_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/token')
KEYCLOAK_USERINFO_URL = os.environ.get('KEYCLOAK_USERINFO_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/userinfo')
KEYCLOAK_LOGOUT_URL = os.environ.get('KEYCLOAK_LOGOUT_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/logout')
KEYCLOAK_CLIENT_ID = os.environ.get('KEYCLOAK_CLIENT_ID', 'ContextBroker')
KEYCLOAK_CLIENT_SECRET = os.environ.get('KEYCLOAK_CLIENT_SECRET', '')  # Set this if your Keycloak client has a secret
QUANTUM_LEAP_URL = os.environ.get('QUANTUM_LEAP_URL', 'http://quantum.sensorsreport.net:8668')
#DATA_PRODUCT_URL = os.environ.get('DATA_PRODUCT_URL', 'http://localhost:31483')
DATA_PRODUCT_URL = os.environ.get('DATA_PRODUCT_URL', 'http://data-product-manager:8000')
CONTEXT_BROKER_URL = os.environ.get('CONTEXT_BROKER_URL', 'http://orion-ld-broker:1026')
SECURE_COOKIES = os.environ.get('SECURE_COOKIES', 'false').lower() in ('true', 't', '1', 'yes')
HOST = os.environ.get('HOST', '0.0.0.0')
PORT = int(os.environ.get('PORT', '5000'))
DEBUG_MODE = os.environ.get('DEBUG', 'false').lower() in ('true', 't', '1', 'yes')

# Redirect URI for Keycloak
REDIRECT_URI = os.environ.get('REDIRECT_URI')

# Constants
DEFAULT_TOKEN_SCOPE = 'openid profile email'
DEFAULT_TOKEN_RESPONSE_TYPE = 'code'
DEFAULT_TOKEN_REDIRECT_PATH = '/api/auth/callback'

# Set up logging based on environment variable
log_level_name = LOG_LEVEL
log_level = getattr(logging, log_level_name, logging.DEBUG)
logging.basicConfig(level=log_level)
logger = logging.getLogger(__name__)

# Initialize Flask app with secret key from environment
app = Flask(__name__, static_folder='.')
app.secret_key = SECRET_KEY  # Change in production

# Parse allowed origins for CORS
cors_origins = CORS_ORIGINS
if cors_origins != '*':
    cors_origins = cors_origins.split(',')

# Enable CORS with configuration from environment
CORS(app, 
     resources={r"/*": {"origins": cors_origins}},
     supports_credentials=True,
     allow_headers=["Content-Type", "Authorization", "NGSILD-Tenant"],
     methods=["GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH"])

# Frontend URL detection
def get_frontend_url():
    # Check request origin to determine the frontend URL dynamically
    if request.headers.get('Origin'):
        return request.headers.get('Origin')
    
    # Check if we're running on localhost
    if request.host.startswith('localhost') or request.host.startswith('127.0.0.1'):
        return f"http://{request.host}"
        
    # If running on an IP address, use that IP with the default port
    if any(x.isdigit() for x in request.host.split('.')):
        host_parts = request.host.split(':')
        if len(host_parts) > 1:
            # If port is in host, use it
            return f"http://{request.host}"
        else:
            # Default to the same host without a specific port
            return f"http://{request.host}"
    
    # Fallback to HTTP based on current host
    return f"http://{request.host}"

# Keycloak configuration from environment variables
KEYCLOAK_CONFIG = {
    'auth_url': os.environ.get('KEYCLOAK_AUTH_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/auth'),
    'token_url': os.environ.get('KEYCLOAK_TOKEN_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/token'),
    'userinfo_url': os.environ.get('KEYCLOAK_USERINFO_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/userinfo'),
    'logout_url': os.environ.get('KEYCLOAK_LOGOUT_URL', 'http://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/logout'),
    'client_id': os.environ.get('KEYCLOAK_CLIENT_ID', 'ContextBroker'),
    'client_secret': os.environ.get('KEYCLOAK_CLIENT_SECRET', ''),  # Set this if your Keycloak client has a secret
}

# Quantum Lead configuration from environment variables
QUANTUM_LEAP_CONFIG = {
    'base_url': os.environ.get('QUANTUM_LEAP_URL', 'http://quantumleap:8668')
}


# Queue to store notifications
notification_queue = Queue()

# Serve static files (for development)
@app.route('/', defaults={'path': 'index.html'})
@app.route('/<path:path>')
def serve_static(path):
    return app.send_static_file(path)

@app.route('/api/schemas/<path:filename>')
def serve_schema(filename):
    """Serve JSON schema files from the js/schemas directory"""
    return send_from_directory('js/schemas', filename)

# Helper function to validate subscription payload
def validate_subscription_payload(payload):
    """
    Validate a subscription payload against NGSI-LD requirements
    """
    required_fields = ['type', 'notification']
    
    if not isinstance(payload, dict):
        return False, "Payload must be a JSON object"
        
    # Check required fields
    for field in required_fields:
        if field not in payload:
            return False, f"Missing required field: {field}"
            
    # Validate type
    if payload['type'] != 'Subscription':
        return False, "type field must be 'Subscription'"
        
    # Validate notification object
    notification = payload.get('notification', {})
    if not isinstance(notification, dict):
        return False, "notification must be an object"
        
    if 'endpoint' not in notification:
        return False, "notification must contain an endpoint"
        
    return True, None

@app.route('/api/ngsi-ld/v1/subscriptions', methods=['GET'])
def get_all_subscriptions():
    logger.info("GET /api/ngsi-ld/v1/subscriptions called")
    try:
        logger.debug("Getting all subscriptions")
        logger.debug(f"Request headers: {dict(request.headers)}")
        
        # Get context broker URL from environment variable with fallback
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/subscriptions"
        logger.debug(f"Target URL: {target_url}")
        
        # Forward the request with tenant header if present
        headers = {
            'Accept': 'application/json'
        }
        
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id:
            logger.debug(f"Using tenant: {tenant_id}")
            headers['NGSILD-Tenant'] = tenant_id
            
        logger.debug(f"Request headers to broker: {headers}")
        response = requests.get(target_url, headers=headers)
        logger.debug(f"Response from broker: status={response.status_code}, content={response.text[:200]}...")
        
        if response.status_code != 200:
            logger.error(f"Error from broker: status={response.status_code}, content={response.text}")
            return jsonify({'error': f"Broker error: {response.text}"}), response.status_code
            
        return response.json()
        
    except Exception as e:
        logger.error(f"Error fetching subscriptions: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['GET'])
def get_subscription(subscription_id):
    """
    Get a specific subscription by ID
    """
    try:
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/subscriptions/{subscription_id}"
        
        headers = {
            'Accept': 'application/json',
        }
        
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['NGSILD-Tenant'] = tenant_id
            
        response = requests.get(target_url, headers=headers)
        return make_response(response.content, response.status_code)
        
    except Exception as e:
        logger.error(f"Error fetching subscription {subscription_id}: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/subscriptions', methods=['POST'])
def create_subscription():
    """
    Create a new subscription
    """
    try:
        payload = request.json
        
        # Validate subscription payload
        is_valid, error_message = validate_subscription_payload(payload)
        if not is_valid:
            return jsonify({'error': error_message}), 400
            
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/subscriptions"
        # Prepare headers from incoming request
        headers = {}
        # If the payload has @context, use application/ld+json, else application/json
        if payload and ('@context' in payload or "@context" in payload):
            headers['Content-Type'] = 'application/ld+json'
        else:
            headers['Content-Type'] = 'application/json'
        headers['Accept'] = 'application/json'
        # Forward NGSILD-Tenant header if present
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['NGSILD-Tenant'] = tenant_id
            
        response = requests.post(target_url, headers=headers, json=payload)
        return make_response(response.content, response.status_code)
        
    except Exception as e:
        logger.error(f"Error creating subscription: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['PATCH'])
def update_subscription(subscription_id):
    """
    Update an existing subscription
    """
    try:
        payload = request.json
        
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/subscriptions/{subscription_id}"
        
        headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
        }
        
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['NGSILD-Tenant'] = tenant_id
            
        response = requests.patch(target_url, headers=headers, json=payload)
        return make_response(response.content, response.status_code)
        
    except Exception as e:
        logger.error(f"Error updating subscription {subscription_id}: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['DELETE'])
def delete_subscription(subscription_id):
    """
    Delete a subscription
    """
    try:
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/subscriptions/{subscription_id}"
        
        headers = {
            'Accept': 'application/json',
        }
        
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['NGSILD-Tenant'] = tenant_id
            
        response = requests.delete(target_url, headers=headers)
        return make_response(response.content, response.status_code)
        
    except Exception as e:
        logger.error(f"Error deleting subscription {subscription_id}: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/subscriptions', methods=['OPTIONS'])
@app.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['OPTIONS'])
def subscription_options(subscription_id=None):
    """
    Handle OPTIONS requests for subscriptions endpoints (CORS preflight)
    """
    response = make_response()
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type, Authorization, NGSILD-Tenant')
    response.headers.add('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, PATCH, OPTIONS')
    return response

# NGSI-LD routes
@app.route('/api/ngsi-ld/v1/entities', methods=['GET'])
def ngsi_ld_entities():
    """Get list of entities from NGSI-LD broker"""
    try:
        # Get context broker URL from environment variable with fallback
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/entities"
        
        # Forward the request with tenant header if present
        headers = {
            'Accept': 'application/json'
        }
        
        # Forward query parameters
        params = request.args.to_dict()
        
        # Handle tenant headers
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id:
            headers['NGSILD-Tenant'] = tenant_id
        
        logger.debug(f"Making request to NGSI-LD broker: {target_url}")
        logger.debug(f"Headers: {headers}")
        logger.debug(f"Params: {params}")
        
        response = requests.get(target_url, headers=headers, params=params)
        
        # Log response details
        logger.debug(f"NGSI-LD response status: {response.status_code}")
        logger.debug(f"NGSI-LD response headers: {dict(response.headers)}")
        
        if response.status_code != 200:
            logger.error(f"NGSI-LD error response: {response.text}")
            return jsonify({
                'error': 'Error from NGSI-LD broker',
                'status_code': response.status_code,
                'details': response.text
            }), response.status_code
            
        # Try to parse JSON response
        try:
            json_response = response.json()
            return jsonify(json_response)
        except ValueError as e:
            logger.error(f"Invalid JSON response from NGSI-LD broker: {response.text}")
            return jsonify({
                'error': 'Invalid JSON response from NGSI-LD broker',
                'details': str(e)
            }), 500
            
    except requests.exceptions.RequestException as e:
        error_msg = f"Error connecting to NGSI-LD broker: {str(e)}"
        logger.error(error_msg)
        return jsonify({'error': error_msg}), 500
    except Exception as e:
        error_msg = f"Unexpected error getting NGSI-LD entities: {str(e)}"
        logger.error(error_msg)
        return jsonify({'error': error_msg}), 500

@app.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['GET'])
def ngsi_ld_entity(entity_id):
    """Get a specific entity from NGSI-LD broker"""
    try:
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/entities/{entity_id}"
        
        headers = {
            'Accept': 'application/json'
        }
        
        # Forward query parameters
        params = request.args.to_dict()
        
        # Handle tenant headers
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id:
            headers['NGSILD-Tenant'] = tenant_id
            
        logger.debug(f"Making request to NGSI-LD broker: {target_url}")
        logger.debug(f"Headers: {headers}")
        logger.debug(f"Params: {params}")
        
        response = requests.get(target_url, headers=headers, params=params)
        return make_response(response.content, response.status_code)
        
    except Exception as e:
        logger.error(f"Error getting NGSI-LD entity {entity_id}: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/entities', methods=['POST'])
def create_ngsi_ld_entity():
    """Create a new entity in the NGSI-LD broker"""
    try:
        payload = request.json
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/entities"
        # Set Content-Type based on presence of @context
        if payload and ('@context' in payload or "@context" in payload):
            content_type = 'application/ld+json'
        else:
            content_type = 'application/json'
        headers = {
            'Content-Type': content_type,
            'Accept': 'application/json',
        }
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id:
            headers['NGSILD-Tenant'] = tenant_id
        response = requests.post(target_url, headers=headers, json=payload)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error creating NGSI-LD entity: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['PUT'])
def update_ngsi_ld_entity(entity_id):
    """Replace an entity in the NGSI-LD broker"""
    try:
        payload = request.json
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/entities/{entity_id}"
        headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
        }
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id:
            headers['NGSILD-Tenant'] = tenant_id
        response = requests.put(target_url, headers=headers, json=payload)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error replacing NGSI-LD entity: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['PATCH'])
def patch_ngsi_ld_entity(entity_id):
    """Update an entity in the NGSI-LD broker (partial update)"""
    try:
        payload = request.json
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/entities/{entity_id}"
        # Set Content-Type based on presence of @context
        if payload and ('@context' in payload or "@context" in payload):
            content_type = 'application/ld+json'
        else:
            content_type = 'application/merge-patch+json'
        headers = {
            'Content-Type': content_type,
            'Accept': 'application/json',
        }
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id:
            headers['NGSILD-Tenant'] = tenant_id
        logger.debug(f"Patching NGSI-LD entity {entity_id} at {target_url} with payload: {payload}")    
        logger.debug(f"Headers: {headers}")
        response = requests.patch(target_url, headers=headers, json=payload)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error patching NGSI-LD entity: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['DELETE'])
def delete_ngsi_ld_entity(entity_id):
    """Delete an entity in the NGSI-LD broker"""
    try:
        context_broker_url = os.environ.get('CONTEXT_BROKER_URL', 'http://orion.sensorsreport.net:31026')
        target_url = f"{context_broker_url}/ngsi-ld/v1/entities/{entity_id}"
        headers = {
            'Accept': 'application/json',
        }
        tenant_id = request.headers.get('NGSILD-Tenant')
        if tenant_id:
            headers['NGSILD-Tenant'] = tenant_id
        response = requests.delete(target_url, headers=headers)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error deleting NGSI-LD entity: {str(e)}")
        return jsonify({'error': str(e)}), 500

# Quantum Lead API proxy routes
@app.route('/api/quantum/version', methods=['GET'])
def quantum_version():
    """Get Quantum Lead version"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/version"
        response = requests.get(target_url)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead version: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/quantum/health', methods=['GET'])
def quantum_health():
    """Get Quantum Lead health status"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/health"
        response = requests.get(target_url)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead health: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/quantum/v2/entities', methods=['GET'])
def quantum_entities():
    """Get list of entities from Quantum Lead"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/v2/entities"
        headers = {}
        
        # Handle tenant headers
        tenant_id = request.headers.get('Fiware-Service')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['Fiware-Service'] = tenant_id
            headers['Fiware-ServicePath'] = request.headers.get('Fiware-ServicePath', '/')
        
        # Forward all query parameters
        params = request.args.to_dict()
        
        logger.debug(f"Making request to Quantum Leap: {target_url}")
        logger.debug(f"Headers: {headers}")
        logger.debug(f"Params: {params}")
        
        response = requests.get(target_url, headers=headers, params=params)
        
        # Log response details
        logger.debug(f"Quantum Leap response status: {response.status_code}")
        logger.debug(f"Quantum Leap response headers: {dict(response.headers)}")
        
        if response.status_code != 200:
            logger.error(f"Quantum Leap error response: {response.text}")
            return jsonify({
                'error': 'Error from Quantum Leap service',
                'status_code': response.status_code,
                'details': response.text
            }), response.status_code
            
        # Try to parse JSON response to validate it
        try:
            json_response = response.json()
            return jsonify(json_response)
        except ValueError as e:
            logger.error(f"Invalid JSON response from Quantum Leap: {response.text}")
            return jsonify({
                'error': 'Invalid JSON response from Quantum Leap',
                'details': str(e)
            }), 500
            
    except requests.exceptions.RequestException as e:
        error_msg = f"Error connecting to Quantum Leap: {str(e)}"
        logger.error(error_msg)
        return jsonify({'error': error_msg}), 500
    except Exception as e:
        error_msg = f"Unexpected error getting Quantum Leap entities: {str(e)}"
        logger.error(error_msg)
        return jsonify({'error': error_msg}), 500

@app.route('/api/quantum/v2/types', methods=['GET'])
def quantum_types():
    """Get entity types from Quantum Lead"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/v2/types"
        headers = {}
        
        # Handle tenant headers
        tenant_id = request.headers.get('Fiware-Service')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['Fiware-Service'] = tenant_id
            headers['Fiware-ServicePath'] = request.headers.get('Fiware-ServicePath', '/')
        
        response = requests.get(target_url, headers=headers)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead types: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/quantum/v2/types/<entity_type>/attrs', methods=['GET'])
def quantum_type_attrs(entity_type):
    """Get attributes for a specific entity type from Quantum Lead"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/v2/types/{entity_type}/attrs"
        headers = {}
        
        # Handle tenant headers
        tenant_id = request.headers.get('Fiware-Service')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['Fiware-Service'] = tenant_id
            headers['Fiware-ServicePath'] = request.headers.get('Fiware-ServicePath', '/')
        
        response = requests.get(target_url, headers=headers)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead type attributes: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/quantum/v2/entities/<entity_id>/attrs/<attr_name>', methods=['GET'])
def quantum_entity_attr_values(entity_id, attr_name):
    """Get time series values for an entity attribute from Quantum Lead"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/v2/entities/{entity_id}/attrs/{attr_name}"
        headers = {}
        
        # Handle tenant headers
        tenant_id = request.headers.get('Fiware-Service')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['Fiware-Service'] = tenant_id
            headers['Fiware-ServicePath'] = request.headers.get('Fiware-ServicePath', '/')
        
        # Forward all query parameters
        params = request.args.to_dict()
        
        response = requests.get(target_url, headers=headers, params=params)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead entity attribute values: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/quantum/v2/entities/<entity_id>/attrs/<attr_name>/value', methods=['GET'])
def quantum_entity_attr_last_value(entity_id, attr_name):
    """Get last value for an entity attribute from Quantum Lead"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/v2/entities/{entity_id}/attrs/{attr_name}/value"
        headers = {}
        
        # Handle tenant headers
        tenant_id = request.headers.get('Fiware-Service')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['Fiware-Service'] = tenant_id
            headers['Fiware-ServicePath'] = request.headers.get('Fiware-ServicePath', '/')
        
        response = requests.get(target_url, headers=headers)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead entity attribute last value: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/quantum/v2/entities/<entity_id>/attrs', methods=['GET'])
def quantum_entity_attrs(entity_id):
    """Get all attributes for an entity from Quantum Lead"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/v2/entities/{entity_id}/attrs"
        headers = {}
        
        # Handle tenant headers
        tenant_id = request.headers.get('Fiware-Service')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['Fiware-Service'] = tenant_id
            headers['Fiware-ServicePath'] = request.headers.get('Fiware-ServicePath', '/')
        
        params = request.args.to_dict()
        
        response = requests.get(target_url, headers=headers, params=params)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead entity attributes: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/quantum/v2/entities/<entity_id>', methods=['GET'])
def quantum_entity_values(entity_id):
    """Get all values for all attributes of an entity from Quantum Lead"""
    try:
        target_url = f"{QUANTUM_LEAP_CONFIG['base_url']}/v2/entities/{entity_id}"
        headers = {}
        
        # Handle tenant headers
        tenant_id = request.headers.get('Fiware-Service')
        if tenant_id and tenant_id.lower() != 'default' and tenant_id != 'Synchro':
            headers['Fiware-Service'] = tenant_id
            headers['Fiware-ServicePath'] = request.headers.get('Fiware-ServicePath', '/')
        
        params = request.args.to_dict()
        
        response = requests.get(target_url, headers=headers, params=params)
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error getting Quantum Lead entity values: {str(e)}")
        return jsonify({'error': str(e)}), 500

# Add CORS configuration for Quantum Lead endpoints
@app.route('/api/quantum/v2/entities', methods=['OPTIONS'])
@app.route('/api/quantum/v2/types', methods=['OPTIONS'])
@app.route('/api/quantum/v2/types/<path:subpath>', methods=['OPTIONS'])
@app.route('/api/quantum/v2/entities/<path:subpath>', methods=['OPTIONS'])
def quantum_options(subpath=None):
    """Handle OPTIONS requests for Quantum Lead endpoints (CORS preflight)"""
    response = make_response()
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type, Authorization, NGSILD-Tenant, Fiware-Service, Fiware-ServicePath')
    response.headers.add('Access-Control-Allow-Methods', 'GET, OPTIONS')
    return response

# Routes for authentication
@app.route('/api/auth/login')
def login():
    """
    Start the authentication flow by redirecting to Keycloak
    """
    logger.debug("Starting login flow")
    state = str(uuid.uuid4())
    session['oauth_state'] = state

    # Use REDIRECT_URI from env if set, else default
    redirect_uri = REDIRECT_URI or (request.url_root.rstrip('/') + '/api/auth/callback')
    auth_params = {
        'client_id': KEYCLOAK_CONFIG['client_id'],
        'redirect_uri': redirect_uri,
        'response_type': 'code',
        'scope': 'openid profile email',
        'state': state
    }
    auth_url = f"{KEYCLOAK_CONFIG['auth_url']}?{urlencode(auth_params)}"
    logger.debug(f"Redirecting to auth URL: {auth_url}")
    return redirect(auth_url)

@app.route('/api/auth/callback')
def callback():
    """
    Handle the callback from Keycloak after authentication
    """
    logger.debug("Auth callback received")
    state = request.args.get('state')
    stored_state = session.get('oauth_state')
    if not state or state != stored_state:
        logger.error(f"State mismatch: received {state}, stored {stored_state}")
        return jsonify({'error': 'Invalid state parameter'}), 400
    code = request.args.get('code')
    if not code:
        logger.error("No authorization code provided")
        return jsonify({'error': 'No authorization code provided'}), 400
    # Use REDIRECT_URI from env if set, else default
    redirect_uri = REDIRECT_URI or (request.url_root.rstrip('/') + '/api/auth/callback')
    token_data = {
        'grant_type': 'authorization_code',
        'client_id': KEYCLOAK_CONFIG['client_id'],
        'client_secret': KEYCLOAK_CONFIG['client_secret'],
        'code': code,
        'redirect_uri': redirect_uri
    }
    try:
        # Get tokens from Keycloak
        logger.debug(f"Exchanging code for token at: {KEYCLOAK_CONFIG['token_url']}")
        response = requests.post(KEYCLOAK_CONFIG['token_url'], data=token_data)
        
        if response.status_code != 200:
            logger.error(f"Token retrieval failed: {response.status_code} {response.text}")
            return jsonify({'error': f'Failed to retrieve token: {response.text}'}), 400
        
        tokens = response.json()
        logger.debug("Successfully retrieved tokens")
        
        # Store tokens in secure HTTP-only cookies
        resp = make_response(redirect(get_frontend_url()))
        
        # Clear any logged_out cookie if it exists
        resp.delete_cookie('logged_out', path='/')
        
        # Get secure cookie setting from environment variable
        secure_cookies_str = os.environ.get('SECURE_COOKIES', 'false').lower()
        secure_cookies = secure_cookies_str in ('true', 't', '1', 'yes')
        # Set access token in cookie
        resp.set_cookie(
            'access_token',
            tokens['access_token'],
            httponly=True,
            secure=secure_cookies,
            max_age=tokens['expires_in'],
            samesite='Lax',
            path='/'
        )
        
        # Store refresh token in a cookie
        resp.set_cookie(
            'refresh_token',
            tokens['refresh_token'],
            httponly=True,
            secure=secure_cookies,
            max_age=tokens['expires_in'] * 2,  # Longer expiry for refresh token
            samesite='Lax',
            path='/'
        )
        
        # Extract user info from token for session
        try:
            payload = tokens['access_token'].split('.')[1]
            # Add padding if needed
            payload += '=' * ((4 - len(payload) % 4) % 4)
            decoded_payload = base64.b64decode(payload)
            token_data = json.loads(decoded_payload)
            
            # Extract TenantId from token data with enhanced handling
            tenant = 'Default'
            
            # Log a sanitized version of the token data for debugging
            logger.debug("Token payload received and parsed successfully")
            logger.debug(f"Token contains fields: {', '.join(token_data.keys())}")
            
            # Check all possible variations of tenant field
            if 'TenantId' in token_data:
                if isinstance(token_data['TenantId'], list) and token_data['TenantId']:
                    tenant = token_data['TenantId'][0]
                    logger.debug(f"Using TenantId (array) from token: {tenant}")
                elif isinstance(token_data['TenantId'], str):
                    tenant = token_data['TenantId']
                    logger.debug(f"Using TenantId (string) from token: {tenant}")
            elif 'tenant_id' in token_data:
                tenant = token_data['tenant_id']
                logger.debug(f"Using tenant_id from token: {tenant}")
            elif 'tenantId' in token_data:
                tenant = token_data['tenantId']
                logger.debug(f"Using tenantId from token: {tenant}")
            elif 'Tenant' in token_data:
                tenant = token_data['Tenant']
                logger.debug(f"Using Tenant from token: {tenant}")
            elif 'tenant' in token_data:
                tenant = token_data['tenant']
                logger.debug(f"Using tenant from token: {tenant}")
            else:
                logger.debug("No tenant information found in token, using default")
            
            # Store user info in session
            session['user'] = {
                'username': token_data.get('preferred_username', 'unknown_user'),
                'tenant': tenant
            }
        except Exception as e:
            logger.exception("Error extracting user info from token")
        
        logger.debug(f"Redirecting to frontend: {get_frontend_url()}")
        return resp
    
    except Exception as e:
        logger.exception("Error in callback processing")
        return jsonify({'error': f'Error processing callback: {str(e)}'}), 500

@app.route('/api/auth/logout')
def logout():
    """
    Handle logout by clearing cookies and session, and invalidating the token on Keycloak
    Redirects directly to Keycloak login page after logout.
    """
    logger.debug("Processing logout")
    # Get the tokens from cookies
    access_token = request.cookies.get('access_token')
    refresh_token = request.cookies.get('refresh_token')
    # First invalidate the token on Keycloak (if we have a refresh token)
    if refresh_token:
        try:
            logout_data = {
                'client_id': KEYCLOAK_CONFIG['client_id'],
                'client_secret': KEYCLOAK_CONFIG['client_secret'],
                'refresh_token': refresh_token
            }
            logger.debug(f"Sending logout request to Keycloak at: {KEYCLOAK_CONFIG['logout_url']}")
            requests.post(KEYCLOAK_CONFIG['logout_url'], data=logout_data)
            logger.debug("Logout request sent to Keycloak")
        except Exception as e:
            logger.exception("Error during Keycloak logout")
    session.clear()
    # Build Keycloak login URL (same as /api/auth/login)
    state = str(uuid.uuid4())
    session['oauth_state'] = state
    auth_params = {
        'client_id': KEYCLOAK_CONFIG['client_id'],
        'redirect_uri': request.url_root.rstrip('/') + '/api/auth/callback',
        'response_type': 'code',
        'scope': 'openid profile email',
        'state': state
    }
    auth_url = f"{KEYCLOAK_CONFIG['auth_url']}?{urlencode(auth_params)}"
    logger.debug(f"Redirecting to Keycloak login after logout: {auth_url}")
    resp = make_response(redirect(auth_url))
    # Clear cookies
    resp.set_cookie('access_token', '', expires=0, path='/', domain=None, secure=False, httponly=True, max_age=0)
    resp.set_cookie('refresh_token', '', expires=0, path='/', domain=None, secure=False, httponly=True, max_age=0)
    resp.delete_cookie('access_token', path='/')
    resp.delete_cookie('refresh_token', path='/')
    resp.headers['Cache-Control'] = 'no-cache, no-store, must-revalidate'
    resp.headers['Pragma'] = 'no-cache'
    resp.headers['Expires'] = '0'
    resp.set_cookie('logged_out', 'true', max_age=60, path='/', secure=False, httponly=False, samesite='Lax')
    return resp

def decode_jwt_token(token):
    try:
        # Decode without verification (for info only)
        payload = token.split('.')[1]
        payload += '=' * ((4 - len(payload) % 4) % 4)
        decoded_payload = base64.b64decode(payload)
        return json.loads(decoded_payload)
    except Exception as e:
        logger.error(f"Error decoding JWT: {e}")
        return None

@app.route('/api/auth/token-details', methods=['GET'])
def token_details():
    access_token = request.cookies.get('access_token')
    user = session.get('user')
    if not access_token:
        return jsonify({'authenticated': False, 'error': 'No access token'}), 401
    token_info = decode_jwt_token(access_token)
    if not token_info:
        return jsonify({'authenticated': False, 'error': 'Invalid token'}), 401
    # Optionally, you could verify the token signature here if you have the public key
    return jsonify({
        'authenticated': True,
        'token': token_info,
        'user': user,
        'source': 'cookie'
    })

@app.route('/api/auth/user', methods=['GET'])
def auth_user():
    user = session.get('user')
    access_token = request.cookies.get('access_token')
    if user and access_token:
        return jsonify({'authenticated': True, 'user': user})
    return jsonify({'authenticated': False, 'user': None}), 401

@app.route('/api/notifications', methods=['POST'])
def receive_notification():
    """
    Receive and log notifications from Orion-LD context broker
    """
    try:
        # Log the full notification
        logger.info("Received notification from Orion-LD")
        logger.debug("Notification headers:")
        for header, value in request.headers.items():
            logger.debug(f"  {header}: {value}")
        
        # Get the notification data
        notification = request.json
        logger.debug("Notification body:")
        logger.debug(json.dumps(notification, indent=2))

        # Extract and log specific notification details
        if notification.get('data'):
            for entity in notification['data']:
                entity_id = entity.get('id')
                entity_type = entity.get('type')
                logger.info(f"Entity Update - ID: {entity_id}, Type: {entity_type}")
                
                # Log changed attributes
                for attr_name, attr_value in entity.items():
                    if attr_name not in ['id', 'type']:
                        logger.info(f"  Changed attribute: {attr_name} = {json.dumps(attr_value)}")
            
            # Add notification to queue for SSE clients
            notification_queue.put(notification)

        return jsonify({'status': 'success', 'message': 'Notification received and logged'}), 200
    
    except Exception as e:
        logger.error(f"Error processing notification: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/api/notifications/stream')
def notification_stream():
    """
    Server-Sent Events endpoint for streaming notifications to clients
    """
    def generate():
        while True:
            # Get notification from queue (blocking)
            notification = notification_queue.get()
            
            # Format as SSE
            data = f"data: {json.dumps(notification)}\n\n"
            yield data
            
            # Mark task as done
            notification_queue.task_done()

    return Response(generate(), mimetype='text/event-stream')

@app.route('/api/dataProducts', methods=['GET'])
def get_all_data_products():
    logger.info("GET /api/dataProducts called")
    try:
        target_url = f"{DATA_PRODUCT_URL}/dataProducts"
        logger.debug(f"Forwarding GET to {target_url}")
        response = requests.get(target_url, headers={'Accept': 'application/json'})
        logger.debug(f"Response: {response.status_code} {response.text[:200]}...")
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error fetching all data products: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/dataProducts/<data_product_id>', methods=['GET'])
def get_data_product(data_product_id):
    logger.info(f"GET /api/dataProducts/{data_product_id} called")
    try:
        target_url = f"{DATA_PRODUCT_URL}/dataProducts/{data_product_id}"
        logger.debug(f"Forwarding GET to {target_url}")
        response = requests.get(target_url, headers={'Accept': 'application/json'})
        logger.debug(f"Response: {response.status_code} {response.text[:200]}...")
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error fetching data product {data_product_id}: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/dataProducts', methods=['POST'])
def create_data_product():
    logger.info("POST /api/dataProducts called")
    try:
        # Accept both JSON and multipart/form-data
        if request.content_type and request.content_type.startswith('multipart/form-data'):
            files = {}
            data = {}
            for key in request.form:
                data[key] = request.form[key]
            for file_key in request.files:
                files[file_key] = request.files[file_key]
            logger.debug(f"Forwarding multipart POST to {DATA_PRODUCT_URL}/dataProducts with data: {list(data.keys())} and files: {list(files.keys())}")
            response = requests.post(f"{DATA_PRODUCT_URL}/dataProducts", data=data, files=files)
        else:
            payload = request.get_json(force=True)
            logger.debug(f"Forwarding JSON POST to {DATA_PRODUCT_URL}/dataProducts with payload: {json.dumps(payload)[:200]}...")
            response = requests.post(f"{DATA_PRODUCT_URL}/dataProducts", json=payload, headers={'Content-Type': 'application/json', 'Accept': 'application/json'})
        logger.debug(f"Response: {response.status_code} {response.text[:200]}...")
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error creating data product: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/dataProducts', methods=['DELETE'])
def delete_all_data_products():
    logger.info("DELETE /api/dataProducts called")
    try:
        target_url = f"{DATA_PRODUCT_URL}/dataProducts"
        logger.debug(f"Forwarding DELETE to {target_url}")
        response = requests.delete(target_url, headers={'Accept': 'application/json'})
        logger.debug(f"Response: {response.status_code} {response.text[:200]}...")
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error deleting all data products: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/dataProducts/<data_product_id>', methods=['DELETE'])
def delete_data_product(data_product_id):
    logger.info(f"DELETE /api/dataProducts/{data_product_id} called")
    try:
        target_url = f"{DATA_PRODUCT_URL}/dataProducts/{data_product_id}"
        logger.debug(f"Forwarding DELETE to {target_url}")
        response = requests.delete(target_url, headers={'Accept': 'application/json'})
        logger.debug(f"Response: {response.status_code} {response.text[:200]}...")
        return make_response(response.content, response.status_code)
    except Exception as e:
        logger.error(f"Error deleting data product {data_product_id}: {str(e)}")
        return jsonify({'error': str(e)}), 500

# Health check endpoint
@app.route('/health')
def health_check():
    return jsonify({"status": "ok", "version": "1.0.0"})

# Add endpoint to return redirect_uri
@app.route('/api/auth/redirect-uri', methods=['GET'])
def get_redirect_uri():
    """
    Return the redirect_uri used for Keycloak authentication
    """
    redirect_uri = REDIRECT_URI or (request.url_root.rstrip('/') + '/api/auth/callback')
    return jsonify({'redirect_uri': redirect_uri})

if __name__ == '__main__':
    # Get configuration from environment variables
    host = os.environ.get('HOST', '0.0.0.0')
    port = int(os.environ.get('PORT', '5000'))
    debug_mode = os.environ.get('DEBUG', 'false').lower() in ('true', 't', '1', 'yes')
    
    app.run(host=host, port=port, debug=debug_mode)