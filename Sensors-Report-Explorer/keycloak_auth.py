import requests
import os
import logging
from flask import Flask, request, jsonify, session, redirect

logging.basicConfig(level=logging.DEBUG)
logger = logging.getLogger(__name__)

app = Flask(__name__)
app.secret_key = os.environ.get('FLASK_SECRET_KEY', 'your-secret-key')

class KeycloakAuth:
    def __init__(self):
        self.auth_url = os.environ.get('KEYCLOAK_AUTH_URL', 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/auth')
        self.token_url = os.environ.get('KEYCLOAK_TOKEN_URL', 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/token')
        self.userinfo_url = os.environ.get('KEYCLOAK_USERINFO_URL', 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/userinfo')
        self.logout_url = os.environ.get('KEYCLOAK_LOGOUT_URL', 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/logout')
        self.client_id = os.environ.get('KEYCLOAK_CLIENT_ID', 'ContextBroker')
        self.client_secret = os.environ.get('KEYCLOAK_CLIENT_SECRET', '')
        self.redirect_uri = os.environ.get('REDIRECT_URI', 'https://explorer.sensorsreport.net/api/auth/callback')

    def get_token(self, username, password):
        """Get a JWT token for a user"""
        try:
            url = f"{self.server_url}/realms/{self.realm}/protocol/openid-connect/token"
            headers = {'Content-Type': 'application/x-www-form-urlencoded'}
            data = {
                'grant_type': 'password',
                'client_id': self.client_id,
                'client_secret': self.client_secret,
                'username': username,
                'password': password
            }
            response = requests.post(url, headers=headers, data=data)
            response.raise_for_status()
            token_info = response.json()
            return token_info['access_token']
        except Exception as e:
            raise Exception(f"Failed to obtain token from Keycloak: {str(e)}")

    def validate_token(self, token):
        """Validate a JWT token"""
        try:
            url = f"{self.server_url}/realms/{self.realm}/protocol/openid-connect/token/introspect"
            headers = {'Content-Type': 'application/x-www-form-urlencoded'}
            data = {'token': token}
            response = requests.post(url, headers=headers, data=data)
            response.raise_for_status()
            token_info = response.json()
            return token_info
        except Exception as e:
            raise Exception(f"Failed to validate token with Keycloak: {str(e)}")
    @app.route('/api/auth/callback', methods=['GET'])
    def keycloak_callback():
        logger.debug('Received callback request from Keycloak')

        # Extract query parameters
        code = request.args.get('code')
        state = request.args.get('state')

        logger.debug(f'Received code: {code}')
        logger.debug(f'Received state: {state}')

        if not code or not state:
            logger.error('Missing code or state in callback request')
            return jsonify({'error': 'Missing code or state'}), 400

        try:
            # Exchange authorization code for tokens
            keycloak = KeycloakAuth()
            token_url = keycloak.token_url
            headers = {'Content-Type': 'application/x-www-form-urlencoded'}
            data = {
                'grant_type': 'authorization_code',
                'client_id': keycloak.client_id,
                'client_secret': keycloak.client_secret,
                'code': code,
                'redirect_uri': keycloak.redirect_uri
            }

            logger.debug(f'Sending token request to {token_url} with data: {data}')
            response = requests.post(token_url, headers=headers, data=data)
            response.raise_for_status()

            token_response = response.json()
            logger.debug(f'Token response: {token_response}')

            # Store tokens in session
            session['access_token'] = token_response.get('access_token')
            session['refresh_token'] = token_response.get('refresh_token')
            session['expires_in'] = token_response.get('expires_in')

            # Redirect to index.html on success, including the code in the URL as a query parameter
            return redirect(f"/index.html?code={code}&state={state}")
    
        except requests.exceptions.RequestException as e:
            logger.error(f'Error during token exchange: {str(e)}')
            return jsonify({'error': 'Token exchange failed', 'details': str(e)}), 500
        except Exception as e:
            logger.error(f'Unexpected error: {str(e)}')
            return jsonify({'error': 'Unexpected error occurred', 'details': str(e)}), 500

    @app.route('/api/auth/token', methods=['GET', 'POST'])
    def get_token():
        """Retrieve the access token securely"""
        try:
            # Ensure the session contains the access token
            access_token = session.get('access_token')
            if not access_token:
                return jsonify({'error': 'No access token found in session'}), 401

            # Return the access token securely
            return jsonify({'access_token': access_token}), 200
        except Exception as e:
            logger.error(f"Failed to retrieve access token: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @app.route('/api/auth/refresh', methods=['GET', 'POST'])
    def refresh_token():
        """Refresh the access token using the refresh token"""
        try:
            # Ensure the session contains the refresh token
            refresh_token = session.get('refresh_token')
            if not refresh_token:
                return jsonify({'error': 'No refresh token found in session'}), 401

            # Exchange the refresh token for a new access token
            keycloak = KeycloakAuth()
            token_url = keycloak.token_url
            headers = {'Content-Type': 'application/x-www-form-urlencoded'}
            data = {
                'grant_type': 'refresh_token',
                'client_id': keycloak.client_id,
                'client_secret': keycloak.client_secret,
                'refresh_token': refresh_token
            }

            response = requests.post(token_url, headers=headers, data=data)
            response.raise_for_status()

            token_response = response.json()
            logger.debug(f"Token refresh response: {token_response}")

            # Update session with new tokens
            session['access_token'] = token_response.get('access_token')
            session['refresh_token'] = token_response.get('refresh_token')
            session['expires_in'] = token_response.get('expires_in')

            return jsonify({'access_token': token_response.get('access_token')}), 200
        except requests.exceptions.RequestException as e:
            logger.error(f"Error during token refresh: {str(e)}")
            return jsonify({'error': 'Token refresh failed', 'details': str(e)}), 500
        except Exception as e:
            logger.error(f"Unexpected error: {str(e)}")
            return jsonify({'error': 'Unexpected error occurred', 'details': str(e)}), 500
