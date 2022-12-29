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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Hat tip to /src/Clients/Tridion.AccessManagement.Client.Desktop/LocalHttpEndpoint.cs</remarks>
    internal class InfoShareOpenIdConnectLocalHttpEndpoint : IDisposable
    {
        private readonly HttpListener _httpListener;

        internal string BaseUrl { get; }

        internal string ErrorUrlPath { get; set; } = "/error";

        internal HttpListenerContext Context { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        internal InfoShareOpenIdConnectLocalHttpEndpoint()
        {
            BaseUrl = $"http://127.0.0.1:{GetFreePort()}";

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"{BaseUrl}/");
        }

        public void Dispose()
            => _httpListener.Stop();

        internal void StartListening()
            => _httpListener.Start();

        internal void AwaitHttpRequest(CancellationToken cancellationToken)
        {
            Task<HttpListenerContext> httpListenerContextTask = _httpListener.GetContextAsync();

            while (!httpListenerContextTask.IsCompleted)
            {
                Thread.Sleep(100);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (httpListenerContextTask.IsCanceled)
            {
                throw new TaskCanceledException();
            }
            else if (httpListenerContextTask.IsFaulted)
            {
                throw httpListenerContextTask.Exception;
            }

            Context = httpListenerContextTask.Result;

            HttpListenerRequest request = Context.Request;
            if (request != null && request.RawUrl.StartsWith(ErrorUrlPath, StringComparison.OrdinalIgnoreCase))
            {
                HandleErrorNotification(request.QueryString["msg"]);
            }
        }

        internal void HandleErrorNotification(string errorMessage)
        {
            WriteHttpResponseAsync("text/plain", "Error received.", default).Wait();
            Thread.Sleep(100);

            string exceptionMessage = "Error reported by Tridion Access Management";
            if (!string.IsNullOrEmpty(errorMessage))
            {
                exceptionMessage += $": {errorMessage}";
            }

            throw new ApplicationException(exceptionMessage);
        }

        internal string GetHttpRequestBody()
        {
            HttpListenerRequest request = Context?.Request;
            if (request == null || !request.HasEntityBody)
            {
                return null;
            }

            using (Stream bodyStream = request.InputStream)
            {
                using (var bodyStreamReader = new StreamReader(bodyStream, request.ContentEncoding))
                {
                    return bodyStreamReader.ReadToEnd();
                }
            }
        }

        internal async Task WriteHttpResponseAsync(string contentType, string responseBody, CancellationToken cancellationToken)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(responseBody);

            HttpListenerResponse response = Context.Response;
            response.ContentLength64 = buffer.Length;
            response.ContentType = contentType;

            using (Stream bodyStream = response.OutputStream)
            {
                await bodyStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            }
        }

        internal Task SendHttpRedirectAsync(string redirectUrl, CancellationToken cancellationToken)
        {
            string responseBody = $"<html><head><meta http-equiv='refresh' content='0;url={redirectUrl}'></head><body>Redirecting...</body></html>";
            return WriteHttpResponseAsync("text/html", responseBody, cancellationToken);
        }

        private static int GetFreePort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }
    }
}

