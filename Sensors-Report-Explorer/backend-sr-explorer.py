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
import secrets
sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from keycloak_auth import KeycloakAuth

# =====================
# Global Configuration
# =====================

# Environment variables (with defaults)
LOG_LEVEL = os.environ.get('LOG_LEVEL', 'DEBUG').upper()
SECRET_KEY = os.environ.get('SECRET_KEY', 'dev-secret-key')  # Change in production
CORS_ORIGINS = os.environ.get('CORS_ORIGINS', '*')
QUANTUM_LEAP_URL = os.environ.get('QUANTUM_LEAP_URL', 'http://162.244.27.122:8668')
DATA_PRODUCT_URL = os.environ.get('DATA_PRODUCT_URL', 'http://data-product-manager:8000')
CONTEXT_BROKER_URL = os.environ.get('CONTEXT_BROKER_URL', 'http://orion-ld-broker:1026')
SECURE_COOKIES = os.environ.get('SECURE_COOKIES', 'false').lower() in ('true', 't', '1', 'yes')
HOST = os.environ.get('HOST', '0.0.0.0')
PORT = int(os.environ.get('PORT', '5000'))
DEBUG_MODE = os.environ.get('DEBUG', 'true').lower() in ('true', 't', '1', 'yes')
# Quantum Lead configuration from environment variables
# QUANTUM_LEAP_CONFIG = {
#     'base_url': os.environ.get('QUANTUM_LEAP_URL', 'http://quantumleap:8668')
# }

# Add new environment variables for authentication
JWT_SECRET = os.environ.get('JWT_SECRET', 'default-secret')
AUTH_API_URL = os.environ.get('AUTH_API_URL', 'http://localhost:5000')

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

# Health check endpoint
@app.route('/health')
def health_check():
    return jsonify({"status": "ok", "version": "1.0.0"})

# Read backend version from version.txt and log it at startup
BACKEND_VERSION = None
version_file_path = os.path.join(os.path.dirname(__file__), 'version.txt')
try:
    with open(version_file_path) as vf:
        BACKEND_VERSION = vf.read().strip()
    logger.info(f"Sensors-Report-Explorer Backend Version: {BACKEND_VERSION}")
except Exception as e:
    logger.warning(f"Could not read backend version.txt: {e}")

# Import blueprints from endpoints.py
from endpoints import blueprints

# Register blueprints
for blueprint in blueprints:
    app.register_blueprint(blueprint)


# Create new routes for login, logout, and token validation
@app.route('/api/auth/login', methods=['GET'])
def login():
    """Redirect the browser to the Keycloak login page"""
    try:
        # Generate a unique state parameter for CSRF protection
        state = secrets.token_urlsafe(16)

        # Build the Keycloak login URL with OIDC parameters
        keycloak = KeycloakAuth()
        auth_url = f"{keycloak.auth_url}?client_id={keycloak.client_id}&response_type=code&redirect_uri={keycloak.redirect_uri}&state={state}&scope=openid profile email"
        logger.debug(f"Redirecting to Keycloak login URL: {auth_url}")
        # Redirect the browser to the Keycloak login page
        return redirect(auth_url)
    except Exception as e:
        logger.error(f"Failed to redirect to Keycloak login: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/auth/logout', methods=['GET'])
def logout():
    keycloak = KeycloakAuth()
    logout_url = keycloak.logout_url
    # Clear session data
    session.clear()
    # Redirect to Keycloak logout URL
    logger.debug(f"Redirecting to Keycloak logout URL: {logout_url}")
    return redirect(logout_url)

@app.route('/api/auth/validate', methods=['POST'])
def validate_token():
    """Validate a JWT token"""
    try:
        data = request.json
        token = data.get('token')
        if not token:
            return jsonify({'error': 'Token is required'}), 400

        # Validate token using Keycloak
        keycloak = KeycloakAuth()
        user_info = keycloak.validate_token(token)
        return jsonify({'user_info': user_info}), 200
    except Exception as e:
        logger.error(f"Token validation failed: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/api/auth/user', methods=['GET'])
def get_user_info():
    """Retrieve user information based on the authentication token"""
    try:
        token = request.headers.get('Authorization')
        if not token:
            return jsonify({'error': 'Authorization token is required'}), 401

        # Validate token and retrieve user info
        keycloak = KeycloakAuth()
        user_info = keycloak.validate_token(token.split(' ')[1])
        return jsonify({'user_info': user_info}), 200
    except Exception as e:
        logger.error(f"Failed to retrieve user info: {str(e)}")
        return jsonify({'error': str(e)}), 500


if __name__ == '__main__':
    # Get configuration from environment variables
    host = os.environ.get('HOST', '0.0.0.0')
    port = int(os.environ.get('PORT', '5000'))
    debug_mode = os.environ.get('DEBUG', 'false').lower() in ('true', 't', '1', 'yes')
    
    app.run(host=host, port=port, debug=debug_mode)