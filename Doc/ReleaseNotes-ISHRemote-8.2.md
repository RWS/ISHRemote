# Release Notes of ISHRemote v8.2

High level release notes are on [Github](https://github.com/rws/ISHRemote/releases/tag/v8.2), below the most detailed release notes we have :)

**Before I forget, all people with a Github account, could you spare a moment to STAR this repository - see top-right Star icon on https://github.com/RWS/ISHRemote/ Appreciated!**


## General

This release inherits the v0.1 to v0.14 up to v8.1 development branch and features. All cmdlets and business logic are fully compatible even around authentication. In short, we expect it all to work still :)


### Remember
* All C# source code of the ISHRemote library is online at [master](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote), including handling of the different [Connection](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Connection) protocols in a NET 4.8 and NET 6.0+ style.
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

The below text describes the delta compared to fielded release ISHRemote v8.1.


## Actively recover interactive sessions

Every usage of a cmdlet will refresh the security tokens. However, when not using ISHRemote cmdlets or the implicit local or global `$ISHRemoteSessionStateIshSession` or explicit `$ishSession` object, the session expires by default after around 57 minutes when using ISHID or similar on other identity providers. In turn resulting in error `An unsecured or incorrectly secured fault was received from the other party. See the inner FaultException for the fault code and detail.`.

In this ISHRemote version, the session will attempt to get a new token automatically on every triggererd ISHRemote cmdlet. If you created the IShSession object over an interactive browser, you will see the browser again perhaps with or without a credential challenge in the browser. Change is only for protocols `WcfSoapWithOpenIdConnect` and `OpenApiWithOpenIdConnect`; no change for `WcfSoapWithWsTrust`.

Infamous random annoying error `The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it is in the Faulted state.` should now recover within the cmdlet or worst-case when rerunning the same cmdlet. This without applying the earlier workaround of building a `New-IshSession`.


## Adding experimental ISHRemote MCP Server

ISHRemote is a PowerShell library that abstracts some authentication and metadata modelling complexity from the Tridion Docs API in an opionated way like default metadata. ISHRemote also comes with a built-in help for every cmdlet - or should I say MCP Tool - as you can see in for example `Get-Help New-IshSession`.

Model Concept Protocol (MCP) is the language to offer tools to your chosen Large Language Model (LLM) to have smart data interactions.

This experiment serves two purposes...
1. As the MCP Tools to ISHRemote cmdlet ratio including cmdlet parameters is one-to-one. You can reuse your LLM's PowerShell knowledge and the ISHRemote MCP Server to draft PowerShell scripts. You can ask it questions on parameter usage, filters, concepts, etc that would get you started... although the LLM will not always offer working code, but something that you can debug and get working.
2. As the MCP Tools are offered over PowerShell (pwsh), it means you can ask it questions like `Create a new ishsession to https://ish.example.com/ISHWS/`. And yes it will create you that `$ishSession` variable in the background that you can use over the other MCP Tools - euh cmdlets ;) - allowing you to follow up with `How many user roles are there?`.

So what do you need...
1. You need ISHRemote (PreRelease) installed in your PowerShell v7 (not Windows PowerShell!)
2. Inside Visual Studio Code, you need to extend your `/.vscode/mcp.json` with the below code block. Make sure to point to an existing log file path and use the double backslashes to comply with the json file syntax.
3. Start the `IshRemoteMcpServer` using the decorator `Start`
4. Go to your CoPilot, put it in `Agent` mode and start with a `Create a new ishsession to https://ish.example.com/ISHWS/`

```json
{
    "servers": {
        "IshRemoteMcpServer": {
            "type": "stdio",
            "command": "pwsh",
            "args": [                
                "-NoProfile",
                "-Command",
                "& { Start-IshRemoteMcpServer -CmdletsToRegister (Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name -LogFilePath \"D:\\GITHUB\\ISHRemote\\IshRemoteMcpServer.log\" }"
            ]
        }
    }
}
```

How does it technically work? This allows the ISHRemote library to register itself as a local `stdio` transport Mcp server `ISHRemoteMcpServer`. Overall as a thin client layer that pushes the heavy lifting via PowerShell over ISHRemote and in turn HTTPS to a Tridion Docs server. Hat tip to [dfinke/PSMCP](https://github.com/dfinke/PSMCP) offering a generic PowerShell library wrapper which is self-contained ISHRemote to make it easier for our user base.


## Implementation Details

* Refactored `WcfSoapWithOpenIdConnect`-based code to cache the Access Tokens and in turn SOAP channels on a class level lower than `IShSession` class. This to optimize `Refresh Token` usage for NET48 and NET6+ #210 Thanks @ddemeyer 
* Along the way errors like `The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it is in the Faulted state.`, `An unsecured or incorrectly secured fault was received from the other party.` or `Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException: IDX10223: Lifetime validation failed. The token is expired.` are recovered more automatically when using protocol `WcfSoapWithOpenIdConnect`. #210 Thanks @ddemeyer
* Added a better detection of the error and in turn retry for `The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it is in the Faulted state.` by introducing a upon `Faulted` `EventHandler` handler for every `Get...Channel()` function for `WcfSoapWithOpenIdConnect` and `WcfSoapWithWsTrust`. #214 #219 Thanks @copilot @ddemeyer 
* `New-IShSession ... -IgnoreSslPolicyErrors` option on PowerShell 7+ is now even more resilient. The parameter is now passed over `InfoShareOpenIdConnectConnectionParameters` class to activate the same `SslCertificateAuthentication` code as `InfoShareWcfSoapWithWsTrustConnectionParameters` does. Thereby avoiding errors like `The remote certificate is invalid because of errors in the certificate chain: NotTimeValid` and `The remote certificate is invalid because of errors in the certificate chain: UnTrustedRoot`. #211 Thanks @ddemeyer 
* Added extra global variable `$global:ISHRemoteSessionStateIshSession` next to existing local variable `$ISHRemoteSessionStateIshSession` to allow nested function usage like in a PowerShell-based ModelContextProtocol (MCP) Server. #224  Thanks @ivandelagemaat @ddemeyer 
* Since 15.3.0 WcfSoapWithOpenIdConnect will no longer be hosted by Windows .NET Framework 4.8 as runtime but switched to .NET10+ runtime as host with the help of [CoreWCF](https://github.com/CoreWCF/CoreWCF). Although public SOAP API compatibility is of uttermost importance, the product team could not get _managing ReaderQuotas_ working in exactly the same way as before causing an ISHRemote crash. The ISHRemote `ReaderQuotas` implementation is now much sturdier and works for `WcfSoapWithOpenIdConnect` (ISHWS) hosted on .NET Framework 4.8-based and .NET8+based runtimes - works across Tridion Docs product versions. #207 Thanks @jlaridon 
* OpenAPI proxies of Access Management and Tridion Docs CMS were refreshed. The Access Management API proxies, accessible over `$ishSession.OpenApiAM10Client`, changed and standardized as all API OperationIds are no longer written in CamelCase but Snake_Case. So for example `$ishhSession.OpenApiAM10Client.IdentityProvidersGetAsync()` becomes `$ishhSession.OpenApiAM10Client.IdentityProviders_GetAsync()` with one extra underscore. #223 
* `Remove-IshBaselineItem` used version `999999` when removing an entry from the baseline. Since 15.2.0 this has to be the correct version or an empty version, but not the wrong `999999` version. #205 Thanks @OlegTokar 
* Rewrite cmdlet `Add-IshBackgroundTask` to not use Microsoft.IdentityModel.Tokens.CollectionUtilities.IsNullOrEmpty extension method which was made `private` (and no longer `public`) in version 8.0.0 of `Microsoft.IdentityModel.Tokens.dll` #216 Thanks @ddemeyer 


## Breaking Changes - Cmdlets

All cmdlets and business logic are fully compatible.


## Breaking Changes - Code

* The Access Management API proxies, accessible over `$ishSession.OpenApiAM10Client`, changed and standardized as all API OperationIds are no longer written in CamelCase but Snake_Case. So for example `$ishhSession.OpenApiAM10Client.IdentityProvidersGetAsync()` becomes `$ishhSession.OpenApiAM10Client.IdentityProviders_GetAsync()` with one extra underscore. #223


## Breaking Changes - Platform

* Replaced package references of deprecated `IdentityModel.OidcClient` to supported `Duende.IdentityModel.OidcClient` plus matching code changes. More details in the releases notes and on #220 

| PackageReference From Version                         | PackageReference To Version                     | Remarks                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| ----------------------------------------------------- | ----------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| IdentityModel 7.0.0                                   | Duende.IdentityModel 7.1.0                      |                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| IdentityModel.OidcClient 7.0.0                        | Duende.IdentityModel.OidcClient 6.0.1           |                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| IdentityModel.OidcClient.IdentityTokenValidator 6.0.0 | \-                                              | With the removal of Hybrid Flow support from (Duende.)IdentityModel.OidcClient, it is not necessary anymore to validate id_tokens. However, You can still do you own validation via our extensibility points if desired. See [https://community.auth0.com/t/managing-tokens-in-net-maui/101577/25?page=2](https://community.auth0.com/t/managing-tokens-in-net-maui/101577/25?page=2)                                                                             |
| Microsoft.PowerShell.Commands.Management 7.2.23       | Microsoft.PowerShell.Commands.Management 7.2.24 | Unsupported PowerShell 7.2/NET6 but all works well; will wait for future 7.6/NET10 or higher to bump this package and the underlying net6.0/Trisoft.ISHRemote.dll compilation.                                                                                                                                                                                                                                                                                    |
| System.Runtime.CompilerServices.Unsafe 6.0.0          | System.Runtime.CompilerServices.Unsafe 6.1.2    |                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| System.Numerics.Vectors 4.5.0                         | System.Numerics.Vectors 4.5.0                   | Tried 4.6.1 \`ApplicationException: GetTokensOverClientCredentialsAsync Access Error[Could not load file or assembly 'System.Numerics.Vectors, Version=4.1.4.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified.]; either invalid ClientId/ClientSecret combination or expired ClientSecret.\`Assembly is lock stepped with System.ServiceModel packages, so rolled back to earlier 4.5.0 |
| System.Text.Json 8.0.5                                | System.Text.Json 8.0.6                          |                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |


## Known Issues

* Aborting the `New-IShSession`/`Test-IShSession` cmdlets using `Ctrl-C` in PowerShell is not possible, you have to await the non-configurable 60 seconds timeout potentially resulting in `GetTokensOverSystemBrowserAsync Error[Browser login canceled after 60 seconds.]`. Typically happens if you did not authenticate in the System Browser.
* Several Authentication known issues...
    * Authentication over System Browser, so Authorization Code Flow with Proof Key for Code Exchange (PKCE), will give you 60 seconds. Any slower and you will see the `New-IShSession`/`Test-IShSession` cmdlets respond with `TaskCanceledException` exception stating `Browser login canceled after 60 seconds.`
    * Authentication over Client Credentials Flow with non-existing `-ClientId` will error out with `GetTokensOverClientCredentialsAsync Access Error[invalid_client]; either invalid ClientId/ClientSecret combination or expired ClientSecret.`. Please make sure you activate a client/secret on your Access Management User Profile (ISHAM).
    * Authentication over Client Credentials Flow with expired `-ClientId`/`-ClientSecret` combination will error out with `GetTokensOverClientCredentialsAsync Access Error[invalid_client]; either invalid ClientId/ClientSecret combination or expired ClientSecret.`. Please recycle expired client/secret on your Access Management User Profile (ISHAM).
    * Authentication over Client Credentials Flow with valid `-ClientId`/`-ClientSecret` combination, but not mapped in the CMS to a User Profile over `FISHEXTERNALID` will `[-14] The access is denied because no profile match was found. 0`. Please make sure that the client (which you can find on the Access Management User Profile) is added in Organize Space on one CMS User Profile in the comma-seperated External Id field.
    * Authentication over Client Credentials Flow with valid `-ClientId`/`-ClientSecret` combination, and mapped in the CMS to a User Profile over `FISHEXTERNALID` which is disabled will error out with `[-6] Your account has been disabled. Please see your system administrator.`. Please make sure in Organize Space that the one CMS User Profile holding the client in the External Id field is an enabled profile.
    * Refresh Token is not used to refresh the Access Token in the background (seperate thread), it is only used to refresh when the next cmdlet is triggered before expiration. Authentication over either Client Credentials or System Browser was succesful but the Access Token expired. You do not need to create a `New-IShSession`, every cmdlet will attempt to get a token (either refresh or re-logon if required) based on the cmdlets (implicit) `-IShSession` parameter. 
* Using `New-IshSession` parameter `-PSCredential` on 14SP4/14.0.4 or earlier works like before, as it means username/password authentication over protocol `WcfSoapWithWsTrust`.  However, using `-PSCredential` on 15/15.0.0+ means that you are using protocol `WcfSoapOverOpenIdConnect`, so expecting a client/secret. If you then provide username/password, you will get error `GetTokensOverClientCredentialsAsync Access Error[invalid_client]`. Note that you can force by adding `-Protocol WcfSoapWithWsTrust` to the `New-IshSession` cmdlet.
* On the Github Actions container-based build I received error `Could not load file or assembly 'System.ServiceModel.Primitives, Version=4.10.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified.`. This PowerShell 7.2.x issue is seemingly resolved since 7.3.6 as mentioned [here](https://github.com/dotnet/wcf/issues/2862) and has to do with loading .NET Standard libaries in platform libraries (like Trisoft.ISHRemote.dll). Therefor extended the `continuous-integration.yml` to upgrade to PowerShell Preview using [pwshupdater](https://github.com/marketplace/actions/pwshupdater).


## Quality Assurance

Added more Invoke-Pester 5.7.1 Tests, see Github actions for the Windows PowerShell 5.1 and PowerShell 7.5+ hosts where
* the skipped are about SslPolicyErrors testing
* the failed are about IMetadata bound fields (issue #58)

Below is not an official performance compare, but a recurring thing noticed along the way. Using the same client machine, same ISHRemote build and same backend but different PowerShell hosts we noticed a considerable speed up of the Pester tests. However, adding (complicated) tests along the way and knowing that ISHRemote as client library greatly depends on the server-side load, we have to take these numbers at face value.

| Name                     | Client Platform ($PSVersionTable on [Environment]::Version)  | Server Platform       | Test Results         |
|--------------------------|-------------------------------------|----------------------|----------------|
| ISHRemote 8.0.10919.0    | PowerShell 7.4.0 on .NET 8.0.0 | LVNDEVDEM... | Tests completed in 449.72s AND Tests Passed: 1057, Failed: 0, Skipped: 3 NotRun: 0 |
| ISHRemote 8.1.11623.0    | Windows PowerShell 5.1 on .NET 4.8.1  | LVNDEVDEM... | Tests completed in 515.62s AND Tests Passed: 1063, Failed: 0, Skipped: 4 NotRun: 0 |
| ISHRemote 8.1.11623.0    | PowerShell 7.4.5 on .NET 8.0.0 | LVNDEVDEM... | Tests completed in 467s AND Tests Passed: 1063, Failed: 0, Skipped: 4 NotRun: 0 |
| ISHRemote 8.1.11716.0    | Windows PowerShell 5.1 on .NET 4.8.1  | LVNDEVDEM... | Tests completed in 642.2s AND Tests Passed: 1064, Failed: 0, Skipped: 4 NotRun: 0 |
| ISHRemote 8.1.11716.0    | PowerShell 7.5.1 on .NET 9.0.4  | LVNDEVDEM...@15.2.0 | Tests completed in 662.81s AND Tests Passed: 1064, Failed: 0, Skipped: 4 NotRun: 0 |
| ISHRemote 8.2.13001.0    | Windows PowerShell 5.1 on .NET 4.8.1  | LEUDEVDDE...@15.2.0 | Tests completed in 197.85s AND Tests Passed: 1071, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.2.13001.0    | PowerShell 7.5.3 on .NET 9.0.8  | LEUDEVDDE...@15.2.0 | Tests completed in 173.09s AND Tests Passed: 1071, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.2.13001.0    | Windows PowerShell 5.1 on .NET 4.8.1 | LEUDEVDDE...@15.3.0b2216 | Tests completed in 125.49s AND Tests Passed: 1071, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.2.13001.0    | PowerShell 7.5.3 on .NET 9.0.8  | LEUDEVDDE...@15.3.0b2216 | Tests completed in 111.41s AND Tests Passed: 1071, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.2.13106.0    | Windows PowerShell 5.1 on .NET 4.8.1 | LEUDEVDDE...@15.3.0b2303 | Tests completed in 151.73s AND Tests Passed: 1071, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.2.13106.0    | PowerShell 7.5.4 on .NET 9.0.10  | LEUDEVDDE...@15.3.0b2303 | Tests completed in 144.6s AND Tests Passed: 1071, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
