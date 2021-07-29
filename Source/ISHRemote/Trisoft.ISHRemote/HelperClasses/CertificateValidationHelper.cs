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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Text.RegularExpressions;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.Cmdlets;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// Provides optional callback overwrite and restore function. 
    /// Do note however that ServicePointManager.ServerCertificateValidationCallback changes apply to the whole AppDomain.
    /// </summary>
    public static class CertificateValidationHelper
    {
        private static RemoteCertificateValidationCallback _orignalCallback;
        private static readonly ILogger _logger = TrisoftCmdletLogger.Instance();

        private static bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            string host = ((HttpWebRequest)sender).Address.Host;
            // [Could] CertificateValidationHelper could get finer granualarity through regex on which hostnames to bypass ssl certificate validation (e.g. Regex.IsMatch(host, validator.HostNameRegEx, RegexOptions.IgnoreCase))
            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.RemoteCertificateChainErrors:
                    {
                        //_logger.WriteDebug($"CertificateValidationHelper RemoteCertificateChainErrors host[{host}]");
                        return true;
                    }
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    {
                        //_logger.WriteDebug($"CertificateValidationHelper RemoteCertificateNameMismatch host[{host}]");
                        return true;
                    }
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    {
                        //_logger.WriteDebug($"CertificateValidationHelper RemoteCertificateNotAvailable host[{host}]");
                        return true;
                    }
                case SslPolicyErrors.None:
                default:
                    return true;
            }
        }

        /// <summary>
        /// Sets our custom AppDomain ssl/certificate overwrite callback using ServicePointManager, including a backup of any existing callback 
        /// </summary>
        public static void OverrideCertificateValidation()
        {
            _logger.WriteWarning("Applying certificate validation overwrite for the AppDomain. (OnValidateCertificate)");
            _orignalCallback = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnValidateCertificate);
            ServicePointManager.Expect100Continue = true;
        }

        /// <summary>
        /// Removes our custom AppDomain ssl/certificate overwrite callback using ServicePointManager by restoring our ealier backup of any existing callback 
        /// </summary>
        public static void RestoreCertificateValidation()
        {
            _logger.WriteDebug("Restoring backup of the earlier saved certificate validation for the AppDomain. (OnValidateCertificate)");
            ServicePointManager.ServerCertificateValidationCallback = _orignalCallback;
        }
    }
}
