# Release Notes of ISHRemote v7.0

Actual detailed release notes are on [Github](https://github.com/rws/ISHRemote/releases/tag/v7.0), below some code samples.

Remember
* All C# source code of the ISHRemote library is online at [master](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote), including handling of WS-Trust protocol ([InfoShareWcfConnection.cs](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/InfoShareWcfConnection.cs)) in a NET 4.8 and NET 6.0+ style.
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

The below text describes the delta compared to fielded release ISHRemote v0.14, so will repeat some v1.0 highlights.

## General

This release inherits the v0.1 to v0.14 and v1.0 development branch and features. By enabling PowerShell 7+ powered by NET 6+ next to existing Windows PowerShell 5.1 powered by NET Framework 4.8; we had to do some breaking changes forced by platform support. Most cmdlets and business logic are fully compatible except around authentication (`New-IshSession`, `Test-IshSession` and `New-IshObfuscatedFile`) as described below.

Yes, this is the ISHRemote version that works on Windows PowerShell and PowerShell (Core)! However, only for username/password-based authentication and not for Windows Authentication (typically WS-Trust protocol over Microsoft ADFS using `windowsmixed`).

This is execution of the plan as communicated and described on [ThePlan-ISHRemote-7.0.md](ThePlan-ISHRemote-7.0.md).

Encryption in flight - https - can now also go over Tls 1.3 while before releases only had Tls 1.0, 1.1 or 1.2 as options. #102

## Breaking Changes - Cmdlets

Again, most cmdlets and business logic are fully compatible, except the below:

1. `New-IshSession` and `Test-IshSession` hosted by PowerShell (Core) 7+ can no longer do `windowsmixed` authentication, also known as Windows Authentication typically required for Microsoft ADFS. Note that these cmdlets hosted by Windows PowerShell 5.1 still suppored Windows Authenticationas before.
2. 
3. `New-IshObfuscatedFile` works as before (v0.14 and earlier) on Windows platforms on Windows PowerShell and PowerShell (Core). The cmdlet will give a warning that images cannot be obfuscated on non-Windows system because of missing platform extensions.

## Breaking Changes - Requirements

* The platform requirements moved requiring **.NET Framework 4.8** (compared to obsolete 4.5 in the past). Microsoft .NET Framework 4.8 is the final version of the Windows-based .NET Framework, all future successors are Microsoft (Core) 6.0 and up based running on linux, macos and windows.

* .NET Framework 4.8 expects **Windows PowerShell 5.1** and older versions are no longer supported. Similar Windows PowerShell 5.1 is the final version of the Windows-based PowerShell, all future successors are PowerShell (Core) 7.0 and up running on linux, macos and windows - powered by Microsoft .NET (Core) 6.0 and up.

* The compiled library of cmdlets will no longer be strong named with an RWS/fSDL private key. This only affects you if you wrote an application (not PowerShell script) on to of this library and in turn you signed your application. #80 
