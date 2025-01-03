# The Plan of ISHRemote v8.2

This plan brings together input from several stakeholders and outlines where and how we intend to move with ISHRemote. This plan is not set-in-stone and will evolve as we work on the release based on what we learn. This learning includes feedback from people like you, so please let us know what you think!

# TLDR (Too Long; Didn't Read)...
The plan is to work on an ISHRemote where conversion into OASIS DITA is easily enabled. Conversion from standard, mostly engineering file formats, like API Spec JSon or triple-slash (`///`) csproj sdk documentation are transformed into maintainable OASIS DITA structures. Ready to be imported into the CMS.



## The problem...
Does engineering documentation belong in OASIS DITA or not?

Many developer teams love to have their documentation right next to the source files for easy maintenance. Often this means locked in or even separately delivered documentation optimized for the creator and not the consumer of this documentation set. For example the main documentation of products is on https://docs.rws.com while developer oriented documentation is on https://developers.rws.com. A disconnected experience covered by some cross linking, but in essence they are 2+ knowledge silos with repeated concepts.

Given that https://docs.rws.com is [Tridion Docs Genius](https://www.rws.com/blog/tridion-docs-genius-where-content-clicks/) powered, I thought it was a good idea to find a way to merge developer documentation in with the rest of the OASIS DITA-based enduser, administrator, integrator documentation set. As **Tridion Docs Genius** has a proper semantic search engine under the hood and is AI-powered through **Trusted Conversation**, you open up even more value compared to some standalone *Swagger instance* (Api Spec Json).


## The plan...
The plan is to describe a life cycle from an often change structured source format (e.g. Api Spec Json) over standard ISHRemote cmdlets and an example glue script.

The example will be described based on Tridion Docs Open Api Spec Json format available since 15/15.0.0, extended in 15.1/15.1.0 and actually OnDemand updated for the next release using agile development.

## The algorithm... 

### Initial Convert from Api Spec Json to OASIS DITA on the File System

```powershell
ConvertTo-IShOasisDita -ApiSpecJsonFilePath .\Trisoft.ISHRemote.OpenApiISH30\OpenApiISH30.json
                       -ExportFolderPath C:\Temp\API30Docs\
                       -Name "API30"
                       -GenerationMode "ReferenceTopicsWitConrefsV1"
```
After a first run on an empty `ExportFolderPath` the folder could look like
```
\Maps\{Name} Map====.xml
\Maps\API30 Map====.xml
\Topics\{Class}\{Name}{OperationId}====.xml
\Topics\User\API30 GetUser====.xml
\Libraries\API30 User Library====.xml
```
having
- Multiple folders that inherit the later mentioned root `IshFolder` security paradigm
- One Map file that links all information together. Note that this file will be updated/regenerated in consecutive runs to add new classes, functions, etc
- One OASIS DITA Reference Topic per function, so OperationId, found. This topic is a template topic that should be imported once, then never overwritten. As these are edited by users to offer *guidance* on top of the generated information coming from OpenApiDocument
- Library Topics holding *conrefable* reusable information. For example, a conref library holding all Overview/Parameters/Responses per {Class} like User or {Schema} like FieldGroup and more.
- File names where LogicalId, Version, Language (and Resolution) remain empty, they will be added later when importing


### Initial import OASIS DITA from the File System into the CMS
Do note that ISHRemote has no plans of offering a multi folders/files cmdlet. So the below code is an example algorithm that to be added to `Source\ISHRemote\Trisoft.ISHRemote\Samples`. A list of script parameters, so should be uppercased.
```powershell
$ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
$rootIshFolder = Get-IshFolder -FolderPath "General\__ISHRemote"
$sourceLanguageLabel = (Get-IshLovValue -LovId DLANGUAGE -LovValueId VLANGUAGEEN).Label
# mandatory/optional metadata for Add-IshDocumentObj
$topicMetadata = Set-IshMetadataField -Level Logical -Name FISHNOREVISIONS -Element Element -Value FALSE
$libraryTopicMetadata = Set-IshMetadataField -Level Logical -Name FISHNOREVISIONS -Element Element -Value TRUE
$mapMetadata = Set-IshMetadataField -Level Logical -Name FISHNOREVISIONS -Element Element -Value TRUE
```
Make sure the file system folder structure is present in the CMS. Or do this in the folder loop to upload the content object immediately in the right folder.
```powershell
$folders = (Get-ChildItem -Directory -Recurse).FullName
# replace -ExportFolderPath to have relative paths
Set-IshFolder -IshFolder $rootIshFolder -FolderPath $folders  #array-of-folders
```
Extend `Set-IshFolder` cmdlet to do idempotent create/update for parameter group `FolderPathGroup`. 
- OwnedBy and ReadAccess come from the root folder `$rootIshFolder`
- Optionally let `-FolderPath` take a string array to optimize read operations on path existance.

As a result the folder structure exists.

Next is the initial create of IShDocumentObj in the CMS based on the generated files. Important here is that files matching `*=====.xml` expect to be generated using a new GUID, new Version, specified Language.

```powershell
# Probably a loop per FolderType (so ISHLibrary, ISHModule, ISHMasterDoc) for easier folder creation
$librariesExportFolderPath = Join-Path -Path $exportFolderPath -ChildPath "Libraries"
$librariesInitialImportFilePaths = Get-ChildItem -Path $ishmoduleExportFolderPath -Filter *====.xml -Recurse
$filePathsToSkipForUpdate = @()
foreach ($file in $librariesInitialImportFilePaths)
{
    $relativeFolderPath = $file.Replace($file.DirectoryName, $exportFolderPath)
    $ishFolder = Set-IshFolder -IshFolder $rootIshFolder -FolderType ISHLibrary -FolderPath $relativeFolderPath
    $ishObject = Add-IshDocumentObj -IshFolder $ishFolder -FilePath $file.FullName -Lng $sourceLanguageLabel -Edt EDTXML -Metadata $topicMetadata

    # After Get-IshDocumentObjData download with structured file path, delete the original file that was imported
    $filePathsToSkipForUpdate += Get-IshDocumentObjData -IshObject $ishObject -FolderPath $file.DirectoryName
    Remove-Item -Path $file.FullName
}
```

After the run, there are no more files matching ``*=====.xml``, the folder looks like
```
\Maps\API30 Map=GUID-M=1=en=.xml
\Topics\User\API30 GetUser=GUID-A=1=en.xml
\Libraries\API30 User Library=GUID-L=1=en=.xml
```

This script should be a proper sample cmdlet with parameters, help, whatif, progress, etc

### Next convert from Api Spec Json to OASIS DITA on the File System

On an existing `ExportFolderPath`, the folder could look like
```
\Maps\API30 Map=GUID-M=1=en=.xml
\Topics\User\API30 GetUser=GUID-A=1=en.xml
\Libraries\API30 User Library=GUID-L=1=en=.xml
```

The Map and Libraries are regenerated. While the Topics remain untouched, they are manually edited in the CMS. So an extra API class, extra function/operationId or changed Overview/Parameters/Responses could result in a folder structure looking like.
```
\Maps\API30 Map=GUID-M=1=en=.xml
\Topics\User\API30 GetUser=GUID-A=1=en.xml
\Topics\User\API30 GetBaseline====.xml
\Libraries\API30 User Library=GUID-L=1=en=.xml
\Libraries\API30 Baseline Library====.xml
```
Where 
- Baseline Topic and Library are new so need to be imported as new.
- Library User Library potentially is changed, for now just Set-IshDocumentObj without delta detection
- Map definitely has changed as there is a new OperationId, so Set-IshDocumentObj 

In the transformation the {Class}{OperationId} so title is actually the key to distuingish betwween new files and update the files

### Next import OASIS DITA from the File System into the CMS

after first generation there are now folders with files and logical ID that can be adapted reusing them on next import…folders have to be always checked and potentially created… Files with === are new and have to be added… Flies with existing logicalid but version empty/latest require a set-ishdocumentobj avoiding superfluous updates

```powershell
# ... add new content objects first
$nextImportFilePaths = Get-ChildItem -Path exportFolderPath -Filter *=*=*=*=.xml -Recurse
foreach ($file in $nextImportFilePaths)
{
    # if file in $filePathsToSkipForUpdate, just created, so skip for update
    $ishObject = Set-IshDocumentObj -FilePath $file.FullName 
    # optionally add not released -RequiredCurrentMetadata (Set-IshMetadataFilterField -Level Lng -Name FISHSTATUSTYPE -FilterOperator LessThan -Value 20)
}
```


$filePathsToSkipForUpdate


### Maintenance Cleanup

Some scripts to clean up the File System and matching CMS $rootIshFolder structure.

### Maintenance Restore ExportFolderPath

Below script allows you to build the ExportFolderPath from the CMS.

### Optional change detection

The algorithm relies on filenames with or without GUID, new Version, specified Language. Library Topics and Maps are however often (nightly or OnDemand) generated, so resubmitted. Optionally a smarter change detection can be added, submit the file with a processing instruction looking like `<?ishremote sha256 somehash?>`. This does however still mean a Get-IshDocumentObjData to check if current and future hash are different, to then Set-IshDocumentObj. For now easier to just use Set-IshDocumentObj.

Perhaps ConvertTo-IshOasisDita can message through a return parameter or file stamp what the adapted/new files are.



## Story - Add ConvertTo-IShOasisDita cmdlet with parameter group ApiSpecJsonFilePath
Create a new `FileProcessor` cmdlet named `ConvertTo-IShOasisDita`.

Parameters of `ConvertTo-IShOasisDita`
- `-ApiSpecJsonFilePath` .\Trisoft.ISHRemote.OpenApiISH30\OpenApiISH30.json will be read using package `Microsoft.OpenApi.Models.OpenApiDocument`
- `-ExportFolderPath` C:\Temp\ holds a folder path that will contain folders and file names that have to remain there across first and iterative runs to hold state
- `-Name` API30 holds a unique prefix used in filenames, `FTITLE` field, `<title>` element and more
- `-GenerationMode` holds modes of geheration for the folders and OASIS DITA files. Initial versioned modes considered are `ReferenceTopicsWitConrefsV1` or `AllInOneReferenceTopicsV1`

- [ ] Test using OpenApiISH30--15.0.0.json, OpenApiISH30--15.1.0.json, OpenApiISH30--15.2.0.json and OpenApiAM10--2.1.0.json

## Story - Extend Set-IshFolder

Goal is to allowed a simplified folder create and retrieve `$ishFolder = Set-IshFolder -IshFolder (Get-IshFolder -FolderPath "General\__ISHRemote") -FolderType ISHModule -FolderPath "first\second\third"`. The folder is always returned, potentially after creation, so idempotent.

- Parameter `-FolderPath` respects the IshSession.FolderPathSeparator

- [ ] Test after Set using `Get-IshFolderLocation`

## Story - Extend Add-IshDocumentObj

Goal is to allowed a simplified `Add-IshDocumentObj -IshFolder $ishFolder -FilePath "C:\Temp\My Title=GUID-A=NEW=en=.xml" -Metadata $topicMetadata` (and perhaps matching `Set-IshDocumentObj`).

Extend `Add-IshDocumentObj` existing parameter group `ParameterGroupFilePath` with an overload. There is already an overload for 
- [x] Parameters `-FolderId` vs `-IshFolder`
- [x] Parameter `-IshType` can be derived from `-FolderId` or `-IshFolder`
- [ ] Parameters `-LogicalId`, `-Version`, `-Lng`, `-Resolution` and `-Edt` can be derived from a structured `-FilePath` complying with `*=*=*=*=*.xml`

- Explicit parameters `-LogicalId`, `-Version`, `-Lng`, `-Resolution` and `-Edt` overwrite derived `-FilePath` versions.
- Empty `LogicalId` means `newGUID`
- Empty `Version` means `new`. `Version` can explicitly be `new`. `Version` can be `latest`. `Version` can be an explicit version number.
- Empty `Edt` means `EDTXML`, extra `Find-IshEdt` file extension mappings could be added later as enhancement.


## Story - Extend Set-IshDocumentObj

Goal is to allowed a simplified `Set-IshDocumentObj -FilePath C:\Temp\My Title=GUID-A=3=en=.xml -Metadata $topicMetadata` (like matching `Add-IshDocumentObj`).

Extend `Set-IshDocumentObj` existing parameter group `ParameterGroupFilePath` with an overload.


## Story - Add ConvertTo-IShOasisDita cmdlet with parameter group SdkDocumentationFile 
`-SdkDocumentationFile ..some///commentfile...`

## Story - Add ConvertTo-IShOasisDita cmdlet with parameter group JsDocHtmlFile
`-JsDocHtmlFile`
- https://www.typescriptlang.org/docs/handbook/jsdoc-supported-types.html
- https://jsdoc.app/about-getting-started actually generates HTML, so it could be -JsDocHtmlFile as parameter
@tridion-docs/extensions --> Oasis Dita "reference" files



# Suggestions...
Your feedback on planning is important. The best way to indicate the importance of an issue is to vote (👍) for that issue on GitHub. This data will then feed into the planning process for the next release.

In addition, please comment on this post if you believe we are missing something that is critical, or are focusing on the wrong areas.