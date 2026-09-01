# Release Notes of ISHRemote v8.3

High level release notes are on [Github](https://github.com/rws/ISHRemote/releases/tag/v8.2), below the most detailed release notes we have :)

**Before I forget, all people with a Github account, could you spare a moment to STAR this repository - see top-right Star icon on https://github.com/RWS/ISHRemote/ Appreciated!**


## General

This release inherits the v0.1 to v0.14 up to v8.2 development branch and features. All cmdlets and business logic are fully compatible even around authentication. In short, we expect it all to work still :)

The one that respects the details of Model Context Protocol (MCP) enabling usage in other toolings like Claude Code or OpenCode/OpenChamber. All `OpenApiWithOpenIdConnect` is now using in-flight http compression which benefits `Get-IshPublicationOutputData` offering faster downloads on 15.1.0+ environments.


### Remember
* All C# source code of the ISHRemote library is online at [master](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote), including handling of the different [Connection](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Connection) protocols in a NET 4.8, NET 6.0 and .NET 10.0+ style.
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

The below text describes the delta compared to fielded release ISHRemote v8.2.


## Platform Support for PowerShell ....



## Implementation Details

* Enabled HTTP response compression (`gzip`, `deflate`, `brotli`) on the `HttpClient` for non-SOAP usage across protocols — `WcfSoapWithWsTrust`, `WcfSoapWithOpenIdConnect`, and `OpenApiWithOpenIdConnect`. Both `AutomaticDecompression` on the handler and the matching `Accept-Encoding` request headers are set, so the server can compress response bodies. On .NET 4.8 `brotli` is not available; `gzip` and `deflate` are used instead. See #232. Thanks @ddemeyer
* Augmented `Get-IshPublicationOutputData` with an `OpenApiWithOpenIdConnect` protocol implementation on Tridion Docs 15.1 and higher. Instead of a SOAP byte-level chunk loop (many sequential HTTP calls), a single streaming REST `GET /v3/Publications/ByLanguageCardId/{languageCardId}/Content` request is used, making large publication output downloads significantly faster especially over high-latency connections. Sessions using `OpenApiWithOpenIdConnect` on servers older than 15.1 transparently fall back to the existing SOAP chunk loop. Downloads are in essence network restricted, still streaming shows improvements between 20% and 40% compared to the SOAP variation on the same environment. See #245. Thanks @ddemeyer 
* Fixed `Start-IshRemoteMcpServer` failing to connect on Windows with newer MCP clients (e.g. OpenCode 1.18.11, protocol `2025-11-25`) with errors `MCP error -32001: Request timed out` and `Failed to get tools`. Three root causes: (1) `initialize` requests with `"id":0` were silently dropped because PowerShell treats `0` as falsy; (2) `[Console]::InputEncoding` defaults to OEM code page (`ibm437`) when `pwsh.exe` is spawned with redirected stdio on Windows, causing `ReadLine()` to block forever on UTF-8 JSON — fixed by explicitly setting UTF-8 encoding and replacing `Console.Out` with an auto-flushing `StreamWriter` via `[Console]::SetOut()`; (3) `Register-IshRemoteMcpTool` emitted an invalid `type: "object"` field in `ToolAnnotations` and used string `"true"`/`"false"` instead of boolean `$true`/`$false` for hint values, causing strict MCP schema validation to reject the tools list. Server name updated from `"PowerShell MCP Server (Template)"` to `"ISHRemote MCP Server"` and version bumped to `0.3.0`. Also fixed the server looping forever on stdin EOF (orphaned `pwsh` processes) by breaking the while loop when `ReadLine()` returns `$null`. See #243 and #261. Thanks @ddemeyer
* Migrated all 58 `*.Tests.ps1` files from Pester v5 to Pester v6 (`Should -Be` to `Should-Be`, `Should -BeExactly` to `Should-BeString -CaseSensitive`, `Should -Not -BeNullOrEmpty` to `Should-NotBeNull`, `Should -Throw "msg"` to `Should-Throw -ExceptionMessage "msg"`, etc.). CI install gates updated to `-MinimumVersion 6.0.0`. Classic `Should -Not -Throw` retained as there is no `Should-NotThrow` equivalent in Pester 6. Hardened the library for parallel test execution by replacing the process-wide `TrisoftCmdletLogger` singleton with per-cmdlet `ILogger` routing and adding a double-checked lock on `IshSession._ishTypeFieldSetup` to eliminate Collection was modified races under `Run.Parallel = $true`. CI Pester invocations now use `New-PesterConfiguration` (with `Run.Parallel = $false`) so parallel mode can be toggled in one place when ready. See #242, #265, #266.
* Fixed `New-IshSession` (protocol `WcfSoapWithOpenIdConnect`, PowerShell 7.2+/.NET 6.0+) throwing `FileLoadException: Could not load file or assembly 'Microsoft.IdentityModel.Tokens, Version=8.14.0.0, ...'. The located assembly's manifest definition does not match the assembly reference.` on machines where a different build of `Microsoft.IdentityModel.Tokens` (and related `Duende.IdentityModel.OidcClient`) is registered in the Global Assembly Cache (GAC) — observed on machines with Microsoft Intune Management Extension installed. `AppDomainModuleAssemblyInitializer` now force-loads ISHRemote's own bundled copies of `Duende.IdentityModel`, `Duende.IdentityModel.OidcClient`, `Microsoft.IdentityModel.Abstractions/.Logging/.Tokens/.Tokens.Saml/.Xml` as early as possible during module import, and `SessionCmdlet.BeginProcessing` now reports the full forced list over `-Verbose`. Root cause for the `Duende.IdentityModel.OidcClient` variant: `InfoShareOpenIdConnectSystemBrowser` was `public` and implemented `Duende.IdentityModel.OidcClient.Browser.IBrowser`, which put it in `Trisoft.ISHRemote.dll`'s exported types, forcing PowerShell's own binary-module cmdlet discovery (`Assembly.GetExportedTypes()`) to resolve `Duende.IdentityModel.OidcClient` before `IModuleAssemblyInitializer.OnImport()` ever ran — see Breaking Changes - Code. A new `TestPrerequisite.Tests.ps1` check asserts no assembly is ever loaded from the GAC on PowerShell Core. See #272. Thanks @ddemeyer
* Fixed cmdlets over protocol `WcfSoapWithOpenIdConnect` occasionally throwing `An unsecured or incorrectly secured fault was received from the other party` on the first SOAP call after a channel fault, requiring the user to re-run the same cmdlet for it to succeed (the existing #201/#219 rebuild-on-next-call logic only kicked in on a second, separate call). Each `Get*25Channel()` method in `InfoShareWcfSoapWithOpenIdConnectConnection` now returns the channel wrapped in a new `RetryOnFaultProxy<T>` (`System.Reflection.DispatchProxy`) that catches `CommunicationException`/`FaultException` on the actual SOAP call, rebuilds the channel via the existing rebuild logic, and retries exactly once within the same cmdlet invocation before propagating any further failure — with the original exception type/stack trace preserved. Scoped to `WcfSoapWithOpenIdConnect` only; `WcfSoapWithWsTrust` (maintain-only/deprecated) is untouched. Requires a new `net48`-only NuGet dependency, `System.Reflection.DispatchProxy` (built into the BCL on `net6.0`/`net10.0`). No automated test coverage was added — the `Connection/` layer has no existing Pester tests (WCF channel/fault behavior isn't reproducible without a live server and fault injection); validated by manual reproduction against a live server. See #273. Thanks @ddemeyer



## Breaking Changes - Cmdlets

All cmdlets and business logic are fully compatible.


## Breaking Changes - Code

* `Trisoft.ISHRemote.Connection.InfoShareOpenIdConnectSystemBrowser` changed from `public` to `internal`. It was never intended for external consumption (it only implements `Duende.IdentityModel.OidcClient.Browser.IBrowser` for internal use by `InfoShareOpenIdConnectConnectionBase`), and its `public` visibility was the root cause of the GAC-loading issue described above under Implementation Details. If you compiled against this type directly, switch to your own `IBrowser` implementation. See #272.


## Breaking Changes - Platform

* n/a


## Known Issues

* Aborting the `New-IShSession`/`Test-IShSession` cmdlets using `Ctrl-C` in PowerShell is not possible, you have to await the non-configurable 60 seconds timeout potentially resulting in `GetTokensOverSystemBrowserAsync Error[Browser login canceled after 60 seconds.]`. Typically happens if you did not authenticate in the System Browser.
* Several Authentication known issues...
    * Authentication over System Browser, so Authorization Code Flow with Proof Key for Code Exchange (PKCE) or Pushed Authorization Requests (PAR), will give you 60 seconds. Any slower and you will see the `New-IShSession`/`Test-IShSession` cmdlets respond with `TaskCanceledException` exception stating `Browser login canceled after 60 seconds.`
    * Authentication over Client Credentials Flow with non-existing `-ClientId` will error out with `GetTokensOverClientCredentialsAsync Access Error[invalid_client]; either invalid ClientId/ClientSecret combination or expired ClientSecret.`. Please make sure you activate a client/secret on your Access Management User Profile (ISHAM).
    * Authentication over Client Credentials Flow with expired `-ClientId`/`-ClientSecret` combination will error out with `GetTokensOverClientCredentialsAsync Access Error[invalid_client]; either invalid ClientId/ClientSecret combination or expired ClientSecret.`. Please recycle expired client/secret on your Access Management User Profile (ISHAM).
    * Authentication over Client Credentials Flow with valid `-ClientId`/`-ClientSecret` combination, but not mapped in the CMS to a User Profile over `FISHEXTERNALID` will `[-14] The access is denied because no profile match was found. 0`. Please make sure that the client (which you can find on the Access Management User Profile) is added in Organize Space on one CMS User Profile in the comma-seperated External Id field.
    * Authentication over Client Credentials Flow with valid `-ClientId`/`-ClientSecret` combination, and mapped in the CMS to a User Profile over `FISHEXTERNALID` which is disabled will error out with `[-6] Your account has been disabled. Please see your system administrator.`. Please make sure in Organize Space that the one CMS User Profile holding the client in the External Id field is an enabled profile.
    * Refresh Token is not used to refresh the Access Token in the background (seperate thread), it is only used to refresh when the next cmdlet is triggered before expiration. Authentication over either Client Credentials or System Browser was succesful but the Access Token expired. You do not need to create a `New-IShSession`, every cmdlet will attempt to get a token (either refresh or re-logon if required) based on the cmdlets (implicit) `-IShSession` parameter. 
* Using `New-IshSession` parameter `-PSCredential` on 14SP4/14.0.4 or earlier works like before, as it means username/password authentication over protocol `WcfSoapWithWsTrust`.  However, using `-PSCredential` on 15/15.0.0+ means that you are using protocol `WcfSoapOverOpenIdConnect`, so expecting a client/secret. If you then provide username/password, you will get error `GetTokensOverClientCredentialsAsync Access Error[invalid_client]`. Note that you can force by adding `-Protocol WcfSoapWithWsTrust` to the `New-IshSession` cmdlet.


## Quality Assurance

Added more Invoke-Pester 6.0.0 Tests, see Github actions for the Windows PowerShell 5.1 and PowerShell 7.6+ hosts where
* the skipped are about SslPolicyErrors testing and `ISHRemoteMcpServer` is PowerShell 7+ only
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
| ISHRemote 8.2.13523.0    | PowerShell 7.5.4 on .NET 9.0.10  | LEUDEVDDE...@15.3.0b2303 | Tests completed in 141.61s AND Tests Passed: 1128, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.2.13525.0    | Windows PowerShell 5.1 on .NET 4.8.1 | LEUDEVDDE...@15.3.0b2303 | Tests completed in 132.15s AND Tests Passed: 1087, Failed: 0, Skipped: 52, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.2.13525.0    | PowerShell 7.6.0 on .NET 10.0.5  | LEUDEVDDE...@15.3.0b2303 | Tests completed in 139.45s AND Tests Passed: 1135, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.3.14019.0    | Windows PowerShell 5.1 on .NET 4.8.1 | LEUDEVDDE...@15.3.0b3005 | Tests completed in 123.35s AND Tests Passed: 1260, Failed: 0, Skipped: 54, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.3.14019.0    | PowerShell 7.6.5 on .NET 10.0.11 | LEUDEVDDE...@15.3.0b3005 | Tests completed in 149.16s AND Tests Passed: 1310, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 |
| ISHRemote 8.3.14019.0    | PowerShell 7.6.5 on .NET 10.0.11 | LEUDEVDDE...@15.3.0b3005 | Tests completed in 79.77s AND Tests Passed: 1270, Failed: 0, Skipped: 4, Inconclusive: 0, NotRun: 0 (`$config.Run.Parallel = $true`) |
