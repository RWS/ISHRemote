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
    /// ISHRemote will listen to the Redirect Url (typically 127.0.01 and a free port) where the System Browser will federate out for authentication but eventually will call back to ISHRemote with InfoShareOpenIdConnectTokens
    /// </summary>
    /// <remarks>Hat tip to /src/Clients/Tridion.AccessManagement.Client.Desktop/LocalHttpEndpoint.cs</remarks>
    internal class InfoShareOpenIdConnectLocalHttpEndpoint : IDisposable
    {
        private readonly HttpListener _httpListener;

        internal string BaseUrl { get; }

        internal string ErrorUrlPath { get; set; } = "/error";

        internal HttpListenerContext Context { get; private set; }

        TaskCompletionSource<string> _source = new TaskCompletionSource<string>();

        public InfoShareOpenIdConnectLocalHttpEndpoint(int port, string path = null)
        {
            path = path ?? String.Empty;
            if (path.StartsWith("/")) path = path.Substring(1);
            BaseUrl = $"http://127.0.0.1:{port}/{path}";

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(BaseUrl);

            _httpListener.Start();
        }

        /// <summary>
        /// Listener has to be kept alive to allow 2nd redirect to page showing 'You are now signed in.'/'This browser tab can be closed.' to be shown.
        /// </summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                // TODO [Should] Refactor sleep-thread for closing http listener for System Browser modern authentication to explicit stop/start listening
                await Task.Delay(20000);
                _httpListener.Stop();
            });
        }



        public Task<string> WaitForCallbackAsync(int timeoutInSeconds = 300, CancellationToken cancellationToken = default)
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

            _source.TrySetResult(request.RawUrl); // Context?.Request.QueryString.Value  // request.QueryString.ToString(); #... .Value;

            return _source.Task;
        }

        #region Private Functions

        private void HandleErrorNotification(string errorMessage)
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

        private string GetHttpRequestBody()
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

        private async Task WriteHttpResponseAsync(string contentType, string responseBody, CancellationToken cancellationToken)
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

        public Task SendHttpRedirectAsync(string redirectUrl, CancellationToken cancellationToken)
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
        #endregion


    }
}