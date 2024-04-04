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
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;


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
    /// and https://stackoverflow.com/questions/62764744/could-not-load-file-or-assembly-system-runtime-compilerservices-unsafe and http://www.chilkatsoft.com/p/p_502.asp
    /// we can force a higher version to be loaded.</para>
    /// </summary>
    /// <remarks>All this is only required for .NET 4.8, so PowerShell 5.1. OidcClient worked 
    /// without changes on .NET 7.0, so PowerShell 7.3.1 at the time of writing.</remarks>
    internal static class AppDomainAssemblyResolveHelper
    {
        /// <summary>
        /// Making sure the ResolveEventHandler only happens once
        /// </summary>
        private static bool _isAppDomainAssemblyResolveHelperRegistered = false;

        /// <summary>
        /// Storing forcefully loaded assemblies in the dictionary at the time of writing on Windows11/NET4.8.1
        /// </summary>
        private static readonly ConcurrentDictionary<string, Assembly> _forcedLoadedAssemblies = new ConcurrentDictionary<string, Assembly>();



        /// <summary>
        /// NET 4.8 comes with System.Runtime.CompilerServices.Unsafe v4.0.4 while OidcClient 
        /// is compiled against v5.0.0 and OpenApi/NSwag expects v6.0.0. Few solutions describe to 
        /// do .config assemblyBinding redirects for System.Text.Json to minimally v5.0.2 and 
        /// System.Runtime.CompilerServices.Unsafe to minimally v5.0.0. 
        /// On PowerShell however, the powershell.exe.config is off limits, hence the below 
        /// AssemblyResolve solution.
        /// * System.Runtime.CompilerServices.Unsafe requested 4.0.4.1 or 5.0.0.0 but we now return 6.0.0.0
        /// * System.Text.Json requested 5.0.0.0 but we now return 5.0.0.2
        /// * IdentityModel.OidcClient requested but we now return
        /// * Microsoft.Bcl.AsyncInterfaces requested 5.0.0.0 but we now return 6.0.0.0
        /// * System.ComponentModel.Annotations requested 4.2.0.0 but we now return 5.0.0.0
        /// </summary>
        internal static void Redirect()
        {
            if (!_isAppDomainAssemblyResolveHelperRegistered)
            {
                //WriteWarning("Attaching AssemblyResolve handler to force System.Runtime.CompilerServices.Unsafe v6+ and System.Text.Json v5+.");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                
                string filePathSRCSUnsafe = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Runtime.CompilerServices.Unsafe.dll");
                var assemblySRCSUnsafe = Assembly.LoadFrom(filePathSRCSUnsafe);
                _forcedLoadedAssemblies.GetOrAdd("System.Runtime.CompilerServices.Unsafe", assemblySRCSUnsafe);


                string filePathSTJson = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Text.Json.dll");
                var assemblySTJson = Assembly.LoadFrom(filePathSTJson);
                _forcedLoadedAssemblies.GetOrAdd("System.Text.Json", assemblySTJson);

                string filePathIdentityModel = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"IdentityModel.dll");
                var assemblyIdentityModel = Assembly.LoadFrom(filePathIdentityModel);
                _forcedLoadedAssemblies.GetOrAdd("IdentityModel.OidcClient", assemblyIdentityModel);

                string filePathMBclAsyncInterfaces = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Microsoft.Bcl.AsyncInterfaces.dll");
                var assemblyMBclAsyncInterfaces = Assembly.LoadFrom(filePathMBclAsyncInterfaces);
                _forcedLoadedAssemblies.GetOrAdd("Microsoft.Bcl.AsyncInterfaces", assemblyMBclAsyncInterfaces);

                string filePathSCMAnnotations = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.ComponentModel.Annotations.dll");
                var assemblySCMAnnotations = Assembly.LoadFrom(filePathSCMAnnotations);
                _forcedLoadedAssemblies.GetOrAdd("System.ComponentModel.Annotations", assemblySCMAnnotations);

                _isAppDomainAssemblyResolveHelperRegistered = true;
            }
        }

        /// <summary>
        /// Return previously force-loaded (higher-version) assembly
        /// </summary>
        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            Assembly outAssembly = null;
            _forcedLoadedAssemblies.TryGetValue(name, out outAssembly);
            return outAssembly;
        }
    }
#endif
}
