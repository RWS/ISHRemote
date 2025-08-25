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

using IdentityModel.Client;
using IdentityModel.OidcClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trisoft.ISHRemote.Interfaces;

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// OpenIdConnect base class to attempt to single-source all token retrieval functions. To be shared between OpenApiWithOpenIdConnect and WcfSoapWithOpenIdConnect
    /// </summary>
    internal abstract class InfoShareOpenIdConnectConnectionBase
    {
        #region Private Members
        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;
        /// <summary>
        /// HttpClient. Incoming reused, probably Ssl/Tls initialized already.
        /// </summary>
        protected HttpClient _httpClient;
        /// <summary>
        /// Parameters that configure the connection behavior.
        /// </summary>
        protected InfoShareOpenIdConnectConnectionParameters _connectionParameters;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a base instance of <c>InfoShareOpenIdConnectConnectionBase</c> class.
        /// </summary>
        /// <param name="logger">Instance of Interfaces.ILogger implementation</param>
        /// <param name="httpClient">Incoming reused, probably Ssl/Tls initialized already.</param>
        /// <param name="infoShareOpenIdConnectConnectionParameters">OpenIdConnect connection parameters to be shared with WcfSoapWithOpenIdConnect and OpenApiWithOpenIdConnect</param>
        public InfoShareOpenIdConnectConnectionBase(ILogger logger, HttpClient httpClient, InfoShareOpenIdConnectConnectionParameters infoShareOpenIdConnectConnectionParameters)
        {
            _logger = logger;
            _httpClient = httpClient;
            _connectionParameters = infoShareOpenIdConnectConnectionParameters;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets when access token should be refreshed (relative to its expiration time). Default skew time is 3 minutes.
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(3);
        #endregion Public Properties


        /*
        /// <summary>
        /// Rough get Bearer/Access token based on class parameters without using OidcClient class library. Could be used for debugging
        /// </summary>
        /// <returns>Bearer Token</returns>
        protected string GetTokensOverClientCredentialsRaw()
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            _logger.WriteDebug($"GetTokensOverClientCredentialsRaw from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]" );

            FormUrlEncodedContent credentialsForm = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _connectionParameters.ClientId),
                new KeyValuePair<string, string>("client_secret", _connectionParameters.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            HttpResponseMessage response = _httpClient.PostAsync(requestUri, credentialsForm).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            // response holds something like
            // {"access_token":"eyJhbGciOiJSUzI1NiIsImtpZCI6IjA5RTNGMzY3NDdCMEVCODMzOUNDNERENENGQzdDQ0M1N0FBQjQwRkRSUzI1NiIsIng1dCI6IkNlUHpaMGV3NjRNNXpFM1V6OGZNeFhxclFQMCIsInR5cCI6ImF0K2p3dCJ9.eyJpc3MiOiJodHRwczovL21lY2RldjEycWEwMS5nbG9iYWwuc2RsLmNvcnAvaXNoYW1vcmExOSIsIm5iZiI6MTY3MjM5MDI5NywiaWF0IjoxNjcyMzkwMjk3LCJleHAiOjE2NzIzOTM4OTcsImF1ZCI6WyJUcmlkaW9uX0RvY3NfQ29udGVudF9NYW5hZ2VyX0FwaSIsIlRyaWRpb25fRG9jc19XZWJfRXh0ZW5zaW9uc19BcGkiLCJUcmlkaW9uLkFjY2Vzc01hbmFnZW1lbnQiXSwic2NvcGUiOlsiVHJpZGlvbl9Eb2NzX0NvbnRlbnRfTWFuYWdlcl9BcGkiLCJUcmlkaW9uX0RvY3NfV2ViX0V4dGVuc2lvbnNfQXBpIiwiVHJpZGlvbi5BY2Nlc3NNYW5hZ2VtZW50Il0sImNsaWVudF9pZCI6ImM4MjZlN2UxLWMzNWMtNDNmZS05NzU2LWUxYjYxZjQ0YmI0MCIsInN1YiI6ImM4MjZlN2UxLWMzNWMtNDNmZS05NzU2LWUxYjYxZjQ0YmI0MCIsInVzZXJfaWQiOiIzOTYiLCJyb2xlIjpbIlRyaWRpb24uQWNjZXNzTWFuYWdlbWVudC5BZG1pbmlzdHJhdG9yIiwiVHJpZGlvbi5Eb2NzLkNvbnRlbnRNYW5hZ2VyLkFkbWluaXN0cmF0b3IiLCJUcmlkaW9uLkRvY3MuQ29udGVudE1hbmFnZXIuVXNlciIsIlRyaWRpb24uRG9jcy5XZWIuRXh0ZW5zaW9ucy5Vc2VyIl0sImp0aSI6IkMyMkNGQjhDMzVDQzNDN0VBODI3OUI5QkYyOTU5NkY1In0.oPKgzEkLkgaOqmb25uXVQzK4pNh72TBHRFl2ycnX5rHvoheBzsaGasqTwNVtzlCVbnUJkxjPV_pevUSR4dkB6UpgTqvsEfk_AeXVw-f_Nz250fAwjug0Xongp7un5VCFjSiNFUdCBfpBV0fLadyTAWAjMfr1XaFJhoDGk3lCOiH59WvcWazkr5C8LDQt129bCDEZZs3aWMf-TiAxwOkfVmEAcJz-KFz4BwgfhzqAd5sJI98mIfFx_aXEAFt7JcwWKhgwxLleYKKx2sXbL8sFQ2oe8S0e5HR7AQonNx6ygAw9Q1317_Y-fdGHDmGM7SC6Z7EUAsKH9-r2Uf4AuCBR1w","expires_in":3600,"token_type":"Bearer","scope":"Tridion_Docs_Content_Manager_Api Tridion_Docs_Web_Extensions_Api Tridion.AccessManagement"}
            string tokenResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var tokenObject = JsonConvert.DeserializeAnonymousType(tokenResponse, new { access_Token = "" });
            _logger.WriteDebug($"GetTokensOverClientCredentialsRaw from requestUri[{requestUri}] resulted in AccessToken.Length[{tokenObject.access_Token.Length}]");
            return tokenObject.access_Token;
        }
        */

        /// <summary>
        /// OidcClient-based get Bearer/Access based on class parameters. Will refresh if possible.
        /// </summary>
        /// <param name="cancellationToken">Default</param>
        /// <returns>New InfoShareOpenIdConnectTokens with new or refreshed valeus</returns>
        protected async Task<InfoShareOpenIdConnectTokens> GetTokensOverClientCredentialsAsync(CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            InfoShareOpenIdConnectTokens returnTokens = null;
            _logger.WriteDebug($"GetTokensOverClientCredentialsAsync from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] ClientSecret.Length[{_connectionParameters.ClientSecret.Length}]");
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = requestUri.ToString(),
                ClientId = _connectionParameters.ClientId,
                ClientSecret = _connectionParameters.ClientSecret
            };
            TokenResponse response = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest, cancellationToken).ConfigureAwait(false);
            if (response.IsError || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException($"GetTokensOverClientCredentialsAsync Access Error[{response.Error}]; either invalid ClientId/ClientSecret combination or expired ClientSecret.");
            }

            returnTokens = new InfoShareOpenIdConnectTokens
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
            };

            return returnTokens;
        }

        /// <summary>
        /// Returns a valid Access Token that can be used as Issued Token or Bearer Token on the various communication technologies.
        /// If using 'Authorization Code Flow with PKCE' and the Access Token is almost expired or expired, then a silent refresh token flow will be triggered. Else a full interactive browser flow should be triggered somewhere else including new Connections.
        /// If using 'Client Credentials Flow' and the Access Token is almost expired or expired, then a silent 'Client Credentials Flow' will be triggered.
        /// IsAccessTokenRefreshed holds if the earlier Access Token was returned, or a new one over Refresh Token or Client Credentials.
        /// </summary>
        /// <remarks>Function historically used to call GetTokensOverSystemBrowserAsync, because of interactive browser flow, this is now decided on a higher layer.</remarks>
        protected (string Value, bool IsAccessTokenRefreshed) GetAccessToken()
        {
            // Check if the token is expired, and attempt to get a new one
            bool isAccessTokenRefreshed = false;
            if (DateTime.Now.Add(RefreshBeforeExpiration) > _connectionParameters.Tokens.AccessTokenExpiration)
            {
                _logger.WriteVerbose($"InfoShareOpenIdConnectConnectionBase Access Token is (almost) expired (" +
                    DateTime.Now.Add(RefreshBeforeExpiration).ToString("yyyyMMdd.HHmmss.fff") +
                    " > " +
                    _connectionParameters.Tokens.AccessTokenExpiration.Add(RefreshBeforeExpiration).ToString("yyyyMMdd.HHmmss.fff") +
                    "), attempting refresh");

                if ((string.IsNullOrEmpty(_connectionParameters.ClientId)) && (string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // For authentication code flow, refreshing the token.
                    _logger.WriteDebug($"InfoShareOpenIdConnectConnectionBase Refresh Token");
                    _connectionParameters.Tokens = RefreshTokensAsync().GetAwaiter().GetResult();
                    isAccessTokenRefreshed = true;
                }
                else if ((!string.IsNullOrEmpty(_connectionParameters.ClientId)) && (!string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // For client credentials flow, getting a new token
                    _logger.WriteDebug($"InfoShareOpenIdConnectConnectionBase Client Credentials");
                    _connectionParameters.Tokens = GetTokensOverClientCredentialsAsync().GetAwaiter().GetResult();
                    isAccessTokenRefreshed = true;
                }
                else
                {
                    throw new ArgumentException("Expected ClientId and ClientSecret to be not null or empty. How did you get here?");
                }
            }

            return (_connectionParameters.Tokens.AccessToken, isAccessTokenRefreshed);
        }

        protected async Task<InfoShareOpenIdConnectTokens> GetTokensOverSystemBrowserAsync(CancellationToken cancellationToken = default)
        {
            _logger.WriteDebug($"GetTokensOverSystemBrowserAsync from Authority[{_connectionParameters.IssuerUrl.ToString()}] using ClientAppId[{_connectionParameters.ClientAppId}] Scope[{_connectionParameters.Scope}]");

            var browser = new InfoShareOpenIdConnectSystemBrowser(_logger, _connectionParameters.RedirectUri, _connectionParameters.SystemBrowserTimeout);

            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var oidcClientOptions = new OidcClientOptions
            {
                Authority = _connectionParameters.IssuerUrl.ToString(),
                ClientId = _connectionParameters.ClientAppId,
                Scope = _connectionParameters.Scope,
                RedirectUri = redirectUri,
                FilterClaims = false,
                Policy = new Policy()
                {
                    Discovery = new IdentityModel.Client.DiscoveryPolicy
                    {
                        ValidateIssuerName = false,  // Casing matters, otherwise "Error loading discovery document: "PolicyViolation" - "Issuer name does not match authority"
                        ValidateEndpoints = false  // Otherwise "Error loading discovery document: Endpoint belongs to different authority: https://mecdev12qa01.global.sdl.corp/ISHAMORA19/.well-known/openid-configuration/jwks"
                    }
                },
                Browser = browser,
                IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
                RefreshTokenInnerHttpHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true
                }
            };

#if NET48
            // Certificate validation works different on .NET Framework 4.8 versus .NET (Core) 6.0+, below is a catch all
            // bypass for /.well-known/openid-configuration detection. Otherwise you get error 
            // "Error loading discovery document: Error connecting to /.well-known/openid-configuration. Operation is not valid due to the current state of the object..'"
            oidcClientOptions.BackchannelHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true
            };
#endif

            var oidcClient = new OidcClient(oidcClientOptions);
            var loginResult = await oidcClient.LoginAsync(new LoginRequest());
            if (loginResult.IsError)
            {
                throw new ApplicationException($"GetTokensOverSystemBrowserAsync Error[{loginResult.Error}]");
            }
            var result = new InfoShareOpenIdConnectTokens
            {
                AccessToken = loginResult.AccessToken,
                IdentityToken = loginResult.IdentityToken,
                RefreshToken = loginResult.RefreshToken,
                AccessTokenExpiration = loginResult.AccessTokenExpiration.LocalDateTime
            };
            return result;
        }

        protected async Task<InfoShareOpenIdConnectTokens> RefreshTokensAsync(CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            InfoShareOpenIdConnectTokens returnTokens = null;
            _logger.WriteDebug($"RefreshTokensAsync from requestUri[{requestUri}] using ClientAppId[{_connectionParameters.ClientAppId}] RefreshToken.Length[{_connectionParameters.Tokens.RefreshToken.Length}]");
            var refreshTokenRequest = new RefreshTokenRequest
            {
                Address = requestUri.ToString(),
                ClientId = _connectionParameters.ClientAppId,
                RefreshToken = _connectionParameters.Tokens.RefreshToken
            };
            TokenResponse response = await _httpClient.RequestRefreshTokenAsync(refreshTokenRequest, cancellationToken).ConfigureAwait(false);
            // initial usage response.IsError throws error about System.Runtime.CompilerServices.Unsafe v5 required, but OidcClient needs v6
            if (response.IsError || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException($"RefreshTokensAsync Refresh Error[{response.Error}]; likely an expired Refresh Token so please rebuild a IShSession connection.");
            }
            returnTokens = new InfoShareOpenIdConnectTokens
            {
                AccessToken = response.AccessToken,
                IdentityToken = response.IdentityToken,
                RefreshToken = response.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
            };
            return returnTokens;
        }
    }
}
