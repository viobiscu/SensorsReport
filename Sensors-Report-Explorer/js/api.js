/**
 * API client for interacting with the Orion-LD Context Broker
 */
import { appendToLogs } from './logging.js';


// Use try-catch to handle module import failures
let ErrorBoundary, errorBoundary, store, authManager;

try {
    // Import with relative paths
    import('./ErrorBoundary.js').then(module => {
        ErrorBoundary = module.default;
        errorBoundary = module.errorBoundary;
    }).catch(error => {
        console.error('Failed to load ErrorBoundary module:', error);
        // Provide fallback implementation
        ErrorBoundary = class ErrorBoundary {
            constructor() { this.errors = []; }
            wrap(fn) { return async (...args) => { try { return await fn(...args); } catch (e) { console.error(e); throw e; } }; }
            addError(error) { console.error('API Error:', error); }
            clearErrors() {}
            getErrors() { return []; }
        };
        errorBoundary = new ErrorBoundary();
    });

    import('./state/store.js').then(module => {
        store = module.store;
    }).catch(error => {
        console.error('Failed to load store module:', error);
        // Provide fallback implementation
        store = {
            setLoading: (loading) => console.log('Store: setLoading', loading),
            setError: (error) => console.error('Store error:', error),
            getState: () => ({ loading: false, error: null, data: null })
        };
    });

    import('./auth-backend.js').then(module => {
        authManager = module.authManager;
    }).catch(error => {
        console.error('Failed to load auth module:', error);
        // Provide fallback implementation
        authManager = {
            getToken: () => null,
            isAuthenticated: () => false,
            login: () => console.log('Auth: login called')
        };
    });
} catch (e) {
    console.error('Module import error:', e);
    // Ensure fallback implementations are created even if the entire try block fails
    if (!ErrorBoundary) {
        ErrorBoundary = class ErrorBoundary {
            constructor() { this.errors = []; }
            wrap(fn) { return async (...args) => { try { return await fn(...args); } catch (e) { console.error(e); throw e; } }; }
            addError(error) { console.error('API Error:', error); }
            clearErrors() {}
            getErrors() { return []; }
        };
        errorBoundary = new ErrorBoundary();
    }
    
    if (!store) {
        store = {
            setLoading: (loading) => console.log('Store: setLoading', loading),
            setError: (error) => console.error('Store error:', error),
            getState: () => ({ loading: false, error: null, data: null })
        };
    }
    
    if (!authManager) {
        authManager = {
            getToken: () => null,
            isAuthenticated: () => false,
            login: () => console.log('Auth: login called')
        };
    }
}

// JSON output utility function using window.mainEditor for consistency
function displayJSON(data) {
    // Check if the main editor is available
    if (window.mainEditor && typeof window.mainEditor.setValue === 'function') {
        try {
            // Use the main editor to display the JSON data
            window.mainEditor.setValue(JSON.stringify(data, null, 2));
            console.log('Updated main editor with JSON data');
        } catch (error) {
            console.error('Error updating main editor:', error);
            fallbackDisplayJSON(data);
        }
    } else {
        console.warn('Main JSON editor not available, using fallback display method');
        fallbackDisplayJSON(data);
    }
}

// Fallback display method if main editor is not available
function fallbackDisplayJSON(data) {
    const displayArea = document.getElementById('displayArea');
    if (displayArea) {
        // Create or get the pre element with code block
        let preElement = document.getElementById('jsonDisplay');
        if (!preElement || preElement.tagName !== 'PRE') {
            const oldElement = preElement;
            preElement = document.createElement('pre');
            preElement.id = 'jsonDisplay';
            const codeElement = document.createElement('code');
            codeElement.className = 'language-json hljs-custom';
            preElement.appendChild(codeElement);
            if (oldElement) {
                oldElement.parentNode.replaceChild(preElement, oldElement);
            } else {
                displayArea.appendChild(preElement);
            }
        }

        // Format and display the JSON
        const formattedJson = JSON.stringify(data, null, 2);
        const codeElement = preElement.querySelector('code');
        codeElement.textContent = formattedJson;
        
        // Apply syntax highlighting in a safer way
        if (hljs) {
            try {
                const result = hljs.highlight(formattedJson, {
                    language: 'json',
                    ignoreIllegals: true
                });
                codeElement.innerHTML = result.value;
                codeElement.classList.add('hljs-custom');
            } catch (error) {
                console.error('Error applying syntax highlighting:', error);
                codeElement.textContent = formattedJson;
            }
        }
    } else {
        console.error('No display element found for JSON output');
    }
}

/**
 * Main client for Orion-LD Context Broker
 */
class OrionLDClient {
    constructor(baseURL = null, contextURL = null) {
        // Use the local backend to proxy requests instead of direct connection
        // This solves CORS issues by routing through our backend
        this.backendBaseUrl = window.location.origin;
        
        // Set base URL with fallback to proxy path
        this.baseURL = baseURL || `${this.backendBaseUrl}/api/ngsi-ld/v1`;
        
        // Set context URL with fallback
        this.context = contextURL || '/context/synchro-context.jsonld';
        
        // Set default headers
        this.headers = {
            "Content-Type": "application/ld+json",
            "Accept": "application/json",
            // Add cache control to prevent caching issues with auth
            "Cache-Control": "no-cache, no-store, must-revalidate",
            "Pragma": "no-cache"
        };
        
        // Add Authorization header if token is available (for APIs that need it)
        const token = localStorage.getItem('auth_token') || localStorage.getItem('access_token');
        if (token) {
            this.headers['Authorization'] = `Bearer ${token}`;
            console.log('Added Authorization header with token');
        } else {
            console.log('No token available for Authorization header');
        }
        
        // Add tenant header if available and not "default" or "Synchro"
        const tenantInputValue = document.getElementById('tenantname')?.value;
        const tenantName = tenantInputValue || localStorage.getItem('tenantName');
        
        // Only add NGSILD-Tenant header if it's not "default" or "Synchro"
        if (tenantName && tenantName.toLowerCase() !== "default" && tenantName !== "Synchro") {
            this.headers['NGSILD-Tenant'] = tenantName;
            console.log(`Setting NGSILD-Tenant header to: ${tenantName}`);
        } else {
            console.log(`Not sending NGSILD-Tenant header for tenant: ${tenantName || 'none'}`);
        }
        
        console.log("OrionLDClient initialized with headers:", this.headers);
        console.log("Using base URL:", this.baseURL);
    }

    // Core request method
    async makeRequest(endpoint, method, body = null) {
        // Safely access store with fallback if it's not initialized yet
        if (store) {
            store.setLoading(true);
            store.setError(null);
        } else {
            console.log('Store not initialized yet, skipping loading state update');
        }

        try {
            console.log(`Making ${method} request to: ${endpoint}`);
            console.log("Request headers:", this.headers);
            
            appendToLogs(`Request: ${method} ${endpoint}`);
            
            if (body) {
                console.log("Request body:", body);
                appendToLogs(`Request body: ${JSON.stringify(body).substring(0, 100)}${JSON.stringify(body).length > 100 ? '...' : ''}`);
            }

            // Before making the request, check if we need to refresh the token from localStorage
            this.refreshHeadersFromLocalStorage();
            
            const response = await fetch(endpoint, {
                method,
                headers: this.headers,
                body: body ? JSON.stringify(body) : null,
                credentials: 'include', // Include cookies for auth with SameSite=Lax settings
                cache: 'no-store' // Prevent caching to avoid stale auth state
            });
            
            console.log(`Response status: ${response.status} ${response.statusText}`);
            
            // Log response headers for debugging
            console.log("Response headers:");
            response.headers.forEach((value, key) => {
                console.log(`  ${key}: ${value}`);
            });
            
            appendToLogs(`Response: ${response.status} ${response.statusText}`);
            
            // Handle 401 Unauthorized
            if (response.status === 401) {
                appendToLogs('Authentication error, trying to refresh token');
                
                try {
                    const refreshResponse = await fetch('/api/auth/refresh', {
                        credentials: 'include',
                        cache: 'no-store',
                        headers: {
                            'Cache-Control': 'no-cache, no-store, must-revalidate',
                            'Pragma': 'no-cache'
                        }
                    });
                    
                    if (refreshResponse.ok) {
                        // Try to get the new token from the response
                        try {
                            const refreshData = await refreshResponse.json();
                            if (refreshData.access_token) {
                                // Store the refreshed token
                                localStorage.setItem('access_token', refreshData.access_token);
                                localStorage.setItem('auth_token', refreshData.access_token);
                                
                                // Update Authorization header
                                this.headers['Authorization'] = `Bearer ${refreshData.access_token}`;
                            }
                        } catch (e) {
                            console.log('Token refresh succeeded but did not return a new token');
                        }
                        
                        appendToLogs('Token refreshed, retrying request');
                        return this.makeRequest(endpoint, method, body);
                    } else {
                        appendToLogs('Token refresh failed, redirecting to login');
                        if (authManager && typeof authManager.login === 'function') {
                            authManager.login();
                        }
                        throw new Error('Authentication failed. Please log in again.');
                    }
                } catch (refreshError) {
                    appendToLogs(`Token refresh error: ${refreshError.message}`);
                    if (authManager && typeof authManager.login === 'function') {
                        authManager.login();
                    }
                    throw new Error('Authentication failed. Please log in again.');
                }
            }

            // Read the response body text once
            const responseText = await response.text();
            
            // Handle empty responses
            if (!responseText.trim()) {
                if (response.ok) {
                    // Some successful operations (like DELETE) may return empty responses
                    console.log("Empty response received with successful status code");
                    appendToLogs("Operation completed successfully (empty response)");
                    if (store) store.setLoading(false);
                    return { status: 'success', message: 'Operation completed successfully' };
                } else {
                    // Empty error response
                    const errorMessage = `Server responded with status ${response.status} (empty response)`;
                    console.error(errorMessage);
                    appendToLogs(`Error: ${errorMessage}`);
                    this.showErrorMessage(errorMessage);
                    throw new Error(errorMessage);
                }
            }

            // Handle error responses
            if (!response.ok) {
                try {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        try {
                            const errorData = JSON.parse(responseText);
                            console.log("Error response data:", errorData);
                            appendToLogs(`Error data: ${JSON.stringify(errorData)}`);
                            this.showErrorMessage(`Request failed: ${response.status} ${response.statusText}`);
                            return errorData;
                        } catch (parseError) {
                            console.error("Failed to parse error response as JSON:", parseError);
                        }
                    }
                    
                    console.log(`Error response text: ${responseText.substring(0, 500)}`);
                    appendToLogs(`Error: Non-JSON response. Status: ${response.status}. Text: ${responseText.substring(0, 100)}${responseText.length > 100 ? '...' : ''}`);
                    this.showErrorMessage(`Server error (${response.status}): ${responseText.substring(0, 100) || 'No response details'}`);
                    throw new Error(`Server responded with status ${response.status}: ${responseText.substring(0, 100) || 'Non-JSON response'}`);
                } catch (parseError) {
                    console.error("Parse error:", parseError);
                    appendToLogs(`Parse error: ${parseError.message}`);
                    this.showErrorMessage(`Error processing response: ${parseError.message}`);
                    throw new Error(`Server responded with status ${response.status}: ${parseError.message}`);
                }
            }

            // Try to parse the response as JSON
            try {
                const data = JSON.parse(responseText);
                console.log("Response data:", data);
                if (store) store.setLoading(false);
                return data;
            } catch (parseError) {
                // If response is not valid JSON, log the actual response content for debugging
                console.error("Failed to parse JSON response. Response content:", responseText);
                appendToLogs(`Error: Failed to parse JSON. Response: ${responseText.substring(0, 100)}...`);
                this.showErrorMessage(`Failed to parse server response. The response was not valid JSON. Check the console for details.`);
                throw new Error(`Failed to parse JSON response: The server returned invalid JSON data`);
            }
        } catch (error) {
            const errorMessage = error.message || 'Unknown error';
            console.error("Request error:", errorMessage);
            
            const isNetworkError = errorMessage.includes('NetworkError') || 
                               errorMessage.includes('Failed to fetch') ||
                               errorMessage.includes('Network request failed') ||
                               errorMessage.includes('Cross-Origin Request Blocked');
            
            if (isNetworkError) {
                appendToLogs(`Network error accessing ${endpoint}: ${errorMessage}`);
                if (store) store.setError(`Network error: Unable to connect to the server. Check your connection and CORS settings.`);
                this.showErrorMessage(`Network error: Unable to connect to the server. The service may be unavailable or there might be a connectivity issue.`);
            } else {
                appendToLogs(`Error in request: ${errorMessage}`);
                if (store) store.setError(errorMessage);
                this.showErrorMessage(`Error: ${errorMessage}`);
            }
            
            return { error: errorMessage, status: 'error' };
        } finally {
            if (store) store.setLoading(false);
        }
    }
    
    // Helper method to refresh headers from localStorage
    refreshHeadersFromLocalStorage() {
        // Update Authorization header if token is available
        const token = localStorage.getItem('auth_token') || localStorage.getItem('access_token');
        if (token) {
            this.headers['Authorization'] = `Bearer ${token}`;
        }
        
        // Update tenant header if needed
        const tenantInputValue = document.getElementById('tenantname')?.value;
        const tenantName = tenantInputValue || localStorage.getItem('tenantName');
        
        if (tenantName && tenantName.toLowerCase() !== "default" && tenantName !== "Synchro") {
            this.headers['NGSILD-Tenant'] = tenantName;
        } else if (this.headers['NGSILD-Tenant']) {
            delete this.headers['NGSILD-Tenant'];
        }
    }
    
    // Helper method to show error messages in the UI
    showErrorMessage(message) {
        // Always use the main editor for displaying error information with timestamp
        if (window.mainEditor && typeof window.mainEditor.setValue === 'function') {
            window.mainEditor.setValue(JSON.stringify({
                error: message,
                timestamp: new Date().toISOString(),
                hint: "Check browser console (F12) for more details"
            }, null, 2));
            
            // Also log the error in the request logs
            appendToLogs(`Error: ${message}`);
        } else {
            // Fallback display methods if main editor is not available
            const jsonDisplay = document.getElementById('jsonDisplay');
            if (jsonDisplay) {
                jsonDisplay.value = JSON.stringify({
                    error: message,
                    timestamp: new Date().toISOString(),
                    hint: "Check browser console (F12) for more details"
                }, null, 2);
            }
            
            // Also try to display in the fallbackUI element if available
            const fallbackUI = document.getElementById('fallbackUI');
            if (fallbackUI) {
                const errorDiv = document.createElement('div');
                errorDiv.className = 'network-error';
                errorDiv.innerHTML = `<strong>Error:</strong> ${message}<br><small>Check browser console (F12) for technical details.</small>`;
                
                // Clear previous error messages
                fallbackUI.innerHTML = '';
                fallbackUI.appendChild(errorDiv);
                
                // Auto-remove after 15 seconds
                setTimeout(() => {
                    if (fallbackUI.contains(errorDiv)) {
                        errorDiv.remove();
                    }
                }, 15000);
            }
            
            // Make sure it also appears in logs
            appendToLogs(`Error: ${message}`);
        }
    }

    // Entity operations
    async createEntity(entity) {
        const endpoint = `${this.baseURL}/entities`;
        return await this.makeRequest(endpoint, "POST", entity);
    }

    async getEntity(entityId) {
        const endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
        return await this.makeRequest(endpoint, "GET");
    }

    async getAttribute(entityId, attributeName) {
        // Use query parameter approach instead of direct attribute endpoint
        const endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}?attrs=${encodeURIComponent(attributeName)}`;
        
        // Save current headers
        const originalHeaders = { ...this.headers };
        
        // Update headers for NGSI-LD context
        this.headers = {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Link': '<https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld>; rel="http://www.w3.org/ns/json-ld#context"; type="application/ld+json"'
        };
        
        try {
            const response = await this.makeRequest(endpoint, "GET");
            // Extract just the attribute data from the response
            return response[attributeName];
        } finally {
            // Restore original headers
            this.headers = originalHeaders;
        }
    }

    async updateAttribute(entityId, attributeName, attributeValue) {
        // Use query parameter approach for attribute updates
        const endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
        
        // Save current headers
        const originalHeaders = { ...this.headers };
        
        // Update headers for NGSI-LD context
        this.headers = {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Link': '<https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld>; rel="http://www.w3.org/ns/json-ld#context"; type="application/ld+json"'
        };
        
        try {
            // Create a patch object that only updates the specified attribute
            const patchData = {
                [attributeName]: attributeValue
            };
            return await this.makeRequest(endpoint, "PATCH", patchData);
        } finally {
            // Restore original headers
            this.headers = originalHeaders;
        }
    }

    async replaceEntity(entityId, entity) {
        return errorBoundary.wrap(async () => {
            const endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
            return await this.makeRequest(endpoint, "PUT", entity);
        });
    }

    async updateEntity(entityId, attributes) {
        // If it's a single attribute update, use the attribute-specific endpoint
        if (Object.keys(attributes).length === 1) {
            const [attributeName] = Object.keys(attributes);
            return await this.updateAttribute(entityId, attributeName, attributes[attributeName]);
        }
        
        // Otherwise update all attributes
        const endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
        return await this.makeRequest(endpoint, "PATCH", attributes);
    }

    async deleteEntity(entityId) {
        const endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
        return await this.makeRequest(endpoint, "DELETE");
    }

    async deleteAttribute(entityId, attributeName) {
        const endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}/attrs/${encodeURIComponent(attributeName)}`;
        return await this.makeRequest(endpoint, "DELETE");
    }

    // Batch operations
    async batchCreate(entities) {
        return errorBoundary.wrap(async () => {
            const endpoint = `${this.baseURL}/entityOperations/create`;
            return await this.makeRequest(endpoint, "POST", entities);
        });
    }

    async batchUpsert(entities) {
        return errorBoundary.wrap(async () => {
            const endpoint = `${this.baseURL}/entityOperations/upsert`;
            return await this.makeRequest(endpoint, "POST", entities);
        });
    }

    async batchDelete(entityIds) {
        return errorBoundary.wrap(async () => {
            const endpoint = `${this.baseURL}/entityOperations/delete`;
            return await this.makeRequest(endpoint, "POST", entityIds);
        });
    }
}

/**
 * Extended client with search capabilities
 */
class OrionLDSearchClient extends OrionLDClient {
    constructor(baseURL = null, contextURL = null, pageSize = 100) {
        super(baseURL, contextURL);
        this.pageSize = pageSize;
    }

    async getAllEntities(limit = this.pageSize, offset = 0) {
        // Add local=true parameter as the NGSI-LD broker requires at least one filter
        const endpoint = `${this.baseURL}/entities?limit=${limit}&offset=${offset}&local=true`;
        return await this.makeRequest(endpoint, "GET");
    }

    async getEntitiesByType(type, limit = this.pageSize, offset = 0) {
        const endpoint = `${this.baseURL}/entities?type=${encodeURIComponent(type)}&limit=${limit}&offset=${offset}&local=true`;
        return await this.makeRequest(endpoint, "GET");
    }

    async getSubscriptions() {
        const endpoint = `${this.baseURL}/subscriptions`;
        return await this.makeRequest(endpoint, "GET");
    }

    async getAllTypes() {
        try {
            const allEntities = await this.fetchAllEntitiesWithPagination();
            const types = new Set();
            allEntities.forEach(entity => {
                if (entity.type) types.add(entity.type);
            });
            return Array.from(types);
        } catch (error) {
            appendToLogs(`Error fetching entity types: ${error.message}`);
            throw error;
        }
    }

    async getAllAttributes() {
        try {
            const allEntities = await this.fetchAllEntitiesWithPagination();
            const attributes = new Set();
            allEntities.forEach(entity => {
                Object.keys(entity).forEach(key => {
                    if (key !== 'id' && key !== 'type' && !key.startsWith('@')) {
                        attributes.add(key);
                    }
                });
            });
            return Array.from(attributes);
        } catch (error) {
            appendToLogs(`Error fetching attributes: ${error.message}`);
            throw error;
        }
    }

    async getAllRelationships() {
        try {
            const allEntities = await this.fetchAllEntitiesWithPagination();
            const relationships = new Set();
            allEntities.forEach(entity => {
                Object.entries(entity).forEach(([key, value]) => {
                    if (typeof value === 'object' && value !== null &&
                        (value.object || value.type === 'Relationship' ||
                        (Array.isArray(value) && value.some(item => item.object)))) {
                        relationships.add(key);
                    }
                });
            });
            return Array.from(relationships);
        } catch (error) {
            appendToLogs(`Error fetching relationships: ${error.message}`);
            throw error;
        }
    }

    async fetchAllEntitiesWithPagination() {
        const allEntities = [];
        let offset = 0;
        let hasMore = true;

        while (hasMore) {
            const response = await this.getAllEntities(this.pageSize, offset);
            if (Array.isArray(response) && response.length > 0) {
                allEntities.push(...response);
                offset += response.length;
                hasMore = response.length >= this.pageSize;
            } else {
                hasMore = false;
            }
        }
        return allEntities;
    }

    async getAllEntityInformation() {
        try {
            if (store) store.setLoading(true);
            const [types, attributes, subscriptions, relationships] = await Promise.all([
                this.getAllTypes(),
                this.getAllAttributes(),
                this.getSubscriptions(),
                this.getAllRelationships()
            ]);
            return { types, attributes, subscriptions, relationships };
        } catch (error) {
            appendToLogs(`Error fetching entity information: ${error.message}`);
            throw error;
        } finally {
            if (store) store.setLoading(false);
        }
    }
}

/**
 * Client for interacting with Quantum Leap time series database
 */
class QuantumLeapClient {
    constructor(baseURL = null) {
        // Use the local backend to proxy requests instead of direct connection
        this.backendBaseUrl = window.location.origin;
        
        // Set base URL with fallback to proxy path - using correct API path
        this.baseURL = baseURL || `${this.backendBaseUrl}/api/quantum/v2`;
        
        // Set default headers
        this.headers = {
            "Content-Type": "application/json",
            "Accept": "application/json",
            // Add cache control to prevent caching issues
            "Cache-Control": "no-cache, no-store, must-revalidate",
            "Pragma": "no-cache"
        };

        // Set Fiware Service headers based on tenant
        this.updateFiwareHeaders();
        
        console.log("QuantumLeapClient initialized with headers:", this.headers);
        console.log("Using base URL:", this.baseURL);
    }

    // Helper method to update Fiware headers based on tenant
    updateFiwareHeaders() {
        // Get tenant from input or localStorage
        const tenantInputValue = document.getElementById('tenantname')?.value;
        const tenantName = tenantInputValue || localStorage.getItem('tenantName');
        
        // Only add Fiware-Service header if tenant is not "default" or "Synchro"
        if (tenantName && tenantName.toLowerCase() !== "default" && tenantName !== "Synchro") {
            this.headers['Fiware-Service'] = tenantName;
            // Default ServicePath if not specified
            this.headers['Fiware-ServicePath'] = '/';
            console.log(`Setting Fiware-Service header to: ${tenantName}`);
        } else {
            // Remove headers if they exist
            delete this.headers['Fiware-Service'];
            delete this.headers['Fiware-ServicePath'];
            console.log(`Not sending Fiware-Service header for tenant: ${tenantName || 'none'}`);
        }
    }

    // Core request method
    async makeRequest(endpoint, method, body = null) {
        // Update headers before each request
        this.updateFiwareHeaders();

        try {
            console.log(`Making ${method} request to: ${endpoint}`);
            console.log("Request headers:", this.headers);
            
            appendToLogs(`QuantumLeap Request: ${method} ${endpoint}`);
            
            if (body) {
                console.log("Request body:", body);
                appendToLogs(`QuantumLeap Request body: ${JSON.stringify(body).substring(0, 100)}${JSON.stringify(body).length > 100 ? '...' : ''}`);
            }

            const response = await fetch(endpoint, {
                method,
                headers: this.headers,
                body: body ? JSON.stringify(body) : null,
                credentials: 'include'
            });
            
            console.log(`Response status: ${response.status} ${response.statusText}`);
            appendToLogs(`QuantumLeap Response: ${response.status} ${response.statusText}`);

            const responseText = await response.text();
            
            if (!responseText.trim()) {
                if (response.ok) {
                    console.log("Empty response received with successful status code");
                    appendToLogs("QuantumLeap operation completed successfully (empty response)");
                    return { status: 'success', message: 'Operation completed successfully' };
                } else {
                    throw new Error(`Server responded with status ${response.status} (empty response)`);
                }
            }

            // Try to parse the response as JSON
            try {
                const data = JSON.parse(responseText);
                console.log("Response data:", data);
                return data;
            } catch (parseError) {
                console.error("Failed to parse JSON response. Response content:", responseText);
                throw new Error(`Failed to parse JSON response: ${parseError.message}`);
            }
        } catch (error) {
            console.error("QuantumLeap request error:", error);
            appendToLogs(`QuantumLeap Error: ${error.message}`);
            throw error;
        }
    }

    // Get entity time series data
    async getEntityTimeSeries(entityId, type, attributes, fromDate = null, toDate = null) {
        let endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
        const params = new URLSearchParams();
        
        if (type) params.append('type', type);
        if (attributes) params.append('attrs', Array.isArray(attributes) ? attributes.join(',') : attributes);
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        
        const queryString = params.toString();
        if (queryString) endpoint += `?${queryString}`;
        
        return await this.makeRequest(endpoint, "GET");
    }

    // Get entity attribute time series data
    async getAttributeTimeSeries(entityId, attrName, type = null, fromDate = null, toDate = null) {
        let endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}/attrs/${encodeURIComponent(attrName)}`;
        const params = new URLSearchParams();
        
        if (type) params.append('type', type);
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        
        const queryString = params.toString();
        if (queryString) endpoint += `?${queryString}`;
        
        return await this.makeRequest(endpoint, "GET");
    }

    // Get historical values for multiple entities
    async getEntitiesTimeSeries(type = null, attributes = null, fromDate = null, toDate = null) {
        let endpoint = `${this.baseURL}/entities`;
        const params = new URLSearchParams();
        
        if (type) params.append('type', type);
        if (attributes) params.append('attrs', Array.isArray(attributes) ? attributes.join(',') : attributes);
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        
        const queryString = params.toString();
        if (queryString) endpoint += `?${queryString}`;
        
        return await this.makeRequest(endpoint, "GET");
    }

    // Get last N values for an entity
    async getLastN(entityId, type, attributes, n = 10) {
        let endpoint = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
        const params = new URLSearchParams({
            lastN: n.toString()
        });
        
        if (type) params.append('type', type);
        if (attributes) params.append('attrs', Array.isArray(attributes) ? attributes.join(',') : attributes);
        
        endpoint += `?${params.toString()}`;
        
        return await this.makeRequest(endpoint, "GET");
    }
}

/**
 * Client for NGSI-LD Subscription operations
 */
class SubscriptionClient {
    constructor(baseURL = null) {
        this.backendBaseUrl = window.location.origin;
        this.baseURL = baseURL || `${this.backendBaseUrl}/api/ngsi-ld/v1/subscriptions`;
        this.headers = {
            "Content-Type": "application/ld+json",
            "Accept": "application/json",
            "Cache-Control": "no-cache, no-store, must-revalidate",
            "Pragma": "no-cache"
        };
        const token = localStorage.getItem('auth_token') || localStorage.getItem('access_token');
        if (token) {
            this.headers['Authorization'] = `Bearer ${token}`;
        }
        this.updateTenantHeaders();
    }

    // Helper method to update tenant header
    updateTenantHeaders() {
        const tenantInputValue = document.getElementById('tenantname')?.value;
        const tenantName = tenantInputValue || localStorage.getItem('tenantName');
        if (tenantName && tenantName.toLowerCase() !== "default" && tenantName !== "Synchro") {
            this.headers['NGSILD-Tenant'] = tenantName;
        } else {
            delete this.headers['NGSILD-Tenant'];
        }
    }

    async makeRequest(endpoint, method, body = null) {
        this.updateTenantHeaders();
        // Check if body has @context or context property
        if (body && (body['@context'] || body.context)) {
            this.headers["Content-Type"] = "application/ld+json";
        } else {
            this.headers["Content-Type"] = "application/json";
        }
        console.log(`Making ${method} request to: ${endpoint}`);
        console.log("Request headers:", this.headers);
        appendToLogs(`Subscription Request: ${method} ${endpoint}`);
        const response = await fetch(endpoint, {
            method,
            headers: this.headers,
            body: body ? JSON.stringify(body) : null,
            credentials: 'include',
            cache: 'no-store'
        });
        const text = await response.text();
        if (!text.trim()) return response.ok ? { status: 'success' } : { status: 'error' };
        try {
            return JSON.parse(text);
        } catch {
            return { status: response.status, text };
        }
    }

    // GET /subscriptions
    async getAll() {
        return await this.makeRequest(this.baseURL, 'GET');
    }

    // GET /subscriptions/:id
    async getById(subscriptionId) {
        const endpoint = `${this.baseURL}/${encodeURIComponent(subscriptionId)}`;
        return await this.makeRequest(endpoint, 'GET');
    }

    // POST /subscriptions
    async create(subscription) {
        return await this.makeRequest(this.baseURL, 'POST', subscription);
    }

    // PATCH /subscriptions/:id
    async update(subscriptionId, patch) {
        const endpoint = `${this.baseURL}/${encodeURIComponent(subscriptionId)}`;
        return await this.makeRequest(endpoint, 'PATCH', patch);
    }

    // DELETE /subscriptions/:id
    async delete(subscriptionId) {
        const endpoint = `${this.baseURL}/${encodeURIComponent(subscriptionId)}`;
        return await this.makeRequest(endpoint, 'DELETE');
    }

    // OPTIONS /subscriptions or /subscriptions/:id
    async options(subscriptionId = null) {
        const endpoint = subscriptionId ? `${this.baseURL}/${encodeURIComponent(subscriptionId)}` : this.baseURL;
        return await this.makeRequest(endpoint, 'OPTIONS');
    }
}

/**
 * Client for Data Product Manager API
 */
class DataProductClient {
    constructor(baseURL = null) {
        this.backendBaseUrl = window.location.origin;
        this.baseURL = baseURL || `${this.backendBaseUrl}/api/dataProducts`;
        this.headers = {
            "Content-Type": "application/json",
            "Accept": "application/json",
            "Cache-Control": "no-cache, no-store, must-revalidate",
            "Pragma": "no-cache"
        };
        // Add Authorization header if token is available
        const token = localStorage.getItem('auth_token') || localStorage.getItem('access_token');
        if (token) {
            this.headers['Authorization'] = `Bearer ${token}`;
        }
    }

    async makeRequest(endpoint, method, body = null, isMultipart = false) {
        try {
            appendToLogs(`DataProduct Request: ${method} ${endpoint}`);
            console.log(`DataProductClient: ${method} ${endpoint}`);
            let options = {
                method,
                headers: isMultipart ? undefined : this.headers,
                credentials: 'include',
                body: null
            };
            if (body) {
                if (isMultipart) {
                    options.body = body; // FormData
                } else {
                    options.body = JSON.stringify(body);
                }
            }
            const response = await fetch(endpoint, options);
            const text = await response.text();
            appendToLogs(`DataProduct Response: ${response.status} ${response.statusText}`);
            if (!text.trim()) return response.ok ? { status: 'success' } : { status: 'error' };
            try {
                return JSON.parse(text);
            } catch {
                return { status: response.status, text };
            }
        } catch (error) {
            appendToLogs(`DataProduct Error: ${error.message}`);
            console.error('DataProductClient error:', error);
            throw error;
        }
    }

    async getAll() {
        return await this.makeRequest(this.baseURL, 'GET');
    }

    async getById(id) {
        const endpoint = `${this.baseURL}/${encodeURIComponent(id)}`;
        return await this.makeRequest(endpoint, 'GET');
    }

    async create(data, isMultipart = false) {
        // If isMultipart, data should be FormData
        return await this.makeRequest(this.baseURL, 'POST', data, isMultipart);
    }

    async deleteAll() {
        return await this.makeRequest(this.baseURL, 'DELETE');
    }

    async deleteById(id) {
        const endpoint = `${this.baseURL}/${encodeURIComponent(id)}`;
        return await this.makeRequest(endpoint, 'DELETE');
    }
}


// POST data from form
export async function processPostQuery(dataPost) {
    try {
        const client = new OrionLDClient();
        const dataJson = JSON.parse(dataPost);
        const data = await client.createEntity(dataJson);
        displayJSON(data);
        appendToLogs(`Successfully created entity with ID: ${dataJson.id}`);
    } catch (error) {
        console.error('Error:', error);
        appendToLogs(`Error: ${error.message}`);
    }
}

// PATCH or PUT data
export async function handlePatchQuery(endpoint, jsonData) {
    try {
        // Always save the entity ID when attempting a PATCH
        localStorage.setItem('entityPatchInputText', endpoint);
        localStorage.setItem('jsonDisplay', jsonData);

        const client = new OrionLDClient();
        let data;
        const entityData = JSON.parse(jsonData);
        let entityId = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;

        if (entityData.type && entityData.id) {
            data = await client.replaceEntity(entityId, entityData);
            appendToLogs(`Successfully processed PUT request for ${entityId}`);
        } else {
            data = await client.updateEntity(entityId, entityData);
            appendToLogs(`Successfully processed PATCH request for ${entityId}`);
        }

        displayJSON(data);
    } catch (error) {
        console.error('Error:', error);
        appendToLogs(`Error: ${error.message}`);
    }
}

// Search all entities
export async function searchEntities() {
    try {
        const searchClient = new OrionLDSearchClient();
        const data = await searchClient.getAllEntityInformation();
        displayJSON(data);
        appendToLogs('Successfully retrieved entity information');
        return data;
    } catch (error) {
        console.error('Error:', error);
        appendToLogs(`Error: ${error.message}`);
    }
}

// Load entity types into UI
export async function loadEntityTypes() {
    try {
        const searchClient = new OrionLDSearchClient();
        const types = await searchClient.getAllTypes();
        const typesList = document.getElementById('entity-types-list');
        
        if (typesList) {
            typesList.innerHTML = '';
            types.forEach(type => {
                const listItem = document.createElement('li');
                const icon = document.createElement('i');
                icon.className = 'fa-regular fa-file custom-icon';
                const span = document.createElement('span');
                span.textContent = type;
                const button = document.createElement('button');
                button.className = 'method-button get-button';
                button.textContent = 'GET';
                // Direct call to filterEntitiesByType without parameters
                button.onclick = () => filterEntitiesByType(type);
                
                listItem.appendChild(icon);
                listItem.appendChild(span);
                listItem.appendChild(button);
                typesList.appendChild(listItem);
            });
            
            appendToLogs(`Loaded ${types.length} entity types`);
        }
    } catch (error) {
        console.error('Error loading entity types:', error);
        appendToLogs(`Error loading entity types: ${error.message}`);
    }
}

// Filter entities by type
export async function filterEntitiesByType(type) {
    try {
        console.log(`filterEntitiesByType called in api.js with type: ${type}`);
        const searchClient = new OrionLDSearchClient();
        const entities = await searchClient.getEntitiesByType(type);
        
        // Ensure we have a valid response
        if (!entities || entities.error) {
            throw new Error(entities?.error || 'Failed to retrieve entities');
        }
        
        // Create a new tab to display the results
        displayEntitiesInNewTab(entities, type);
        
        // Also update the entity GET results editor if it exists (for consistency)
        if (window.getResultsEditor && typeof window.getResultsEditor.setValue === 'function') {
            window.getResultsEditor.setValue(JSON.stringify(entities, null, 2));
        }
        
        appendToLogs(`Retrieved ${entities.length} entities of type "${type}"`);
        return entities;
    } catch (error) {
        console.error('Error fetching entities by type:', error);
        appendToLogs(`Error: ${error.message}`);
        
        // Display error in editor
        if (window.mainEditor) {
            window.mainEditor.setValue(JSON.stringify({
                error: error.message,
                timestamp: new Date().toISOString(),
                entityType: type
            }, null, 2));
        }
        
        throw error;
    }
}

// Handle GET queries for entities
export async function handleGetQuery(query) {
    console.log(`Handling GET query: ${query}`);
    const client = new OrionLDClient();

    try {
        // Show loading state in the editor if available
        if (window.getResultsEditor) {
            window.getResultsEditor.setValue(JSON.stringify({
                status: "loading",
                query: query,
                message: "Fetching data..."
            }, null, 2));
        }

        // Handle different query formats
        let endpoint = '';
        if (query.startsWith('?')) {
            // Query parameters format
            endpoint = `${client.baseURL}/entities${query}`;
        } else if (query.startsWith('/')) {
            // Path format with leading slash
            endpoint = `${client.baseURL}${query}`;
        } else if (query.startsWith('urn:')) {
            // URN format without leading slash
            endpoint = `${client.baseURL}/entities/${encodeURIComponent(query)}`;
        } else {
            // Default to entities endpoint with the query as a parameter
            endpoint = `${client.baseURL}/entities/${encodeURIComponent(query)}`;
        }

        const response = await fetch(endpoint, {
            method: 'GET',
            headers: client.headers,
            credentials: 'include'
        });

        const data = await response.json();

        // Update the editor with the response
        if (window.getResultsEditor) {
            window.getResultsEditor.setValue(JSON.stringify(data, null, 2));
        }

        // Also update main editor if it exists and is different from results editor
        if (window.mainEditor && window.mainEditor !== window.getResultsEditor) {
            window.mainEditor.setValue(JSON.stringify(data, null, 2));
        }

        appendToLogs(`GET query successful: ${query}`);
        return data;

    } catch (error) {
        console.error('Error executing GET query:', error);
        const errorMessage = {
            error: "Failed to execute GET query",
            details: error.message || String(error),
            query: query,
            timestamp: new Date().toISOString()
        };

        if (window.getResultsEditor) {
            window.getResultsEditor.setValue(JSON.stringify(errorMessage, null, 2));
        }

        appendToLogs(`Error in GET query: ${error.message}`);
        throw error;
    }
}

// Expose utility functions for global use
if (typeof window !== 'undefined') {

    window.processPostQuery = processPostQuery;
    window.handlePatchQuery = handlePatchQuery;
    window.searchEntities = searchEntities;
    window.loadEntityTypes = loadEntityTypes;
    window.filterEntitiesByType = filterEntitiesByType;
    window.handleGetQuery = handleGetQuery;
    
    // Only set handleEntityGet if it's not already defined
    if (typeof window.handleEntityGet !== 'function') {
        window.handleEntityGet = function(templatePath) {
            if (!templatePath || typeof templatePath !== 'string') {
                console.error('API handleEntityGet: Invalid template path:', templatePath);
                return;
            }
            
            console.log('API handleEntityGet called with:', templatePath);
            
            fetch(templatePath)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Failed to fetch ${templatePath}: ${response.status} ${response.statusText}`);
                    }
                    return response.text();
                })
                .then(data => {
                    const displayArea = document.getElementById('displayArea');
                    if (displayArea) {
                        displayArea.innerHTML = data;
                    } else {
                        console.error('Display area element not found');
                    }
                })
                .catch(error => {
                    console.error('Error loading template:', error);
                    const displayArea = document.getElementById('displayArea');
                    if (displayArea) {
                        displayArea.innerHTML = `<p>Error loading content: ${error.message}</p>`;
                    }
                });
        };
    }
}

function getAuthToken() {
    const token = localStorage.getItem('access_token');
    console.debug('[API] getAuthToken() returns:', token ? token.substring(0, 10) + '...' : 'null');
    return token;
}

function addAuthHeader(headers = {}) {
    const token = getAuthToken();
    if (token) {
        headers['Authorization'] = 'Bearer ' + token;
        console.debug('[API] Added Authorization header.');
    } else {
        console.warn('[API] No token available for Authorization header.');
    }
    return headers;
}

// Patch all API calls to use addAuthHeader
async function apiFetch(url, options = {}) {
    options.headers = addAuthHeader(options.headers || {});
    console.debug('[API] apiFetch:', url, options);
    return fetch(url, options);
}

export { OrionLDClient, OrionLDSearchClient, QuantumLeapClient, SubscriptionClient, DataProductClient };
export default OrionLDClient;