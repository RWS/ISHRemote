# Release Notes of ISHRemote v7.0

High level release notes are on [Github](https://github.com/rws/ISHRemote/releases/tag/v7.0), below the most detailed release notes we have :)

Remember
* All C# source code of the ISHRemote library is online at [master](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote), including handling of WS-Trust protocol ([InfoShareWcfConnection.cs](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/InfoShareWcfConnection.cs)) in a NET 4.8 and NET 6.0+ style.
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

The below text describes the delta compared to fielded release ISHRemote v0.14, so will repeat some v1.0 highlights.

## General

This release inherits the v0.1 to v0.14 and v1.0 development branch and features. By enabling PowerShell 7+ powered by NET 6+ next to existing Windows PowerShell 5.1 powered by NET Framework 4.8; we had to do some breaking changes forced by platform support. Most cmdlets and business logic are fully compatible except around authentication (`New-IshSession`, `Test-IshSession` and `New-IshObfuscatedFile`) as described below.

Yes, this is the ISHRemote version that works on Windows PowerShell and PowerShell (Core)! However, regarding PowerShell (Core), only for username/password-based authentication and not for Windows Authentication (WS-Trust protocol over Microsoft ADFS using `windowsmixed`).

| ISHRemote v7.x powered by ISHWS WCF-SOAP | On Windows PowerShell 5.1 powered by .NET Framework 4.8 | On PowerShell (Core) 7.2+ powered by .NET (Core) 6.0+ |
|-|-|-|
| With issuerwstrustbindingtype set to usernamemixed, typically known as ISHSTS | Supported, same as ISHRemote v1 and earlier | *New! Supported since ISHRemote v7!* |
| With issuerwstrustbindingtype set to windowsmixed, typically known as ADFS with Windows Authentication | Supported, same as ISHRemote v1 and earlier | Platform Not Supported |


This is execution of the plan as communicated and described on [ThePlan-ISHRemote-7.0.md](ThePlan-ISHRemote-7.0.md). Furthermore some notable changes are

* Encryption in flight - https - can now also go over Tls 1.3 while before releases only had Tls 1.0, 1.1 or 1.2 as options. #102
* `Add-IshBackgroundTask` fix regarding date time parsing incoming dates incorrectly swapping month/day #137 Thanks @ddemeyer 
* `New-IshObfuscatedFile`, `New-IshDitaGeneralizedXml` and `Test-IshValidXml` received updated xml catalog handling classes aligning with product 14SP4/14.0.4 code base. #146  Thanks @jlaridon

## Implementation Details

Added `\Cmdlets\_TestEnvironment\TestPrerequisite.Tests.ps1` that requires ISHRemote to test if the typical prerequisites for running Pester integration tests are present. Test descriptions give a hint on what services are expected or how to overwrite values in `ISHRemote.PesterSetup.Debug.ps1` (e.g. unexisting status) to comply with your test environment. #4

This is the release where we switched from manifest file `ISHRemote.psd1` entry `NestedModules` (pointing to nested module `Trisoft.ISHRemote.dll`) to `CmdletsToExport` holding a list of cmdlets. Remember to review `CmdletsToExport` if new cmdlets are added.

## Breaking Changes - Cmdlets

Again, most cmdlets and business logic are fully compatible, except the below:

1. `New-IshSession` and `Test-IshSession` hosted by PowerShell (Core) 7+ can only do `usernamemixed`. The other variation called `windowsmixed` authentication, also known as Windows Authentication typically required for Microsoft ADFS, is not supported by the .NET 6.0 platform. Note that these cmdlets hosted by Windows PowerShell 5.1 still support `windowsmixed` as before.
    1. Parameter sets ending with `ExplicitIssuer` offering explicit WS-Trust Issuer Url `-WsTrustIssuerUrl` and WS-Trust Issuer Metadata Exchange Url `-WsTrustIssuerMexUrl` are removed.
    2. Parameters `-TimeoutIssue` and `-TimeoutService` are removed. Any usage is replaced by `-Timeout`.
    3. Parameter `-IshUserName` can still be left empty on Windows PowerShell 5.1/NET4.8 but will throw an error on PowerShell7+/NET6+ as Microsoft's [WSFederationHttpBinding](https://devblogs.microsoft.com/dotnet/wsfederationhttpbinding-in-net-standard-wcf/) library so far has no support for the `windowsmixed` WS-Trust protocol variation.
    4. Parameter `-IgnoreSslPolicyErrors` still works as before! Although the scope of this parameter is reduced to `ISHRemote` instead of your complete PowerShell process session.
        * On .NET Framework 4.8, HttpClient is built on top of HttpWebRequest, therefore ServicePointManager settings for the .NET AppDomain will apply to it.
        * On .NET 6+, we switched away from ServicePointManager which only affected HttpWebRequest and not HttpClient. We switched to HttpClientHandler.DangerousAcceptAnyServerCertificateValidator and Client.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication.
2. `New-IshObfuscatedFile` works as before (v0.14 and earlier) on Windows platforms on Windows PowerShell and PowerShell (Core). The cmdlet will give a warning that images cannot be obfuscated on non-Windows systems because of missing Microsoft Platform Extensions.

## Breaking Changes - Requirements

* The platform requirements moved, requiring **.NET Framework 4.8** (compared to obsolete 4.5 in the past). Microsoft .NET Framework 4.8 is the final version of the Windows-based .NET Framework, all future successors are Microsoft .NET (Core) 6.0 and up based running on linux, macos and windows.

* .NET Framework 4.8 expects **Windows PowerShell 5.1** and older versions are no longer supported. Similar Windows PowerShell 5.1 is the final version of the Windows-based PowerShell, all future successors are PowerShell (Core) 7+ running on linux, macos and windows - powered by Microsoft .NET (Core) 6.0 and up.

* .NET 6.0+ expects **PowerShell 7.2** or up.

* An explicit or implicit `Import-Module` cmdlet will load file `ISHRemote.psm1` from the `ISHRemote` module that in turn based on `$PSVersionTable.PSEdition` will decide if the .NET 4.8 binary or the .NET 6.0+ binary should be loaded. Note that both variations share the help (`Get-Help`) of the .NET 4.8 variation build by the `XmlDoc2CmdletDoc` package.

* .NET Framework 4.8 code still uses single `AppliesTo` realm url like `https://ish.example.com/ISHWS/` since Knowledge Center 2016/12.0.0. Before the product used full `AppliesTo` urls like `https://ish.example.com/ISHWS/Wcf/API25/Application.svc`, all these legacy entries still exist in ISHSTS (and Microsoft ADFS scripts) and are now required again for this `ISHRemote` version. This because .NET 6.0+ expects full `AppliesTo` urls. Otherwise you might get errors like:
    1. Client side error message was `An error occurred when processing the security tokens in the message.` ([1](https://stackoverflow.com/questions/2763592/the-communication-object-system-servicemodel-channels-servicechannel-cannot-be?utm_medium=organic&amp)/[2](https://social.msdn.microsoft.com/Forums/vstudio/en-US/fc60cd6d-1df9-47ff-90a8-dd8d5de1f080/the-communication-object-cannot-be-used-because-it-is-in-the-faulted-state?forum=wcf)), which after enabling diagnostics in \ISHWS\web.config WCF diagnostics was more detailed to `ID1038: The AudienceRestrictionCondition was not valid because the specified Audience is not present in AudienceUris. Audience: 'https://ish.example.com/ISHWS/Wcf/API25/Edt.svc'`. .NET 6.0+ code offers the full .svc url while .NET 4.8 ends with ISHWS/ in this example. And this is all case-sensitive, ISHSTS contains `EDT.svc` while the code contained `Edt.svc`. A temporary developer's workaround was adding the correct casing in ISHWS\web.config under `<system.identityModel><identityConfiguration...><audienceUris>`.
    2. Client side error message was `An error occurred when processing the security tokens in the message.`, which after enabling diagnostics in \ISHWS\web.config WCF diagnostics was more detailed to `ID4022: The key needed to decrypt the encrypted security token could not be resolved. Ensure that the SecurityTokenResolver is populated with the required key.` This error only occured on managed systems (like sdlproducts.com) where a token-signing certificate rollover in ISHSTS was not done for two more recently added endpoints `Wcf/API25/Annotation.svc` and `Wcf/API25/BackgroundTask.svc` where on the relying party they were still reading `Encrypting Certificate CN=tokensigning4.sdlproducts.com` instead of `Encrypting Certificate CN=tokensigning5.sdlproducts.com`.
* Remember that .NET Framework 4.8 code base uses the `IssueToken` call to get a token to validate expiration and reuses it for every channel/client. .NET 6.0+ still calls `IssueToken` to do token `ValidTo` expiration but individual tokens are retrieved per `.svc` endpoint as `CreateChannelWithIssuedToken` is not present in Microsoft's [WSFederationHttpBinding](https://devblogs.microsoft.com/dotnet/wsfederationhttpbinding-in-net-standard-wcf/).

* The compiled library of cmdlets will no longer be strong named with an RWS/fSDL private key. This only affects you if you wrote an application (not PowerShell script) on top of this library and if you in turn also signed your compiled application. #80 


## Quality Assurance

Added more Invoke-Pester 5.3.0 Tests, see Github actions for the Windows PowerShell 5.1 and PowerShell 7+ hosts where
* the skipped are about SslPolicyErrors testing
* the failed are about IMetadata bound fields (issue #58)

Below is not an official performance compare, but a recurring thing noticed along the way. Using the same client machine, same ISHRemote build and same backend but different PowerShell hosts we noticed a considerable speed up of the Pester tests.

| Name                     | Client Platform                     | Server Platform       | Test Results         |
|--------------------------|-------------------------------------|----------------------|----------------|
| ISHRemote 6.0.9523.0     | Windows PowerShell 5.1 on .NET 4.8  | SOAP-WCF and WS-Trust | Tests completed in 353.57s AND                                                                                Tests Passed: 917, Failed: 0, Skipped: 8 NotRun: 0 |
| ISHRemote 6.0.9523.0     | PowerShell 7.3.0 on .NET 7.0.0      | SOAP-WCF and WS-Trust | Tests completed in 305.46s AND Tests Passed: 921, Failed: 0, Skipped: 8 NotRun: 0 |
