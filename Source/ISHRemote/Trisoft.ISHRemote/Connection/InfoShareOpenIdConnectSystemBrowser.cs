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

using Duende.IdentityModel.OidcClient.Browser;
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
    /// <summary>
    /// Knows how to reliably launch your default web browser (the one that opens any https:// url in any application) across the supported platforms Windows, Linux and MacOS
    /// </summary>
    /// <remarks>
    /// Intentionally internal, not public: this class implements Duende.IdentityModel.OidcClient.Browser.IBrowser,
    /// with method signatures (InvokeAsync) that reference BrowserOptions/BrowserResult types from that assembly.
    /// If this class were public, PowerShell's binary module loader (PSSnapInHelpers.GetAssemblyTypes, via
    /// assembly.ExportedTypes) would need to fully resolve Duende.IdentityModel.OidcClient while discovering
    /// cmdlets - before IModuleAssemblyInitializer.OnImport() ever runs. On machines where a different build of
    /// Duende.IdentityModel.OidcClient is registered in the Global Assembly Cache under the same strong name
    /// identity (observed with Microsoft Intune Management Extension installed), that early, uncontrollable
    /// resolution can end up loading the GAC copy instead of ISHRemote's own bundled one - see
    /// AppDomainModuleAssemblyInitializer.cs for the full history of this investigation. Keeping this class
    /// internal removes it from ExportedTypes entirely, so that early reflection pass never needs to touch
    /// Duende.IdentityModel.OidcClient at all.
    /// </remarks>
    internal class InfoShareOpenIdConnectSystemBrowser : IBrowser
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;
        public string RedirectUrl = "https://www.rws.com";
        public TimeSpan SystemBrowserTimeout = new TimeSpan(0,1,30);
        public int Port { get; }
        private readonly string _path;

        public InfoShareOpenIdConnectSystemBrowser(ILogger logger, string redirectUrl, TimeSpan systemBrowserTimeout, int? port = null, string path = null)
        {
            _logger = logger;
            RedirectUrl = redirectUrl;
            SystemBrowserTimeout = systemBrowserTimeout;
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
            _logger.WriteDebug($"InfoShareOpenIdConnectSystemBrowser InvokeAsync port[{Port}] path[{_path}] systemBrowserTimeout[{SystemBrowserTimeout}]");
            using (var listener = new InfoShareOpenIdConnectLocalHttpEndpoint(Port, _path))
            {
                OpenBrowser(options.StartUrl);

                try
                {
                    var result = await listener.WaitForCallbackAsync(Convert.ToInt32(SystemBrowserTimeout.TotalSeconds));

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
            // NOTE: this method is only ever called with options.StartUrl — the IDP authorization
            // endpoint URL. The OIDC redirect callback (http://127.0.0.1:PORT/?code=...) is
            // returned by listener.WaitForCallbackAsync() and never reaches here.
            //
            // Validate that url is a well-formed http/https URI before passing to Process.Start.
            // Breaks taint flow for CodeQL cs/uncontrolled-process-creation (CWE-78) by rejecting
            // dangerous schemes (javascript:, file:, cmd:, ms-settings:, etc.). Both http and https
            // are allowed so that internal/dev IDPs without TLS (IgnoreSslPolicyErrors scenarios)
            // continue to work. The re-serialised validatedUri.AbsoluteUri is used throughout so
            // the tainted raw string never reaches any process-creation call.
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri validatedUri) ||
                (validatedUri.Scheme != Uri.UriSchemeHttps && validatedUri.Scheme != Uri.UriSchemeHttp))
            {
                throw new ArgumentException($"InfoShareOpenIdConnectSystemBrowser requires a well-formed http or https url[{url}]");
            }
            try
            {
                Process.Start(validatedUri.AbsoluteUri);
            }
            catch
            {
                // Optimized to bypass issue https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //validatedUri.AbsoluteUri = validatedUri.AbsoluteUri.Replace("&", "^&");
                    //Process.Start(new ProcessStartInfo("cmd", $"/c start {validatedUri.AbsoluteUri}") { CreateNoWindow = true });
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = validatedUri.AbsoluteUri,
                        UseShellExecute = true
                    };
                    Process.Start(processStartInfo);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", validatedUri.AbsoluteUri);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", validatedUri.AbsoluteUri);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
