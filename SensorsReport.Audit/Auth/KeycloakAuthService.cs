using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SensorsReportAudit.Auth
{
    public class KeycloakAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuditConfig _config;
        private readonly ILogger<KeycloakAuthService> _logger;
        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

        private string _accessToken = string.Empty;
        private DateTime _tokenExpiry = DateTime.MinValue;
        private readonly TimeSpan _tokenRefreshThreshold = TimeSpan.FromMinutes(5);

        public KeycloakAuthService(AuditConfig config, ILogger<KeycloakAuthService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Gets a valid access token, refreshing if necessary
        /// </summary>
        public async Task<string> GetAccessTokenAsync()
        {
            await _tokenSemaphore.WaitAsync();
            try
            {
                // If token is still valid and not close to expiry
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow.Add(_tokenRefreshThreshold) < _tokenExpiry)
                {
                    return _accessToken;
                }

                // Token expired or close to expiry, get a new one
                await RefreshTokenAsync();
                return _accessToken;
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        /// <summary>
        /// Verifies if a token is valid by calling Keycloak introspection endpoint
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var discoveryDocument = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                {
                    Address = _config.KeycloakBaseUrl,
                    Policy = new DiscoveryPolicy
                    {
                        RequireHttps = false,
                        ValidateIssuerName = false
                    }
                });

                if (discoveryDocument.IsError)
                {
                    _logger.LogError("Discovery document error: {Error}", discoveryDocument.Error);
                    return false;
                }

                var introspectionRequest = new TokenIntrospectionRequest
                {
                    Address = _config.KeycloakIntrospectEndpoint,
                    ClientId = _config.KeycloakClientId,
                    ClientSecret = _config.KeycloakClientSecret,
                    Token = token
                };

                var response = await _httpClient.IntrospectTokenAsync(introspectionRequest);
                if (response.IsError)
                {
                    _logger.LogError("Token introspection error: {Error}", response.Error);
                    return false;
                }

                return response.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        /// <summary>
        /// Requests a new access token using client credentials
        /// </summary>
        private async Task RefreshTokenAsync()
        {
            try
            {
                var tokenRequest = new ClientCredentialsTokenRequest
                {
                    Address = _config.KeycloakTokenEndpoint,
                    ClientId = _config.KeycloakClientId,
                    ClientSecret = _config.KeycloakClientSecret
                };

                var response = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest);
                if (response.IsError)
                {
                    _logger.LogError("Token request error: {Error}", response.Error);
                    throw new Exception($"Failed to get access token: {response.Error}");
                }

                _accessToken = response.AccessToken!;
                
                // Parse expiry from token
                if (response.ExpiresIn > 0)
                {
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
                    _logger.LogDebug("Token refreshed, expires at: {Expiry}", _tokenExpiry);
                }
                else
                {
                    // Default expiry if not specified
                    _tokenExpiry = DateTime.UtcNow.AddHours(1);
                    _logger.LogWarning("Token expiry not specified, using default 1 hour");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                throw;
            }
        }
        
        /// <summary>
        /// Creates an HttpClient with the authorization header set
        /// </summary>
        public async Task<HttpClient> CreateAuthorizedClientAsync()
        {
            var client = new HttpClient();
            var token = await GetAccessTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}