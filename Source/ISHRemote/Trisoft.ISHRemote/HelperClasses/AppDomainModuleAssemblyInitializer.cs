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

using System.Collections.Concurrent;
using System.IO;
using System.Management.Automation;
using System.Reflection;
#if NET48
using System;
#else
using System.Runtime.Loader;
#endif

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// PROBLEM:
    /// Solves the PS51/NET48 problem of OidcClient which continuously threw 
    /// 'Error connecting to https://sts.windows.net/{tenant}/.well-known/openid-configuration. 
    /// Operation is not valid due to the current state of the object' in GetDiscoveryDocumentAsync
    /// as described on https://github.com/IdentityModel/Documentation/issues/13
    /// 
    /// EARLIER SOLUTION:
    /// See AppDomainAssemblyResolveHelper.cs which was loaded in typical first cmdlet a 
    /// SessionCmdlet.cs
    /// 
    /// THIS SOLUTION:
    /// NET 4.8 comes with System.Runtime.CompilerServices.Unsafe v4.0.4 while OidcClient 
    /// is compiled against v5.0.0 and OpenApi/NSwag expects v6.0.0. Few solutions describe to 
    /// do .config assemblyBinding redirects for System.Text.Json to minimally v5.0.2 and 
    /// System.Runtime.CompilerServices.Unsafe to minimally v5.0.0. 
    /// On PowerShell however, the powershell.exe.config is off limits, hence the below 
    /// AssemblyResolve solution.
    /// 
    /// MAPPINGS:
    /// * System.Runtime.CompilerServices.Unsafe requested 4.0.4.1 or 5.0.0.0 but we now return 6.0.0.0
    /// * System.Text.Json requested 5.0.0.0 but we now return 5.0.0.2
    /// * IdentityModel.OidcClient requested but we now return
    /// * Microsoft.Bcl.AsyncInterfaces requested 5.0.0.0 but we now return 6.0.0.0
    /// </summary>
    /// <remarks>Focus was to getting this working on NETFramework, more implementation is required to align with
    /// proposed solution of https://devblogs.microsoft.com/powershell/resolving-powershell-module-assembly-dependency-conflicts/
    /// and https://github.com/rjmholt/ModuleDependencyIsolationExample/blob/master/new/JsonModule.Cmdlets/JsonModuleInitializer.cs 
    /// regarding Engine introduction.</remarks>
    public class AppDomainModuleAssemblyInitializer : IModuleAssemblyInitializer
    {
        /// <summary>
        /// Storing forcefully loaded assemblies in the dictionary at the time of writing on Windows11/NET4.8.1
        /// </summary>
        private static readonly ConcurrentDictionary<string, Assembly> _forcedLoadedAssemblies = new ConcurrentDictionary<string, Assembly>();

        private static string binaryFolderPath = Path.GetFullPath(
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                ".."));

        private static string binaryCommonAssembliesFolderPath = Path.Combine(binaryFolderPath, "Common");

#if NET48
        private static string binaryNetFrameworkAssembliesPath = Path.Combine(binaryFolderPath, "net48");
#else
        private static string binaryNetCoreAssembliesPath = Path.Join(binaryFolderPath, "net6.0");
#endif

        /// <summary>
        /// Early registration of my AssemblyResolve call.
        /// </summary>
        /// <remarks>Earlier than the AppDomainAssemblyResolveHelper.cs which was loaded in typical first cmdlet a SessionCmdlet.cs</remarks>
        public void OnImport()
        {
#if NET48
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly_NetFramework;

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Runtime.CompilerServices.Unsafe.dll");
            var assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd("System.Runtime.CompilerServices.Unsafe", assembly);

            filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Text.Json.dll");
            assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd("System.Text.Json", assembly);

             filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Microsoft.Bcl.AsyncInterfaces.dll");
            assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd("Microsoft.Bcl.AsyncInterfaces", assembly);

             filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Text.Encodings.Web.dll");
            assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd("System.Text.Encodings.Web", assembly);

            filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"IdentityModel.dll");
            assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd("IdentityModel", assembly);

            filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"System.Memory.dll");
            assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd("System.Memory", assembly);
#else
            AssemblyLoadContext.Default.Resolving += ResolveAssembly_NetCore;

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"IdentityModel.dll");
            var assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd("IdentityModel", assembly);
            
#endif
        }

#if NET48

        /// <summary>
        /// Return previously force-loaded (higher-version) assembly
        /// </summary>
        private static Assembly ResolveAssembly_NetFramework(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            Assembly outAssembly = null;
            _forcedLoadedAssemblies.TryGetValue(name, out outAssembly);
            return outAssembly;

            /*
            // In .NET Framework, we must try to resolve ALL assemblies under the dependency dir here
            // This essentially means we are combining the .NET Core ALC and resolve events into one here
            // Note that:
            //   - This is not a recommended usage of Assembly.LoadFile()
            //   - Even doing this will not bypass the GAC

            // Parse the assembly name to get the file name
            var asmName = new AssemblyName(args.Name);
            var dllFileName = $"{asmName.Name}.dll";

            // Look for the DLL in our .NET Framework directory
            string frameworkAsmPath = Path.Combine(binaryNetFrameworkAssembliesPath, dllFileName);
            if (File.Exists(frameworkAsmPath))
            {
                return Assembly.LoadFile(frameworkAsmPath);
            }

            // Now look in the dependencies directory to resolve .NET Standard dependencies
            string commonAsmPath = Path.Combine(binaryCommonAssembliesFolderPath, dllFileName);
            if (File.Exists(commonAsmPath))
            {
                return Assembly.LoadFile(commonAsmPath);
            }

            // We've run out of places to look
            return null;
            */
        }

#else

        private static Assembly ResolveAssembly_NetCore(
            AssemblyLoadContext assemblyLoadContext,
            AssemblyName assemblyName)
        {
            var name = assemblyName.Name;
            Assembly outAssembly = null;
            _forcedLoadedAssemblies.TryGetValue(name, out outAssembly);
            return outAssembly;

            /*
            // In .NET Core, PowerShell deals with assembly probing so our logic is much simpler
            // We only care about our Engine assembly
            if (!assemblyName.Name.Equals("JsonModule.Engine"))
            {
                return null;
            }

            // Now load the Engine assembly through the dependency ALC, and let it resolve further dependencies automatically
            return DependencyAssemblyLoadContext.GetForDirectory(binaryCommonAssembliesFolderPath).LoadFromAssemblyName(assemblyName);
            */
        }

#endif

    }
}