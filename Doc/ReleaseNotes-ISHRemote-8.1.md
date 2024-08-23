# Release Notes of ISHRemote v8.1

High level release notes are on [Github](https://github.com/rws/ISHRemote/releases/tag/v8.1), below the most detailed release notes we have :)

**Before I forget, all people with a Github account, could you spare a moment to STAR this repository - see top-right Star icon on https://github.com/RWS/ISHRemote/ Appreciated!**


## General

This release inherits the v0.1 to v0.14 up to v8.0 development branch and features. All cmdlets and business logic are fully compatible even around authentication. In short, we expect it all to work still :)

### Remember
* All C# source code of the ISHRemote library is online at [master](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote), including handling of the different [Connection](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Connection) protocols in a NET 4.8 and NET 6.0+ style.
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

The below text describes the delta compared to fielded release ISHRemote v8.0.


## Boosting performance for protocol WcfSoapWithWsTrust

In [ISHRemote v8.0](./Installation-ISHRemote-8.0.md) we refactored the proxies to introduce Modern Authentication next to existing legacy authentication now known as `-protocol WcfSoapWithWsTrust`. Performance was boosted and memory handling was optimized by making the token expiration less overzealous. #196

All customers on ISHRemote v8.0 are advised to upgrade, especially when using ISHRemote to Tridion Docs 14SP4/14.0.4 and earlier or when using the `New-IShSession -IShUserName ... -IShPassword ...` parameter set.


## Extending cmdlet Add-IshBackgroundTask with parameter InputDataTemplate to enable Metrics feature

The `Add-IshBackgroundTask` cmdlet, introduced in #112, offered a shorthand way of enabling the `SMARTTAG` feature (*SemanticAI*) plus it offered a raw `InputData` pass through option.

The contract of what you put on the BackgroundTask message queue under `InputData` and how the BackgroundTask handler interprets it is up to the implementer. For the standard product however there are only a handful of `InputData` contracts. An overview where you'll notice that the client triggering the message prefers a minimal contract, so providing the least amount of information as possible as the matching BackgroundTask handler (EventTypes) can retrieve more data if desired. #193

| EventTypes | `InputDataTemplate` | incoming IShObjects | `InputData` sample |
|-|-|-|-|
| SMARTTAG, CREATETRANSLATIONFROMLIST  | `IshObjectsWithLngRef` | `IShDocumentObj` | `<ishobjects><ishobject ishtype='ISHMasterDoc' ishref='GUID-X' ishlogicalref='45677' ishversionref='45678' ishlngref='45679'><ishobject ishtype='ISHIllustration' ishref='GUID-Y' ishlogicalref='345677' ishversionref='435678' ishlngref='345679'></ishobjects>` |
| CLEANUPMETRICS, DITADELIVERYUPDATEPUBLICATIONMETADATA,... everything that goes over IWrite plugin `OnMultiFieldChangeSendEvent` | `IshObjectWithLngRef ` | `IShPublicationOutput`, `IShDocumentObj` or `IShBaseline` | `<ishobject ishtype='ISHBaseline' ishref='GUID-X' ishbaselineref='45798'>` or `<ishobject ishtype='ISHMasterDoc' ishref='GUID-X' ishlogicalref='45677' ishversionref='45678' ishlngref='45679'>` |
| SYNCHRONIZEMETRICS | `IshObjectsWithIshRef` | `IShDocumentObj` | `<ishobjects><ishobject ishtype='ISHMasterDoc' ishref='GUID-X'><ishobject ishtype='ISHIllustration' ishref='GUID-Y'></ishobjects>` |
| INBOXEXPORT, REPORTEXPORT, SEARCHEXPORT, PUBLICATIONEXPORT, FOLDEREXPORT | `EventDataWithIshLngRefs` | `IShDocumentObj` | `<eventdata><lngcardids>13043819, 13058357, 14246721, 13058260</lngcardids></eventdata>` |
| *custom* | when not specified you have to pass `-RawInputData` | *custom* | value of `-RawInputData` should match your BackgroundTask handler implementation |

### Example using SMARTTAG
Add BackgroundTask with event type `SMARTTAG` for the objects located under the `General\MyFolder\Topics` path. One BackgroundTask message will appear per folder containing a list of all latest version English (`en`) content objects in the InputData of the message. Note that there is no devide on `$ishSession.MetadataBatchSize` (default was 999) anymore since this v8.1 version of ISHRemote.
```powershell
Get-IshFolder -FolderPath "General\Myfolder\Topics" -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary") -Recurse |
ForEach-Object -Process {
    Get-IshFolderContent -IshFolder $_ -VersionFilter Latest -LanguagesFilter en |
    Add-IshBackgroundTask -EventType "SMARTTAG" -InputDataTemplate IshObjectsWithLngRef  # plural content objects...
}
```

### Example using SYNCHRONIZEMETRICS 
Rebuilding the Metrics subsystem, introduced in Tridion Docs 15.1 Update 1 (15.1.1), is optimally done in the order of Images, Topics and Publications. Add BackgroundTask with event type `SYNCHRONIZEMETRICS` for the objects located under the `General` path (`Data` folder). One BackgroundTask message will appear per folder containing a list of LogicalIds in the `InputData` of the message, hence the content of one folder is passed in one message.

Note that a more complex script will be offered in the product (IShCD) that covers error handling, logging transcript and more. The below illustrates that ISHRemote cmdlets are an enabler for the feature and offering variations like partial rebuilds and more.
```powershell
# First Images
Get-IshFolder -BaseFolder Data -FolderTypeFilter @("ISHIllustration") -Recurse |
ForEach-Object -Process {
    Get-IshFolderContent -IshFolder $_ -VersionFilter Latest |
    Add-IshBackgroundTask -EventType "SYNCHRONIZEMETRICS" -EventDescription "SYNCHRONIZEMETRICS Images" -InputDataTemplate IshObjectsWithIshRef  # plural LogicalIds
}
# Then Topics
Get-IshFolder -BaseFolder Data -FolderTypeFilter @("ISHModule") -Recurse |
ForEach-Object -Process {
    Get-IshFolderContent -IshFolder $_ -VersionFilter Latest |
    Add-IshBackgroundTask -EventType "SYNCHRONIZEMETRICS" -EventDescription "SYNCHRONIZEMETRICS Topics" -InputDataTemplate IshObjectsWithIshRef  # plural LogicalIds
}
# Then Publications
Get-IshFolder -BaseFolder Data -FolderTypeFilter @("ISHPublication") -Recurse |
ForEach-Object -Process {
    Get-IshFolderContent -IshFolder $_ -VersionFilter Latest |
    Add-IshBackgroundTask -EventType "SYNCHRONIZEMETRICS" -EventDescription "SYNCHRONIZEMETRICS Publications" -InputDataTemplate IshObjectsWithIshRef  # plural LogicalIds
}
```

### Example using FOLDEREXPORT
Add BackgroundTask with event type `FOLDEREXPORT` for the objects located under the `General\MyFolder\Images` path. Note that the BackgroundTask handler behind all `...EXPORT` events like `SEARCHEXPORT` or `INBOXEXPORT` on Tridion Docs 15.1 and earlier is identical. One BackgroundTask message will appear per folder containing a list of all latest version English (`en`) content objects in the `InputData` of the message.
```powershell
Get-IshFolder -FolderPath "General\MyFolder\Images" -Recurse |
ForEach-Object -Process {
    Get-IshFolderContent -IshFolder $_ -VersionFilter Latest -LanguagesFilter en |
    Add-IshBackgroundTask -EventType "FOLDEREXPORT" -InputDataTemplate EventDataWithIshLngRefs
}
```
Note that without the `ForEach-Object` construction all recursively found content objects would all be passed in one BackgroundTask message.
```powershell
Get-IshFolder -BaseFolder EditorTemplate -Recurse |
Get-IshFolderContent -VersionFilter Latest -LanguagesFilter en |
Add-IshBackgroundTask -EventType "FOLDEREXPORT" -EventDescription "Folder Export of General\MyFolder\Images" -InputDataTemplate EventDataWithIshLngRefs -WhatIf

Get-IshBackgroundTask -MetadataFilter (Set-IshMetadataFilterField -Level Task -Name EVENTTYPE -FilterOperator Equal -Value 'FOLDEREXPORT')
```

## Implementation Details

*  `Add-IShBackgroundTask` implementation, especially `SMARTTAG`, switched from `DocumentObj25.RaiseEventByIshLngRefs` to `BackgroundTask25.CreateBackgroundTaskWithStartAfter` API calls. In turn `DocumentObj25.RaiseEventByIshLngRefs` is no longer used by ISHRemote (still used by Client Tools though). #193
*  `Add-IShBackgroundTask` no longer has an implicit `DevideListInBatchesBy...` function batching per `$ishSession.MetadataBatchSize` (default was 999). So if batching is required, then it has to happen before calling `Add-IShBackgroundTask`. Typical suggested batching is per folder. #193
*  `Add-IShBackgroundTask` received optional parameter `-EventDescription ` on parameter set `IShObjectsGroup`. 
*  `Add-IShBackgroundTask` received an optional parameter parameter `-HashId`. When not specified, the default, the implicit HashId will calculate a SHA256 based on the generated `InputData` to make sure that the BackgroundTask handler only picks up one of the generated messages. When set to empty string `""`, an empty HashId will be passed downstream, otherwise the given value. #193


## Breaking Changes - Cmdlets

All cmdlets and business logic are fully compatible.


## Breaking Changes - Code

n/a


## Breaking Changes - Platform

*  Security and Platform Updates, bumped version of System.Text.Json to 8.0.4; Duende's IdentityModel libraries to 6.0.0 and 7.0.0; Microsoft.Extensions.ApiDescription.Client to 8.0.8 and matching NSwag.ApiDescription.Client to 14.1.0; and  Microsoft.PowerShell.Commands.Management to 7.2.23, the latest to support the technically obsolete PowerShell 7.2+/NET6+ combination.
*  Note that `Microsoft.Extensions.Logging.dll` is no longer preloaded by `AppDomainModuleAssemblyInitializer.cs` as a side-effect of the above assembly version bumps where bugs where fixed in the third party libraries.


## Known Issues

* Aborting the `New-IShSession`/`Test-IShSession` cmdlets using `Ctrl-C` in PowerShell is not possible, you have to await the non-configurable 60 seconds timeout potentially resulting in `GetTokensOverSystemBrowserAsync Error[Browser login cannceled after 60 seconds.]`. Typically happens if you did not authenticate in the System Browser.
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

Added more Invoke-Pester 5.3.0 Tests, see Github actions for the Windows PowerShell 5.1 and PowerShell 7.2+ hosts where
* the skipped are about SslPolicyErrors testing
* the failed are about IMetadata bound fields (issue #58)

Below is not an official performance compare, but a recurring thing noticed along the way. Using the same client machine, same ISHRemote build and same backend but different PowerShell hosts we noticed a considerable speed up of the Pester tests. However, adding (complicated) tests along the way and knowing that ISHRemote as client library greatly depends on the server-side load, we have to take these numbers at face value.

| Name                     | Client Platform                     | Protocol       | Test Results         |
|--------------------------|-------------------------------------|----------------------|----------------|
| ISHRemote 8.0.10919.0     | PowerShell 7.4.0 on .NET 8.0.0 | WcfSoapWithOpenIdConnect | Tests completed in 449.72s AND Tests Passed: 1057, Failed: 0, Skipped: 3 NotRun: 0 |
| ISHRemote 8.1.11623.0     | Windows PowerShell 5.1 on .NET 4.8.1  | WcfSoapWithOpenIdConnect | Tests completed in 515.62s AND Tests Passed: 1063, Failed: 0, Skipped: 4 NotRun: 0 |
| ISHRemote 8.1.11623.0   | PowerShell 7.4.5 on .NET 8.0.0 | WcfSoapWithOpenIdConnect | Tests completed in 467s AND Tests Passed: 1063, Failed: 0, Skipped: 4 NotRun: 0 |



