// Function to create and show modal with action buttons
function showStateModal(title, content, actions = null) {
    // Remove any existing modal
    const existingModal = document.getElementById('state-info-modal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Create modal container
    const modal = document.createElement('div');
    modal.id = 'state-info-modal';
    modal.style.position = 'fixed';
    modal.style.top = '0';
    modal.style.left = '0';
    modal.style.width = '100%';
    modal.style.height = '100%';
    modal.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
    modal.style.display = 'flex';
    modal.style.justifyContent = 'center';
    modal.style.alignItems = 'center';
    modal.style.zIndex = '2000';
    
    // Create modal content
    const modalContent = document.createElement('div');
    modalContent.style.backgroundColor = 'var(--content-bg, #fff)';
    modalContent.style.color = 'var(--text-color, #333)';
    modalContent.style.borderRadius = '8px';
    modalContent.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.2)';
    modalContent.style.width = '90%';
    modalContent.style.maxWidth = '800px';
    modalContent.style.maxHeight = '90vh';
    modalContent.style.overflow = 'auto';
    modalContent.style.position = 'relative';
    modalContent.style.padding = '20px';
    modalContent.style.border = '1px solid var(--border-color, #ccc)';
    
    // Create close button
    const closeButton = document.createElement('button');
    closeButton.textContent = '×';
    closeButton.style.position = 'absolute';
    closeButton.style.top = '10px';
    closeButton.style.right = '10px';
    closeButton.style.border = 'none';
    closeButton.style.background = 'none';
    closeButton.style.fontSize = '24px';
    closeButton.style.cursor = 'pointer';
    closeButton.style.color = 'var(--text-color, #666)';
    closeButton.addEventListener('mouseover', () => {
        closeButton.style.color = 'var(--icon-color, #2c7be5)';
    });
    closeButton.addEventListener('mouseout', () => {
        closeButton.style.color = 'var(--text-color, #666)';
    });
    closeButton.addEventListener('click', () => modal.remove());
    
    // Create header
    const header = document.createElement('div');
    header.style.marginBottom = '16px';
    header.style.display = 'flex';
    header.style.alignItems = 'center';
    // Create and append title element
    const titleEl = document.createElement('h2');
    titleEl.textContent = title;
    titleEl.style.margin = '0';
    titleEl.style.fontSize = '1.5em';
    header.appendChild(titleEl);
    
    // Create content area
    const contentArea = document.createElement('div');
    contentArea.style.fontFamily = 'monospace';
    contentArea.style.whiteSpace = 'pre-wrap';
    contentArea.style.fontSize = '14px';
    contentArea.innerHTML = content;
    
    // Create action buttons container if actions are provided
    if (actions) {
        const actionContainer = document.createElement('div');
        actionContainer.style.marginTop = '20px';
        actionContainer.style.borderTop = '1px solid #eee';
        actionContainer.style.paddingTop = '15px';
        actionContainer.style.display = 'flex';
        actionContainer.style.justifyContent = 'flex-end';
        actionContainer.style.gap = '10px';

        actions.forEach(action => {
            const button = document.createElement('button');
            button.textContent = action.text;
            button.style.padding = '8px 16px';
            button.style.borderRadius = '4px';
            button.style.border = 'none';
            button.style.cursor = 'pointer';
            button.style.fontSize = '14px';
            
            if (action.type === 'primary') {
                button.style.backgroundColor = '#007bff';
                button.style.color = 'white';
            } else if (action.type === 'danger') {
                button.style.backgroundColor = '#dc3545';
                button.style.color = 'white';
            } else {
                button.style.backgroundColor = '#6c757d';
                button.style.color = 'white';
            }
            
            button.addEventListener('mouseover', () => {
                button.style.opacity = '0.9';
            });
            button.addEventListener('mouseout', () => {
                button.style.opacity = '1';
            });
            button.addEventListener('click', () => {
                if (action.onClick) {
                    action.onClick();
                }
                modal.remove();
            });
            
            actionContainer.appendChild(button);
        });
        
        modalContent.appendChild(actionContainer);
    }
    
    // Assemble modal
    modalContent.appendChild(closeButton);
    modalContent.appendChild(header);
    modalContent.appendChild(contentArea);
    modal.appendChild(modalContent);
    
    // Close on click outside
    modal.addEventListener('click', (e) => {
        if (e.target === modal) modal.remove();
    });
    
    // Close on Escape key
    document.addEventListener('keydown', function escHandler(e) {
        if (e.key === 'Escape') {
            modal.remove();
            document.removeEventListener('keydown', escHandler);
        }
    });
    
    // Add to document
    document.body.appendChild(modal);
    return modal;
}

// Function to get current storage state
function getStorageState() {
    const state = {
        localStorage: {},
        sessionStorage: {},
        cookies: document.cookie.split(';').reduce((acc, cookie) => {
            const [key, value] = cookie.trim().split('=');
            if (key) acc[key] = value;
            return acc;
        }, {})
    };
    
    // Get localStorage items
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        let value = localStorage.getItem(key);
        // Mask sensitive data
        if (key.includes('token')) {
            value = '[REDACTED]';
        }
        state.localStorage[key] = value;
    }
    
    // Get sessionStorage items
    for (let i = 0; i < sessionStorage.length; i++) {
        const key = sessionStorage.key(i);
        state.sessionStorage[key] = sessionStorage.getItem(key);
    }
    
    return state;
}

// Function to format state for display
function formatState(state) {
    return `<div style="margin-bottom: 20px;">
        <h3 style="color: #333; margin-bottom: 10px;">localStorage:</h3>
        <pre style="background: #f5f5f5; padding: 10px; border-radius: 4px; max-height: 200px; overflow: auto;">${JSON.stringify(state.localStorage, null, 2)}</pre>
        
        <h3 style="color: #333; margin: 20px 0 10px;">sessionStorage:</h3>
        <pre style="background: #f5f5f5; padding: 10px; border-radius: 4px; max-height: 200px; overflow: auto;">${JSON.stringify(state.sessionStorage, null, 2)}</pre>
        
        <h3 style="color: #333; margin: 20px 0 10px;">Cookies:</h3>
        <pre style="background: #f5f5f5; padding: 10px; border-radius: 4px; max-height: 200px; overflow: auto;">${JSON.stringify(state.cookies, null, 2)}</pre>
    </div>`;
}

// Function to clear authentication data
export function clearAuthData() {
    console.debug('[Debug] Clear Auth button clicked - Starting auth data cleanup');
    
    // Get state before clearing
    const stateBefore = getStorageState();
    console.debug('auth-utils: before clearAuthData', { stateBefore });
    
    // Show state before clearing with action buttons
    showStateModal('State Before Clearing Auth Data', formatState(stateBefore), [
        {
            text: 'Cancel',
            type: 'secondary',
            onClick: () => console.log('Auth clear cancelled')
        },
        {
            text: 'Clear Auth Data',
            type: 'danger',
            onClick: () => {
                console.debug('auth-utils: clearAuthData action triggered');
                
                // Remove all authentication-related items from localStorage
                localStorage.removeItem('auth_token');
                localStorage.removeItem('access_token');
                localStorage.removeItem('id_token');
                localStorage.removeItem('user_info');
                localStorage.removeItem('login_timestamp');
                localStorage.removeItem('expires_at');
                localStorage.removeItem('oauth_state');
                console.debug('auth-utils: Removed items from localStorage', { 
                    auth_token: localStorage.getItem('auth_token'),
                    access_token: localStorage.getItem('access_token'),
                    id_token: localStorage.getItem('id_token'),
                    user_info: localStorage.getItem('user_info'),
                    login_timestamp: localStorage.getItem('login_timestamp'),
                    expires_at: localStorage.getItem('expires_at'),
                    oauth_state: localStorage.getItem('oauth_state')
                });
                
                // Clear session storage auth flags
                sessionStorage.removeItem('logged_out');
                sessionStorage.removeItem('just_logged_out');
                sessionStorage.removeItem('auth_redirect_count');
                console.debug('auth-utils: Removed items from sessionStorage', { 
                    logged_out: sessionStorage.getItem('logged_out'),
                    just_logged_out: sessionStorage.getItem('just_logged_out'),
                    auth_redirect_count: sessionStorage.getItem('auth_redirect_count')
                });
                
                // Clear auth cookies
                document.cookie = "logged_out=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                document.cookie = "manual_logout=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                console.debug('auth-utils: Cleared auth cookies');
                
                // Update UI elements
                const loginUserSpan = document.getElementById('loginUser');
                if (loginUserSpan) {
                    loginUserSpan.textContent = 'User: Not logged in';
                }
                
                // Reset tenant to default
                const tenantName = document.getElementById('tenantName');
                if (tenantName) {
                    tenantName.textContent = 'Tenant: Default';
                }
                const tenantInput = document.getElementById('tenantname');
                if (tenantInput) {
                    tenantInput.value = '';
                }
                
                // Sync with AuthManager if available
                if (window.AuthBackend) {
                    window.AuthBackend.clearAuthState();
                    window.AuthBackend.isUserAuthenticated = false;
                    window.AuthBackend.userInfo = null;
                    window.AuthBackend.tenant = 'Default';
                    console.debug('auth-utils: Synced with AuthBackend', { 
                        isUserAuthenticated: window.AuthBackend.isUserAuthenticated,
                        userInfo: window.AuthBackend.userInfo,
                        tenant: window.AuthBackend.tenant
                    });
                }
                
                // Get state after clearing
                const stateAfter = getStorageState();
                console.debug('auth-utils: after clearAuthData', { stateAfter });
                
                // Show state after clearing with reload option
                showStateModal('State After Clearing Auth Data', formatState(stateAfter), [
                    {
                        text: 'Continue',
                        type: 'secondary',
                        onClick: () => console.log('Continuing without reload')
                    },
                    {
                        text: 'Reload Page',
                        type: 'primary',
                        onClick: () => window.location.reload()
                    }
                ]);

                // Log the action
                console.log('Authentication data cleared');
                if (typeof window.Logging !== 'undefined' && typeof window.Logging.appendToLogs === 'function') {
                    window.Logging.appendToLogs('Authentication data cleared');
                }
            }
        }
    ]);
}

// Function to clear all localStorage data
export function clearAllData() {
    console.debug('[Debug] Clear All button clicked - Starting complete data cleanup');
    
    // Get state before clearing
    const stateBefore = getStorageState();
    console.debug('auth-utils: before clearAllData', { stateBefore });
    
    // Show state before clearing with action buttons
    showStateModal('State Before Clearing All Data', formatState(stateBefore), [
        {
            text: 'Cancel',
            type: 'secondary',
            onClick: () => console.log('Clear all cancelled')
        },
        {
            text: 'Clear All Data',
            type: 'danger',
            onClick: () => {
                console.debug('auth-utils: clearAllData action triggered');
                
                // Clear all localStorage data
                localStorage.clear();
                console.debug('auth-utils: Cleared localStorage');
                
                // Clear all sessionStorage data
                sessionStorage.clear();
                console.debug('auth-utils: Cleared sessionStorage');
                
                // Clear all auth cookies
                document.cookie = "logged_out=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                document.cookie = "manual_logout=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                console.debug('auth-utils: Cleared auth cookies');
                
                // Reset UI elements
                const loginUserSpan = document.getElementById('loginUser');
                if (loginUserSpan) {
                    loginUserSpan.textContent = 'User: Not logged in';
                }
                
                // Reset tenant information
                const tenantName = document.getElementById('tenantName');
                if (tenantName) {
                    tenantName.textContent = 'Tenant: Default';
                }
                const tenantInput = document.getElementById('tenantname');
                if (tenantInput) {
                    tenantInput.value = '';
                }
                
                // Sync with AuthManager if available
                if (window.AuthBackend) {
                    window.AuthBackend.clearAuthState();
                    window.AuthBackend.isUserAuthenticated = false;
                    window.AuthBackend.userInfo = null;
                    window.AuthBackend.tenant = 'Default';
                    console.debug('auth-utils: Synced with AuthBackend', { 
                        isUserAuthenticated: window.AuthBackend.isUserAuthenticated,
                        userInfo: window.AuthBackend.userInfo,
                        tenant: window.AuthBackend.tenant
                    });
                }
                
                // Get state after clearing
                const stateAfter = getStorageState();
                console.debug('auth-utils: after clearAllData', { stateAfter });
                
                // Show state after clearing with reload option
                showStateModal('State After Clearing All Data', formatState(stateAfter), [
                    {
                        text: 'Continue',
                        type: 'secondary',
                        onClick: () => console.log('Continuing without reload')
                    },
                    {
                        text: 'Reload Page',
                        type: 'primary',
                        onClick: () => window.location.reload()
                    }
                ]);

                // Log the action
                console.log('All data cleared');
                if (typeof window.Logging !== 'undefined' && typeof window.Logging.appendToLogs === 'function') {
                    window.Logging.appendToLogs('All data cleared');
                }
            }
        }
    ]);
}

// Make functions available globally for direct use in HTML
if (typeof window !== 'undefined') {
    window.clearAuthData = clearAuthData;
    window.clearAllData = clearAllData;
}