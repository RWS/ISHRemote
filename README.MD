# Summary

`ISHRemote` is a PowerShell module on SDL Knowledge Center Content Manager. Its goal is business automation on top of the Component Content Management System (InfoShare). This library is constructed close to the "Web Services API" to:
* allow business logic automation ranging from triggering publishing into the continuous integration pipeline over legacy data correction up to provisioning
* show case code examples and API best practices

# Features & Samples

* This library is a relatively thin client layer on top of the business API. 
* `-WhatIf`/`-Confirm` are implemented on write operations.
* `Add-*` cmdlets will immediately create objects in the CMS, and return you an in-memory result holding descriptive identifiers to continue the pipeline.
* `Remove-*` cmdlets will not return any in-memory result.
* `Set-*`, `Move-*`, `Publish-*` and `Stop-*` cmdlets will immediately update existing objects in the CMS, and return you an in-memory result holding descriptive identifiers to continue the pipeline. You cannot use `Set-*` for creation.
* `New-*` cmdlets will create in-memory objects to locally update and then potentially pass to `Add-*` and`Set-*` cmdlets.
* `Get-*` and `Find-*` cmdlets return existing objects present in the CMS, and return you an in-memory result holding descriptive identifiers to continue the pipeline.
    * The `Find-` cmdlets was built on top of the API Find operation, which triggers a query to find all objects matching the filter criteria.
    * The `Get-` cmdlets was built on top of the API Retrieve/Get operations, which trigger a query given object identifiers and then potentially extra filter criteria.
* Supports ISHIntegrationSTSInternalAuthentication as implemented by `ISHDeploy`.

# Install & Update

## Prerequisites

When you have PowerShell 5 on your client machine, the PSVersion entry in `$PSVersionTable` reads 5.0... and PackageManagement is there implicitly.
When you have a PowerShell version lower than 5 on your client machine, the PSVersion entry in `$PSVersionTable` reads 4.0 or even 3.0. Note that the latest Knowledge Center 2016SP1/12.0.1 release is only verified with PowerShell 4 (not 5 or above), so don't upgrade your servers. As ISHRemote is about a client-side web services driven library I actually don't expect you to even install it on a server.

So either upgrade to [Windows Management Framework 5.0](https://www.microsoft.com/en-us/download/details.aspx?id=50395) or stay on PowerShell 4 and install [Package Management Preview – March 2016 for PowerShell 4 & 3](https://blogs.msdn.microsoft.com/powershell/2016/03/08/package-management-preview-march-2016-for-powershell-4-3-is-now-available/).

## Install
Open a PowerShell, then you can find and install the ISHRemote module. CurrentUser `-Scope` indicates that you don't have to run PowerShell as Administrator. The `-Force` will make you bypass some security/trust questions.

		Install-Module ISHRemote -Repository PSGallery -Scope CurrentUser -Force 

## Update

Open a PowerShell and run.

        Update-Module ISHRemote

 
## Uninstall

Open a PowerShell and run.

        Uninstall-Module ISHRemote -AllVersion 

# Backlog & Feedback
Any feedback is welcome. Please log a GitHub issue, make sure you submit your version number, expected and current result,...

[Backlog]

[Backlog]: BACKLOG.MD "Backlog"

# Known Issues & FAQ

## Execution Known Issues
* If you get `New-IshSession : Reference to undeclared entity 'raquo'. Line 98, position 121.`, most likely you specified an existing "Web Services API" url. Make sure your url ends with an ending slash `/`.
* If a test fails with `The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it is in the Faulted state.`,
  it probably means you didn't provide enough (mandatory) parameters to the WCF/SVC code so passing null parameters. Typically an `-IshPassword` is missing or using an existing username.
* ISHDeploy `Enable-ISHIntegrationSTSInternalAuthentication/Disable-ISHIntegrationSTSInternalAuthentication` adds a /ISHWS/Internal/connectionconfiguration.xml that a different issuer should be used. As ISHRemote doesn't have an app.config, all the artifacts are derived from the RelyingParty WSDL provided mex endpoint (e.g. /ISHSTS/issue/wstrust/mex).  
If you get error `New-IshSession : The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it is in the Faulted state.`, it probably means you initialized `-WsBaseUrl` without the `/Internal/` (or `/SDL/`) segment, meaning you are using the primary configured STS.

## Testing Known Issues

* If a test fails with `Object reference not set to an instance of an object.`, it probably means that somewhere in your `*.Tests.ps1` you return an object and forgot a `Write-Host` or something.

        Result StackTrace: 
        at PowerShellTools.TestAdapter.PowerShellTestExecutor.RunTest(PowerShell powerShell, TestCase testCase, IRunContext runContext)
        at PowerShellTools.TestAdapter.PowerShellTestExecutor.RunTests(IEnumerable``1 tests, IRunContext runContext, IFrameworkHandle frameworkHandle)

* When using Visual Studio to run a single "Run Selected Test", the Test Explorer in turn will 
  run an [Invoke-Pester](https://github.com/pester/Pester/wiki/Invoke-Pester) command with the following parameters
    * `Path` The path where Invoke-Pester begins to search for test files. The default is the current directory.
    * `TestName` Informs Invoke-Pester to only run Describe blocks that match this name. This value may contain wildcards.  
    This means that all `*.Tests.ps1` are executed in the same folder back-to-back, but only the Describe that matches the given TestName will be trully tested. So every `*.Tests.ps1` prerequisites and finally block will be called and have to be clean.

## Documentation Known Issues
* [XmlDoc2CmdletDoc bug 22](https://github.com/red-gate/XmlDoc2CmdletDoc/issues/22) `System.ArgumentException: Property Get method was not found.` at `XmlDoc2CmdletDoc.Core.Domain.Parameter.get_DefaultValue()`   means you are missing a Get'er on a Property like `public IshFolder IshFolder { set { _folderId = value.IshFolderRef; } }`
* [XmlDoc2CmdletDoc bug 23](https://github.com/red-gate/XmlDoc2CmdletDoc/issues/23) `System.NullReferenceException: Object reference not set to an instance of an object.` at `XmlDoc2CmdletDoc.Core.Domain.Command.<>c.<get_OutputTypes>` `Retrieve-IshFolder` had `[OutputType(nameof(IshFolder))]`, which should have been `[OutputType(typeof(IshFolder))]`.


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
* ISHRemote-build project holds the artefacts for in-house testing, signing, and publishing the library

## Documentation Standards

* Inline `*.cs` tripple-slash `///` documentation. Syntax options are explained on [simple-talk.com](https://www.simple-talk.com/dotnet/software-tools/documenting-your-powershell-binary-cmdlets/). [XmlDoc2CmdletDoc](https://github.com/red-gate/XmlDoc2CmdletDoc) generates the requisite MAML file for you using essentially standard C# doc-comments embedded directly in your code, just like  documenting any other C# library. Now you can document PowerShell simply and easily, and keep it synchronized with the code.
* Document the container object class like `IshSession`, and not the using parameters like  `-IshSession` everywhere.

### XmlDoc2CmdletDoc
Inline `*.cs` tripple-slash `///` documentation. Syntax options are explained on [simple-talk.com](https://www.simple-talk.com/dotnet/software-tools/documenting-your-powershell-binary-cmdlets/). [XmlDoc2CmdletDoc](https://github.com/red-gate/XmlDoc2CmdletDoc) generates the requisite 
MAML file for you using essentially standard C# doc-comments embedded directly in your code, just like 
documenting any other C# library. Now you can document PowerShell simply and easily, and keep it 
synchronized with the code.

## Testing Standards

* Most Pester tests are acceptance test, enriched which some unit tests where possible.
* Using a dot-sourced `ISHRemote.PesterSetup.ps1` include in all `*.Tests.ps1` to specify the variables (not initialization) consistently
* Using the -Tag with "Create", "Read", "Update", "Delete" to allow [Pester grouping](See https://github.com/pester/Pester/wiki/Describe) Tags
* Data initialization and breakdown are key but also time consuming. BeforeEach/AfterEach are run for every It block, so makes things slow.

# Coding Prerequisites  

## Testing and Debugging using Pester in Visual Studio 2015

[src1](http://www.powershellmagazine.com/2014/03/12/get-started-with-pester-powershell-unit-testing-framework/) [src2](http://www.powershellmagazine.com/2014/03/27/testing-your-powershell-scripts-with-pester-assertions-and-more/) [src3](https://www.simple-talk.com/sysadmin/powershell/practical-powershell-unit-testing-getting-started/) [src4](https://github.com/pester/Pester/wiki
Prerequisites)

In `ISHRemote.PesterSetup.ps1` override the global variables used for tests. Look for the region `Placeholder to inject your variable overrides. DONT FORGET TO DELETE`. **Don't forget not to commit those custom values**.

### Prerequisites

1. Install or make sure you run Windows Management Framework v5, so PowerShell v5 for Pester support, see https://msdn.microsoft.com/en-us/powershell/wmf/requirements
1. Install [Powershell Tools for Visual Studio 2015](https://visualstudiogallery.msdn.microsoft.com/c9eb3ba8-0c59-4944-9a62-6eee37294597/view/Discussions/1)
1. Install Pester and have it available in `%userprofile%\My Documents\WindowsPowerShell\Modules\Pester`

    [Warning](https://www.nuget.org/packages/Pester/): Install `Pester` as Administrator, see  :  `Install-Package Pester -Source PSGallery` will eventually show you `Failed to load Pester module. The specified module 'Pester' was not loaded because no valid module 
file was found in any module directory.` when running [Tests in Visual Studio](http://rostacik.net/2015/12/16/how-to-use-nuget-packages-even-with-powershell-projects-with-visual-studio-2015/)
[Solution](https://writeabout.net/2016/01/01/visual-studio-github-powershell-pester/) is to copy content of `%ProgramFiles%\WindowsPowerShell\Modules\Pester\3.4.0" to "%userprofile%\My Documents\WindowsPowerShell\Modules\Pester`

## Debugging using F5 In Visual Studio 2015
[src](http://www.powershellmagazine.com/2014/04/08/basics-of-writing-a-powershell-module-with-c-part-2-debugging/)

In the properties window, select Debug tab and select Start external program under Start Options. 
Navigate to PowerShell.exe for the architecture of your project. In the Start Options -> 
Command Line Arguments textbox, enter the parameters for the PowerShell executable that will 
automatically load the module once it is compiled. The debugger starts always in the default 
location where the DLL is generated. So, there is no need to specify the full path. In the case 
of our module the option would be:

    C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe
	-noexit -command "&{ import-module .\ISHRemote.psm1 -verbose}"
	OR
	-noexit -command "&{ ..\..\Samples\Sample.Automate.EventMonitor.ps1}"
	OR COMBINED
	-noexit -command "&{ import-module .\ISHRemote.psm1 -verbose; ..\..\Samples\Sample.Automate.EventMonitor.ps1}"
 
Warning: If you get a security warning, run the above exact powershell.exe command using Run As Administrator, 
then execute `Set-ExecutionPolicy Unrestricted`

