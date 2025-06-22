//import apiClient from './api.js';
import { authManager } from './auth-backend.js';
import Editor from './Editor.js';
import { initializeUI } from './ui.js';
import { clearAuthData, clearAllData } from './auth-utils.js';

/**
 * Main application class to kick off initialization
 */
class AppInitializer {
    constructor() {
        console.log('Initializing app...');
        
        // Initialize tenant and user info from localStorage first
        //this.initFromLocalStorage();
        
        // Initialize UI components
        this.uiManager = initializeUI();
        
        // Initialize the JSON editor
        this.initializeMainEditor();
        
        // Start auth status check with a slight delay to ensure DOM is fully loaded
        // setTimeout(() => {
        //     this.checkAndInitializeAuth();
        // }, 500);
        
        // Add event listener for tenant input changes
        this.setupTenantChangeListener();
        
    }
    
    /**
     * Initialize from localStorage values first if available
     */
    initFromLocalStorage() {
        const tenant = localStorage.getItem('tenantName');
        const tenantInput = document.getElementById('tenantname');
        const tenantName = document.getElementById('tenantName');
        
        if (tenant && tenantInput) {
            console.log('Setting tenant input from localStorage:', tenant);
            tenantInput.value = tenant;
        }
        
        if (tenant && tenantName) {
            console.log('Setting tenant display from localStorage:', tenant);
            tenantName.textContent = `Tenant: ${tenant}`;
        }
    }
    
    /**
     * Initialize the main JSON editor instance
     * This ensures we have a single editor instance used throughout the application
     */
    initializeMainEditor() {
        console.log('Initializing main JSON editor...');
        
        // Check if JsonEditor class is available
        if (typeof Editor !== 'function') {
            console.error('JsonEditor not available. Please reload the page.');
            return;
        }
        
        try {
            // Only create the main editor if it doesn't already exist
            if (!window.mainEditor) {
                // Create the main editor instance
                window.mainEditor = new Editor({
                    containerId: 'mainJsonEditorContainer',
                    initialValue: JSON.stringify({ 
                        message: "Welcome to Orion-LD Explorer",
                        instructions: "Select an operation from the sidebar to get started."
                    }, null, 2),
                    height: 500,
                    resize: true
                });
                
                console.log('Main JSON editor initialized successfully');
            } else {
                console.log('Main JSON editor already exists, skipping initialization');
            }
        } catch (error) {
            console.error('Error initializing main JSON editor:', error);
        }
    }
    
    /**
     * Set up listener for tenant input changes
     */
    setupTenantChangeListener() {
        const tenantInput = document.getElementById('tenantname');
        if (tenantInput) {
            tenantInput.addEventListener('change', (event) => {
                const newTenant = event.target.value;
                console.log('Tenant input changed to:', newTenant);
                
                // Update localStorage
                localStorage.setItem('tenantName', newTenant);
                
                // Update display
                const tenantName = document.getElementById('tenantName');
                if (tenantName) {
                    tenantName.textContent = `Tenant: ${newTenant}`;
                }
                
                // Reload entity types with new tenant
                if (window.loadEntityTypes) {
                    console.log('Reloading entity types with new tenant...');
                    window.loadEntityTypes();
                }
            });
        }
    }
    
    /**
     * Check authentication status and initialize app accordingly
     */
    checkAndInitializeAuth() {
        console.log('Checking auth status on app init...');

        // Redirect to Keycloak login if no token is found
        const localToken = localStorage.getItem('auth_token');
        if (!localToken) {
            console.log('No auth token found, redirecting to Keycloak login...');
            authManager.redirectToKeycloakLogin();
            return;
        }

        // Validate token and update UI
        try {
            const tokenParts = localToken.split('.');
            if (tokenParts.length === 3) {
                const base64Url = tokenParts[1];
                const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                const jsonPayload = decodeURIComponent(
                    atob(base64)
                        .split('')
                        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                        .join('')
                );

                const payload = JSON.parse(jsonPayload);
                const expirationTime = payload.exp * 1000; // Convert to milliseconds

                if (expirationTime > Date.now()) {
                    console.log('Token is valid and not expired');
                    const username = payload.preferred_username || payload.email || payload.sub || 'Unknown User';
                    const tenant = payload.tenant || 'Default';

                    const loginUser = document.getElementById('loginUser');
                    const tenantName = document.getElementById('tenantName');
                    const tenantInput = document.getElementById('tenantname');

                    if (loginUser) loginUser.textContent = `User: ${username}`;
                    if (tenantName) tenantName.textContent = `Tenant: ${tenant}`;
                    if (tenantInput) tenantInput.value = tenant;

                    localStorage.setItem('tenantName', tenant);
                } else {
                    console.warn('Token is expired, clearing it');
                    localStorage.removeItem('auth_token');
                    authManager.redirectToKeycloakLogin();
                }
            } else {
                console.warn('Invalid token format (not a JWT)');
                localStorage.removeItem('auth_token');
                authManager.redirectToKeycloakLogin();
            }
        } catch (error) {
            console.error('Error validating token:', error);
            localStorage.removeItem('auth_token');
            authManager.redirectToKeycloakLogin();
        }
    }
    
}

// Create and export app initializer
const appInitializer = new AppInitializer();

// Make functions available globally
window.setTenant = function(tenant) {
    const tenantInput = document.getElementById('tenantname');
    if (tenantInput) {
        tenantInput.value = tenant;
        // Trigger the change event
        const event = new Event('change');
        tenantInput.dispatchEvent(event);
    }
};

export default appInitializer;
