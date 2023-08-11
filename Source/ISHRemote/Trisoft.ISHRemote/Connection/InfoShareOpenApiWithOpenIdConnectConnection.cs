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
    internal sealed class InfoShareOpenApiWithOpenIdConnectConnection : InfoShareOpenIdConnectConnectionBase, IDisposable
    {
        #region Private Members
        /// <summary>
        /// OpenApi InfoShare Web Service v3.0 NSwag generated client
        /// </summary>
        private OpenApiISH30Service _openApiISH30Service;
        /// <summary>
        /// Tracking standard dispose pattern
        /// </summary>
        private bool disposedValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <c>InfoShareOpenApiWithOpenIdConnectConnection</c> class.
        /// </summary>
        /// <param name="logger">Instance of Interfaces.ILogger implementation</param>
        /// <param name="httpClient">Incoming reused, probably Ssl/Tls initialized already.</param>
        /// <param name="infoShareOpenIdConnectConnectionParameters">Connection parameters.</param>
        public InfoShareOpenApiWithOpenIdConnectConnection(ILogger logger, HttpClient httpClient, InfoShareOpenIdConnectConnectionParameters infoShareOpenIdConnectConnectionParameters)
            : base(logger, httpClient, infoShareOpenIdConnectConnectionParameters)
        {
            // Not attaching logging to OidcClient anyway, there is a bug that still does logging although not configured
            // See https://github.com/IdentityModel/IdentityModel.OidcClient/pull/67
            LogSerializer.Enabled = false;
            var infoShareWSUrl = _connectionParameters.InfoShareWSUrl.ToString().Replace("OWcf/", "").Replace("OWcf", "");
            var infoShareWSUrlForOpenApi = (infoShareWSUrl.EndsWith("/")) ? new Uri(infoShareWSUrl) : new Uri(infoShareWSUrl.ToString() + "/");

            _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection InfoShareWSUrl[{infoShareWSUrlForOpenApi}] IssuerUrl[{_connectionParameters.IssuerUrl}] AuthenticationType[{_connectionParameters.AuthenticationType}]");
            if (_connectionParameters.Tokens == null)
            {
                if ((string.IsNullOrEmpty(_connectionParameters.ClientId)) && (string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // attempt System Browser retrieval of Access/Bearer Token
                    _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection System Browser");
                    _connectionParameters.Tokens = GetTokensOverSystemBrowserAsync().GetAwaiter().GetResult();
                }
                else if ((!string.IsNullOrEmpty(_connectionParameters.ClientId)) && (!string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // Raw method without OidcClient works
                    //_connectionParameters.BearerToken = GetTokensOverClientCredentialsRaw();
                    _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]");
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
                _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection reusing AccessToken[{ _connectionParameters.Tokens.AccessToken}] AccessTokenExpiration[{ _connectionParameters.Tokens.AccessTokenExpiration}]");
            }
            _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection Access Token received ValidTo[{_connectionParameters.Tokens.AccessTokenExpiration.ToString("yyyyMMdd.HHmmss.fff")}]");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _connectionParameters.Tokens.AccessToken);
            _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection using Normalized infoShareWSBaseUri[{infoShareWSUrlForOpenApi}]"); 
            _openApiISH30Service = new Trisoft.ISHRemote.OpenApiISH30.OpenApiISH30Service(_httpClient);
            _openApiISH30Service.BaseUrl = new Uri(infoShareWSUrlForOpenApi, "api").ToString();
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
                    //_logger.WriteDebug($"Access Token is valid ({_connectionParameters.InfoShareOpenIdConnectTokens.AccessTokenExpiration.Add(RefreshBeforeExpiration).ToUniversalTime()} >= {DateTime.UtcNow})");
                    return true;
                }
                else if (_connectionParameters.Tokens.AccessTokenExpiration.ToUniversalTime() >= DateTime.UtcNow)
                {
                    //_logger.WriteDebug($"Access Token refresh  ({_connectionParameters.InfoShareOpenIdConnectTokens.AccessTokenExpiration.ToUniversalTime()} >= {DateTime.UtcNow})");
                    _connectionParameters.Tokens = RefreshTokensAsync().GetAwaiter().GetResult();
                    _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection Access Token received ValidTo[{_connectionParameters.Tokens.AccessTokenExpiration.ToString("yyyyMMdd.HHmmss.fff")}]");
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
