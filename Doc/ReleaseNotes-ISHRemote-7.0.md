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
    1. Parameter sets ending with `ExplicitIssuer` offering explicit WS-Trust Issuer Url `-WsTrustIssuerUrl` and WS-Trust Issuer Metadata Exchange Url `-WsTrustIssuerMexUrl` are removed.
    2. Parameters `-TimeoutIssue` and `-TimeoutService` are removed. Any usage is replaced by `-Timeout`.
    3. Parameter `-IshUserName` can still be left empty on Windows PowerShell 5.1/NET4.8 but will throw an error on PowerShell7+/NET6+ as Microsoft's [WSFederationHttpBinding](https://devblogs.microsoft.com/dotnet/wsfederationhttpbinding-in-net-standard-wcf/) library so far has no support for the `windowsmixed` WS-Trust protocol variation.
    4. Parameter `-IgnoreSslPolicyErrors` still works as before! 
        * On .NET Framework 4.8, the built-in HttpClient is built on top of HttpWebRequest, therefore ServicePointManager settings for the .NET AppDomain will apply to it.
        * On .NET 6+, we switched away from ServicePointManager which only affected HttpWebRequest and not HttpClient. We switched to HttpClientHandler.DangerousAcceptAnyServerCertificateValidator and Client.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication.
2. `New-IshObfuscatedFile` works as before (v0.14 and earlier) on Windows platforms on Windows PowerShell and PowerShell (Core). The cmdlet will give a warning that images cannot be obfuscated on non-Windows system because of missing platform extensions.

## Breaking Changes - Requirements

* The platform requirements moved, requiring **.NET Framework 4.8** (compared to obsolete 4.5 in the past). Microsoft .NET Framework 4.8 is the final version of the Windows-based .NET Framework, all future successors are Microsoft .NET (Core) 6.0 and up based running on linux, macos and windows.

* .NET Framework 4.8 expects **Windows PowerShell 5.1** and older versions are no longer supported. Similar Windows PowerShell 5.1 is the final version of the Windows-based PowerShell, all future successors are PowerShell (Core) 7.0 and up running on linux, macos and windows - powered by Microsoft .NET (Core) 6.0 and up.

* .NET 6.0+ expects **PowerShell 7** or up.

* An explicit or implicit `Import-Module` cmdlet will load file `ISHRemote.psm1` from the `ISHRemote` module that in turn based on `$PSVersionTable.PSEdition` will decide if the .NET 4.8 binary or the .NET 6.0+ binary should be loaded. Note that both variations share the help (`Get-Help`) of the .NET 4.8 variation build by the `XmlDoc2CmdletDoc` package.

* The compiled library of cmdlets will no longer be strong named with an RWS/fSDL private key. This only affects you if you wrote an application (not PowerShell script) on top of this library and if you in turn also signed your compiled application. #80 

## Performance

Below is not an official compare, but a recurring thing we noticed along the way. Using the same backend, same client machine, same ISHRemote build but different PowerShell host we noticed a considerable speed up of the Pester tests.

| Name                     | Platform                            | Branch         |
|--------------------------|-------------------------------------|----------------|
| ISHRemote 6.0.9226.0     | Windows PowerShell 5.1 on .NET 4.8  | Tests completed in 312.4s AND                                                                                Tests Passed: 897, Failed: 0, Skipped: 8 NotRun: 0 |
| ISHRemote 6.0.9226.0     | PowerShell 7.2.6 on .NET 6.0.8      | Tests completed in 281.33s AND Tests Passed: 897, Failed: 0, Skipped: 8 NotRun: 0 |
