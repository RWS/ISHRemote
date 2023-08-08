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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.OpenApiISH30;
using System.Diagnostics;
using System.Runtime.InteropServices;
using IdentityModel.OidcClient.Infrastructure;

namespace Trisoft.ISHRemote.Connection
{
    internal sealed class InfoShareOpenApiConnection : IDisposable
    {
        #region Private Members
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// HttpClient. Incoming reused, probably Ssl/Tls initialized already.
        /// </summary>
        private HttpClient _httpClient;
        /// <summary>
        /// OpenApi InfoShare Web Service v3.0 NSwag generated client
        /// </summary>
        private OpenApiISH30Service _openApiISH30Service;
        /// <summary>
        /// Parameters that configure the connection behavior.
        /// </summary>
        private InfoShareOpenApiConnectionParameters _connectionParameters;
        /// <summary>
        /// Tracking standard dispose pattern
        /// </summary>
        private bool disposedValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <c>InfoShareOpenApiConnection</c> class.
        /// </summary>
        /// <param name="logger">Instance of Interfaces.ILogger implementation</param>
        /// <param name="httpClient">Incoming reused, probably Ssl/Tls initialized already.</param>
        /// <param name="infoShareOpenApiConnectionParameters">Connection parameters.</param>
        public InfoShareOpenApiConnection(ILogger logger, HttpClient httpClient, InfoShareOpenApiConnectionParameters infoShareOpenApiConnectionParameters)
        {
            _logger = logger;
            _httpClient = httpClient;
            _connectionParameters = infoShareOpenApiConnectionParameters;
            // Could to more strict _connectionParameters checks

            // Not attaching logging to OidcClient anyway, there is a bug that still does logging although not configured
            // See https://github.com/IdentityModel/IdentityModel.OidcClient/pull/67
            LogSerializer.Enabled = false;

            _logger.WriteDebug($"InfoShareOpenApiConnection InfoShareWSUrl[{_connectionParameters.InfoShareWSUrl}] IssuerUrl[{_connectionParameters.IssuerUrl}] AuthenticationType[{_connectionParameters.AuthenticationType}]");
            if (_connectionParameters.Tokens == null)
            {
                if ((string.IsNullOrEmpty(_connectionParameters.ClientId)) && (string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // attempt System Browser retrieval of Access/Bearer Token
                    _logger.WriteDebug($"InfoShareOpenApiConnection System Browser");
                    _connectionParameters.Tokens = GetTokensOverSystemBrowserAsync().GetAwaiter().GetResult();
                }
                else if ((!string.IsNullOrEmpty(_connectionParameters.ClientId)) && (!string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // Raw method without OidcClient works
                    //_connectionParameters.BearerToken = GetTokensOverClientCredentialsRaw();
                    _logger.WriteDebug($"InfoShareOpenApiConnection ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]");
                    _connectionParameters.Tokens = GetTokensOverClientCredentialsAsync().GetAwaiter().GetResult();
                }
                else
                {
                    throw new ArgumentException("Expected ClientId and ClientSecret to be not null or empty.");
                }
            }
            else 
            {
                // Don't think this will happen
                _logger.WriteDebug($"InfoShareOpenApiConnection reusing AccessToken[{ _connectionParameters.Tokens.AccessToken}] AccessTokenExpiration[{ _connectionParameters.Tokens.AccessTokenExpiration}]");
            }
            _logger.WriteDebug($"InfoShareOpenApiConnection Access Token received ValidTo[{_connectionParameters.Tokens.AccessTokenExpiration.ToString("yyyyMMdd.HHmmss.fff")}]");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _connectionParameters.Tokens.AccessToken);
            _logger.WriteDebug($"InfoShareOpenApiConnection using Normalized infoShareWSBaseUri[{_connectionParameters.InfoShareWSUrl}]"); 
            _openApiISH30Service = new Trisoft.ISHRemote.OpenApiISH30.OpenApiISH30Service(_httpClient);
            _openApiISH30Service.BaseUrl = new Uri(_connectionParameters.InfoShareWSUrl, "api").ToString();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets when access token should be refreshed (relative to its expiration time).
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>
        /// Create a /Wcf/API25/Annotation.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public OpenApiISH30Service GetOpenApiISH30ServiceProxy()
        {
            return _openApiISH30Service;
        }
        #endregion

        #region Token Handling, keep IN SYNC with InfoShareWcfSoapWithOpenIdConnectConnection
        /*
        /// <summary>
        /// Rough get Bearer/Access token based on class parameters without using OidcClient class library. Could be used for debugging
        /// </summary>
        /// <returns>Bearer Token</returns>
        private string GetTokensOverClientCredentialsRaw()
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
            _logger.WriteDebug($"GetTokensOverClientCredentialsRaw from requestUri[{requestUri}] resulted in BearerToken.Length[{tokenObject.access_Token.Length}]");
            return tokenObject.access_Token;
        }
        */

        /// <summary>
        /// OidcClient-based get Bearer/Access based on class parameters. Will refresh if possible.
        /// </summary>
        /// <param name="cancellationToken">Default</param>
        /// <returns>New Tokens with new or refreshed valeus</returns>
        private async Task<Tokens> GetTokensOverClientCredentialsAsync(CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            Tokens returnTokens = null;
            _logger.WriteDebug($"GetTokensOverClientCredentialsAsync from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] ClientSecret.Length[{_connectionParameters.ClientSecret.Length}]");
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = requestUri.ToString(),
                ClientId = _connectionParameters.ClientId,
                ClientSecret = _connectionParameters.ClientSecret
            };
            TokenResponse response = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest, cancellationToken).ConfigureAwait(false);

            // initial usage response.IsError throws error about System.Runtime.CompilerServices.Unsafe v5 required, but OidcClient needs v6
            if (response.IsError || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException($"GetTokensOverClientCredentialsAsync Access Error[{response.Error}]");
            }

            returnTokens = new Tokens
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
            };

            return returnTokens;
        }

        private async Task<Tokens> GetTokensOverSystemBrowserAsync(CancellationToken cancellationToken = default)
        {
            _logger.WriteDebug($"GetTokensOverSystemBrowserAsync from Authority[{_connectionParameters.IssuerUrl.ToString()}] using ClientAppId[{_connectionParameters.ClientAppId}] Scope[{_connectionParameters.Scope}]");

            var browser = new InfoShareOpenIdConnectSystemBrowser(_logger, _connectionParameters.RedirectUri);

            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var oidcClientOptions = new OidcClientOptions
            {
                Authority = _connectionParameters.IssuerUrl.ToString(),
                ClientId = _connectionParameters.ClientAppId,
                Scope = _connectionParameters.Scope,
                RedirectUri = redirectUri,
                FilterClaims = false,
                Policy = new Policy() { Discovery = new IdentityModel.Client.DiscoveryPolicy
                { 
                    ValidateIssuerName = false,  // Casing matters, otherwise "Error loading discovery document: "PolicyViolation" - "Issuer name does not match authority"
                    ValidateEndpoints = false  // Otherwise "Error loading discovery document: Endpoint belongs to different authority: https://mecdev12qa01.global.sdl.corp/ISHAMORA19/.well-known/openid-configuration/jwks"
                } },
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

            var result = new Tokens
            {
                AccessToken = loginResult.AccessToken,
                IdentityToken = loginResult.IdentityToken,
                RefreshToken = loginResult.RefreshToken,
                AccessTokenExpiration = loginResult.AccessTokenExpiration.LocalDateTime
            };
            return result;
        }

        private async Task<Tokens> RefreshTokensAsync(CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            Tokens returnTokens = null;
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
                throw new ApplicationException($"RefreshTokensAsync Refresh Error[{response.Error}]");
            }
            returnTokens = new Tokens
            {
                AccessToken = response.AccessToken,
                IdentityToken = response.IdentityToken,
                RefreshToken = response.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
            };
            return returnTokens;
        }

        /// <summary>
        /// Checks whether the token is issued and still valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                // we have the actual issued token which we can check for expiring
                if (_connectionParameters.Tokens.AccessTokenExpiration.Add(RefreshBeforeExpiration).ToUniversalTime() >= DateTime.UtcNow)
                {
                    //_logger.WriteDebug($"Access Token is valid ({_connectionParameters.Tokens.AccessTokenExpiration.Add(RefreshBeforeExpiration).ToUniversalTime()} >= {DateTime.UtcNow})");
                    return true;
                }
                else if (_connectionParameters.Tokens.AccessTokenExpiration.ToUniversalTime() >= DateTime.UtcNow)
                {
                    //_logger.WriteDebug($"Access Token refresh  ({_connectionParameters.Tokens.AccessTokenExpiration.ToUniversalTime()} >= {DateTime.UtcNow})");
                    _connectionParameters.Tokens = RefreshTokensAsync().GetAwaiter().GetResult();
                    _logger.WriteDebug($"InfoShareOpenApiConnection Access Token received ValidTo[{_connectionParameters.Tokens.AccessTokenExpiration.ToString("yyyyMMdd.HHmmss.fff")}]");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _connectionParameters.Tokens.AccessToken);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region IDisposable Methods
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_openApiISH30Service != null)
                    {
                        ((IDisposable)_openApiISH30Service).Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
