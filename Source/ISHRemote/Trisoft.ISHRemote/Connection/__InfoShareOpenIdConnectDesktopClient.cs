/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// Client used to obtain Access Tokens from Access Managements in a desktop client app.
    /// </summary>
    /// <remarks>Hat tip to /src/Clients/Tridion.AccessManagement.Client.Desktop/AccessManagementDesktopClient.cs</remarks>
    internal class InfoShareOpenIdConnectDesktopClient
    {
        private class Tokens
        {
            internal string AccessToken { get; set; }
            internal string IdentityToken { get; set; }
            internal string RefreshToken { get; set; }
            internal DateTime AccessTokenExpiration { get; set; }
        }

        private const string OidcScope = "openid profile email role forwarded offline_access";

        private readonly Policy _oidcPolicy = new Policy()
        {
            Discovery = new DiscoveryPolicy
            {
                ValidateIssuerName = false,
                RequireHttps = false
            }
        };

        private static readonly Dictionary<string, Tokens> _tokensCache = new Dictionary<string, Tokens>();
        private static readonly SemaphoreSlim _tokensCacheSemaphore = new SemaphoreSlim(1, 1);
        private readonly string _accessManagementBaseUrl;

        private string TokenEndpointUrl => $"{_accessManagementBaseUrl}/connect/token";

        private bool ConfigAwait => false; //UserAuthListener != null;

        /// <summary>
        /// Gets or sets when access token should be refreshed (relative to its expiration time).
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets a listener for user authentication events.
        /// </summary>
        public InfoShareOpenIdConnectLocalHttpEndpoint UserAuthListener { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="AccessManagementDesktopClient"/>.
        /// </summary>
        /// <param name="accessManagementBaseUrl">The Access Management Base URL.</param>
        public InfoShareOpenIdConnectDesktopClient(string accessManagementBaseUrl)
        {
            if (string.IsNullOrEmpty(accessManagementBaseUrl))
            {
                throw new ArgumentNullException(nameof(accessManagementBaseUrl));
            }
            _accessManagementBaseUrl = accessManagementBaseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Obtains an Access Token for given Client Credentials.
        /// </summary>
        /// <param name="clientId">The Client ID.</param>
        /// <param name="clientSecret">The Client Secret.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The Access Token.</returns>
        public async Task<string> GetAccessTokenForClientCredentialsAsync(
            string clientId,
            string clientSecret,
            CancellationToken cancellationToken = default)
        {
            Tokens tokens = null;
            await _tokensCacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                tokens = await GetCachedTokensAsync(clientId, cancellationToken).ConfigureAwait(false);
                if (tokens == null)
                {
                    using (var client = new HttpClient())
                    {
                        var tokenRequest = new ClientCredentialsTokenRequest
                        {
                            Address = TokenEndpointUrl,
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        };

                        TokenResponse response = await client.RequestClientCredentialsTokenAsync(tokenRequest, cancellationToken).ConfigureAwait(false);
                        if (response.IsError)
                        {
                            throw new ApplicationException("Error requesting Access Token: " + response.Error);
                        }

                        tokens = new Tokens
                        {
                            AccessToken = response.AccessToken,
                            RefreshToken = response.RefreshToken,
                            AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
                        };
                    }
                }
                _tokensCache[GetTokensCacheKey(clientId)] = tokens;
            }
            finally
            {
                _tokensCacheSemaphore.Release();
            }

            return tokens?.AccessToken;
        }

        /// <summary>
        /// Obtains a User Access Token for a given Application.
        /// </summary>
        /// <remarks>
        /// For an initial invocation, this will trigger an OpenID Connect user authentication flow in a browser.
        /// For subsequent invocations, it will returned cached Access Tokens (and it will refresh cached Access Tokens using Refresh Tokens).
        /// </remarks>
        /// <param name="applicationClientId">The Client ID of the Application to log in to.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The Access Token.</returns>
        public async Task<string> GetAccessTokenForUserAsync(string applicationClientId, CancellationToken cancellationToken = default)
        {
            await _tokensCacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                // First try if we have a valid cached access token (or we can obtain a new one using a cached refresh toen)
                Tokens tokens = await GetCachedTokensAsync(applicationClientId, cancellationToken).ConfigureAwait(ConfigAwait);

                // If not, we will have to start a user authentication flow (in a browser)
                tokens ??= await AuthenticateUserAsync(applicationClientId, cancellationToken).ConfigureAwait(ConfigAwait);

                _tokensCache[GetTokensCacheKey(applicationClientId)] = tokens;

                return tokens?.AccessToken;
            }
            finally
            {
                _tokensCacheSemaphore.Release();
            }
        }

        /// <summary>
        /// Signs out the user.
        /// </summary>
        /// <param name="applicationClientId">The Client ID of the Application triggering the sign out.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public async Task SignOutUserAsync(string applicationClientId, CancellationToken cancellationToken = default)
        {
            Tokens tokens = await GetCachedTokensAsync(applicationClientId, cancellationToken).ConfigureAwait(false);
            if (tokens != null)
            {
                _tokensCache.Remove(GetTokensCacheKey(applicationClientId));
            }

            var logoutRequest = new LogoutRequest
            {
                IdTokenHint = tokens?.IdentityToken
            };

            using (var localHttpEndpoint = new InfoShareOpenIdConnectLocalHttpEndpoint())
            {
                var oidcClientOptions = new OidcClientOptions
                {
                    Authority = _accessManagementBaseUrl,
                    ClientId = applicationClientId,
                    PostLogoutRedirectUri = localHttpEndpoint.BaseUrl,
                    Policy = _oidcPolicy
                };

                var oidcClient = new OidcClient(oidcClientOptions);
                string endSessionUrl = await oidcClient.PrepareLogoutAsync(logoutRequest, cancellationToken).ConfigureAwait(false);

                localHttpEndpoint.StartListening();

                // Run the OIDC end session flow in a system browser
                Process.Start(endSessionUrl);

                // Wait for HTTP GET signalling the end of the end session flow (front-channel notification).
                localHttpEndpoint.AwaitHttpRequest(cancellationToken);

                // Send HTTP response to the browser; this is rendered in a hidden iframe, so not visible to the end-user.
                await localHttpEndpoint.WriteHttpResponseAsync("text/plain", "Signed out.", cancellationToken).ConfigureAwait(false);

                // Wait a bit before shutting down the HTTP listener
                Thread.Sleep(100);
            }
        }


        private string GetTokensCacheKey(string clientId)
            => $"{clientId}@{_accessManagementBaseUrl}";


        private async Task<Tokens> GetCachedTokensAsync(string clientId, CancellationToken cancellationToken)
        {
            if (!_tokensCache.TryGetValue(GetTokensCacheKey(clientId), out Tokens tokens) || tokens == null)
            {
                // No cached tokens available
                return null;
            }

            // Only use cached tokens if the Access Token is not about to expire.
            if (tokens.AccessTokenExpiration > DateTime.Now + RefreshBeforeExpiration)
            {
                return tokens;
            }

            if (string.IsNullOrWhiteSpace(tokens.RefreshToken))
            {
                // Access token no longer valid and no refresh token available
                return null;
            }

            await RefreshTokensAsync(clientId, tokens, cancellationToken).ConfigureAwait(false);

            return tokens;
        }

        private async Task RefreshTokensAsync(string clientId, Tokens tokens, CancellationToken cancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                var refreshTokenRequest = new RefreshTokenRequest
                {
                    Address = TokenEndpointUrl,
                    ClientId = clientId,
                    RefreshToken = tokens.RefreshToken
                };

                TokenResponse response = await httpClient.RequestRefreshTokenAsync(refreshTokenRequest, cancellationToken).ConfigureAwait(false);
                if (response.IsError)
                {
                    throw new ApplicationException("Error requesting Refresh Token: " + response.Error);
                }

                tokens.AccessToken = response.AccessToken;
                tokens.IdentityToken = response.IdentityToken;
                tokens.RefreshToken = response.RefreshToken;
                tokens.AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn);
            }
        }

        private async Task<Tokens> AuthenticateUserAsync(string clientId, CancellationToken cancellationToken)
        {
            try
            {
                using (var localHttpEndpoint = new InfoShareOpenIdConnectLocalHttpEndpoint())
                {
                    var oidcClientOptions = new OidcClientOptions
                    {
                        Authority = _accessManagementBaseUrl,
                        ClientId = clientId,
                        Scope = OidcScope,
                        RedirectUri = localHttpEndpoint.BaseUrl,
                        Policy = new Policy()
                        {
                            Discovery = new DiscoveryPolicy
                            {
                                ValidateIssuerName = false,
                                RequireHttps = false
                            }
                        },
                        Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                        ResponseMode = OidcClientOptions.AuthorizeResponseMode.FormPost
                    };

                    var oidcClient = new OidcClient(oidcClientOptions);

                    AuthorizeState state = await oidcClient.PrepareLoginAsync(cancellationToken: cancellationToken).ConfigureAwait(ConfigAwait);

                    localHttpEndpoint.StartListening();

                    // Open system browser to start the OIDC authentication flow
                    Process.Start(state.StartUrl);

                    // Wait for HTTP POST signalling end of authentication flow
                    localHttpEndpoint.AwaitHttpRequest(cancellationToken);
                    string formdata = localHttpEndpoint.GetHttpRequestBody();

                    // Send an HTTP Redirect to Access Management logged in page.
                    await localHttpEndpoint.SendHttpRedirectAsync($"{_accessManagementBaseUrl}/Account/LoggedIn?clientId={clientId}", cancellationToken).ConfigureAwait(ConfigAwait);

                    LoginResult loginResult = await oidcClient.ProcessResponseAsync(formdata, state, cancellationToken: cancellationToken).ConfigureAwait(ConfigAwait);
                    if (loginResult.IsError)
                    {
                        throw new AccessManagementClientException("Error processing authentication response: " + loginResult.Error);
                    }
                    if (string.IsNullOrEmpty(loginResult.AccessToken))
                    {
                        throw new AccessManagementClientException("No Access Token received.");
                    }

                    var result = new Tokens
                    {
                        AccessToken = loginResult.AccessToken,
                        IdentityToken = loginResult.IdentityToken,
                        RefreshToken = loginResult.RefreshToken,
                        AccessTokenExpiration = loginResult.AccessTokenExpiration
                    };
                    return result;
                }
            }
            finally
            {
                UserAuthListener?.OnAuthFinished();
            }
        }
    }
}*/