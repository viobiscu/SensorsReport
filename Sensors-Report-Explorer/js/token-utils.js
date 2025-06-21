// token-utils.js
export function setupTokenButton() {
    const showTokenButton = document.getElementById('showTokenButton');
    if (showTokenButton) {
        showTokenButton.addEventListener('click', function() {
            console.debug('[setupTokenButton] showTokenButton clicked');
            waitForAuthBackendAndShowToken();
        });
    } else {
        console.warn('[setupTokenButton] showTokenButton not found in DOM');
    }
}

export function showTokenDialog() {
    console.debug('[showTokenDialog] called');
    // Always use the robust wait function
    waitForAuthBackendAndShowToken();
}

export function waitForAuthBackendAndShowToken(retries = 10, delay = 500) {
    console.debug(`[waitForAuthBackendAndShowToken] called, retries left: ${retries}`);
    if (window.AuthBackend && typeof window.AuthBackend.showToken === 'function') {
        console.log('waitForAuthBackendAndShowToken: AuthBackend.showToken is available, calling it.');
        window.AuthBackend.showToken();
    } else if (retries > 0) {
        console.log(`waitForAuthBackendAndShowToken: waiting, attempts left: ${retries}`);
        setTimeout(() => waitForAuthBackendAndShowToken(retries - 1, delay), delay);
    } else {
        console.error('waitForAuthBackendAndShowToken: AuthBackend.showToken still not available after retries');
        const token = localStorage.getItem('access_token') || localStorage.getItem('auth_token');
        if (token) {
            alert('Token is available but the display functionality is not ready. Using simple display instead.');
            if (window.mainEditor) {
                window.mainEditor.setValue(JSON.stringify({
                    message: "Authentication Token (Simple View)",
                    token_preview: token.substring(0, 15) + '...' + token.substring(token.length - 10),
                    note: "This is a simplified view. The full token viewer is not available."
                }, null, 2));
            }
        } else {
            alert('Token display functionality is not available and no token was found in localStorage.');
        }
    }
}