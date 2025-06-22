/**
 * Authentication checker module
 * Simplified integration with the main auth-backend.js
 */

// Import the authentication manager
import { authManager } from './auth-backend.js';

// Function to check if user is authenticated (async, always checks status)
// export async function checkAuthentication() {
//   try {
//     console.debug('authChecker: Checking authentication status');
//     await authManager.checkAuthStatus();
//     if (authManager.isUserAuthenticated) {
//       console.debug('authChecker: User is authenticated');
//       sessionStorage.setItem('auth_redirect_count', '0');
//       return true;
//     } else {
//       console.debug('authChecker: User is not authenticated');
//       return false;
//     }
//   } catch (err) {
//     console.error('authChecker: Error checking authentication status', err);
//     showAuthError('Error checking authentication status: ' + (err?.message || err));
//     return false;
//   }
// }

// // Function to redirect to login page if not authenticated
// // Only redirects on protected pages, not all pages
// export async function redirectIfNotAuthenticated() {
//   const isMainPage = window.location.pathname === '/' || 
//                      window.location.pathname.endsWith('/') || 
//                      window.location.pathname.endsWith('index.html');
//   const redirectCount = parseInt(sessionStorage.getItem('auth_redirect_count') || '0');

//   // Always check/refresh auth status before acting
//   let authenticated = false;
//   try {
//     console.debug('authChecker: Refreshing authentication status');
//     await authManager.checkAuthStatus();
//     authenticated = authManager.isUserAuthenticated;
//   } catch (err) {
//     console.error('authChecker: Error checking authentication status', err);
//     showAuthError('Error checking authentication status: ' + (err?.message || err));
//     return false;
//   }

//   if (!authenticated && isMainPage && redirectCount < 3) {
//     console.warn('authChecker: Authentication failed, redirecting to login', { redirectCount });
//     // Diagnose why authentication failed
//     let details = '';
//     if (!navigator.cookieEnabled) {
//       details = '<li>Your browser is blocking cookies. Please enable cookies and try again.</li>';
//     } else if (window.location.protocol !== 'https:') {
//       details = '<li>Authentication cookies require HTTPS. Please use a secure connection.</li>';
//     } else {
//       details = '<li>Possible causes: session expired, cookies blocked, or server misconfiguration.</li>';
//     }
//     showAuthError('You are not authenticated. ' +
//       'The application could not detect a valid login session.<ul>' + details + '</ul>' +
//       '<p>What to do next:</p><ul>' +
//       '<li>Try logging in again.</li>' +
//       '<li>If the problem persists, clear your cookies and local storage, then reload the page.</li>' +
//       '<li>If using incognito/private mode, try a normal window.</li>' +
//       '<li>If you see repeated errors, contact support with a screenshot of this message.</li>' +
//       '</ul>');
//     // Increment the counter to detect loops
//     sessionStorage.setItem('auth_redirect_count', redirectCount + 1);
//     authManager.login();
//     return false;
//   }

//   if (redirectCount >= 3) {
//     console.error('authChecker: Authentication redirect loop detected');
//     showAuthError('Authentication redirect loop detected. ' +
//       'This usually means your browser is blocking cookies, or there is a server configuration problem.' +
//       '<ul><li>Try clearing your cookies and refreshing the page.</li>' +
//       '<li>If the problem persists, try a different browser or disable privacy extensions.</li></ul>');
//     sessionStorage.setItem('auth_redirect_count', '0');
//     return false;
//   }

//   if (authenticated) {
//     console.debug('authChecker: User is authenticated, resetting redirect count');
//     sessionStorage.setItem('auth_redirect_count', '0');
//   }
//   return true;
// }

// // Helper function to display auth error
// function showAuthError(message) {
//   // Check if error is already displayed
//   if (document.querySelector('.auth-error')) {
//     console.debug('authChecker: Error message already displayed, skipping');
//     return;
//   }
  
//   console.debug('authChecker: Displaying error message', { message });
//   // Display a friendly error message to the user
//   const errorMessage = document.createElement('div');
//   errorMessage.className = 'auth-error';
//   errorMessage.style.position = 'fixed';
//   errorMessage.style.top = '50%';
//   errorMessage.style.left = '50%';
//   errorMessage.style.transform = 'translate(-50%, -50%)';
//   errorMessage.style.backgroundColor = '#fff';
//   errorMessage.style.padding = '20px';
//   errorMessage.style.borderRadius = '5px';
//   errorMessage.style.boxShadow = '0 0 10px rgba(0,0,0,0.5)';
//   errorMessage.style.zIndex = '1000';
  
//   errorMessage.innerHTML = `
//     <h3>Authentication Error</h3>
//     <p>${message}</p>
//     <p>Possible causes:</p>
//     <ul>
//       <li>Session timeout</li>
//       <li>Network connectivity issues</li>
//       <li>Server configuration problems</li>
//     </ul>
//     <button id="retry-auth" style="padding: 8px 16px; margin-top: 10px; cursor: pointer;">Try Again</button>
//     <button id="clear-auth-data" style="padding: 8px 16px; margin-top: 10px; margin-left: 10px; cursor: pointer;">Clear Auth Data</button>
//   `;
  
//   // Add to the document
//   document.body.appendChild(errorMessage);
  
//   // Add event listener to retry button
//   document.getElementById('retry-auth')?.addEventListener('click', () => {
//     console.debug('authChecker: Retry authentication button clicked');
//     errorMessage.remove();
//     sessionStorage.setItem('auth_redirect_count', '0');
//     window.location.reload();
//   });
  
//   // Add event listener to clear auth data button
//   document.getElementById('clear-auth-data')?.addEventListener('click', () => {
//     console.debug('authChecker: Clear auth data button clicked');
//     errorMessage.remove();
//     localStorage.clear();
//     sessionStorage.clear();
//     document.cookie.split(";").forEach(function(c) {
//       document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
//     });
//     window.location.reload();
//   });
// }

// // Check auth status after a short delay to ensure the DOM is loaded
// // and auth status has been checked, but only on pages that need it
// setTimeout(async () => {
//   const isProtectedPage = window.location.pathname === '/' || 
//                           window.location.pathname.endsWith('/') || 
//                           window.location.pathname.endsWith('index.html');
//   if (isProtectedPage) {
//     console.log('Checking authentication status on protected page');
//     await redirectIfNotAuthenticated();
//   } else {
//     console.log('Not a protected page, skipping auth check');
//   }
// }, 500);

// export default {
//   checkAuthentication,
//   redirectIfNotAuthenticated
// };