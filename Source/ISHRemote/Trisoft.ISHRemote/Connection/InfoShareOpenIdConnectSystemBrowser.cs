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

using IdentityModel.OidcClient.Browser;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trisoft.ISHRemote.Interfaces;

namespace Trisoft.ISHRemote.Connection
{
    public class InfoShareOpenIdConnectSystemBrowser : IBrowser
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;
        public string RedirectUrl = "https://www.rws.com"; 
        public int Port { get; }
        private readonly string _path;

        public InfoShareOpenIdConnectSystemBrowser(ILogger logger, string redirectUrl, int? port = null, string path = null)
        {
            _logger = logger;
            RedirectUrl = redirectUrl;
            _path = path;

            if (!port.HasValue)
            {
                Port = GetRandomUnusedPort();
            }
            else
            {
                Port = port.Value;
            }
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
        {
            _logger.WriteDebug($"InfoShareOpenIdConnectSystemBrowser InvokeAsync port[{Port}] path[{_path}]");
            using (var listener = new InfoShareOpenIdConnectLocalHttpEndpoint(Port, _path))
            {
                OpenBrowser(options.StartUrl);

                try
                {
                    var result = await listener.WaitForCallbackAsync();

                    _logger.WriteDebug($"InfoShareOpenIdConnectSystemBrowser SendHttpRedirectAsync RedirectUrl[{RedirectUrl}]");
                    await listener.SendHttpRedirectAsync(RedirectUrl, cancellationToken);

                    if (String.IsNullOrWhiteSpace(result))
                    {
                        return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
                    }

                    return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
                }
                catch (TaskCanceledException ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
                }
                catch (Exception ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
                }
            }
        }

        public void OpenBrowser(string url)
        {
            _logger.WriteDebug($"InfoShareOpenIdConnectSystemBrowser OpenBrowser url[{url}]");
            try
            {
                Process.Start(url);
            }
            catch
            {
                // Optimized to bypass issue https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //url = url.Replace("&", "^&");
                    //Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    Process.Start(processStartInfo);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}