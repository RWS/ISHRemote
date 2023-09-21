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
    internal sealed class InfoShareWcfSoapWithWsTrustConnectionParameters
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
            set { _issuerUrl = value; }
        }

        /// <summary>
        /// The connection credential.
        /// </summary>
        public NetworkCredential Credential { get; set; }
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
        /// If True, authenticate immediately; otherwise, authenticate on the first service request.
        /// </summary>
        public bool AutoAuthenticate { get; set; } = false;
        /// <summary>
        /// If True, certificate validation for HTTPS and the Service will be skipped
        /// </summary>
        public bool IgnoreSslPolicyErrors { get; set; } = false;
    }
}
