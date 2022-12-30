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

namespace Trisoft.ISHRemote.Connection
{
    internal sealed class InfoShareOpenApiConnection : IDisposable
    {
        /// <summary>
        /// Gets or sets when access token should be refreshed (relative to its expiration time).
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);

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
        private bool disposedValue;
        #endregion

        public class Tokens
        {
            internal string AccessToken { get; set; }
            internal string IdentityToken { get; set; }
            internal string RefreshToken { get; set; }
            internal DateTime AccessTokenExpiration { get; set; }
        }

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

            _logger.WriteDebug($"InfoShareOpenApiConnection InfoShareWSUrl[{_connectionParameters.InfoShareWSUrl}] IssuerUrl[{_connectionParameters.IssuerUrl}] AuthenticationType[{_connectionParameters.AuthenticationType}]");
            if (string.IsNullOrEmpty(_connectionParameters.BearerToken))
            {
                if ((string.IsNullOrEmpty(_connectionParameters.ClientId)) || (string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // attempt System Browser retrieval of Access/Bearer Token
                    _logger.WriteDebug($"InfoShareOpenApiConnection System Browser");
                    Tokens tokens = GetTokensOverSystemBrowserAsync().GetAwaiter().GetResult();
                }
                else
                {
                    // Raw method without OidcClient works
                    _logger.WriteDebug($"InfoShareOpenApiConnection ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]");
                    _connectionParameters.BearerToken = GetNewBearerToken();
                    // OidcClient fails
                    // Tokens tokens = GetTokensOverClientCredentialsAsync(null).GetAwaiter().GetResult();
                    // _connectionParameters.BearerToken = tokens.AccessToken;
                }
            }
            else 
            {
                _logger.WriteDebug($"InfoShareOpenApiConnection reusing BearerToken[{ _connectionParameters.BearerToken}]");
                _connectionParameters.BearerToken = _connectionParameters.BearerToken;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _connectionParameters.BearerToken);
            _logger.WriteDebug($"InfoShareOpenApiConnection using Normalized infoShareWSBaseUri[{_connectionParameters.InfoShareWSUrl}]"); 
            _openApiISH30Service = new Trisoft.ISHRemote.OpenApiISH30.OpenApiISH30Service(_httpClient);
            _openApiISH30Service.BaseUrl = new Uri(_connectionParameters.InfoShareWSUrl, "api").ToString();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Rough get Bearer/Access token based on class parameters
        /// </summary>
        /// <returns>Bearer Token</returns>
        private string GetNewBearerToken()
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            _logger.WriteDebug($"GetNewBearerToken from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]" );

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
            _logger.WriteDebug($"GetNewBearerToken from requestUri[{requestUri}] resulted in BearerToken.Length[{tokenObject.access_Token.Length}]");
            return tokenObject.access_Token;
        }

        /// <summary>
        /// OidcClient-based get Bearer/Access based on class parameters. Will refresh if possible.
        /// </summary>
        /// <param name="tokens">Incoming tokens, can be null. Forcing new Access Token, or attempt Refresh</param>
        /// <param name="cancellationToken">Default</param>
        /// <returns>New Tokens with new or refreshed valeus</returns>
        private async Task<Tokens> GetTokensOverClientCredentialsAsync(Tokens tokens, CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            Tokens returnTokens = null;
            if ((tokens != null) && (tokens.AccessTokenExpiration.Add(RefreshBeforeExpiration) > DateTime.Now))  // skew 60 seconds
            {
                _logger.WriteDebug($"GetTokensOverClientCredentialsAsync from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] RefreshToken[{new string('*', tokens.RefreshToken.Length)}]");
                var refreshTokenRequest = new RefreshTokenRequest
                {
                    Address = requestUri.ToString(),
                    ClientId = _connectionParameters.ClientId,
                    RefreshToken = tokens.RefreshToken
                };
                TokenResponse response = await _httpClient.RequestRefreshTokenAsync(refreshTokenRequest, cancellationToken).ConfigureAwait(false);
                // initial usage response.IsError throws error about System.Runtime.CompilerServices.Unsafe v5 required, but OidcClient needs v6
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"GetTokensOverClientCredentialsAsync Refresh Error[{response.Error}]");
                }
                returnTokens = new Tokens
                {
                    AccessToken = response.AccessToken,
                    IdentityToken = response.IdentityToken,
                    RefreshToken = response.RefreshToken,
                    AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
                };
            }
            else // tokens where null, or expired
            {
                _logger.WriteDebug($"GetTokensOverClientCredentialsAsync from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]");
                var tokenRequest = new ClientCredentialsTokenRequest
                {
                    Address = requestUri.ToString(),
                    ClientId = _connectionParameters.ClientId,
                    ClientSecret = _connectionParameters.ClientSecret
                };
                TokenResponse response = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest, cancellationToken).ConfigureAwait(false);

                // initial usage response.IsError throws error about System.Runtime.CompilerServices.Unsafe v5 required, but OidcClient needs v6
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"GetTokensOverClientCredentialsAsync Access Error[{response.Error}]");
                }

                returnTokens = new Tokens
                {
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                    AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
                };
            }
            return returnTokens;
        }

        private async Task<Tokens> GetTokensOverSystemBrowserAsync(CancellationToken cancellationToken = default)
        {

            using (var localHttpEndpoint = new InfoShareOpenIdConnectLocalHttpEndpoint())
            {
                var oidcClientOptions = new OidcClientOptions
                {
                    Authority = _connectionParameters.IssuerUrl.ToString(),
                    ClientId = _connectionParameters.ClientId,
                    Scope = "openid profile email role forwarded offline_access",
                    RedirectUri = localHttpEndpoint.BaseUrl,
                    Policy = new Policy()
                    {
                        Discovery = new DiscoveryPolicy
                        {
                            ValidateIssuerName = false,
                            RequireHttps = false
                        }
                    }
                };
                var oidcClient = new OidcClient(oidcClientOptions);

                AuthorizeState state = await oidcClient.PrepareLoginAsync(cancellationToken: cancellationToken);

                localHttpEndpoint.StartListening();
                // Open system browser to start the OIDC authentication flow
                Process.Start(state.StartUrl);
                // Wait for HTTP POST signalling end of authentication flow
                localHttpEndpoint.AwaitHttpRequest(cancellationToken);
                string formdata = localHttpEndpoint.GetHttpRequestBody();

                // Send an HTTP Redirect to Access Management logged in page.
                await localHttpEndpoint.SendHttpRedirectAsync($"{_connectionParameters.IssuerUrl}/Account/LoggedIn?clientId={_connectionParameters.ClientId}", cancellationToken);

                LoginResult loginResult = await oidcClient.ProcessResponseAsync(formdata, state, cancellationToken: cancellationToken);
                if (loginResult.IsError)
                {
                    throw new ApplicationException($"GetTokensOverSystemBrowserAsync Error[{loginResult.Error}]");
                }
                if (string.IsNullOrEmpty(loginResult.AccessToken))
                {
                    throw new ApplicationException($"GetTokensOverSystemBrowserAsync No Access Token received.");
                }

                var result = new Tokens
                {
                    AccessToken = loginResult.AccessToken,
                    IdentityToken = loginResult.IdentityToken,
                    RefreshToken = loginResult.RefreshToken,
                    AccessTokenExpiration = loginResult.AccessTokenExpiration.LocalDateTime
                };
                return result;
            }
        }

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

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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

        #region Public Methods
        /// <summary>
        /// Create a /Wcf/API25/Annotation.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public OpenApiISH30Service GetOpenApiISH30ServiceProxy()
        {
            return _openApiISH30Service;
        }
        

        bool IsValid => throw new NotImplementedException();
        #endregion
    }
}
