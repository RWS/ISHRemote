# The Execution of the plan of ISHRemote v8.0

This page will try to track work in progress. And because I work on it in free time, it will help trace how I got where I am in the first place plus what is next. Inspired by [ThePlan-ISHRemote-7.0.md](./ThePlan-ISHRemote-7.0.md) and [TheExecution-ISHRemote-7.0.md](./TheExecution-ISHRemote-7.0.md).

Remember
* https://mecdev12qa01.global.sdl.corp/ISHWSSQL2017/Api/api-docs/index.html requires pre-authentication
* https://mecdev12qa01.global.sdl.corp/ISHCSSQL2017/OrganizeSpace/OApi/api-docs/index.html forces authentication



# On Tridion Docs 14SP4/14.0.4 and earlier
`New-IShSession` offered 3 parameter groups on Tridion Docs 14SP4/14.0.4 and earlier depending on the WS-Federation/WS-Trust configuration. That configuration is decided by `\InfoShareWS\connectionconfiguration.xml` for ISHRemote and repurposed from the Client Tools.
* ActiveDirectory, so only `-WsBaseUrl`, where an empty/non-provided `-IShUserName` indicated fall back to `NetworkCredentials` which is also known as Windows Authentication.
* PSCredential, so `-WsBaseUrl` and `-PSCredential`, where PowerShell will prompt for a username/password combination.
* UserNamePassword, so `-WsBaseUrl`, `-IShUserName` and `-IShPassword`. The classic authentication on Tridion Docs User Profiles.



# Since Tridion Docs 15/15.0.0
First you need to get Authenticated, then you need to get Authorized. The Authentication happens over Access Management (ISHAM), potentially federated out through a user-agent (System Browser) that supports redirection from the authorization server.

Once Authenticated, you have an external id.

Repurpose or introduce `New-IShSession` parameter groups, and matching `Test-IshSession` groups.

## Add parameter group ClientSecret [APPROVED]
Parameters `-Client` and `-Secret` can be easily codified
As Access Management (ISHAM) owns the client/secret combination which is linked to the Tridion Docs User Profile, a seperate flow makes sense. Perhaps short-circuited through `connectionconfiguration.xml` configuration.
Used on protocçols `WcfSoapWithOpenIdConnect` and `OpenApiWithOpenIdConnect`.

## Repurpose parameter group ActiveDirectory by rename to Interactive [APPROVED]
Active Directory only makes sense for interactive mode and in essence there was no parameter to fill in. ISHRemote prefilled credentials with `NetworkCredentials`.
Repurposing this group with no explicit parameters make sense. Depending on the `infosharesoftwareversion` mentioned in `connectionconfiguration.xml` to remain passing `NetworkCredentials` or launch a System Browser based authentication.
Used on protocols `WcfSoapWithWsTrust` only for `WindowsMixed` variation; while `WcfSoapWithOpenIdConnect` and `OpenApiWithOpenIdConnect` only over System Browser interactive authentication.
Add `-Timeout` parameter to this parameter group.

## Repurpose parameter group UserNamePassword [DENIED]
`-IShUserName` and`-IShPassword` can be repurposed but will lead to confusion. In unattended mode it is Access Management (ISHAM) that owns the client/secret combination which is linked to the Tridion Docs User Profile but you will not reuse the username/password combination.
Used on protocols `WcfSoapWithWsTrust`.

## Repurpose parameter group PSCredential [PROBABLY]
`-PSCredential` can be repurposed but *might* lead to confusion. In unattended mode it is Access Management (ISHAM) that owns the client/secret combination which is linked to the Tridion Docs User Profile but you will not reuse the username/password combination. Guidance how to use Credential object by `connectionconfiguration.xml`.
Used on protocols `WcfSoapWithWsTrust` only for `UserNameMixed` variation; while `WcfSoapWithOpenIdConnect` and `OpenApiWithOpenIdConnect` only over Client Credentials authentication.




# Protocol and Parameter Group Scenarios

On Tridion Docs 14SPx/14.0.x and earlier, it is always `WcfSoapWithWsTrust`. Full functionality on PowerShell 5.1 regarding `WindowsMixed` and `UserNameMixed` while PowerShell 7.2+ is limited to authentication over `UserNameMixed` provided by `ISHSTS`-only. 
Starting from Tridion Docs 15/15.0.0 most customers will use `WcfSoapWithOpenIdConnect`. Legacy variation `WcfSoapWithWsTrust` can still be selected as well. Experimenting on Tridion Docs 15/15.0.0 is possible for cmdlets having a side-by-side implementation when using `Protocol` `OpenApiWithOpenIdConnect`; when the OpenAPI implementation is not there, a fall back to `WcfSoapWithOpenIdConnect` will happen.

1. Explicit parameter group `Protocol`
    1. Use `WcfSoapWithWsTrust`, so SOAP 1.2 end points protected by WS-Federation/WS-Trust
    2. Use `WcfSoapWithOpenIdConnect`, so SOAP 1.2 end points protected by Access Management
    3. Use `OpenApiWithOpenIdConnect`, so OpenApi end points protected by Access Management
2. Reading `/ISHWS/connectionconfiguration.xml`
3. If `infosharesoftwareversion` <= 15.0.0
    1. Set `Protocol` to `WcfSoapWithWsTrust`
    2. Note that `issuer/authenticationtype` should be `WindowsMixed` (only PowerShell 5.1) or `UserNameMixed` (on PowerShell 5.1 and PowerShell 7.2+)
    3. Allow parameter groups ActiveDirectory/PSCredential/UserNamePassword as before
4. If `infosharesoftwareversion` >= 15.0.0 but < 16.0.0 (so only private OpenApi)
    1. Reading `/ISHWS/owcf/connectionconfiguration.xml`
    3. If `issuer/authenticationtype` equals `AccessManagement`
        1. Set `Protocol` to `WcfSoapWithOpenIdConnect`
        2. Allow parameter groups ClientSecret, Interactive and PSCredential
5. If `infosharesoftwareversion` >= 16.0.0 (so only public OpenApi)
    1. Reading _unknown_ configuration file, future will tell, for now you can only get here by explicit `Protocol` usage
    2. Set `Protocol` to `OpenApiWithOpenIdConnect`
    3. Allow parameter groups ClientSecret, Interactive and PSCredential



# Problem: ISHRemote in Background - Client Credential Flow
ISHRemote can run in unattended mode (see section Authorization Code Flow with Proof Key for Code Exchange (PKCE)) or in unattended mode which is described here.


## Analysis
* You can use the OAuth 2.0 client credentials grant specified in RFC 6749, sometimes called two-legged OAuth, to access web-hosted resources by using the identity of an application. This type of grant is commonly used for server-to-server interactions that must run in the background, without immediate interaction with a user. These types of applications are often referred to as daemons or service accounts. 
* As a side note, refresh tokens will never be granted with this flow as client_id and client_secret (which would be required to obtain a refresh token) can be used to obtain an access token instead. [source](https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
* Using AWS Credentials mentions using parameters `-AccessKey` and `-SecretKey` to manage and store profiles. [source](https://docs.aws.amazon.com/powershell/latest/userguide/specifying-your-aws-credentials.html)



# Problem: ISHRemote in Foreground - Authorization Code Flow with Proof Key for Code Exchange (PKCE)
ISHRemote as an interactive user where you can actively provide credentials. Even follow any federation like a different Secure Token Service (STS) or Multi Factor Authentication (MFA) in the System Browser.


## Analysis
* The OAuth 2.0 authorization code grant type, or auth code flow, enables a client application to obtain authorized access to protected resources like web APIs. The auth code flow requires a user-agent that supports redirection from the authorization server (the Microsoft identity platform) back to your application. For example, a web browser, desktop, or mobile application operated by a user to sign in to your app and access their data.
* Apps using the OAuth 2.0 authorization code flow acquire an access_token to include in requests to resources protected by the Microsoft identity platform (typically APIs). Apps can also request new ID and access tokens for previously authenticated entities by using a refresh mechanism. 
* Access tokens are short lived. Refresh them after they expire to continue accessing resources. You can do so by submitting another POST request to the /token endpoint. Provide the refresh_token instead of the code. (source)[https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow]




# Problem: ISHRemote to combine ISHWS and ISHAM Data Sources


## Add public proxy OpenApiAM20Service next to OpenApiISH30Service
The `IshSession` object will need a public `OpenApiISH30Service` served by NSwag generated `Trisoft.ISHRemote.OpenApi.OpenApiISH30Service`. This proxy, just like pre-authenticated WCF SOAP WS-Trust proxies `DocumentObj25`, will be used by cmdlets but can be used to open functionality that is not supported by the cmdlets yet.

Imagine `IshSession` object offering a public `OpenApiAM20Service` served by NSwag generated `Trisoft.ISHRemote.OpenApi.OpenApiAM20Service`. This pre-authenticated proxy can open functionality that is not supported by cmdlets yet. Pre-authenticated as Tridion Docs User Profiles holding the **Administrator** User Role are administrator on Access Management (ISHAM) anyway.


## IDEA: Add cmdlet Sync-IShUser
By supporting `-WhatIf` one could see which changes will be applied either way, still returning objects that would get changed. By parameters similar to `Compare-IshTypeFieldDefinition` you could do left-hand vs right-hand side validation.

### User Profiles update from Tridion Docs to Access Management
Access Management User profiles have *Basic settings* (Name, Email, Language and Region), *Application Access* and *Services and roles*. Initially this information is correct, but might become incorrect over time.

Tridion Docs User Profiles might offer:
* An updated Name (perhaps a naming convention), Email and Language (needs ISO mapping)
* Account is disabled
* Account User Roles changed, enabling more or less *Services and roles*

Key question how to map an IShUser to an AMUser. Is AMUser `User subject` the claim used in `FISHEXTERNALID`? Yes.

Typical cmdlet behavior - `Sync-IShUser -IShUser <all> -ToAccessManagementUsers` - is to do this synchronize by default for all users or a selected subset. The `-WhatIf` would return `IShUser`s where a create/update would have happened.

### User Profiles update from Access Management to Tridion Docs
Theoretical option, no user scenario yet.

### Service users update from Tridion Docs to Access Management
Access Management Service account profiles have a *Name*, *ClientID with 1-or-2 Secrets* and *Services and roles*. 
* A scenario is to update the Access Management Service account *Name* to comply with a naming convention originating from the Tridion Docs User Profile. One could even have the ClientID the same as the Tridion Docs Username value, so both `ServiceUser` for example.
* In case the Access Management Service account is missing, it can be created. And the IShUser could get an `FISHEXTERNALID` entry. Note that if you use ClientID equal to the Username which is set in `FISHEXTERNALID` then you do not need to add anything to `FISHEXTERNALID`. So the trick is to use ClientID equal to `USERNAME` field.
* In case the Tridion Docs User Profile is disabled, it could revoke the Secrets.

Typical cmdlet behavior - `Sync-IShUser -IShUser <selection> -ToAccessManagementServiceAccounts` - is to do this synchronize for a mandatory selection as you don't want all (`Find-IShUsers`) to become service accounts. The `-WhatIf` would return `IShUser`s where a create/update would have happened.

### Service users update from Access Management to Tridion Docs
Theoretical option, no user scenario yet. Perhaps there is Access Management Service user that - over *ClientID* does not have a match with a Tridion Docs User Profile - usage would result in a missing profile match error anyway.


## IDEA: Add IShSession Smart mode parameter to aggregate data sources
Would a **smart** mode on the session make sense? So imagine `Find-IShUser` or `Get-IShUser`...
* Currently IShUser objects holds ISHWS information. So **LastLogin** is only filled in when the `PASSWORD` on this Tridion Docs User Profile was used, more and more scenarios over Access Management (ISHAM) will leave it empty.
* Returning any ClientId and Secret (first characters) could help analysis.


## IDEA: Add Access Management cmdlets 
Either some basic cmdlets in **ISHRemote** that return an object model that can be used as input for other cmdlets. So `Get-AMUser` returns Access Management `AMUser`s, where the `ClientId` field could be used to `Find-IshUser`. And `Set-AMUser` accepting `IShUser` where the `FISHEXTERNALID` could be used to update Access Management user profiles.

Add (nested binary module) AMRemote that could offer cmdlets like
* `New-AMSession`, similar to `New-IShSession`, that returns an `AMSession` object with OpenApi proxy.
* `Get-AMUser`, `Set-AMUser` and `Remove-AMUser`. It would be nice if ISHRemote and AMRemote would understand each others object model. That is why just adding some cmdlets in ISHRemote is so much easier than a clean AMRemote PowerShell automation library.

# Compatiblity
ISHRemote compatibility is on its Cmdlets and parameters groups wherever possible across ISHRemote major and minor versions. On the inside refactoring is required to introduce Wcf Soap with OpenIdConnect authentication or later even OpenApi with OpenIdConnect authentication. So in practice people that link to the ISHRemote assembly in their program code will find feature parity but might not find code compatibility.
Refer to __ConnectionClassDiagram restructering but also example code. Explain the relations among these files, so WcfSoapBearerToken as workaround mentioned by Duende, delivering reused WcfSoap proxies that are compatible (which is cool!)

# Done
* Merging in #115 branch that was AsmxSoapWithAuthenticationContext plus OpenApiWithOpenIdConnect efforts
* Update spec.json
* Rename protocol and ishSession.OpenApi30Service -> ishSession.OpenApiISH30Service so ishSession.OpenApiAM20Service
* Parameter group `New-IShSession` ActiveDirectory/Interactive does not have `-Timeout` parameter.
* Cmdlets `New-IshSession` and `Test-IshSession` received parameter `-Protocol`, `-ClientId` and `-ClientSecret` so when protocol is set to `OpenApiWithOpenIdConnect` it is the preferred route, fall back to `WcfSoapWithWsTrust` when OpenApi calls are unavailable. 
* Case Files
    * IshFolder.cs skipped OpenApi object conversion here with #115 merge?!
    * Enumerations.cs left various enum conversions in #115
    * IshFields.cs skipped OpenApi object conversion here with #115 merge?!
    * IshFolders.cs skipped OpenApi object conversion here with #115 merge?!
    * AddIshFolder.cs
    * GetIshFolder.cs
* Branch #152 has .NET Standard (so no Kestrel like ) based 127.0.0.1:SomePort RedirectUri through classes `InfoShareOpenIdConnectLocalHttpEndpoint` and `InfoShareOpenIdConnectSystemBrowser`. This seemingly works over IdentityServer's `OidcClient` packages for PS7/NET6+ but not on PS5/NET48 ending with errors like below. Only one person in the world has this with an almost impossible solution for PowerShell using AssemblyRedirects (see https://github.com/IdentityModel/Documentation/issues/13)
```at System.Text.Json.JsonElement.EnumerateObject()
   at IdentityModel.Client.DiscoveryDocumentResponse.ValidateEndpoints(JsonElement json, DiscoveryPolicy policy)
   at IdentityModel.Client.DiscoveryDocumentResponse.Validate(DiscoveryPolicy policy)
   at IdentityModel.Client.DiscoveryDocumentResponse.InitializeAsync(Object initializationData)
   at IdentityModel.Client.ProtocolResponse.<FromHttpResponseAsync>d__0`1.MoveNext()
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at IdentityModel.Client.HttpClientDiscoveryExtensions.<GetDiscoveryDocumentAsync>d__1.MoveNext()
   ```
For whoever stumbles on this transitive package dependency of `System.Runtime.CompilerServices.Unsafe` (and/or `System.Text.Json `), solved it through a forced Assembly load of the v6 version (while v5 was expected and .NET Framework 4.8 loads v4.0.4). Solution is in the pragma-protected `SessionCmdlet::BeginProcessing` section in `SessionCmdlet.cs` and `AppDomainAssemblyResolveHelper.cs`. For completeness there is an OidClient logging-not-initialized seralization bug which I bypassed through `LogSerializer.Enabled = false;`. Inspired by https://stackoverflow.com/questions/1460271/how-to-use-assembly-binding-redirection-to-ignore-revision-and-build-numbers/2344624#2344624 in Preprocessing step of the cmdlet that needs it. A handy debugging line for me was: `[System.AppDomain]::CurrentDomain.GetAssemblies() | Out-GridView`. Or `$filePath = ".\Trisoft.ISHRemote\bin\Debug\ISHRemote\net48\IdentityModel.dll";(Get-Item -Path $filePath).VersionInfo.FileVersionRaw;$a= [System.Reflection.Assembly]::LoadFrom($filePath);$a;(Get-Item -Path $a.Location).VersionInfo.FileVersionRaw`
* Verify Token Validation is there, happens for WCF/OpenApi at the same time... refresh token is used when expiration allows. Otherwise build new connection.
* Added `WcfSoapWithOpenIdConnectConnection` next to long time `WcfSoapWithWsTrustConnection` and `OpenApiConnection` attempts as protocols on the `New-IshSession`. This required quite some hefty refactoring in `C:\GITHUB\ISHRemote`\Source\ISHRemote\Trisoft.ISHRemote\Connection\`. End result is ISHWS/OWcf web services next to ISHWS/Wcf web services.
    * Later I read this well-written article about loading binary modules in either PowerShells, see https://devblogs.microsoft.com/powershell/resolving-powershell-module-assembly-dependency-conflicts/ and in turn his rich sample on https://github.com/rjmholt/ModuleDependencyIsolationExample/blob/master/new/JsonModule.Cmdlets/JsonModuleInitializer.cs
* Refactor single source token code
    * Introduce InfoShareOpenIdConnectConnectionBase to hold the token infra
    * Merge InfoShareOpenApiConnectionParameters and InfoShareWcfSoapWithOpenIdConnectConnectionParameters into InfoShareOpenIdConnectConnectionParameters thereby single sourcing Tokens across the two services! And one Timeout is enough to rule them all :)
    * Single source tokens between wcf and openapi login and refresh 
    * Remove interface on soap classes 
    * Rename Tokens to InfoShareOpenIdConnectTokens
    * Rename InfoShareOpenApiConnection to InfoShareOpenApiWithOpenIdConnectConnection
* Extend and document InfoShareOpenApiConnectionParameters (redirectUri, Open up hardcoded client to ISHRemote/Tridion_Docs_Content_Importer , clean up code, check debug/verbose logging
* Refactor from AppDomainAssemblyResolveHelper structure to https://devblogs.microsoft.com/powershell/resolving-powershell-module-assembly-dependency-conflicts/ because it load earlier instead of only New-IshSession
* All examples for Get-Help in New-IshSession are over -PSCredentials or -IshUserName/IshPassword but now we have interactive (so system browser) or -ClientId/ClientSecret ... adapt them all or add sentence in first example?
* Align `Test-IshSession` with `New-IshSession` plus both need tests: `NewIshSession.Tests.ps1` and `TestIshSession.Tests.ps1`
* Extend New-IshSession/Test-IshSession with -PSCredential also working for client/secret (and ishusername/ishpassword)
* * Fix all version based tests on PS7, they should not result in empty server version like ` Context Add-IshBackgroundTask IshObjectsGroup Pipeline IshObject since 14SP4/14.0.4 =<`... Don't put Pester code in `Decribe` or `Context` block, use `It` only.
* Update github ticket that Access Management part of Tridion Docs 15/15.0.0 has an improvement where unattended *Service accounts* have to be explicitly created. Note that interactive logins are still allowed. See ReleaseNotes-ISHRemote-8.0.md
* Refresh OpenApi.json to released Docs 15.0.0 version
* Describe when Last Log On is valid. Always on Access Management (ISHAM) User Profiles, even when logged in over Tridion Docs Identity Provider (ISHID) or any other federated Secure Token Service (STS). On Tridion Docs User Profile, so visible in Organize Space or through `Find-IShUser` cmdlet, only if you used Tridion Docs Identity Provider (ISHID).
* netstandard2.0 lib which in turn references System.ServiceModel.Primitives 4.10.2 https://github.com/dotnet/wcf/issues/2862 ... problem disappears since PowerShell 7.3.6-stable
* GitHub Actions has many issues... had to drop New-ModuleManifest -Prerelease '$(Prerelease)' parameter on PS5.1 and added simple find-replace


# Next
* Test refresh with short expiration 
* Extend perequisites test regarding client I'd and secret, an expired and valid set... Perhaps over isham20proxy
    * User provisioning, see [SRQ-23306] Last login date in user overview is not updated when authentication was done through an external identity provider - RWS Jira https://jira.sdl.com/browse/SRQ-23306
* Automated Test ps5.1 with wstrust, ps7 with both openidconnect
* Test all protocol types on all platforms via newishsession (and one other smoke test) by calling it 6 times (2 ps times 3 protocols) which colors right after prerequisites
* Once branch #152 is merged, update ticket https://github.com/IdentityModel/Documentation/issues/13 with a hint to `AppDomainModuleAssemblyInitializer.cs`
    > Took me a while to find this nugget to resolve my problem. It is unfortunate that `OidcClient` doesn't work without these assemblyBinding redirects. For people who have this issue but do not have access to a `.config` file like I had with `powershell.exe.config` (v5.1 on .NET 4.8) - have a look at `AppDomainModuleAssemblyInitializer.cs` on https://github.com/RWS/ISHRemote/
    > Another hint is adding `LogSerializer.Enabled = false;` because if you do not attach logging to OidcClient, there seemingly is a bug that still does logging although not configured. see https://github.com/IdentityModel/IdentityModel.OidcClient/pull/67
* Describe what Tridion Docs User Profile disable means, and when it kicks in.

 
# Future  
* Go to async model, might be big investment, but theoretically is better, inspiration is on https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/main/NetCoreConsoleClient/src/NetCoreConsoleClient/Program.cs


# Background and References