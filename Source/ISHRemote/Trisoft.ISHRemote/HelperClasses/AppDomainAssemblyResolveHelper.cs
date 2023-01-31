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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trisoft.ISHRemote.HelperClasses
{
#if NET48
    /// <summary>
    /// <para>Solves the PS51/NET48 problem of OidcClient which continuously threw 
    /// 'Error connecting to https://sts.windows.net/{tenant}/.well-known/openid-configuration. 
    /// Operation is not valid due to the current state of the object' in GetDiscoveryDocumentAsync
    /// as described on https://github.com/IdentityModel/Documentation/issues/13 </para>
    /// <para>NET 4.8 comes with System.Runtime.CompilerServices.Unsafe v4.0.4 while OidcClient 
    /// is compiled against v5.0.0 and OpenApi/NSwag expects v6.0.0. Few solutions describe to 
    /// do .config assemblyBinding redirects for System.Text.Json to minimally v5.0.2 and 
    /// System.Runtime.CompilerServices.Unsafe to minimally v5.0.0</para>
    /// <para>On PowerShell however, the powershell.exe.config is off limits, hence the below 
    /// AssemblyResolve solution.</para>
    /// <para>Hat tip to https://stackoverflow.com/questions/1460271/how-to-use-assembly-binding-redirection-to-ignore-revision-and-build-numbers/2344624#2344624
    /// and https://stackoverflow.com/questions/62764744/could-not-load-file-or-assembly-system-runtime-compilerservices-unsafe
    /// we can force a higher version to be loaded.</para>
    /// </summary>
    /// <remarks>All this is only required for .NET 4.8, so PowerShell 5.1. OidcClient worked 
    /// without changes on .NET 7.0, so PowerShell 7.3.1 at the time of writing.</remarks>
    internal static class AppDomainAssemblyResolveHelper
    {
        /// <summary>
        /// Making sure the ResolveEventHandler only happens once
        /// </summary>
        private static bool _isRegistered = false;

        /// <summary>
        /// NET 4.8 comes with System.Runtime.CompilerServices.Unsafe v4.0.4 while OidcClient 
        /// is compiled against v5.0.0 and OpenApi/NSwag expects v6.0.0. Few solutions describe to 
        /// do .config assemblyBinding redirects for System.Text.Json to minimally v5.0.2 and 
        /// System.Runtime.CompilerServices.Unsafe to minimally v5.0.0. 
        /// On PowerShell however, the powershell.exe.config is off limits, hence the below 
        /// AssemblyResolve solution.
        /// </summary>
        internal static void Redirect()
        {
            if (!_isRegistered)
            {
                //WriteWarning("Attaching AssemblyResolve handler to force System.Runtime.CompilerServices.Unsafe v6+ and System.Text.Json v5+.");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                
                string filePathSRCSUnsafe = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Runtime.CompilerServices.Unsafe.dll");
                //WriteDebug($"Forcefully loading filePathSRCSUnsafe[{filePathSRCSUnsafe}]");
                var assemblySRCSUnsafe = Assembly.LoadFrom(filePathSRCSUnsafe);

                string filePathSTJson = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Text.Json.dll");
                //WriteDebug($"Forcefully loading filePathSTJson[{filePathSTJson}]");
                var assemblySTJson = Assembly.LoadFrom(filePathSTJson);

                _isRegistered = true;
            }
        }

        /// <summary>
        /// Return previously force-loaded (higher-version) assembly
        /// </summary>
        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "System.Runtime.CompilerServices.Unsafe")
            {
                return typeof(System.Runtime.CompilerServices.Unsafe).Assembly;
            }
            else if (name.Name == "System.Text.Json")
            {
                return typeof(System.Text.Json.JsonDocument).Assembly;
            }
            return null;
        }
    }
#endif
}
