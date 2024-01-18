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
                    // Raw method without OidcClient, see GetTokensOverClientCredentialsRaw();
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
            _logger.WriteDebug($"InfoShareOpenApiWithOpenIdConnectConnection using Normalized infoShareWSBaseUri[{infoShareWSUrlForOpenApi}]"); 
            _openApiISH30Service = new Trisoft.ISHRemote.OpenApiISH30.OpenApiISH30Service(_httpClient);
            _openApiISH30Service.BaseUrl = new Uri(infoShareWSUrlForOpenApi, "api").ToString();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Create an OpenAPI InfoShare 3.0 proxy
        /// HttpClient with OpenIdConnect authentication need a way to pass the Access/Bearer token.
        /// This method wraps the token up in an authentication/bearer token.
        /// </summary>
        /// <returns>The proxy</returns>
        public OpenApiISH30Service GetOpenApiISH30ServiceProxy()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
            return _openApiISH30Service;
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
