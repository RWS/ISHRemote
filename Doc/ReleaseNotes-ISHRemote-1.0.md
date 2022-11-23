# Release Notes of ISHRemote v1.0

Actual detailed release notes are on [Github](https://github.com/rws/ISHRemote/releases/tag/v1.0), below some code samples.

As a reminder, in case of issues, please log something on [ISHRemote issues](https://github.com/RWS/ISHRemote/issues) or [Tridion Docs Developers Forum](https://community.rws.com/developers-more/developers/tridiondocs-developers/f/livecontent_developer_forum) referring to this ISHRemote version. Remember that `Install-Module` allows you to specify `-MaximumVersion` to install an earlier version.
```powershell
Install-Module ISHRemote -Repository PSGallery -Scope CurrentUser -MaximumVersion 1
```

Remember
* All C# source code of the ISHRemote library is online at [release/v1 source](https://github.com/rws/ISHRemote/tree/release/v1/Source/ISHRemote/Trisoft.ISHRemote), including handling of WS-Trust protocol ([InfoShareWcfConnection.cs](https://github.com/rws/ISHRemote/tree/release/v1/Source/ISHRemote/Trisoft.ISHRemote/InfoShareWcfConnection.cs)).
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/rws/ISHRemote/tree/release/v1/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/rws/ISHRemote/tree/release/v1/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

## General

This release inherits the v0.1 to v0.14 development branch and features. All cmdlets and business logic are fully compatible.

Main reason for v1.0 is to prepare the trunk branch of source control for .NET (Core) as described in the [ThePlan-ISHRemote-7.0.md](ThePlan-ISHRemote-7.0.md).

Encryption in flight - https - can now also go over Tls 1.3 while before releases only had Tls 1.0, 1.1 or 1.2 as options. #102

Improvements on disposing and closing connections in `InfoShareWcfConnection.cs` to better recover from communication faulted exceptions. Fixed casing of `Edt.svc` endpoint. #145

## Breaking Changes

Again, all cmdlets and business logic are fully compatible. The below is for clarity, in practice they will not affect anybody.

* The platform requirements moved requiring **.NET Framework 4.8** (compared to obsolete 4.5 in the past). Microsoft .NET Framework 4.8 is the final version of the Windows-based .NET Framework, all future successors are Microsoft (Core) 6.0 and up based running on linux, macos and windows.

* .NET Framework 4.8 expects **Windows PowerShell 5.1** and older versions are no longer supported. Similar Windows PowerShell 5.1 is the final version of the Windows-based PowerShell, all future successors are PowerShell (Core) 7.0 and up running on linux, macos and windows - powered by Microsoft (Core) 6.0 and up.

* The compiled library of cmdlets will no longer be strong named with an RWS/fSDL private key. This only affects you if you wrote an application (not PowerShell script) on top of this library and in turn you signed your application. #80 
