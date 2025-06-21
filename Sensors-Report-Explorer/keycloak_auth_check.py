import requests
import base64
import hashlib
import os
import secrets
import sys
import urllib.parse

# Configuration
KEYCLOAK_URL = 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect'
CLIENT_ID = 'ContextBroker'  # Change if needed
REDIRECT_URI = 'http://localhost:8765/callback'  # Dummy local URI for manual code copy

# PKCE helpers
def generate_code_verifier():
    return base64.urlsafe_b64encode(os.urandom(40)).rstrip(b'=').decode('utf-8')

def generate_code_challenge(verifier):
    digest = hashlib.sha256(verifier.encode('utf-8')).digest()
    return base64.urlsafe_b64encode(digest).rstrip(b'=').decode('utf-8')

# Step 1: Generate PKCE values
code_verifier = generate_code_verifier()
code_challenge = generate_code_challenge(code_verifier)

# Step 2: Build the authorization URL
params = {
    'client_id': CLIENT_ID,
    'response_type': 'code',
    'scope': 'openid profile email',
    'redirect_uri': REDIRECT_URI,
    'code_challenge': code_challenge,
    'code_challenge_method': 'S256',
    'state': secrets.token_urlsafe(16)
}
auth_url = f"{KEYCLOAK_URL}/auth?{urllib.parse.urlencode(params)}"

print('1. Open this URL in your browser and log in:')
print(auth_url)
print('\n2. After login, you will be redirected to a URL like:')
print(f'{REDIRECT_URI}?code=...&state=...')
print('Copy the value of the "code" parameter and paste it below.')

code = input('Enter the authorization code: ').strip()

# Step 3: Exchange code for tokens
print('\nExchanging code for tokens...')
token_data = {
    'grant_type': 'authorization_code',
    'client_id': CLIENT_ID,
    'code': code,
    'redirect_uri': REDIRECT_URI,
    'code_verifier': code_verifier
}

response = requests.post(f'{KEYCLOAK_URL}/token', data=token_data)

if response.status_code == 200:
    tokens = response.json()
    print('\nAuthentication successful!')
    print('Access Token:', tokens.get('access_token', '[none]')[:80] + '...')
    print('ID Token:', tokens.get('id_token', '[none]')[:80] + '...')
else:
    print('\nAuthentication failed!')
    print('Status:', response.status_code)
    print('Response:', response.text)
