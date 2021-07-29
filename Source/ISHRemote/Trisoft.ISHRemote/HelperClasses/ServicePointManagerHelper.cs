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
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.Cmdlets;
using System.Net;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// Provides configuration helper functions for low level network options. 
    /// </summary>
    public static class ServicePointManagerHelper
    {
        private static readonly ILogger _logger = TrisoftCmdletLogger.Instance();

        /// <summary>
        /// Removes our custom AppDomain ssl/certificate overwrite callback using ServicePointManager by restoring our ealier backup of any existing callback 
        /// </summary>
        public static void RestoreCertificateValidation()
        {
            _logger.WriteDebug("Enabling Tls, Tls11 and Tls12 security protocols");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }
    }
}
