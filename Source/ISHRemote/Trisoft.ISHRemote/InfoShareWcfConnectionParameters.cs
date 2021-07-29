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

namespace Trisoft.ISHRemote
{
    /// <summary>
    /// Optional InfoShare Wcf connection parameters.
    /// </summary>
    internal sealed class InfoShareWcfConnectionParameters
    {
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
        public TimeSpan IssueTimeout { get; set; }
        /// <summary>
        /// Timeout to control Send/Receive timeouts of WCF for InfoShareWS proxies
        /// </summary>
        public TimeSpan ServiceTimeout { get; set; }
        /// <summary>
        /// If True, authenticate immediately; otherwise, authenticate on the first service request.
        /// </summary>
        public bool AutoAuthenticate { get; set; }
    }
}
