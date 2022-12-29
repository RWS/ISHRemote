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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.OpenApiISH30;

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
            _connectionParameters = infoShareOpenApiConnectionParameters;
            _logger.WriteDebug($"InfoShareOpenApiConnection InfoShareWSUrl[{_connectionParameters.InfoShareWSUrl}] IssuerUrl[{_connectionParameters.IssuerUrl}] AuthenticationType[{_connectionParameters.AuthenticationType}]");
            if (string.IsNullOrEmpty(_connectionParameters.BearerToken))
            {
                _logger.WriteDebug($"InfoShareOpenApiConnection ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]");
                _connectionParameters.BearerToken = GetNewBearerToken();
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
            string tokenResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var tokenObject = JsonConvert.DeserializeAnonymousType(tokenResponse, new { access_Token = "" });
            _logger.WriteDebug($"GetNewBearerToken from requestUri[{requestUri}] resulted in BearerToken[{tokenObject.access_Token}]");
            return tokenObject.access_Token;
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
