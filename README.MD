# Summary

`ISHRemote` is a PowerShell module on Tridion Docs Content Manager. Its goal is business automation on top of the Component Content Management System (Knowledge Center Content Manager, LiveContent Architect, Trisoft InfoShare). This library is constructed close to the "Web Services API" to:
* allow business logic automation ranging from triggering publishing into the continuous integration pipeline over legacy data correction up to provisioning
* show case code examples and API best practices

# Versions

| Name                     | Platform                                        | Branch         | Status                 |
|--------------------------|-------------------------------------------------|----------------|------------------------|
| [ISHRemote](https://www.powershellgallery.com/packages/ISHRemote/) latest         | PowerShell-5.1/NET-4.8 and PowerShell-7+/NET-6+ | master         | [![](https://github.com/RWS/ISHRemote/actions/workflows/continuous-integration.yml/badge.svg?branch=master)](https://github.com/RWS/ISHRemote/actions/workflows/continuous-integration.yml) |
| [ISHRemote](https://www.powershellgallery.com/packages/ISHRemote/) v1 and earlier | PowerShell-5.1/NET-4.8 | release/v1     | [![](https://github.com/RWS/ISHRemote/actions/workflows/continuous-integration.yml/badge.svg?branch=release/v1)](https://github.com/RWS/ISHRemote/actions/workflows/continuous-integration.yml) |


# Videos

Have a look at the [Automating tasks in Tridion Docs using PowerShell](https://youtu.be/3ItM2LDwh20) webinar. Supporting material, so you can easily copy-paste is on 

* [Webinar Blogpost - Getting started with Microsoft PowerShell](https://community.sdl.com/product-groups/sdl-tridion-dx/tridion-docs/b/weblog/posts/automating-tasks-in-sdl-tridion-docs-using-powershell-getting-started-with-microsoft-powershell)
* [Webinar Blogpost - Examples](https://community.sdl.com/product-groups/sdl-tridion-dx/tridion-docs/b/weblog/posts/automating-tasks-in-sdl-tridion-docs-using-powershell)

# Features & Samples

* This library is a relatively thin client layer on top of the business API. 
* `-WhatIf`/`-Confirm` are implemented on write operations.
* `Add-*` cmdlets will immediately create objects in the CMS, and return you an in-memory result holding basic identifiers (`$ishSession.DefaultRequestedMetadata`) to continue the pipeline.
* `Remove-*` cmdlets will not return any in-memory result.
* `Set-*`, `Move-*`, `Publish-*` and `Stop-*` cmdlets will immediately update existing objects in the CMS, and return you an in-memory result holding descriptive identifiers to continue the pipeline. You cannot use `Set-*` for creation.
* `New-*` cmdlets will create in-memory objects to locally update and then potentially pass to `Add-*` and`Set-*` cmdlets.
* `Get-*` and `Find-*` cmdlets return existing objects present in the CMS, and return you an in-memory result holding descriptive identifiers to continue the pipeline.
    * The `Find-` cmdlets was built on top of the API Find operation, which triggers a query to find all objects matching the filter criteria.
    * The `Get-` cmdlets was built on top of the API Retrieve/Get operations, which trigger a query given object identifiers and then potentially extra filter criteria.
* Supports ISHIntegrationSTSInternalAuthentication as implemented by `ISHDeploy`.

# Install & Update

Below the TLDR; more details are on [Installation-ISHRemote-8.0.md](./Doc/Installation-ISHRemote-8.0.md). This link will guide you on package managers like `PowerShellGet` and newer `PSResourceGet`; cmdlets to install, update and uninstall.

## Install on PowerShell 7.4+ (CoreCLR) powered by .NET 8.0+

Open a PowerShell, then install the ISHRemote module. CurrentUser `-Scope` indicates that you don't have to run PowerShell as Administrator.
```powershell
Install-PSResource -Name ISHRemote -Repository PSGallery -Scope CurrentUser
```

## Install on Windows PowerShell 5.1 (FullCLR) powered by .NET Framework 4.8

Open a Windows PowerShell, then install the ISHRemote module. CurrentUser `-Scope` indicates that you don't have to run PowerShell as Administrator. The `-Force` will make you bypass some security/trust questions.

```powershell
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12  # as PSGallery switched to HTTPS over Tls12 and higher
Install-Module ISHRemote -Repository PSGallery -Scope CurrentUser -Force 
```

# Backlog & Feedback
Any feedback is welcome. Please log a GitHub issue, make sure you submit your version number, expected and current result,...

[Backlog]

[Backlog]: BACKLOG.MD "Backlog"

# Known Issues & FAQ

## Execution Known Issues
* If a `New-IshSession -IshUserName ...` results in `System.ServiceModel.FaultException` followed by `An error occurred when verifying security for the message.`, then you are using `-Protocol WcfSoapWithWsTrust` implicitly. A very likely candidate is that deprecated `/ISHSTS/` which authenticates the WS-Federation protocol was not part of your certificate rollover. A simple workaround is to stop the ISHSTS IIS App Pool, delete `\Web\InfoShareSTS\App_Data\IdentityServerConfiguration-2.3.sdf` and in turn start the IIS App Pool again - which will regenerate the local .sdf database.
* If you get `You cannot call a method on a null-valued expression.` or `The HTTP status code of the response was not expected (401).`, probably while using `$ishSession.OpenApiISH30Client` it means your token expired and requires a refresh.
* If you get `New-IshSession : Reference to undeclared entity 'raquo'. Line 98, position 121.`, most likely you specified an unexisting "Web Services API" url. Make sure your url ends with an ending slash `/`.
* If a test fails with `The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it is in the Faulted state.`,
  it probably means you didn't provide enough (mandatory) parameters to the WCF/SVC code so passing null parameters. Typically an `-IshPassword` is missing or using an existing username.
* ISHDeploy `Enable-ISHIntegrationSTSInternalAuthentication/Disable-ISHIntegrationSTSInternalAuthentication` adds a /ISHWS/Internal/connectionconfiguration.xml that a different issuer should be used. As ISHRemote doesn't have an app.config, all the artifacts are derived from the RelyingParty WSDL provided mex endpoint (e.g. /ISHSTS/issue/wstrust/mex).  
If you get error `New-IshSession : The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it is in the Faulted state.`, it probably means you initialized `-WsBaseUrl` without the `/Internal/` (or `/SDL/`) segment, meaning you are using the primary configured STS.

# Standards To Respect

## Coding Standards 

* Any code change should 
    * respect the coding standard like [Strongly Encouraged Development Guidelines](https://msdn.microsoft.com/en-us/library/dd878270(v=vs.85).aspx) and [Windows PowerShell Cmdlet Concepts](https://msdn.microsoft.com/en-us/library/dd878268(v=vs.85).aspx)
    * come with matching acceptance/unit test, to further improve stability and predictability
    * come with matching tripple-slash `///` documentation verification or adaptation. Remember `Get-Help` drives PowerShell!
    * double check backward compatibility; if you break provide an alternative through `Set-Alias`, Get-Help,...
	* Any url reference should be specified with `...example.com` in samples and Service References.
* Respect PowerShell concepts
    * parameters are Single not plural, so IshObject over IshObjects or FilePath over FilePaths
    * implement `-WhatIf`/`-Confirm` flags for write operations

## Documentation Standards

* Inline `*.cs` tripple-slash `///` documentation. Syntax options are explained on [simple-talk.com](https://www.simple-talk.com/dotnet/software-tools/documenting-your-powershell-binary-cmdlets/). [XmlDoc2CmdletDoc](https://github.com/red-gate/XmlDoc2CmdletDoc) generates the requisite MAML file for you using essentially standard C# doc-comments embedded directly in your code, just like  documenting any other C# library. Now you can document PowerShell simply and easily, and keep it synchronized with the code.
* Document the container object class like `IshSession`, and not the using parameters like  `-IshSession` everywhere.
* Known issues
    * [XmlDoc2CmdletDoc bug 22](https://github.com/red-gate/XmlDoc2CmdletDoc/issues/22) `System.ArgumentException: Property Get method was not found.` at `XmlDoc2CmdletDoc.Core.Domain.Parameter.get_DefaultValue()`   means you are missing a Get'er on a Property like `public IshFolder IshFolder { set { _folderId = value.IshFolderRef; } }`
    * [XmlDoc2CmdletDoc bug 23](https://github.com/red-gate/XmlDoc2CmdletDoc/issues/23) `System.NullReferenceException: Object reference not set to an instance of an object.` at `XmlDoc2CmdletDoc.Core.Domain.Command.<>c.<get_OutputTypes>` `Retrieve-IshFolder` had `[OutputType(nameof(IshFolder))]`, which should have been `[OutputType(typeof(IshFolder))]`.

## Testing Standards

Initial testing was based on Pester 3.4.0 which came as part of Windows 10/2016, to introduce support for PowerShell 7+ we switched to Pester 5+ ([#115](https://github.com/RWS/ISHRemote/issues/115)|[#132](https://github.com/RWS/ISHRemote/issues/132)). To force an upgrade from pre-packaged Pester 3.4.0, have a look at [Pester Installation](https://pester.dev/docs/introduction/installation), although on most systems it comes down to
`Install-Module -Name Pester -Force -SkipPublisherCheck`.
* Most Pester tests are acceptance test, enriched which some unit tests where possible.
* Using a central `ISHRemote.PesterSetup.ps1` (with optional `ISHRemote.PesterSetup.Debug.ps1` override) in all `*.Tests.ps1` to specify the variables (not initialization) consistently
* Data initialization and breakdown are key but also time consuming. After a test run, your system is left in its original state.

In `ISHRemote.PesterSetup.Debug.ps1` override the global variables used for tests. **Don't forget not to commit those custom values**.

# Coding Prerequisites and Best Practices

## Release Build and Publish To PowerShell Gallery

Rought steps to release...
1. Use Visual Studio to build a `Debug` build. Run an `Invoke-Pester` (minimally Pester v5.3.0) on `C:\GITHUB\ishremote\Source\ISHRemote\Trisoft.ISHRemote`. All steps should be green.
1. Use Visual Studio to build a `Release` build. Change `C:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\ISHRemote.PesterSetup.ps1` to forcefully load `\bin\release\` (instead of `\bin\debug\`). Run an `Invoke-Pester` on `C:\GITHUB\ishremote\Source\ISHRemote\Trisoft.ISHRemote`. All steps should be green.
1. The folder content of `C:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\bin\Release\` will be published
    1. Check all files, especially `Scripts` that you have all files.
    1. Check version number of `ISHRemote.dll`
1. Publishing a Github Actions build to PowerShellGallery can be done by submitting a label like `[PublishToPSGalleryAsPreview]` or `[PublishToPSGalleryAsRelease]` in the commit message. See [continuous-integration.yml](/.github/workflows/continuous-integration.yml)
1. Check availability of latest version on https://www.powershellgallery.com/packages/ISHRemote/
1. Github releases
    1. Edit and release the notes on https://github.com/sdl/ISHRemote/releases/
    1. Start new release notes, under a new version number like `v0.14-beta`
1. Close version milestone on https://github.com/sdl/ISHRemote/milestone/

## Building Libraries in Visual Studio

Since #180 where the `Trisoft.ISHRemote.csproj` multi-target/conditional ProjectReference was introduced, you might get a lot of Build errors looking like `error CS0234: The type or namespace name 'Reflection' does not exist in the namespace 'System' (are you missing an assembly reference?) [D:\a\ISHRemote\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote.OpenApiAM10\Trisoft.ISHRemote.OpenApiAM10.NET60.csproj]` or `error CS0518: Predefined type 'System.String' is not defined or imported`. The simple answer is build again, it will eventually restore all project dependencies.
In [continuous-integration.yml](/.github/workflows/continuous-integration.yml) this issue was solved by explicit restores for the multi-target/conditional ProjectReferences.

    dotnet restore Source/ISHRemote/Trisoft.ISHRemote.OpenApiAM10\Trisoft.ISHRemote.OpenApiAM10.NET48.csproj
    dotnet restore Source/ISHRemote/Trisoft.ISHRemote.OpenApiAM10\Trisoft.ISHRemote.OpenApiAM10.NET60.csproj
    dotnet restore Source/ISHRemote/Trisoft.ISHRemote.OpenApiISH30\Trisoft.ISHRemote.OpenApiISH30.NET48.csproj
    dotnet restore Source/ISHRemote/Trisoft.ISHRemote.OpenApiISH30\Trisoft.ISHRemote.OpenApiISH30.NET60.csproj
    dotnet restore Source/ISHRemote/ISHRemote.sln

## Debugging PowerShell in Visual Studio

Since Visual Studio 2022, we use `\ISHRemote\Trisoft.ISHRemote\Properties\launchSettings.json` which theoretically also works on Visual Studio Code. Don't forget to set the matching Framework for debugging the library, either `net48` or `net6.0`, for the matching PowerShell version in the start debugging dropdown.
- Debugging the binary assembly in PowerShell 5.1 you need to use `"executablePath": "%SystemRoot%\\system32\\WindowsPowerShell\\v1.0\\powershell.exe"`
- Debugging the binary assembly in PowerShell 7.3+ you need to use `"executablePath": "%ProgramFiles%\\PowerShell\\7\\pwsh.exe"`. Using the *Windows Store* PowerShell often results in error `Unable to attach to CoreCLR. A debug component is not installed.` while debugging.

## Debugging PowerShell in Visual Studio Code

Setting up your Visual Studio Code is explained on the internet, see [Scripting Guys](https://rkeithhill.wordpress.com/2017/02/19/scripting-guys-blog-posts-on-using-visual-studio-code-for-powershell-development/) who in turn link to
- [Get started with PowerShell development in Visual Studio Code](https://blogs.technet.microsoft.com/heyscriptingguy/2016/12/05/get-started-with-powershell-development-in-visual-studio-code/)
- [Visual Studio Code editing features for PowerShell development – Part 1](https://blogs.technet.microsoft.com/heyscriptingguy/2017/01/11/visual-studio-code-editing-features-for-powershell-development-part-1/)
- [Visual Studio Code editing features for PowerShell development – Part 2](https://blogs.technet.microsoft.com/heyscriptingguy/2017/01/12/visual-studio-code-editing-features-for-powershell-development-part-2/)
- [Debugging PowerShell script in Visual Studio Code – Part 1](https://blogs.technet.microsoft.com/heyscriptingguy/2017/02/06/debugging-powershell-script-in-visual-studio-code-part-1/)
- [Debugging PowerShell script in Visual Studio Code – Part 2](https://blogs.technet.microsoft.com/heyscriptingguy/2017/02/13/debugging-powershell-script-in-visual-studio-code-part-2/)
- [Tyler Leonhardt - Visual Studio Code: deep dive into debugging your PowerShell scripts](https://www.youtube.com/watch?v=cSbIXmlkr8o)
- [Writing Compiled PowerShell Cmdlets by Thomas Rayner](https://www.youtube.com/watch?v=O0lk92W799)

