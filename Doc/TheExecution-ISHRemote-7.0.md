# The Execution of the plan of ISHRemote v7.0

This page will try to track work in progress. And because I work on it in free time, it will help trace how I got where I am in the first place plus what is next. Inspired by [ThePlan-ISHRemote-7.0.md](./ThePlan-ISHRemote-7.0.md)

# Cmdlet Progress and Compatilibity

A table that describes what works, where cmdlets have been rewired, where tests have been adapted (potentially indicating compatibility issues).

1. Public class `IshSession`
    1. Removed WCF proxies

# Project Creation
1. The starting point was just-before ISHRemote v0.13 release. Removed the `\ISHRemote\Source\ISHRemote` folder. Keeping the `\ISHRemote\Source\Tools` folder, holding `XmlDoc2CmdletDoc`.
1. Used Visual Studio 2019 to create a new .NET Standard Class Library in folder `\ISHRemote\Source\ISHRemote` with project name `Trisoft.ISHRemote` and solution name `ISHRemote` (like it was before). Renamed folders `\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote` to trigger source control alignment with the previous solution, file history, etc
1. Through `Manage NuGet Package`, added a reference for `PowerShellStandard.Library` version `7.0.0-preview.1`.
1. Bumped initial `Trisoft.ISHRemote.dll` assembly and file version to 7.0.0.0, but better build.props or script integration is required.
1. Moving from `ISHRemoteCmdlets : PSSnapIn`, so the PowerShell 1.0 way, to the future. ([VMware](https://blogs.vmware.com/PowerCLI/2016/11/saying-farewell-snapins.html)) ... removed the class in total.
1. Started to comment out non-working code using `//TODO [Must] ISHRemotev7+ Cleanup`

## Next
1. Consider build.props, and script
    1. Inspired from `\Properties\AssemblyInfo.cs`, `\Properties\AssemblyInfo.targets` and `\Properties\ModuleManifest.targets`
    1. `Trisoft.ISHRemote\ISHRemote.PostBuild.ps1`
1. It looks like `PSSnapIn` was wiring up the format xml. The class was removed, who now suggests to pick up the rendering format xml.
1. Is `CertificateValidationHelper.cs` and `ServicePointManagerHelper.cs` still the way to do certificate bypass?
1. `TrisoftCmdlet.cs` says `[assembly: ComVisible(false)]` ... brrr? Why?

# Backlog
The below is a list to consider, before execution is preferably transformed into github issues

1. Local `.snk` signing file, get inspired by Azure modules
1. Enable Tls13, this force NET Framework 4.8
1. Auto complete on parameters
1. `Get-Help` can still be based on tripple-slash (`///`) using `\ISHRemote\Source\Tools\XmlDoc2CmdletDoc`. Some source indicate separate markdown files, next to the C# source and Pester test files.