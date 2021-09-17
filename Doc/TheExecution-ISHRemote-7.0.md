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
    1. The logic of 'Internal' (or 'SDL') as provided by ISHDeploy's cmdlet `Enable-ISHIntegrationSTSInternalAuthentication` enabling `https://ish.example.com/ISHWS/Internal/` is removed as that is about WCF-SOAP home realm discovery.
1. Cmdlet `New-IshSession`
    1. Removed parameter sets `ActiveDirectory`, `ActiveDirectory-ExplicitIssuer`, `UserNamePassword-ExplicitIssuer` and `PSCredential-ExplicitIssuer` are removed as that is about WCF-SOAP home realm discovery or Windows Authentication.
    1. Parameters `-WsTrustIssuerUrl` and `-WsTrustIssuerMexUrl` are removed as that is about WCF-SOAP home realm discovery.
    1. Parameter `-IshUserName` can no longer be empty, the default of empty to force `NetworkCredentials` for Windows Authentication doesn't make sense on ASMX-SOAP.
    1. Parameters `-TimeoutIssue` and `-TimeoutService` are removed as they belong to WCF-SOAP.
    2. Added parameter `-Protocol` which defaults to `AsmxAuthenticationContext`, on route for future options `OpenApiBasicAuthentication` (temporary combination to enable performance/compatibility testing) and `OpenApiOpenConnectId` for proper releasing.

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
New-IshSession: Could not load file or assembly 'System.ServiceModel.Primitives'` brought me here [Resolving PowerShell Module Assembly Dependency Conflicts](https://devblogs.microsoft.com/powershell/resolving-powershell-module-assembly-dependency-conflicts/). By running `[System.AppDomain]::CurrentDomain.GetAssemblies() | Out-GridView` you can see that my wanted (latest) version 4.8.1 is not competing with the out-of-the-box version of PowerShell/dotnet.
Publishing problem perhaps, as copying %USER%\.nuget\packages\system.servicemodel.http\4.8.1\lib\netcore50\System.ServiceModel.Http.dll to \Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\net5.0 and same for System.ServiceModel.Primitives.dll made it work under PowerShell 7. So next up is MSBuild/Publish routines/knowledge, inspired by Azure module. Could be caused by `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>`
    1. Kudos to @ivandelagemaat for getting the build sytem running, one module for Windows PowerShell and PowerShell Core. Below some quick notes on the most important victories. Even integrated the NuGet package of `XmlDoc2CmdletDoc`
    1. He added a `.github/workflows/continuous-integration.yml` file that autobuilds this branch. And `\Source\ISHRemote\Directory.Build.props`
    1. Rolled back to older 'System.ServiceModel.Primitives' versions. The ones that came with PowerShell SDK, this avoids the Azure assembly loader variations and more.
    1. Earlier code used proxy `Application25Soap12` which is lowered to `Application25Soap` (probably 1.1) which in essence is ASMX anyway.
    1. Consider build.props, and script
        1. Inspired from `\Properties\AssemblyInfo.cs`, `\Properties\AssemblyInfo.targets` and `\Properties\ModuleManifest.targets`
        1. `Trisoft.ISHRemote\ISHRemote.PostBuild.ps1`
    1. It looks like `PSSnapIn` was wiring up the format xml. The class was removed, who now suggests to pick up the rendering format xml.
    1. `ISHRemote.psm1` in a multi-target framework setup can detect in PowerShell if it is `Core` or `Desktop` and in turn if it is Framework, Core 3, Core 5 (or higher) by `[Environment]::Version` (which returns 5.0.3.-1)... based on `$PSVersionTable`
1. Cleaned up `NewIshSession.Tests.ps1` noticing the HTTPS/SSL (even TLS1.3) is missing, Timeout parameters are uncertain, PSCredential is a gap... but test has been cleaned up given 48 successes. Main branch ISHRemote v0.14 had all Pester tests refactored to Pester 5.3.0 (see #132)... still conditional/skip flags required to distuingish between WCF/ASMX/OpenAPI
6. `TrisoftCmdlet.cs` says `[assembly: ComVisible(false)]` ... brrr? Why? Removed
1. Build `Debug` in the same way as `Release` with copied `Scripts` folder (also the only folder for PSScriptAnalyzer to enforce) so that `ISHRemote.PesterSetup.ps1` can keep pointing to the `Debug` packaged ISHRemote.
2. Aligned Session, so `New-IshSession` and `Test-IshSession` with upcoming 0.14 (#132) including PesterV5 tests.
3. Make sure TLS 1.3 is activated (possible since net4.8 and higher)
4. Is `CertificateValidationHelper.cs` and `ServicePointManagerHelper.cs` still the way to do certificate bypass?  Not according to https://github.com/dotnet/runtime/issues/26048 perhaps needs platform-pragma between Windows PowerShell and PowerShell (Core). Nope solved it via `#115 Enabling Tls13 and IshSession based IgnoreSslPolicyErrors overwrite by switching to ChannelFactor instead of SoapClient. Crosslinking #102 on Tls13 and #22 as IshSession control Ssl-overwrite instead of AppDomain`
    > In .NET Core, ServicePointManager affects only HttpWebRequest. It does not affect HttpClient. You should be able to use HttpClientHandler.ServerCertificateValidationCallback to achieve the same.
In .NET Framework, the built-in HttpClient is built on top of HttpWebRequest, therefore ServicePointManager settings will apply to it.

## Next
1. `New-IshSession -WsBaseUrl .../ISHWS/ -IshUserName ... -IshPassword -Protocol [AsmxAuthenticationContext (default) | OpenApiBasicAuthentication | OpenApiOpenConnectId]` so Protocol as a parameter to use in Switch-cases in every cmdlet on how to route the code
7. Migrate `*-IshFolder` cmdlets as you need them for almost all tests anyway. Easy to do performance runs on Add-IshFolder and Remove-IshFolder. Later we have the following API25 to API30 mapping
   1. Folder25.Create -> API30.Create (ready)
   2. Folder25.RetrieveMetadataByIshFolderRefs -> API30.GetFolderList (NotImplemented planned for PI20.4)
   3. Folder25.Delete -> API30.DeleteFolder (ready)
   4. Folder25.GetMetadataByIshFolderRef -> API30.GetFolder (ready)
   5. Folder25.GetMetadata -> API30.GetFolderByFolderPath, perhaps GetRootFolderList (NotPlanned)
   6. Folder25.GetSubFoldersByIshFolderRef -> API30.GetFolderObjectList (ready)
1. Next is add all SoapClient proxies
9.  Parameter `-PSCredential` doesn't work because of `SecureString` being Windows cryptography only according to https://github.com/PowerShell/PowerShell/issues/1654 ... what is next? Needs alignment with https://devblogs.microsoft.com/powershell/secretmanagement-and-secretstore-release-candidate-2/
    1. Also a `New-IshSession` scheduled task code sample like in the past using Windows-only `ConvertTo-SecureString` is required, perhaps over Secret Management.
10. Upon WCF Proxy retrieval from IshSession object, there used to be a `VerifyTokenValidity` that would check the authentication, and potentially re-authenticate all proxies. For `AuthenticationContext` we only now it is valid for 7 days, so ISHRemote could track that or the script using ISHRemote should handle that for now. Actually if you pass `AuthenticationContext` by ref on every call it gets refreshed anyway, so only a problem if IshSession is not used for 7+ days.
11. ISHRemote 0.x branch replace bad quote `“` with proper quote `"` in `*.Tests.ps1`, for example NewIshSession.Tests.ps1 and SetIshMetadataFilterField.Tests.ps1
12. ISHRemote 0.x branch, commit of 20210917 could be applied to Windows Powershell only version


# Debugging
```powershell
Import-Module "C:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote"
New-IshSession -WsBaseUrl https://192.168.1.160/ISHWSDita/ -IshUserName admin2 -IshPassword admin2 -IgnoreSslPolicyErrors -Verbose -Debug
```

# Performance

Curious about performance, one of the first cmdlets we need to run tests are around IshFolder, so a simple performance test could be around that.
```powershell
$ishSession # as admin to mecdev12qa01/ORA19 as a constant in the equation as it offers the three API variations
(Measure-Command -Expression {
    $folderCmdletRootPath = "\General\__ISHRemote"
    $ishFolder = Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath
    $ownedByTestRootOriginal = $ishfolder.fusergroup_none_element #Get-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element -IshField $ishFolder.IshField
    $readAccessTestRootOriginal = $ishfolder.readaccess_none_element #(Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Separator)
    $ishFoldersToRemove = @()
    foreach ($folderName in 0..99)
    {
        Write-Host ("Creating folder["+$folderName+"]")
        $ishFoldersToRemove += Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef) -FolderType ISHNone -FolderName "Add-IshFolder Performance $folderName" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
    }
    foreach ($ishFolderToRemove in $ishFoldersToRemove)
    {
        Write-Host ("Removing folder["+$ishFolderToRemove.fname+"]")
        Remove-IshFolder -IshSession $ishSession -IshFolder $ishFolderToRemove
    }
}).TotalMilliseconds
```

The below information was collected via `$PSVersionTable`

| Test Runs | WCF (0.13.8112.2) on Desktop 5.1.19041.1151 | ASMX (7.0...) on Desktop 5.1.19041.1151 | ASMX (7.0...) on Core 7.1.4    | OpenAPI (7.0...) on Desktop 5.1.19041.1151 | OpenAPI (7.0...) on Core 7.1.4        | 
| :---      | ---:                   | ---:                   | ---:          | ---:                   | ---:              |
| Run 1     |                50494ms |                        |               |                        |                   |
| Run 2     |                50478ms |                        |               |                        |                   |
| Run 3     |                52067ms |                        |               |                        |                   |
| Run 4     |                50494ms |                        |               |                        |                   |
| Run 5     |                48050ms |                        |               |                        |                   |


# Backlog
The below is a list to consider, before execution is preferably transformed into github issues

1. Local `.snk` signing file, get inspired by Azure modules
1. Enable Tls13, this force NET Framework 4.8 ... is that part of netstandard2.0? Should we consider two build targets and have the `ISHRemote.psm1` decide at runtime which set to load?
1. Auto complete on parameters
1. `Get-Help` can still be based on tripple-slash (`///`) using `\ISHRemote\Source\Tools\XmlDoc2CmdletDoc`. Some source indicate separate markdown files, next to the C# source and Pester test files. There is a NuGet package, native .NET (Core) integrated.

# References
* HTTPS and SoapClient11 or SoapClient12 is discussed on https://medium.com/grensesnittet/integrating-with-soap-web-services-in-net-core-adebfad173fb
* Error `System.Runtime.Loader.AssemblyLoadContext.OnAssemblyResolve(RuntimeAssembly assembly, String assemblyFullName)
New-IshSession: Could not load file or assembly 'System.ServiceModel.Primitives' brought me here [Resolving PowerShell Module Assembly Dependency Conflicts](https://devblogs.microsoft.com/powershell/resolving-powershell-module-assembly-dependency-conflicts/). By running `[System.AppDomain]::CurrentDomain.GetAssemblies() | Out-GridView` you can see which assemblies are loaded in your fresh PowerShell session. Rolled back to older 'System.ServiceModel.Primitives' versions. The ones that came with PowerShell SDK, this avoids the Azure assembly loader variations and more.