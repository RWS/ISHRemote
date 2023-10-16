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
using System.Net;
using System.Text;

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// Optional InfoShare Wcf connection parameters.
    /// </summary>
    internal sealed class InfoShareOpenIdConnectConnectionParameters
    {
        private Uri _infoShareWSUrl;
        /// <summary>
        /// The clientconfiguration.xml discovery file tells us the configured ISHWS Url is "https://ish.example.com/ISHWS/" while you perhaps are doing https://localhost/ or behind the configured load balancer.
        /// </summary>
        public Uri InfoShareWSUrl
        {
            get { return _infoShareWSUrl; }
            set
            {
                var infoShareWSUrl = value.ToString();
                _infoShareWSUrl = (infoShareWSUrl.EndsWith("/")) ? new Uri(infoShareWSUrl) : new Uri(infoShareWSUrl.ToString() + "/");
            }
        }

        /// <summary>
        /// Client Application Id as configured on Access Management which allows a http://127.0.0.1:SomePort redirect url
        /// </summary>
        public string ClientAppId { get; set; }

        /// <summary>
        /// Existing scopes as configured on Access Management
        /// </summary>
        public string Scope { get; set; } = "openid profile email role forwarded offline_access";

        private string _redirectUri = null;
        /// <summary>
        /// When Sign In succeeded the browser is redirect to this link. Typically https://ish.example.com/ISHAM/Account/loggedIn?clientId=c826e7e1-c35c-43fe-9756-e1b61f44bb40 where the ClientId GUID is the ISHAM Account.
        /// </summary>
        public string RedirectUri
        {
            get
            {
                if (string.IsNullOrEmpty(_redirectUri))
                { return $"{IssuerUrl}/Account/LoggedIn?clientId={ClientId}"; }
                else
                { return "https://www.rws.com"; }
            }
            set { _redirectUri = value; }
        }

        /// <summary>
        /// The clientconfiguration.xml discovery file tells us the configured Issuer AuthenticationType. Expected values are WindowsMixed, UserNameMixed and AccessManagement.
        /// </summary>
        public string AuthenticationType { get; set; }

        private Uri _issuerUrl;
        /// <summary>
        /// The clientconfiguration.xml discovery file tells us the configured Issuer Url. Expected values are .../issue/wstrust/mixed/username or .../ISHAM/.
        /// </summary>
        public Uri IssuerUrl
        {
            get { return _issuerUrl; }
            set { _issuerUrl = (value.ToString().EndsWith("/")) ? value : new Uri(value.ToString() + "/"); }
        }
        /// <summary>
        /// Collects various tokens with expiration date
        /// </summary>
        public InfoShareOpenIdConnectTokens Tokens { get; set; } = null;
        /// <summary>
        /// ClientId to request for an access token
        /// </summary>
        public string ClientId { get; set; } = String.Empty;
        /// <summary>
        /// ClientSecret to request for an access token
        /// </summary>
        public string ClientSecret { get; set; } = String.Empty;

        /// <summary>
        /// Timeout to control Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml
        /// </summary>
        public TimeSpan Timeout { get; set; }
        /// <summary>
        /// Timeout to control Send/Receive timeouts of WCF when issuing a token
        /// </summary>
        public TimeSpan IssueTimeout { get { return Timeout; } }
        /// <summary>
        /// Timeout to control Send/Receive timeouts of WCF for InfoShareWS proxies
        /// </summary>
        public TimeSpan ServiceTimeout { get { return Timeout; } }
        /// <summary>
        /// Timeout to control the wait of the interactive system browser localhost redirect flow
        /// </summary>
        public TimeSpan SystemBrowserTimeout { get; set; }
        /// <summary>
        /// If True, certificate validation for HTTPS and the Service will be skipped
        /// </summary>
        public bool IgnoreSslPolicyErrors { get; set; } = false;
    }
}
