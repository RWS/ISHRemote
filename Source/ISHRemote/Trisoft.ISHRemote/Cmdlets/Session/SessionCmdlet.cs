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
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.Session
{
    /// <summary>
    /// Abstract class used for the session commandlets.
    /// </summary>
    /// <remarks>Inherits from <see cref="TrisoftCmdlet"/>.</remarks>
    public abstract class SessionCmdlet : TrisoftCmdlet
    {
        /// <summary>
        /// Solves the PS51/NET48 problem of OidcClient which continuously threw 
        /// 'Error connecting to https://sts.windows.net/{tenant}/.well-known/openid-configuration. 
        /// Operation is not valid due to the current state of the object' in GetDiscoveryDocumentAsync
        /// as described on https://github.com/IdentityModel/Documentation/issues/13
        /// <see cref="AppDomainModuleAssemblyInitializer"/>
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
#if NET48
            WriteVerbose("ISHRemote module on PS5.1/NET48 forces Assembly Redirects for System.Runtime.CompilerServices.Unsafe.dll/System.Text.Json.dll/IdentityModel.OidcClient.dll/Microsoft.Bcl.AsyncInterfaces.dll/System.Text.Encodings.Web.dll");
#else
            WriteVerbose("ISHRemote module on PS7.2+/NET60+ forces Assembly Redirects for IdentityModel.dll");
#endif
            //AppDomainAssemblyResolveHelper.Redirect(); is superseded with AppDomainModuleAssemblyInitializer based on IModuleAssemblyInitializer
        }

    }
}
