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
\Map\{Name} Map====.xml
\Map\API30 Map====.xml
\Topics\{Class}\{Name}{OperationId}====.xml
\Topics\User\API30 GetUser====.xml   //probably not a GUID, generate custom LogicalId "API30-{OperationId}", instantiate template once, later manually created
```
having
- Multiple folders that inherit the later mentioned root `IshFolder` security paradigm
- One Map file that links all information together. Note that this file will be updated/regenerated in consecutive runs to add new classes, functions, etc
- One OASIS DITA Reference Topic per function, so OperationId, found. This topic is a template topic that should be imported once, then never overwritten. As these are edited by users to offer *guidance* on top of the generated information coming from OpenApiDocument
- Library Topics holding *conrefable* reusable information. For example, a conref library holding all Overview/Parameters/Responses per {Class} like User or {Schema} like FieldGroup and more.
- File names where LogicalId, Version, Language (and Resolution) remain empty, they will be added later when importing


## Import OASIS DITA from the File System into the CMS
Do note that ISHRemote has no plans of offering a multi folders/files cmdlet. So the below code is an example algorithm that be added to `Source\ISHRemote\Trisoft.ISHRemote\Samples`.
```powershell
$ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
$rootIshFolder = Get-IshFolder -FolderPath "General\__ISHRemote"
$sourceLanguageLabel = (Get-IshLovValue -LovId DLANGUAGE -LovValueId VLANGUAGEEN).Label
# mandatory/optional metadata for Add-IshDocumentObj
$topicMetadata = Set-IshMetadataField -Level Logical -Name FISHNOREVISIONS -Element Element -Value FALSE
$libraryTopicMetadata = Set-IshMetadataField -Level Logical -Name FISHNOREVISIONS -Element Element -Value TRUE
$mapMetadata = Set-IshMetadataField -Level Logical -Name FISHNOREVISIONS -Element Element -Value TRUE
```
Make sure the file system folder structure is present in the CMS.
```powershell
$folders = (Get-ChildItem -Directory -Recurse).FullName
# replace -ExportFolderPath to have relative paths
Set-IshFolder -IshFolder $rootIshFolder -FolderPath $folders
```
Extend `Set-IshFolder` (or `Add-IshFolder`) cmdlet to do idempotent create/update for parameter group `FolderPathGroup`. 
- Let `-FolderPath` take a string array to optimize read operations on path existance.
- OwnedBy and ReadAccess come from the root folder `$rootIshFolder`
- FolderType is derived from the level 1 folder names

As a result the folder structure exists.

Next is the initial create of IShDocumentObj in the CMS based on the generated files. Important here is that files matching `*=====.xml` expect to be generated using a new GUID, new Version, specified Language.

```powershell
$firstRunImportFilePaths = Get-ChildItem *=*=*=*=*.xml -Recurse
foreach ($file in )
Set-IshFolder -IshFolder $rootIshFolder -FolderPath $file.DirectoryName  #or handle folders separately
Add-IshDocumentObj

# alter the file path to hold the right LogicalId/Version/
```

### Multiple Nightly/OnDemand Runs

Having a folder structure looking like

Imagine 
- extra class
- extra function
- changed parameters (intentional during agile development)

In the transformation the {Class}{OperationId} so title is actually the key to distuingish betwween new files and update the files

after first generation there are now folders with files and logical ID that can be adapted reusing them on next import…folders have to be always checked and potentially created… Files with === are new and have to be added… Flies with existing logicalid but version empty/latest require a set-ishdocumentobj avoiding superfluous updates

Expecting all files, having a filename holding LogicalId/Version/Language, to already exists in the CMS
```powershell

Find-IshDocumentObj (so Retrieve as GetMetadata throws for non-existing objects)
if ($exists)
{
    Set-IshDocumentObj -FilePath $file.FullName -Lng $sourceLanguageLabel -LogicalId NEW -Version NEW-or-LATEST -Edt EDTXML -IshSession $ishSession -Metadata $topicMetadata
} else {
    Add-IshDocumentObj
}
```

### Cleanup

Some scripts to clean up the File System and matching CMS $rootIshFolder structure.

### Optional change detection

The algorithm relies on filenames with or without GUID, new Version, specified Language. Library Topics and Maps are however often (nightly or OnDemand) generated, so resubmitted. Optionally a smarter change detection can be added, submit the file with a processing instruction looking like `<?ishremote sha256 somehash?>`. This does however still mean a Get-IshDocumentObjData to check if current and future hash are different, to then Set-IshDocumentObj. For now easier to just use Set-IshDocumentObj.

Perhaps ConvertTo-IshOasisDita can message through a return parameter or file stamp what the adapted/new files are.



## Milestone - ConvertTo-IShOasisDita parameter group ApiSpecJsonFilePath
Create a new `FileProcessor` cmdlet named `ConvertTo-IShOasisDita`.

Parameters of `ConvertTo-IShOasisDita`
- `-ApiSpecJsonFilePath` .\Trisoft.ISHRemote.OpenApiISH30\OpenApiISH30.json will be read using package `Microsoft.OpenApi.Models.OpenApiDocument`
- `-ExportFolderPath` C:\Temp\ holds a folder path that will contain folders and file names that have to remain there across first and iterative runs to hold state
- `-Name` API30 holds a unique prefix used in filenames, `FTITLE` field, `<title>` element and more
- `-GenerationMode` holds modes of geheration for the folders and OASIS DITA files. Initial versioned modes considered are `ReferenceTopicsWitConrefsV1` or `AllInOneReferenceTopicsV1`


## Milestone - ConvertTo-IShOasisDita parameter group SdkDocumentationFile 
`-SdkDocumentationFile ..some///commentfile...`

## Milestone - ConvertTo-IShOasisDita parameter group JsDocHtmlFile
`-JsDocHtmlFile`
- https://www.typescriptlang.org/docs/handbook/jsdoc-supported-types.html
- https://jsdoc.app/about-getting-started actually generates HTML, so it could be -JsDocHtmlFile as parameter
@tridion-docs/extensions --> Oasis Dita "reference" files

## Suggestions...
Your feedback on planning is important. The best way to indicate the importance of an issue is to vote (👍) for that issue on GitHub. This data will then feed into the planning process for the next release.

In addition, please comment on this post if you believe we are missing something that is critical, or are focusing on the wrong areas.