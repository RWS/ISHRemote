# The Execution of the plan of ISHRemote v7.0

This page will try to track work in progress. And because I work on it in free time, it will help trace how I got where I am in the first place plus what is next. Inspired by [ThePlan-ISHRemote-7.0.md](./ThePlan-ISHRemote-7.0.md)

# Problem
1. First attempt under #81 was porting WCF-SOAP, but that got blocked as mentioned in [ThePlan-ISHRemote-7.0.md](./ThePlan-ISHRemote-7.0.md)
2. Second attempt under #115 on ASMX-SOAP using cross-platform .NET Standard 2.0 library - catering for PowerShell 5.1 and NET Framework 4.8 and PowerShell 7.1+ and NET (Core) 3.1+ - is seemingly bocked by error `This operation is not supported on .NET Standard as Reflection.Emit is not available.` Browsing further I find
    * https://github.com/dotnet/winforms/issues/860 stating "We don't support SOAP in .NET Core. You can try using WebAPI (for hosting) and HttpClient (for consumption)." of November 2019 by a Microsoft employee.
    * https://github.com/dotnet/standard/issues/857 litterally reads "System.Web.Services.Protocols.Soap not supported on .net Standard" and is still open on March 2021.
    * https://github.com/dotnet/runtime/issues/26007 reads "Should there be a .Net Standard 2.0 version of Reflection.Emit?" where Microsoft employees mention that ".Net Standard 2.0 libraries can't use typeBuilder.CreateType() (even though this code works both on .Net Framework 4.6.1 and .Net Core 2.0)" is intentional to not work in .NET Standard 2.0 (while it did work in 1.1 by a hack). So "you can use Reflection.Emit if you are writing .NET Core app or library you just cannot use it while writing .NET Standard library currently".
1. Taking the route of multi-targetting, having `ISHRemote.psm1` resolve it just-in-time.

# Cmdlet Progress and Compatilibity

A table that describes what works, where cmdlets have been rewired, where tests have been adapted (potentially indicating compatibility issues).

1. Runtime compatibility, currently aiming for
    * PowerShell 5.1 and NET Framework 4.7.2
    * PowerShell 7.1+ and NET (Core) 3.1+
1. Public class `IshSession`
    1. Removed WCF proxies, for now the ASMX-SOAP client are internal only
    1. The logic of 'Internal' as provided by ISHDeploy's cmdlet `Enable-ISHIntegrationSTSInternalAuthentication` enabling `https://ish.example.com/ISHWS/Internal/` is removed as that is about WCF-SOAP home realm discovery.
1. Cmdlet `New-IshSession`
    1. Removed parameter sets `ActiveDirectory`, `ActiveDirectory-ExplicitIssuer`, `UserNamePassword-ExplicitIssuer` and `PSCredential-ExplicitIssuer` are removed as that is about WCF-SOAP home realm discovery or Windows Authentication.
    1. Parameters `-WsTrustIssuerUrl` and `-WsTrustIssuerMexUrl` are removed as that is about WCF-SOAP home realm discovery.
    1. Parameter `-IshUserName` can no longer be empty, the default of empty to force `NetworkCredentials` for Windows Authentication doesn't make sense on ASMX-SOAP.
    1. Parameters `-TimeoutIssue` and `-TimeoutService` are removed as they belong to WCF-SOAP.

# Project Creation
1. The starting point was just-before ISHRemote v0.13 release. Removed the `\ISHRemote\Source\ISHRemote` folder. Keeping the `\ISHRemote\Source\Tools` folder, holding `XmlDoc2CmdletDoc`.
1. Used Visual Studio 2019 to create a new .NET Standard Class Library in folder `\ISHRemote\Source\ISHRemote` with project name `Trisoft.ISHRemote` and solution name `ISHRemote` (like it was before). Renamed folders `\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote` to trigger source control alignment with the previous solution, file history, etc
1. Through `Manage NuGet Package`, added a reference for `PowerShellStandard.Library` version `7.0.0-preview.1`.
1. Bumped initial `Trisoft.ISHRemote.dll` assembly and file version to 7.0.0.0, but better build.props or script integration is required.
1. Moving from `ISHRemoteCmdlets : PSSnapIn`, so the PowerShell 1.0 way, to the future. ([VMware](https://blogs.vmware.com/PowerCLI/2016/11/saying-farewell-snapins.html)) ... removed the class in total.
1. Started to comment out non-working code using `//TODO [Must] ISHRemotev7+ Cleanup`
1. To solve Folder25 base folder usage in `TrisoftCmdlet.cs`, I added a "WCF Web Service Reference Provider" (ahum, although it is ASMX) pointing to `https://medevddemeyer10.global.sdl.corp/InfoShareWSDITA/folder25.asmx` as name space `Trisoft.ISHRemote.Folder25ServiceReference` using default settings (always generate message contracts OFF; reuse types ON) and access level for generated class INTERNAL and generate SYNCHRONOUS operations.
    1. Did Find/Replace of `https://medevddemeyer10.global.sdl.corp/InfoShareWSDITA/folder25.asmx` with `https://ish.example.com/ISHWS/`
    1. (optional) Prepened every `Reference.cs`namespace with `Trisoft.ISHRemote.`
    1. Choosing the `Application25Soap12`, Soap12 binding, and perhaps should force httpS
1. Variation on adding ASMX reference to resolve folder usage in `TrisoftCmdlet.cs`
    1. In a command window that has access to `dotnet` SDK tooling, run `dotnet tool install --global dotnet-svcutil`
    1. `cd C:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\Connected Services\Trisoft.ISHRemote.Folder25ServiceReference`
    1. Slightly tweaked version of what Visual Studio runs to try to get rid of `ArrayOfStrings` is `dotnet-svcutil https://medevddemeyer10.global.sdl.corp/InfoShareWSDITA/folder25.asmx --outputDir "C:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\Connected Services\Trisoft.ISHRemote.Folder25ServiceReference" --outputFile "C:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\Connected Services\Trisoft.ISHRemote.Folder25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.Folder25ServiceReference --internal --serializer XmlSerializer --sync` seems to work thanks to simpler `XmlSerializer` approach ... Added that in `Create-AllAsmxWebServices--dotnet-svcutil.bat` (perhaps future matching `Update-...bat` to refresh the references)
1. NET Framework 4.5 project had `ISHTypeFieldSetup.resx` of ResX Schema 2.0 while net .NET Standard project expects ResX Schema 1.3 ... recreated the file.
1. Added `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` in build target `PropertyGroup` of `Trisoft.ISHRemote.csproj` so that referenced (NuGet) assemblies would be copied to `\bin\Debug\netstandard2.0\` and `\bin\Release\netstandard2.0`. By also ticking build xml documentation file and build event `"$(SolutionDir)..\Tools\XmlDoc2CmdletDoc\XmlDoc2CmdletDoc.exe" "$(TargetPath)"` the documentation is generated again.
1. Added `ISHRemote.psd1` in the route to get the module working for PowerShell 7.1, previously this psd1 file was generated from `build.props`
1. Taking the route of multi-targetting, having `ISHRemote.psm1` resolve it just-in-time. And not shipping netstandard2.0 but use that to generate the Get-Help from.
1. The `\netstandard2.0\Trisoft.ISHRemote.dll-Help.xml` (where `netstandard2.0` most likely will not be skipped) will be generated over `XmlDoc2CmdletDoc`. These are MSBuild copied to the target folders in `Trisoft.ISHRemote.csproj`. This because `XmlDoc2CmdletDoc` cannot resolve/Find all references for the .NET (Core) assemblies.
    ```
    <Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'$(TargetFramework)' == 'netstandard2.0'">
      <Exec Command="&quot;$(SolutionDir)..\Tools\XmlDoc2CmdletDoc\XmlDoc2CmdletDoc.exe&quot; &quot;$(TargetPath)&quot;" />
      <Copy SourceFiles="$(TargetPath)-Help.xml" DestinationFolder="$(TargetDir.Replace(`netstandard2.0`,`net472`))\" />
      <Copy SourceFiles="$(TargetPath)-Help.xml" DestinationFolder="$(TargetDir.Replace(`netstandard2.0`,`netcoreapp3.1`))\" />
      <Copy SourceFiles="$(TargetPath)-Help.xml" DestinationFolder="$(TargetDir.Replace(`netstandard2.0`,`net5.0`))\" />
    </Target>
1. Error `System.Runtime.Loader.AssemblyLoadContext.OnAssemblyResolve(RuntimeAssembly assembly, String assemblyFullName)
New-IshSession: Could not load file or assembly 'System.ServiceModel.Primitives` brought me here [Resolving PowerShell Module Assembly Dependency Conflicts](https://devblogs.microsoft.com/powershell/resolving-powershell-module-assembly-dependency-conflicts/). By running `[System.AppDomain]::CurrentDomain.GetAssemblies() | Out-GridView` you can see that my wanted (latest) version 4.8.1 is not competing with the out-of-the-box version of PowerShell/dotnet.
Publishing problem perhaps, as copying %USER%\.nuget\packages\system.servicemodel.http\4.8.1\lib\netcore50\System.ServiceModel.Http.dll to \Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\net5.0 and same for System.ServiceModel.Primitives.dll made it work under PowerShell 7. So next up is MSBuild/Publish routines/knowledge, inspired by Azure module. Could be caused by `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>`

## Next
1. Remove `System.Reflection...` again if I go multi-target.
1. Parameter `-PSCredential` doesn't work because of `SecureString` being Windows cryptography only according to https://github.com/PowerShell/PowerShell/issues/1654 ... what is next? Needs alignment with https://devblogs.microsoft.com/powershell/secretmanagement-and-secretstore-release-candidate-2/
    1. Also a `New-IshSession` scheduled task code sample like in the past using Windows-only `ConvertTo-SecureString` is required, perhaps over Secret Management.
1. Upon WCF Proxy retrieval from IshSession object, there used to be a `VerifyTokenValidity` that would check the authentication, and potentially re-authenticate all proxies. For `AuthenticationContext` we only now it is valid for 7 days, so ISHRemote could track that or the script using ISHRemote should handle that for now. Actually if you pass `AuthenticationContext` by ref on every call it gets refreshed anyway, so only a problem if IshSession is not used for 7+ days.
1. Consider build.props, and script
    1. Inspired from `\Properties\AssemblyInfo.cs`, `\Properties\AssemblyInfo.targets` and `\Properties\ModuleManifest.targets`
    1. `Trisoft.ISHRemote\ISHRemote.PostBuild.ps1`
1. It looks like `PSSnapIn` was wiring up the format xml. The class was removed, who now suggests to pick up the rendering format xml.
1. Is `CertificateValidationHelper.cs` and `ServicePointManagerHelper.cs` still the way to do certificate bypass?
1. `TrisoftCmdlet.cs` says `[assembly: ComVisible(false)]` ... brrr? Why?

# Backlog
The below is a list to consider, before execution is preferably transformed into github issues

1. Local `.snk` signing file, get inspired by Azure modules
1. Enable Tls13, this force NET Framework 4.8 ... is that part of netstandard2.0? Should we consider two build targets and have the `ISHRemote.psm1` decide at runtime which set to load?
1. Auto complete on parameters
1. `Get-Help` can still be based on tripple-slash (`///`) using `\ISHRemote\Source\Tools\XmlDoc2CmdletDoc`. Some source indicate separate markdown files, next to the C# source and Pester test files.

# References
* HTTPS and SoapClient11 or SoapClient12 is discussed on https://medium.com/grensesnittet/integrating-with-soap-web-services-in-net-core-adebfad173fb
* Error `System.Runtime.Loader.AssemblyLoadContext.OnAssemblyResolve(RuntimeAssembly assembly, String assemblyFullName)
New-IshSession: Could not load file or assembly 'System.ServiceModel.Primitives` brought me here [Resolving PowerShell Module Assembly Dependency Conflicts](https://devblogs.microsoft.com/powershell/resolving-powershell-module-assembly-dependency-conflicts/). By running `[System.AppDomain]::CurrentDomain.GetAssemblies() | Out-GridView` you can see which assemblies are loaded in your fresh PowerShell session.