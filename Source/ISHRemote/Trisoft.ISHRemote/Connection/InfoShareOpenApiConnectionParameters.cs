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
using System.Text;
using System.Threading.Tasks;

namespace Trisoft.ISHRemote.Connection
{
    internal sealed class InfoShareOpenApiConnectionParameters
    {
        private Uri _infoShareWSUrl;
        /// <summary>
        /// The clientconfiguration.xml discovery file tells us the configured ISHWS Url is "https://ish.example.com/ISHWS/" while you perhaps are doing https://localhost/ or behind the configured load balancer.
        /// </summary>
        public Uri InfoShareWSUrl {
            get { return _infoShareWSUrl; }
            set {
                var infoShareWSUrl = value.ToString().Replace("OWcf", "api");
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
        public Uri IssuerUrl { 
            get { return _issuerUrl; }
            set { _issuerUrl = (value.ToString().EndsWith("/")) ? value : new Uri(value.ToString() + "/"); } 
        }

        /// <summary>
        /// Access Token (if not specified, it is requested using provided <see cref="ClientId"/> and <see cref="ClientSecret"/>)
        /// </summary>
        public string BearerToken { get; set; } = String.Empty;
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
    }
}
