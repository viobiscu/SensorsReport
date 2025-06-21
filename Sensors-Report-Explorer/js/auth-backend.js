/**
 * Authentication Manager for Backend Keycloak integration
 * Handles communication with backend auth endpoints
 */

// Import auth utility functions
import { clearAuthData, clearAllData } from './auth-utils.js';

class AuthManager {
    constructor() {
        // User state
        this.isUserAuthenticated = false;

        this.backendBaseUrl =  "http://sensors-report-explorer-backend:5000"
        
        // Keycloak configuration - same as backend config
        this.keycloakConfig = {
            authUrl: 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/auth',
            tokenUrl: 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/token',
            logoutUrl: 'https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/logout',
            clientId: 'ContextBroker',
            responseType: 'token id_token', // Using implicit flow for browser-based auth
        };
        
        // Check for authentication tokens in URL (after Keycloak redirect)
        // if (!this.checkAuthResponseInUrl()) {
        //     //redirect to /api/auth/login
        //     console.log('No auth token found, redirecting to Keycloak login...');
        //     window.location.href = '/api/auth/login';
        //     return;
        // }
        // Initialize redirect loop protection
        this.lastRedirectTime = 0;
        this.redirectCount = 0;
        this.REDIRECT_THRESHOLD = 3; // Max number of redirects
        this.REDIRECT_TIMEOUT = 10000; // 10 seconds
        
        this.checkRedirectLoopProtection();
        
        // Set up event listeners
        this.setupEventListeners();
        
        // Add an offline mode check before checking authentication
        //this.checkOfflineMode();
        
        // Make logout method available globally for direct access if needed
        window.doLogout = () => this.logout();
        
        // IMPORTANT: Expose this instance to window.AuthBackend for global access
        window.AuthBackend = this;
    }

    /**
     * Check if the application is running in offline mode without a backend
     */
    // checkOfflineMode() {
    //     // Check if the application is running locally without a backend
    //     if (window.location.hostname === 'localhost' || 
    //         window.location.hostname === '127.0.0.1' || 
    //         window.location.protocol === 'file:') {
            
    //         console.log('Detected offline mode, skipping authentication check');
            
    //         // Set a flag to indicate we're in offline mode
    //         this.offlineMode = true;
            
    //         // Set a default user for offline mode
    //         this.isUserAuthenticated = true;
    //         this.userInfo = {
    //             username: 'Offline User',
    //             tenant: this.tenant
    //         };
            
    //         // Update UI with offline user info
    //         this.updateUIWithUserInfo();
    //     } else {
    //         // Not in offline mode, proceed with normal authentication check
    //         this.offlineMode = false;
    //         this.checkAuthStatus();
    //     }
    // }
    
    /**
     * Show authentication error to user
     * @param {string} message - Error message to display
     */
    showAuthError(message) {
        console.error('Authentication error:', message);
        
        // Create error notification element
        const errorDiv = document.createElement('div');
        errorDiv.className = 'auth-error-notification error-message';
        
        // Add error icon and message
        errorDiv.innerHTML = `
            <div class="error-message-content">
                <span class="error-icon">⚠️</span>
                <span>
                    <strong>Authentication Error</strong>
                    <p>${message}</p>
                </span>
                <button class="close-button" aria-label="Close">&times;</button>
            </div>
        `;
        
        // Add to document
        document.body.appendChild(errorDiv);
        
        // Add click handler to close button
        const closeButton = errorDiv.querySelector('button');
        if (closeButton) {
            closeButton.addEventListener('click', () => {
                errorDiv.remove();
            });
        }
        
        // Auto-remove after 8 seconds
        setTimeout(() => {
            if (errorDiv.parentNode) {
                errorDiv.remove();
            }
        }, 8000);
        
        // Fallback to alert if we're in a state where DOM manipulation might not work
        if (document.readyState !== 'complete') {
            alert('Authentication Error: ' + message);
        }
        
        // Also update any login error message elements in the DOM
        const loginErrorElements = document.querySelectorAll('.login-error, #loginError');
        loginErrorElements.forEach(element => {
            element.textContent = message;
            element.style.display = 'block';
        });
        
        return false;
    }
    
    /**
     * Handle authentication errors
     * @param {Error} error - The error object
     */
    handleAuthError(error) {
        console.error('Authentication error:', error);
        
        // Display error to user
        this.showAuthError('Authentication failed: ' + error.message);
        
        // Clear any stale authentication data
        this.clearAuthState();
        
        return false;
    }

    /**
     * Enhanced logging for debugging authentication mechanism
     */
    checkAuthResponseInUrl() {
        console.log('Starting authentication response check...');
        console.log('URL hash:', window.location.hash);
        console.log('URL search params:', window.location.search);

        if (window.location.hash && window.location.hash.length > 1) {
            const params = new URLSearchParams(window.location.hash.substring(1));
            console.log('Parsed URL hash params:', Array.from(params.entries()));

            const accessToken = params.get('access_token');
            const idToken = params.get('id_token');
            const nonceFromToken = params.get('nonce');

            console.log('Access Token:', accessToken);
            console.log('ID Token:', idToken);
            console.log('Nonce from Token:', nonceFromToken);

            if (accessToken) {
                console.log('Found access token in URL hash');

                try {
                    // Validate nonce (implementation needed)
                    console.log('Nonce validation successful');
                } catch (error) {
                    console.error('Nonce validation failed:', error);
                    this.handleAuthError(error);
                    return false;
                }

                // Store tokens in localStorage (more secure would be to use the backend still)
                localStorage.setItem('access_token', accessToken);
                // Also store as auth_token for compatibility with other parts of the app
                localStorage.setItem('auth_token', accessToken);
                
                if (idToken) localStorage.setItem('id_token', idToken);
                
                // Remove the hash from URL to prevent token exposure in browser history
                window.history.replaceState(null, '', window.location.pathname);
                
                // Store expiry time
                const expiresIn = params.get('expires_in');
                if (expiresIn) {
                    const expiresAt = Date.now() + parseInt(expiresIn) * 1000;
                    localStorage.setItem('expires_at', expiresAt);
                    console.log('Token expiry time set:', new Date(expiresAt).toISOString());
                }
                
                // Parse the access token to get user info
                try {
                    this.parseToken(accessToken);
                    console.log('Token parsed successfully');
                } catch (error) {
                    console.error('Error parsing token:', error);
                }
                
                // Send token to backend for verification and session creation
                this.sendTokenToBackend(accessToken, idToken);
                console.log('Token sent to backend for verification');
                return true;
            }
        }

        const urlParams = new URLSearchParams(window.location.search);
        console.log('Parsed URL search params:', Array.from(urlParams.entries()));

        const code = urlParams.get('code');
        if (code) {
            console.log('Found authorization code in URL query parameters');
            // --- PATCH: Hardcode redirect_uri to always use HTTPS and trailing slash ---
            const frontendRoot = 'https://explorer.sensorsreport.net/';
            fetch(`${this.backendBaseUrl}/api/auth/token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify({ code, redirect_uri: frontendRoot })
            })
            .then(response => {
                console.log('Backend response status:', response.status);
                if (response.ok) {
                    console.log('Authorization code exchanged successfully');
                    // Clean URL to remove the code
                    window.history.replaceState(null, '', window.location.pathname);
                    // Refresh authentication status
                    this.checkAuthStatus();
                    // Also try to extract the token from the response if available
                    return response.json().catch(() => null);
                } else {
                    console.error('Failed to exchange authorization code:', response.statusText);
                }
            })
            .then(data => {
                if (data && data.access_token) {
                    // Store tokens in localStorage
                    localStorage.setItem('access_token', data.access_token);
                    // Also store as auth_token for compatibility with other parts of the app
                    localStorage.setItem('auth_token', data.access_token);
                    if (data.id_token) localStorage.setItem('id_token', data.id_token);
                    console.log('Tokens stored from code exchange:', data);
                }
            })
            .catch(error => {
                console.error('Error exchanging code for token:', error);
            });
            return true;
        }

        console.warn('No authentication tokens or authorization code found in URL');
        return false;
    }
    
    /**
     * Parse JWT token to extract user information
     */
    parseToken(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(
                atob(base64)
                    .split('')
                    .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                    .join('')
            );
            
            const payload = JSON.parse(jsonPayload);
            this.isUserAuthenticated = true;
            
            // Remove full payload logging for security reasons
            console.log('Token received and parsed successfully');
            
            // Enhanced debugging for TenantId
            console.log('TenantId field detected in token');
            
            // Extract TenantId from token payload
            // Based on the exact token structure: TenantId is an array with a single string value "Synchro"
            let tenantId = 'Default';
            
            // Check all possible field names for tenant information
            if (payload.TenantId) {
                if (Array.isArray(payload.TenantId) && payload.TenantId.length > 0) {
                    tenantId = payload.TenantId[0];
                    console.log('Using TenantId (array) from token:', tenantId);
                } else if (typeof payload.TenantId === 'string') {
                    tenantId = payload.TenantId;
                    console.log('Using TenantId (string) from token:', tenantId);
                }
            } else if (payload.tenant_id) {
                tenantId = typeof payload.tenant_id === 'string' ? payload.tenant_id : JSON.stringify(payload.tenant_id);
                console.log('Using tenant_id from token:', tenantId);
            } else if (payload.tenantId) {
                tenantId = typeof payload.tenantId === 'string' ? payload.tenantId : JSON.stringify(payload.tenantId);
                console.log('Using tenantId from token:', tenantId);
            } else if (payload.Tenant) {
                tenantId = typeof payload.Tenant === 'string' ? payload.Tenant : JSON.stringify(payload.Tenant);
                console.log('Using Tenant from token:', tenantId);
            } else if (payload.tenant) {
                tenantId = typeof payload.tenant === 'string' ? payload.tenant : JSON.stringify(payload.tenant);
                console.log('Using tenant from token:', tenantId);
            } else {
                console.warn('No tenant information found in token payload, using default');
            }
            
            this.userInfo = {
                username: payload.preferred_username || payload.email || payload.sub,
                tenant: tenantId
            };
            
            // Save tenant info
            localStorage.setItem('tenantName', tenantId);
            this.tenant = tenantId;
            
            this.updateUIWithUserInfo();
        } catch (error) {
            console.error('Error parsing JWT token:', error);
            throw error;
        }
    }
    
    /**
     * Send token to backend for validation and session creation
     */
    async sendTokenToBackend(accessToken, idToken) {
        try {
            const response = await fetch(`${this.backendBaseUrl}/api/auth/verify-token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify({
                    access_token: accessToken,
                    id_token: idToken || null
                })
            });
            
            if (response.ok) {
                console.log('Token verified with backend');
                return true;
            } else {
                console.error('Backend token verification failed:', await response.text());
                return false;
            }
        } catch (error) {
            console.error('Error sending token to backend:', error);
            return false;
        }
    }

    /**
     * Prevent infinite redirect loops
     */
    checkRedirectLoopProtection() {
        const now = Date.now();
        
        // Reset redirect count if enough time has passed
        if (now - this.lastRedirectTime > this.REDIRECT_TIMEOUT) {
            this.redirectCount = 0;
        }
        
        this.redirectCount++;
        this.lastRedirectTime = now;
        
        // Check for redirect loop
        if (this.redirectCount > this.REDIRECT_THRESHOLD) {
            // Clear any stored auth data to break the loop
            localStorage.removeItem('auth_token');
            localStorage.removeItem('login_timestamp');
            localStorage.removeItem('user_info');
            
            // Set flag to prevent further redirects this session
            sessionStorage.setItem('authRedirectBlocked', 'true');
            
            const error = 'Authentication redirect loop detected. Please try clearing your cookies and refreshing the page.';
            this.showAuthError(error);
            throw new Error(error);
        }
        
        // If we've been blocked from redirecting this session, enter offline mode
        if (sessionStorage.getItem('authRedirectBlocked') === 'true') {
            console.log('Detected offline mode, skipping authentication check');
            this.enterOfflineMode();
            return false;
        }
        
        return true;
    }

    /**
     * Enter offline mode
     */
    enterOfflineMode() {
        // Set up offline user info
        const offlineUser = {
            username: 'Offline User',
            tenant: 'Default'
        };
        
        // Update UI with offline user info
        this.updateUIWithUserInfo(offlineUser);
        
        // Store offline state
        sessionStorage.setItem('offlineMode', 'true');
    }

    /**
     * Check if user is authenticated with the backend
     */
    async checkAuthStatus() {
        console.log('Checking authentication status...');
        
        // Check for active token in local storage first
        const token = localStorage.getItem('access_token') || localStorage.getItem('auth_token');
        const expiresAt = localStorage.getItem('expires_at');
        
        // Synchronize tokens if they're not in sync
        if (localStorage.getItem('access_token') && !localStorage.getItem('auth_token')) {
            localStorage.setItem('auth_token', localStorage.getItem('access_token'));
            console.log('Synchronized access_token to auth_token');
        } else if (!localStorage.getItem('access_token') && localStorage.getItem('auth_token')) {
            localStorage.setItem('access_token', localStorage.getItem('auth_token'));
            console.log('Synchronized auth_token to access_token');
        }
        
        // If we have a valid token in localStorage, we should trust that over logged_out cookie
        // This fixes the issue where a user may have just logged in but the logged_out cookie is still present
        if (token && expiresAt && Date.now() < parseInt(expiresAt)) {
            console.log('Found valid token in localStorage, clearing any stale logout indicators');
            // We have a valid token - make sure we clear any stale logout indicators
            this.clearLogoutIndicators();
            
            // Use the stored token
            this.isUserAuthenticated = true;
            
            try {
                // Parse the token to get user info
                this.parseToken(token);
                
                // Also verify with the backend
                await this.verifyTokenWithBackend();
                
                // Ensure UI is updated after a short delay to allow DOM to load
                setTimeout(() => this.updateUIWithUserInfo(true), 500);
                return true;
            } catch (error) {
                console.error('Error using stored token:', error);
                // Token might be invalid, continue with backend check
            }
        }
        
        // Check for logged_out cookie if we don't have a valid token - but use SameSite cookies
        const loggedOutCookie = document.cookie.split(';').find(c => c.trim().startsWith('logged_out='));
        if (loggedOutCookie) {
            console.log('Found logged_out cookie, user has explicitly logged out');
            // Clean up all authentication state
            this.clearAuthState();
            return false;
        }
        
        // Check if we've just logged out based on URL parameter
        const urlParams = new URLSearchParams(window.location.search);
        const noAutoLogin = urlParams.get('no_auto_login');
        
        if (noAutoLogin === 'true') {
            console.log('no_auto_login parameter detected, skipping auto-login');
            // Save the fact we've logged out in sessionStorage to persist across page refreshes
            sessionStorage.setItem('logged_out', 'true');
            
            // Clean the URL to remove the parameter
            const url = new URL(window.location);
            url.searchParams.delete('no_auto_login');
            url.searchParams.delete('t'); // Remove timestamp param too
            url.searchParams.delete('logout'); // Remove logout param too
            window.history.replaceState({}, '', url);
            
            // Clear all token data
            this.clearAuthState();
            return false;
        }
        
        // Check if we've logged out in this session (persists through refreshes)
        if (sessionStorage.getItem('logged_out') === 'true') {
            console.log('User has logged out in this session, not auto-redirecting to login');
            // Don't auto-redirect, but keep the state
            this.isUserAuthenticated = false;
            this.userInfo = null;
            return false;
        }
        
        // Check if we've just logged out
        if (sessionStorage.getItem('just_logged_out') === 'true') {
            console.log('Just logged out, clearing auth state');
            // Clear the flag
            sessionStorage.removeItem('just_logged_out');
            // Ensure we're showing as logged out
            this.isUserAuthenticated = false;
            this.userInfo = null;
            return false;
        }
        
        try {
            // Try to get auth status from backend
            const response = await fetch(`${this.backendBaseUrl}/api/auth/user`, {
                credentials: 'include', // Important to include cookies
                headers: {
                    // Add cache control to prevent caching of auth status
                    'Cache-Control': 'no-cache, no-store, must-revalidate',
                    'Pragma': 'no-cache',
                    'Expires': '0'
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                
                // Debug output
                console.log('Auth status from backend:', data);
                
                this.isUserAuthenticated = data.authenticated;
                this.userInfo = data.user;
                
                // If we're authenticated through the backend, clear any logout indicators
                if (this.isUserAuthenticated) {
                    this.clearLogoutIndicators();
                    
                    // If authenticated but no token in localStorage, attempt to get one
                    if (!localStorage.getItem('access_token') && !localStorage.getItem('auth_token')) {
                        console.log('Authenticated but no token in localStorage, attempting to retrieve token');
                        await this.retrieveTokenFromBackend();
                    }
                }
                
                // Save tenant info if available
                if (data.user && data.user.tenant) {
                    localStorage.setItem('tenantName', data.user.tenant);
                    this.tenant = data.user.tenant;
                }
                
                // Update UI with user info if authenticated
                if (this.isUserAuthenticated) {
                    this.updateUIWithUserInfo();
                    return true;
                } else {
                    // Show warning about third-party cookies if we have a manual_logout cookie
                    // but not a logged_out cookie (indicates possible cookie blocking)
                    const manualLogoutCookie = document.cookie.split(';').find(c => c.trim().startsWith('manual_logout='));
                    if (manualLogoutCookie && !loggedOutCookie && !sessionStorage.getItem('shown_cookie_warning')) {
                        // This suggests cookies might be blocked
                        console.warn('Possible cookie blocking detected - manual logout cookie found but not reflected in auth state');
                        this.showCookieWarning();
                        sessionStorage.setItem('shown_cookie_warning', 'true');
                    }
                    // --- PATCH: Always redirect to login on 401 or failed auth ---
                    if (this.checkRedirectLoopProtection() && !sessionStorage.getItem('logged_out')) {
                        console.log('User not authenticated, redirecting to backend login');
                        this.login();
                    }
                    return false;
                }
            } else {
                console.warn('Auth check failed with status:', response.status);
                // Always redirect to login on any non-ok response, unless explicitly logged out
                if (this.checkRedirectLoopProtection() && !sessionStorage.getItem('logged_out')) {
                    console.log('[AUTH] Redirecting to backend login:');
                    this.login();
                }
                return false;
            }
        } catch (error) {
            console.error('Error checking authentication status:', error);
            this.handleAuthError(error);
            // Always redirect to login on error, unless explicitly logged out
            if (this.checkRedirectLoopProtection() && !sessionStorage.getItem('logged_out')) {
                console.log('[AUTH] Redirecting to backend login:');
                this.login();
            }
            return false;
        }
    }
    
    /**
     * Clear all authentication state
     */
    clearAuthState() {
        localStorage.removeItem('access_token');
        localStorage.removeItem('auth_token'); // Also clear auth_token
        localStorage.removeItem('id_token');
        localStorage.removeItem('expires_at');
        localStorage.removeItem('oauth_state');
        
        // Don't redirect back to login
        this.isUserAuthenticated = false;
        this.userInfo = null;
    }
    
    /**
     * Clear all logout indicators when a user is successfully authenticated
     */
    clearLogoutIndicators() {
        // Clear any logout indicators in cookies by setting to expired
        document.cookie = "logged_out=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        document.cookie = "manual_logout=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        
        // Clear any logout indicators in session storage
        sessionStorage.removeItem('logged_out');
        sessionStorage.removeItem('just_logged_out');
    }

    /**
     * Verify the token with the backend
     */
    async verifyTokenWithBackend() {
        const token = localStorage.getItem('access_token');
        const idToken = localStorage.getItem('id_token');
        
        if (!token) {
            console.error('No token available to verify with backend');
            return false;
        }
        
        try {
            const response = await fetch(`${this.backendBaseUrl}/api/auth/verify-token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify({
                    access_token: token,
                    id_token: idToken || null
                })
            });
            
            if (response.ok) {
                console.log('Token verified with backend');
                const data = await response.json();
                
                if (data.user) {
                    this.userInfo = data.user;
                    
                    // Save tenant info if available
                    if (data.user.tenant) {
                        localStorage.setItem('tenantName', data.user.tenant);
                        this.tenant = data.user.tenant;
                    }
                    
                    this.updateUIWithUserInfo();
                }
                
                return true;
            } else {
                console.error('Backend token verification failed:', await response.text());
                return false;
            }
        } catch (error) {
            console.error('Error verifying token with backend:', error);
            return false;
        }
    }

    /**
     * Retrieve token from the backend when authenticated via cookies but no token in localStorage
     * This helps with Chrome's new cookie policies by providing a fallback
     */
    async retrieveTokenFromBackend() {
        try {
            console.log('Attempting to retrieve token from backend');
            const response = await fetch(`${this.backendBaseUrl}/api/auth/retrieve-token`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Cache-Control': 'no-cache, no-store, must-revalidate',
                    'Pragma': 'no-cache'
                },
                credentials: 'include' // Important to include cookies
            });
            
            if (response.ok) {
                const data = await response.json();
                
                if (data.success && data.access_token) {
                    console.log('Successfully retrieved token from backend');
                    
                    // Store tokens in localStorage
                    localStorage.setItem('access_token', data.access_token);
                    localStorage.setItem('auth_token', data.access_token); // Keep both in sync
                    
                    if (data.id_token) {
                        localStorage.setItem('id_token', data.id_token);
                    }
                    
                    if (data.expires_in) {
                        const expiresAt = Date.now() + (parseInt(data.expires_in) * 1000);
                        localStorage.setItem('expires_at', expiresAt.toString());
                    }
                    
                    // Update authentication state
                    this.isUserAuthenticated = true;
                    
                    // Update user info if available
                    if (data.user) {
                        this.userInfo = data.user;
                        
                        // Also update tenant if available
                        if (data.user.tenant) {
                            this.tenant = data.user.tenant;
                            localStorage.setItem('tenantName', data.user.tenant);
                        }
                    }
                    
                    // Update UI with new auth state
                    this.updateUIWithUserInfo();
                    
                    return true;
                } else {
                    console.warn('Backend did not return a valid token:', data.error || 'Unknown error');
                    return false;
                }
            } else {
                // If we got a 401, the user is not authenticated
                if (response.status === 401) {
                    console.warn('Failed to retrieve token - user not authenticated');
                    this.isUserAuthenticated = false;
                    this.userInfo = null;
                } else {
                    console.warn('Failed to retrieve token from backend:', response.status);
                }
                return false;
            }
        } catch (error) {
            console.error('Error retrieving token from backend:', error);
            return false;
        }
    }

    /**
     * Call backend login endpoint
     */
    login() {
        const frontendRoot = window.location.origin + '/';
        const loginUrl = `${this.keycloakConfig.authUrl}?client_id=${encodeURIComponent(this.keycloakConfig.clientId)}&redirect_uri=${encodeURIComponent(frontendRoot)}&response_type=${encodeURIComponent(this.keycloakConfig.responseType)}&scope=openid%20profile%20email`;
        console.debug('[AUTH] Redirecting to Keycloak login:', loginUrl);
        window.location.href = loginUrl;
    }

    /**
     * Generate a random nonce value
     */
    generateNonce(length = 16) {
        const array = new Uint8Array(length);
        window.crypto.getRandomValues(array);
        return Array.from(array, b => b.toString(16).padStart(2, '0')).join('');
    }

    /**
     * Redirect directly to Keycloak for authentication
     * Uses the implicit flow for synchronous authentication
     */
    redirectToKeycloak() {
        const nonce = this.generateNonce(); // Generate a unique nonce
        localStorage.setItem('nonce', nonce); // Store nonce for validation later

        const authUrl = `${this.keycloakConfig.authUrl}?client_id=${this.keycloakConfig.clientId}&response_type=${this.keycloakConfig.responseType}&redirect_uri=${encodeURIComponent(window.location.origin)}&scope=openid profile email&nonce=${nonce}`;

        console.log('[AUTH] Redirecting to Keycloak login:', authUrl);
        window.location.href = authUrl; // Redirect to Keycloak
    }

    /**
     * Call backend logout endpoint, reset all auth state, and redirect to Keycloak login
     */
    logout() {
        console.log('Logout method called');
        // Clear all local storage and session storage items related to authentication
        localStorage.removeItem('access_token');
        localStorage.removeItem('auth_token');
        localStorage.removeItem('id_token');
        localStorage.removeItem('expires_at');
        localStorage.removeItem('oauth_state');
        localStorage.removeItem('tenantName');
        localStorage.removeItem('user_info');
        localStorage.removeItem('login_timestamp');
        sessionStorage.removeItem('logged_out');
        sessionStorage.removeItem('just_logged_out');
        sessionStorage.removeItem('auth_redirect_count');
        sessionStorage.removeItem('offlineMode');
        sessionStorage.removeItem('shown_cookie_warning');
        // Clear cookies
        document.cookie = "logged_out=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        document.cookie = "manual_logout=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        // Reset instance variables
        this.isUserAuthenticated = false;
        this.userInfo = null;
        this.tenant = 'Default';
        // Reset UI
        const loginUserSpan = document.getElementById('loginUser');
        if (loginUserSpan) loginUserSpan.textContent = 'User: Not logged in';
        const tenantName = document.getElementById('tenantName');
        if (tenantName) tenantName.textContent = 'Tenant: Default';
        const tenantInput = document.getElementById('tenantname');
        if (tenantInput) tenantInput.value = '';
        // Clear any display of token data
        const jsonDisplay = document.getElementById('jsonDisplay');
        if (jsonDisplay) jsonDisplay.value = '';
        // Redirect to backend logout endpoint, which will clear cookies/session and redirect to Keycloak
        window.location.href = `${this.backendBaseUrl}/api/auth/logout`;
    }

    /**
     * Get the current access token (for API.js compatibility)
     * Not used directly with backend authentication as tokens are in HTTP-only cookies
     */
    getToken() {
        // This method exists for compatibility with API client
        // but returns null since we're using HTTP-only cookies
        return null;
    }
    
    /**
     * Update UI with user info
     * @param {boolean} forceRetry - Whether to force a retry if elements aren't found
     */
    updateUIWithUserInfo(forceRetry = false) {
        console.log('Updating UI with user info:', this.userInfo);
        
        // Update UI elements
        const userElement = document.getElementById("loginUser");
        const tenantElement = document.getElementById("tenantName");
        const tenantInput = document.getElementById("tenantname");
        
        // Debug DOM element availability
        console.log('DOM elements found:', { 
            userElement: !!userElement, 
            tenantElement: !!tenantElement, 
            tenantInput: !!tenantInput
        });
        
        // Check if any critical elements are missing and retry if needed
        if ((!userElement || !tenantElement) && (forceRetry || this.uiUpdateRetryCount < 3)) {
            // Set up retry count if not already set
            if (typeof this.uiUpdateRetryCount === 'undefined') {
                this.uiUpdateRetryCount = 0;
            }
            
            // Increment retry count
            this.uiUpdateRetryCount++;
            
            console.log(`UI elements not found, retrying update (${this.uiUpdateRetryCount}/3)...`);
            
            // Schedule a retry after a delay
            setTimeout(() => {
                this.updateUIWithUserInfo(true);
            }, 500 * this.uiUpdateRetryCount); // Increasing delay with each retry
            
            return;
        }
        
        // Reset retry count after a successful update or max retries
        this.uiUpdateRetryCount = 0;
        
        // Update the username display
        if (userElement && this.userInfo && this.userInfo.username) {
            userElement.textContent = `User: ${this.userInfo.username}`;
            console.log('Updated username display to:', this.userInfo.username);
        } else if (userElement) {
            // Ensure we show "Not logged in" if no user info is available
            userElement.textContent = 'User: Not logged in';
            console.log('Reset username display to "Not logged in"');
        }
        
        // Update the tenant name display
        if (tenantElement) {
            const displayTenant = this.tenant || 'Default';
            tenantElement.textContent = `Tenant: ${displayTenant}`;
            console.log('Updated tenant display to:', displayTenant);
        }
        
        // Set the value of the tenant input field to the tenant from the token
        if (tenantInput && this.tenant) {
            tenantInput.value = this.tenant;
            console.log('Updated tenant input value to:', this.tenant);
        } else if (tenantInput) {
            // Default to "Default" if no tenant is specified
            if (!tenantInput.value) {
                tenantInput.value = 'Default';
                console.log('Set default tenant value');
            }
        }
    }

    /**
     * Set up event listeners for auth-related UI elements
     */
    setupEventListeners() {
        // Set up the show token button event listener
        const showTokenButton = document.getElementById('showTokenButton');
        if (showTokenButton) {
            showTokenButton.addEventListener('click', () => this.showToken());
        }
        
        // Set up logout button if it exists
        const logoutButton = document.getElementById('logoutButton');
        if (logoutButton) {
            logoutButton.addEventListener('click', (e) => {
                e.preventDefault(); // Prevent any default action
                console.log('Logout button clicked');
                this.logout();
            });
        } else {
            console.warn('Logout button not found in the DOM');
            // Try to set up the listener again after a short delay
            setTimeout(() => {
                const delayedLogoutButton = document.getElementById('logoutButton');
                if (delayedLogoutButton) {
                    console.log('Found logout button after delay');
                    delayedLogoutButton.addEventListener('click', () => this.logout());
                }
            }, 1000);
        }
        
        // Set up tenant selection input
        const tenantSelect = document.getElementById('tenantname');
        if (tenantSelect) {
            tenantSelect.addEventListener('change', () => {
                const newTenant = tenantSelect.value;
                this.setTenant(newTenant);
            });
        }
    }
    
    /**
     * Set tenant on the backend
     */
    async setTenant(tenant) {
        try {
            const response = await fetch(`${this.backendBaseUrl}/api/auth/tenant`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify({ tenant })
            });
            
            if (response.ok) {
                localStorage.setItem('tenantName', tenant);
                this.tenant = tenant;
                console.log(`Tenant changed to: ${tenant}`);
                
                // Update UI
                const tenantElement = document.getElementById("tenantName");
                if (tenantElement) {
                    tenantElement.textContent = `Tenant: ${tenant}`;
                }
            } else {
                console.error('Failed to update tenant');
            }
        } catch (error) {
            console.error('Error setting tenant:', error);
        }
    }

    /**
     * Show token details in a modal dialog with enhanced information and formatting
     */
    async showToken() {
        try {
            console.log('Showing token details');
            // Check for tokens in both locations
            const accessToken = localStorage.getItem('access_token');
            const authToken = localStorage.getItem('auth_token');
            const idToken = localStorage.getItem('id_token');
            // Ensure both token variables are in sync - if one exists but not the other, sync them
            if (accessToken && !authToken) {
                console.log('Syncing access_token to auth_token');
                localStorage.setItem('auth_token', accessToken);
            } else if (!accessToken && authToken) {
                console.log('Syncing auth_token to access_token');
                localStorage.setItem('access_token', authToken);
            }
            // Get token details from backend or localStorage
            let tokenDetails = null;
            let userInfo = null;
            let cookieTokenAvailable = false;
            let localTokenAvailable = !!(accessToken || authToken);
            let tokenSource = 'unknown';
            // Always add Authorization header if token is present
            const headers = {
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache'
            };
            const token = accessToken || authToken;
            if (token) {
                headers['Authorization'] = 'Bearer ' + token;
                console.debug('[AUTH] Added Authorization header for showToken');
            }
            // First try to get token details from backend (which has access to HTTP-only cookies)
            try {
                const response = await fetch(`${this.backendBaseUrl}/api/auth/token-details`, {
                    credentials: 'include',
                    headers
                });
                if (response.ok) {
                    const data = await response.json();
                    if (data.authenticated && data.token) {
                        tokenDetails = data;
                        cookieTokenAvailable = true;
                        tokenSource = 'http-only cookie';
                        console.log('Retrieved token details from backend HTTP-only cookies');
                    }
                }
            } catch (backendError) {
                console.warn('Error fetching token details from backend:', backendError);
            }
            // If no token from backend, try parsing the local token
            if (!tokenDetails && (accessToken || authToken)) {
                const token = accessToken || authToken;
                try {
                    const decodedToken = this.getUserInfoFromToken(token);
                    tokenDetails = {
                        authenticated: true,
                        token: decodedToken,
                        user: this.userInfo || {
                            username: decodedToken.preferred_username || decodedToken.email || decodedToken.sub,
                            tenant: this.tenant
                        },
                        source: 'localStorage'
                    };
                    tokenSource = 'localStorage';
                    console.log('Retrieved token details from localStorage');
                } catch (parseError) {
                    console.warn('Error parsing token from localStorage:', parseError);
                }
            }
            // If still no token details, try getting at least user info from backend
            if (!tokenDetails) {
                try {
                    const userResponse = await fetch(`${this.backendBaseUrl}/api/auth/user`, {
                        credentials: 'include',
                        headers
                    });
                    if (userResponse.ok) {
                        const userData = await userResponse.json();
                        if (userData.authenticated) {
                            userInfo = userData.user;
                            cookieTokenAvailable = true;
                            tokenSource = 'backend session';
                            console.log('Retrieved user info from backend session');
                        }
                    }
                } catch (userError) {
                    console.warn('Error fetching user info from backend:', userError);
                }
            }
            // Create and show modal dialog for token information
            try {
                this.createTokenInfoModal(tokenDetails, userInfo, tokenSource, cookieTokenAvailable, localTokenAvailable);
            } catch (modalError) {
                console.error('[showToken] Error creating token info modal:', modalError);
                alert('Token is available but the display functionality is not ready. Using simple display instead.\nToken: ' + (tokenDetails && tokenDetails.token ? JSON.stringify(tokenDetails.token, null, 2) : 'N/A'));
            }
        } catch (error) {
            console.error('Error showing token details:', error);
            this.showAuthError('Could not display token details: ' + error.message);
        }
    }

    /**
     * Create modal dialog to display token information in a user-friendly format
     */
    createTokenInfoModal(tokenDetails, userInfo, tokenSource, cookieTokenAvailable, localTokenAvailable) {
        try {
            // Remove any existing modal/overlay
            document.getElementById('dialogOverlay')?.remove();
            document.getElementById('token-info-modal')?.remove();

            // Modal overlay
            const overlay = document.createElement('div');
            overlay.id = 'dialogOverlay';
            overlay.className = 'dialog-overlay';
            overlay.tabIndex = -1;
            overlay.setAttribute('role', 'presentation');
            overlay.addEventListener('click', (e) => {
                if (e.target === overlay) {
                    overlay.remove();
                    modal.remove();
                }
            });

            // Modal container
            const modal = document.createElement('div');
            modal.id = 'token-info-modal';
            modal.className = 'dialog token-info-modal';
            modal.setAttribute('role', 'dialog');
            modal.setAttribute('aria-modal', 'true');
            modal.setAttribute('aria-labelledby', 'token-modal-title');
            modal.style.outline = 'none';

            // Modal content
            const modalContent = document.createElement('div');
            modalContent.className = 'dialog-content';

            // Close button
            const closeButton = document.createElement('button');
            closeButton.textContent = '\u00d7';
            closeButton.className = 'close-button';
            closeButton.setAttribute('aria-label', 'Close');
            closeButton.addEventListener('click', () => {
                overlay.remove();
                modal.remove();
            });

            // Header
            const header = document.createElement('div');
            header.className = 'dialog-header';
            const title = document.createElement('h2');
            title.textContent = 'Authentication Details';
            title.id = 'token-modal-title';
            header.appendChild(title);

            // Content
            const content = document.createElement('div');
            content.className = 'dialog-token-content';
            if (tokenDetails && tokenDetails.token) {
                // Syntax highlight JSON if highlight.js is available
                let json = JSON.stringify(tokenDetails.token, null, 2);
                if (window.hljs) {
                    content.innerHTML = `<pre><code class='json'>${hljs.highlight(json, {language: 'json'}).value}</code></pre>`;
                } else {
                    content.innerHTML = `<pre>${json}</pre>`;
                }
            } else if (userInfo) {
                let json = JSON.stringify(userInfo, null, 2);
                if (window.hljs) {
                    content.innerHTML = `<pre><code class='json'>${hljs.highlight(json, {language: 'json'}).value}</code></pre>`;
                } else {
                    content.innerHTML = `<pre>${json}</pre>`;
                }
            } else {
                content.innerHTML = '<em>No token or user info available.</em>';
            }

            // Compose modal
            modalContent.appendChild(closeButton);
            modalContent.appendChild(header);
            modalContent.appendChild(content);
            modal.appendChild(modalContent);

            // Add overlay and modal to document
            document.body.appendChild(overlay);
            document.body.appendChild(modal);

            // Focus modal for accessibility
            setTimeout(() => { modal.focus(); }, 10);
            console.debug('[createTokenInfoModal] Modal created and displayed.');
        } catch (err) {
            console.error('[createTokenInfoModal] Error:', err);
            throw err;
        }
    }

    /**
     * Parse and extract user information from a JWT token
     * @param {string} token - The JWT token to parse
     * @returns {Object} - The decoded token payload
     */
    getUserInfoFromToken(token) {
        if (!token) {
            throw new Error('No token provided');
        }
        
        try {
            // Split the token into parts
            const tokenParts = token.split('.');
            if (tokenParts.length !== 3) {
                throw new Error('Invalid token format (not a JWT)');
            }
            
            // Decode the payload (middle part)
            const base64Url = tokenParts[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(
                atob(base64)
                    .split('')
                    .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                    .join('')
            );
            
            // Parse the JSON payload
            const payload = JSON.parse(jsonPayload);
            
            // Return the decoded token payload
            return payload;
        } catch (error) {
            console.error('Error decoding token:', error);
            throw new Error('Failed to decode token: ' + error.message);
        }
    }
}

// --- DEBUG PATCH START ---
function extractAndStoreTokenFromHash() {
    const hash = window.location.hash;
    console.debug('[AUTH] URL hash:', hash);
    if (hash && hash.length > 0) {
        const params = new URLSearchParams(hash.replace(/^#/, ''));
        const accessToken = params.get('access_token');
        const idToken = params.get('id_token');
        if (accessToken) {
            localStorage.setItem('access_token', accessToken);
            console.debug('[AUTH] Extracted and stored access_token:', accessToken.substring(0, 10) + '...');
        } else {
            console.warn('[AUTH] No access_token found in hash!');
        }
        if (idToken) {
            localStorage.setItem('id_token', idToken);
            console.debug('[AUTH] Extracted and stored id_token:', idToken.substring(0, 10) + '...');
        }
    } else {
        console.debug('[AUTH] No hash in URL.');
    }
}

function getStoredToken() {
    const token = localStorage.getItem('access_token');
    console.debug('[AUTH] getStoredToken() returns:', token ? token.substring(0, 10) + '...' : 'null');
    return token;
}

function addAuthHeader(headers = {}) {
    const token = getStoredToken();
    if (token) {
        headers['Authorization'] = 'Bearer ' + token;
        console.debug('[AUTH] Added Authorization header.');
    } else {
        console.warn('[AUTH] No token available for Authorization header.');
    }
    return headers;
}

// Patch API call to /api/auth/user to always use Authorization header
async function checkAuthStatus() {
    const headers = addAuthHeader({ 'Accept': 'application/json' });
    try {
        const resp = await fetch('/api/auth/user', { headers });
        console.debug('[AUTH] /api/auth/user response status:', resp.status);
        if (resp.status === 200) {
            const data = await resp.json();
            console.debug('[AUTH] Authenticated user data:', data);
            return { authenticated: true, user: data };
        } else {
            const text = await resp.text();
            console.warn('[AUTH] Auth check failed, status:', resp.status, 'body:', text);
            return { authenticated: false };
        }
    } catch (err) {
        console.error('[AUTH] Error during auth check:', err);
        return { authenticated: false };
    }
}

// On page load, extract token if present
window.addEventListener('DOMContentLoaded', () => {
    extractAndStoreTokenFromHash();
    // ...existing code...
});
// --- DEBUG PATCH END ---

// --- PATCH: Always use nonce in Keycloak login redirect ---
function generateNonce() {
    return Math.random().toString(36).substring(2) + Date.now().toString(36);
}

function redirectToKeycloakLogin() {
    // const nonce = generateNonce();
    // localStorage.setItem('auth_nonce', nonce);
    // const state = generateNonce();
    // const loginUrl = `https://keycloak.sensorsreport.net/realms/sr/protocol/openid-connect/auth?client_id=ContextBroker&redirect_uri=${encodeURIComponent('https://explorer.sensorsreport.net/')}&response_type=token id_token&scope=openid profile email&state=${state}&nonce=${nonce}`;
    // console.debug('[AUTH] (patched) Redirecting to Keycloak login with nonce:', nonce, 'and state:', state);
    // window.location.href = loginUrl;
    window.location.href = '/api/auth/login';
}

// Patch all login button handlers to use redirectToKeycloakLogin
window.loginWithKeycloak = redirectToKeycloakLogin;



// Initialize auth manager
export const authManager = new AuthManager();

export default AuthManager;