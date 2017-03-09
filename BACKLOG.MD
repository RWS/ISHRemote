# BACKLOG ORDER	

1. [ISHREMOTE-052] Add `Get-IshBaseline`, `Find-IshBaseline` and `Get-IshBaselineItem` for metrics/insights purposes ISSUE #2
1. [ISHREMOTE-011] `IshField` promotion to properties
1. [ISHREMOTE-022] Refactor to `IshTypeFieldSetup` by replacing hardcoded System and Required fields by dynamic ones ISSUE #3
1. [ISHREMOTE-047] `Set-IshMetadataField`/`Set-IshMetadataFilterField` take `object[]` instead of `string` and convert internally
1. [ISHREMOTE-049] Overload `Set-IshRequestedMetadataField` to accept `IshTypeFieldDefinition[]` to generate `IshMetadataField[]`
1. [ISHREMOTE-040] Support ExecutionPolicy AllSigned (SignAndPublish.ps1)
1. [ISHREMOTE-045] Add parameter argument completion for interactive usage.
1. [ISHREMOTE-034] Add `Get-IshTranslationJob`, `Find-IshTranslationJob` and `Get-IshTranslationJobItem` for analysis purposes
1. [ISHREMOTE-033] Add `Get-IshTranslationTemplate`, `Remove-IshTranslationTemplate` and `Add-IshTranslationTemplate`
1. [ISHREMOTE-030] Fix `Stop-IshPublicationOutput` failing with null reference exception on `-WhatIf` usage
1. [ISHREMOTE-025] Add `Get-IshField` that returns `<ishfields>` based `ISHType[]` and switch flags
1. [ISHREMOTE-020] Overload all `-RequestedMetadata` on `Find-`/`Get-` cmdlets using `IshTypeFieldSetup`
1. [ISHREMOTE-024] Add `Filter-IshValueField`, actually `Filter-IshMetadataField`
1. [ISHREMOTE-001] Prepare for OpenSourceSoftware and Continues Integration ISSUE #1
1. [ISHREMOTE-002] Performance boost for legacy imports to avoid Retrieve API calls to feed the pipeline
1. [ISHREMOTE-048] Merge `Find-` cmdlets into a parameter group on the `Get-` cmdlets. So `Get-` allows retrieval with identifiers and based on Find criterias.
1. [ISHREMOTE-008] Add `Unlock-IshUser`
1. [ISHREMOTE-015] Add `Move-IshPublicationOutput` based on `Move-IshDocumentObj`
1. [ISHREMOTE-029] Overload `Set-IshValueField` to accept IshObject/IshFolder
1. [ISHREMOTE-035] Add `Get-IshSettingWorkflow` which in turn requires `IshSettingWorkflowSetup` and `IshSettingWorkflowTransition`
1. [ISHREMOTE-031] Extend parameter `-FolderPath` with intellisense
1. [ISHREMOTE-006] Add Missing/Legacy Pester tests
1. [ISHREMOTE-016] Add `Filter-IshObject` cmdlet taking MetadataFilter parameter to do client-side object filtering
1. [ISHREMOTE-014] Add `Test-IshFolder`
1. [ISHREMOTE-027] Add `Test-IshDocumentObj` and `Test-PublicationOutput`
1. [ISHREMOTE-013] Optimize with logical delete on `Remove-IshDocumentObj` and `Remove-IshPublicationOutput`
1. [ISHREMOTE-010] IshSession should refresh token
1. [ISHREMOTE-051] Mandatory AllowOnCreate fields for `Add-` cmdlets should have their own ParameterSetName `MandatoryParameterGroup`
1. [ISHREMOTE-028] Add `Search-IshDocumentObj`
1. [ISHREMOTE-050] `Get-IshSetting` should return same output type IshField for `-FieldName` (string) and `-RequestedMetadata`. String output could be achieved by `-Raw` flag for both.
1. [ISHREMOTE-012] Respect IshSession.BatchSize and IshSession.BatchSize
1. [ISHREMOTE-005] Implement Client Library retry mechanism next to existing timeout implementation
1. [ISHREMOTE-009] DocumentObj data migration cmdlets like `Get-IshDocumentObjData`, `Save-IshDocumentObj`,...
1. [ISHREMOTE-032] Implement Provider for ISH:// usage
1. [ISHREMOTE-046] More documentation for Get-Help
1. [ISHREMOTE-999] The end!


[ISHREMOTE-001]: #ISHREMOTE-001 "Prepare for OpenSourceSoftware and Continues Integration"
[ISHREMOTE-002]: #ISHREMOTE-002 "Performance boost for legacy imports to avoid Retrieve API calls to feed the pipeline"
[ISHREMOTE-003]: #ISHREMOTE-003 "Convert readme.txt to README.MD and BACKLOG.MD"
[ISHREMOTE-004]: #ISHREMOTE-004 "OpenSource on GitHub"
[ISHREMOTE-005]: #ISHREMOTE-005 "Implement Client Library retry mechanism next to existing timeout implementation"
[ISHREMOTE-006]: #ISHREMOTE-006 "Add Missing/Legacy Pester tests"
[ISHREMOTE-007]: #ISHREMOTE-007 "Publish to PSGallery"
[ISHREMOTE-008]: #ISHREMOTE-008 "Add Unlock-IshUser"
[ISHREMOTE-009]: #ISHREMOTE-009 "DocumentObj data migration cmdlets like `Get-IshDocumentObjData`, `Save-IshDocumentObj`,..."
[ISHREMOTE-010]: #ISHREMOTE-010 "IshSession should refresh token"
[ISHREMOTE-011]: #ISHREMOTE-011 "IshField promotion to properties"
[ISHREMOTE-012]: #ISHREMOTE-012 "Respect IshSession.BatchSize and IshSession.BatchSize"
[ISHREMOTE-013]: #ISHREMOTE-013 "Optimize with logical delete on Remove-IshDocumentObj and Remove-IshPublicationOutput"
[ISHREMOTE-014]: #ISHREMOTE-014 "Add Test-IshFolder"
[ISHREMOTE-015]: #ISHREMOTE-015 "Add Move-IshPublicationOutput based on Move-IshDocumentObj"
[ISHREMOTE-016]: #ISHREMOTE-016 "Add `Filter-IshObject` cmdlet taking MetadataFilter parameter to do client-side object filtering"
[ISHREMOTE-017]: #ISHREMOTE-017 "Add `IshSession.IshTypeFieldSetup` allowing `Get-IshTypeFieldDefinition` and `Compare-IshTypeFieldDefinition`"
[ISHREMOTE-018]: #ISHREMOTE-018 "Add `Get-IshTypeFieldDefinition` takes an optional `IshSession` and allows loading of a TriDKXmlSetup-FullExport"
[ISHREMOTE-019]: #ISHREMOTE-019 "Add `Compare-IshTypeFieldDefinition`"
[ISHREMOTE-020]: #ISHREMOTE-020 "Overload all `-RequestedMetadata` on `Find-`/`Get-` cmdlets using `IshTypeFieldSetup`"
[ISHREMOTE-021]: #ISHREMOTE-021 "Rename `Set-IshValueField`, `Set-IshFilterField` and `Set-IshField` to `Set-IshMetadataField`, `Set-IshMetadataFilterField` and `Set-IshRequestedMetadataField`"
[ISHREMOTE-022]: #ISHREMOTE-022 "Refactor to `IshTypeFieldSetup` by replacing hardcoded System and Required fields by dynamic ones"
[ISHREMOTE-023]: #ISHREMOTE-023 "Extend `Set-IshMetadataField`, `Set-IshMetadataFilterField` and `Set-IshRequestedMetadataField` cmdlets with `-IshSession` to allow ISHTypeFieldDefinition lookup"
[ISHREMOTE-024]: #ISHREMOTE-024 "Add `Filter-IshValueField`, actually `Filter-IshMetadataField`"
[ISHREMOTE-025]: #ISHREMOTE-025 "Add `Get-IshField` that returns `<ishfields>` based `ISHType[]` and switch flags"
[ISHREMOTE-026]: #ISHREMOTE-026 "Break compatiblity, remove all `Trisoft.Automation` artefacts"
[ISHREMOTE-027]: #ISHREMOTE-027 "Add `Test-IshDocumentObj` and `Test-PublicationOutput`"
[ISHREMOTE-028]: #ISHREMOTE-028 "Add `Search-IshDocumentObj`"
[ISHREMOTE-029]: #ISHREMOTE-029 "Overload `Set-IshValueField` to accept IshObject/IshFolder"
[ISHREMOTE-030]: #ISHREMOTE-030 "Fix `Stop-IshPublicationOutput` failing with null reference exception on `-WhatIf` usage"
[ISHREMOTE-031]: #ISHREMOTE-031 "Extend parameter `-FolderPath` with intellisense"
[ISHREMOTE-032]: #ISHREMOTE-032 "Implement Provider for ISH:// usage"
[ISHREMOTE-033]: #ISHREMOTE-033 "Add `Get-IshTranslationTemplate`, `Remove-IshTranslationTemplate` and `Add-IshTranslationTemplate`"
[ISHREMOTE-034]: #ISHREMOTE-034 "Add `Get-IshTranslationJob`, `Find-IshTranslationJob` and `Get-IshTranslationJobItem` for analysis purposes"
[ISHREMOTE-035]: #ISHREMOTE-035 "Add `Get-IshSettingWorkflow` which in turn requires `IshSettingWorkflowSetup` and `IshSettingWorkflowTransition`"
[ISHREMOTE-036]: #ISHREMOTE-036 "Fix `IshSession` by removing Set properties which don't work like Timeout for WCF-proxy"
[ISHREMOTE-037]: #ISHREMOTE-037 "Rename `Add-IshValue`/`Get-IshValue`/`Remove-IshValue`/`Set-IshValue` to`Add-IshLovValue`/`Get-IshLovValue`/`Remove-IshLovValue`/`Set-IshLovValue`"
[ISHREMOTE-038]: #ISHREMOTE-038 "Fix `Set-IshFilterField` to allow more than one filter on the same field (also verified for `Set-IshValueField` and `Set-IshField`)"
[ISHREMOTE-039]: #ISHREMOTE-039 "Fix RemoveSystemFields to not send the new user fields like FISHPASSWORDMODIFIEDON"
[ISHREMOTE-040]: #ISHREMOTE-040 "Support ExecutionPolicy AllSigned (SignAndPublish.ps1)"
[ISHREMOTE-041]: #ISHREMOTE-041 "Respect single type for `IshObjects`/`IshFolders`/`IshEvents`/`IshLovValues`, so mark `internal`"
[ISHREMOTE-042]: #ISHREMOTE-042 "Respect single type for `IshFeatures`, so mark `internal`, improve `ps1xml` formatting"
[ISHREMOTE-043]: #ISHREMOTE-043 "Respect single type for `IshFields`, so mark `internal`, improve `ps1xml` formatting"
[ISHREMOTE-044]: #ISHREMOTE-044 "Respect single type for `IshApplicationSettings` which is used for statistics when `-Count` is specified, `IshApplicationSetting` (single) without `-Count`"
[ISHREMOTE-045]: #ISHREMOTE-045 "Add parameter argument completion for interactive usage."
[ISHREMOTE-046]: #ISHREMOTE-046 "More documentation for Get-Help"
[ISHREMOTE-047]: #ISHREMOTE-047 "`Set-IshMetadataField`/`Set-IshMetadataFilterField` take `object[]` instead of `string` and convert internally"
[ISHREMOTE-048]: #ISHREMOTE-048 "Merge `Find-` cmdlets into a parameter group on the `Get-` cmdlets. So `Get-` allows retrieval with identifiers and based on Find criterias."
[ISHREMOTE-049]: #ISHREMOTE-049 "Overload `Set-IshRequestedMetadataField` to accept `IshTypeFieldDefinition[]` to generate `IshMetadataField[]`"
[ISHREMOTE-050]: #ISHREMOTE-050 "`Get-IshSetting` should return same output type IshField for `-FieldName` (string) and `-RequestedMetadata`. String output could be achieved by `-Raw` flag for both."
[ISHREMOTE-051]: #ISHREMOTE-051 "Mandatory AllowOnCreate fields for `Add-` cmdlets should have their own ParameterSetName `MandatoryParameterGroup`"
[ISHREMOTE-052]: #ISHREMOTE-052 "Add `Get-IshBaseline`, `Find-IshBaseline` and `Get-IshBaselineItem` for metrics/insights purposes"
[ISHREMOTE-999]: #ISHREMOTE-999 "The end!"


# BACKLOG ENTRIES

Below entries are about plans, ideas, cross-linking,...

## <a name="ISHREMOTE-001"></a>[ISHREMOTE-001]
* `ISHRemote.PesterSetup.ps1` through Get-CIVariable("PROJECT", "NAME", "DEFAULT") idea (see ISHBootStrap)
* Continuous Integration (CI) should run on release/debug, project/version/user dimensions; allowing 10.0.x (partially!), 11.0.x, 12.0.x and TEST nightly run. This table needs README.MD documentation, and probably also takes part in ISHRemote major version bumping
* .\Samples\ becomes Articles, Get-Help inspiration and Pester Test. 
* `ISHRemote.PesterSetup.ps1` to hold $ignoreSslPolicyErrors=$false which is explictly passed to all `New-IshSession` switch parameters IgnoreSslPolicyErrors:$ignoreSslPolicyErrors except overwritten by the `-IgnoreSslPolicyErrors` tests. This would allow CI-testing of environments with invalid certificates.

## <a name="ISHREMOTE-002"></a>[ISHREMOTE-002]
* Cancelling the pipeline seems to be [unsupported](http://powershell.com/cs/blogs/tobias/archive/2010/01/01/cancelling-a-pipeline.aspx)
* Any script by default should pass information on the pipeline, which is what ISHRemote does.  
If your script by default doesn't do that, you still force it to do that by passing [-PassThru].  
To avoid the unwanted Retrieve calls, specify -PassThru $false explicitly. Implicitly and when specified it is true, unless explicitly specified as false.  
[Sample PassThru implementation](https://msdn.microsoft.com/en-us/library/ff602036(v=vs.85).aspx)

## <a name="ISHREMOTE-003"></a>[ISHREMOTE-003]  *DONE*
* Convert readme.txt to README.MD and BACKLOG.MD including a why and getting started section.

## <a name="ISHREMOTE-004"></a>[ISHREMOTE-004]  *DONE*
* Public GitHub project

## <a name="ISHREMOTE-005"></a>[ISHREMOTE-005]
* Timeout is implemented, but there is no retry mechanism for API calls inside the cmdlets. Probably means intoducing an API interface/facade to be able to easily wrap that. Potentially include token refresh as well. Might make it easier to change transport layer in a far future (e.g. REST or 3.0 API)

## <a name="ISHREMOTE-006"></a>[ISHREMOTE-006]
* All new code should receive Pester tests, this one is about correcting legacy/missing tests.

## <a name="ISHREMOTE-007"></a>[ISHREMOTE-007]  *DONE*
* Publish to PSGallery, allowing traction, refering to answer in community forums like [How to get word count for all topics in SDL in a particular period of time?](https://community.sdl.com/products-solutions/solutions/customer_experience_cloud/knowledge_delivery/knowledge-center/knowledgecenter-developers/f/215/t/8541)
* Requires [ISHREMOTE-026]
* This aligns but doesn't necessarily require [ISHREMOTE-004]

## <a name="ISHREMOTE-008"></a>[ISHREMOTE-008]
* `Unlock-IshUser` -IshSession -UserName will set FISHLOCKED=false (will throw on FISHLOCKED=true) available since Kojak/13.0.0

## <a name="ISHREMOTE-009"></a>[ISHREMOTE-009]
DocumentObj data migration cmdlets like `Get-IshDocumentObjData`, `Save-IshDocumentObj`,...
* `Get-IshDocumentObjData` should pipeline IshObject if `-FolderPath` parameter is missing. This allows passing metadata and blob in one call as input for Save-IshDocumentObj or even Set-IshDocumentObj. Matching cmdlet Get-IshPublicationOutputData could do the same, although encoding large 'chunked' blobs in the ishdata section is far from ideal.
* Add `Save-IshDocumentObj` to filesystem, probably needs matching `Load-IshDocumentObj` that is pipeline friendly. Consider `Save-IshDocumentObj` as debug/back-up but pipeline friendly to duplicate problems from one ishsession to the next.
    * Save the blob as `[ShortenedTitle]=Logicalname=Version=Language=Resolution.Edt` and the metadata as `[ShortenedTitle]=Logicalname=Version=Language=Resolution.met` holding ishfields (or ishobject)
* Similar data migration cmdlets (Save-Ish* and Load-Ish*) could be for User, UserGroup, PublicationOutput... actually required if we do per cmdlet object promotion, see [ISHREMOTE-011].
* ISHRemote.Objects should all be fully serialiable for temporary file storage and usage through ConvertTo-Xml and ConvertTo-JSon
* Consistency remarks
    1. `Set-IshDocumentObj` probably needs TryGetMetadata extending to do either Create or Update call. So far `Set-` cmdlets do not create objects!
	1. `New-IshDocumentObj`, so a client-side in-memory representation needs to match `Load-IshDocumentObj`. If we ever do that given the complexity of system and mandatory fields.

## <a name="ISHREMOTE-010"></a>[ISHREMOTE-010]
* Token inside IshSession doesn't refresh itself (like in the Client Tools), introduce [-AutoRefresh] which sets clients to null when InfoShareWcfConnection.IsValid.  
  Perhaps a Refresh-IshSession cmdlet (hmm better implicit/silent). Preferably make it work for the ISHRemote as client library as well, or doc it.

## <a name="ISHREMOTE-011"></a>[ISHREMOTE-011]
`IshField` promotion to properties. Either by more advanced `IshObject` inheritance or by `NoteProperty` solutions.
* Dynamic Members/Properties on all ishobject types allowing an easy $ishobject.FISHEMAIL. As an optional 
  switch parameter perhaps respecting a variable like $DebugPreference. Or make it a toggle property on IshSession. [TS-10266]
  NoteProperties are string only, so date comparisons are still hard (using yyyyMMdd.HHmmss allows compare operators), 
  hence Filter-IshObject/Filter-IshEvent/Filter-IshFolder.
    * PSNoteProperty: hardcoded date fields converting dd/MM/yyy HH:mm:ss to iso format allowing string compare: yyyyMMdd.HHmmss
	* `EventCmdlet.WrapAsPSObjectAndAddNoteProperties` exists and holds the code. Question remains if it the right way to do it (PassThru, seperate aliases per object type for `Convert-IshObject`, `Add-IshMember`,..)
* Instead of solving the NoteProperties (which are strings) promotions through a variable like $ISHPromoteObjects, we
  could also introduce [-PassThru] with options 
    1. none, this options boosts import by skipping retrieve for the pipeline
    1. minimal (current), so just enough identifiable information on the pipeline to allow piping and proper errors
    1. decorated(psobject-noteproperty) ... is about the promotion to properties for easy usage in Out-GridView (see Stop-IshPublicationOutput sample)
* Promote `IshObject` results to `IshDocumentObj` for `Get-IshDocumentObj` or `IshUser` for `Get-IshUser` (and `IshEvent` for `Get-IshEvent`), so matching the cmdlet noun. This would allow easier printing, nicer table printing through specialized  `ISHRemote.Format.ps1xml` entries, especially in interactive mode. This probably requires a fancy factory implementation compared to the current IshObject(xml) function.
    * IshDocumentObj groups MasterDoc/Module/.../Template, can print Resolution only for images
	* IshPublicationOutput like IshDocumentObj is a multi-level card hierarchy
	* IshEDT, IshOutputFormat, IshUser, IshUserGroup, IshUserRole, IshOutputFormat (future IshTranslationJob) derive from IshObject
	* Note that IshConfiguration returns an IshField[] currently
	* Set-IshField/Set-IshMetadataField should get aliases on IshObject. Make sure the property and IshObject class documentation is elaborate.
* Use cases 
    * `Get-IshEvent ... | Export-Csv` as easy EventMonitor export [SRQ-4522]
	* `Get-IshEvent ... | Filter-IshMetadataField -name -level -le yesterday | Remove-IshEvent` See [ISHREMOTE-024]. Pipeline filter and delete up to a certain date which was a UI feature we used to have allowing easier analysis [SRQ-4522]
	
## <a name="ISHREMOTE-012"></a>[ISHREMOTE-012]
* Implement and respect IshSession.BatchSize (especially in Retrieve* cmdlets), IshSession.ChunkSize (PublicationOutput and EventMonitor(if any))

## <a name="ISHREMOTE-013"></a>[ISHREMOTE-013]
* `Remove-IshDocumentObj` and probably `Remove-IshPublicationOutput` will only remove version and logical level when `-Force` was presented and if the presented language level was the last one.
  Suggest to add `-ForceLogical` which will delete by LogicalId, only when `TRISOFT_INFOSHARE_DocumentObjDelete_DenyExpandedLogicalObjectDelete` and `TRISOFT_INFOSHARE_PublicationOutputDelete_DenyExpandedLogicalObjectDelete` registry keys are not active.

## <a name="ISHREMOTE-014"></a>[ISHREMOTE-014]
* Add `Test-IshFolder` that calls Folder25.GetSubFoldersByIshFolderRef avoiding server-side errors to check for object existance

## <a name="ISHREMOTE-015"></a>[ISHREMOTE-015]
* Add `Move-IshPublicationOutput` based on `Move-IshDocumentObj` holding the same Folder25.MoveObjects implementation for consistency

## <a name="ISHREMOTE-016"></a>[ISHREMOTE-016]
Add `Filter-IshObject` cmdlet taking MetadataFilter parameter to do client-side object filtering [TS-10372]
* Preferably alias `Filter-IshObject` with `Filter-IshEvent` and `Filter-IshFolder` as null-able ValueFromPipeline overloads. Depending on [ISHREMOTE-011] for all ISHTypes!
* Taking the incoming object, passing throw if it matches the filter and pipeline the same object type out.
    * What if the field is not present? Fail the pass? Yes
	* `-IshSession` is required to pass future field type like string/number/date or multivalue which is important for compare operators, see also [ISHREMOTE-017] like Settings25.RetrieveFieldSetupByIshType.

Also have a look at `Filter-IshValueField` (alias `Filter-IshFieldValue`, actually `Filter-IshMetadataField`), a bit simpler and cleaner than the `-MetadataFilter` complexity. See [ISHREMOTE-024]

## <a name="ISHREMOTE-017"></a>[ISHREMOTE-017]  *DONE*
Add `IshSession.IshTypeFieldSetup` allowing `Get-IshTypeFieldDefinition` and `Compare-IshTypeFieldDefinition`
* `Get-IshTypeFieldDefinition` matching `Settings25.RetrieveFieldSetupByIshType` on Kojak/13.0.0 which returns `IshTypeFieldDefinition` records. `IshTypeFieldDefinition` is a denormalized version of the API result `<ishtypedefinition>` and `<ishfielddefinition>`.
    * It will do the call with `-Force`, or use the JIT version of the incoming `IshSession` available as `IshTypeFieldSetup`. Optional.
	* `IshTypeFieldSetup` returns a flat denormalised list of `IshTypeFieldDefinition`, alphabetically sorted by ISHType/Level/Field by default
	* `IshTypeFieldSetup` on the `IshSession` is initialized by `Settings25.RetrieveFieldSetupByIshType` on Kojak/13.0.0. But on 2014/11.0.x and 2016/12.0.x, based on `ServerVersion` it can be initialized by a TriDKXmlSetup-FullExport coming from the ISHRemote.Resources. Trisoft.ISHRemote.Properties.Resouces.IshTypeFieldSetup.TriDKXmlSetupFullExport_12_00_01 could work.
	* `IshTypeFieldSetup` should never block, only advise/warn. So requesting a custom field which is not in the `IshTypeFieldDefinition[]` will not be stopped client-side, the request will be past to the API which in turn can decide if it needs to error out. Should `TrisoftCmdlet` get clientside virtual `Verify(Filter/Requested/set)Metadata` function like deprecated `RemoveSystemFields` that Warns when you filter on unknown combinations like FISHMODIFIEDON on PublicationOutput
	* `IshTypeFieldDefinition` output should get a nice `ISHRemote.Format.ps1xml` configuration for ease of use. When format as table allowCreate is shortened to a C, multi value is a 1 or N, mandatory is M, description size should be cut. Potententially needs smart getters (Is a boolean True printed in a 1-char-column as 'T'?)
* `IshTypeFieldSetup` is the object that allows questions like filter system fields or get me essential/descriptive/minimal/all fields for reading or writing for these types. 
    * Should answer questions like questions like 'ISHUser-PASSWORD-action:read-->removefield' or 'ISHModule-MODIFIEDON-action:write-->removefield' which in turn enhances the cmdlet level RemoveSystemFields() to become dynamic

## <a name="ISHREMOTE-018"></a>[ISHREMOTE-018]  *DONE*
* Add `Get-IshTypeFieldDefinition` takes an optional `IshSession` and allows loading of a TriDKXmlSetup-FullExport (warning on api data overwrite on 13.0.0/kojak because of api existance)... pipeline result is `IshTypeFieldDefinition[]` and when `IshSession` is present it is persisted on `IshSession.IshTypeFieldSetup`.
    * Function is marked as deprecated from the start. There are no plans for a DBUT-FullExport, only a TriDKXmlSetup-FullExport.
* `Save-IshTypeFieldDefinition` (optionally) is competition for DBUT/TriDKXmlSetup-FullExport but over the API. Note however that the API will never return internal system fields like FFOLDERS.

## <a name="ISHREMOTE-019"></a>[ISHREMOTE-019]  *DONE*
* Add `Compare-IshTypeFieldDefinition` by either parameter set `-IshSessionA`/`-IshSessionB` or `IshTypeFieldDefinitionA[]`/`IshTypeFieldDefinitionB[]`
	* Compare 2 sets of `IshTypeFieldDefinition[]` and only spit out differences...alphabetically sorted by ISHType/Level/Field by default over the two sets... so spitting out A and B version back-to-back if different
	* Compare result could be `IshTypeFieldDefinitionCompare` which inherits `IshTypeFieldDefinition` and adds change property which printed by formats.xml could look #1 column holding typical git-conpare `+` and `-`. Or first column `==` `<<` `>>` `<>` and always print type level field… and only columns with differences, the rest of the cells are empty. So every `IshTypeFieldDefinitionCompare` should be printed as two rows with an additional compare column or without but `+`/`-` (preferably green/red color highlighting)
	* Note that JIT `IshSession` load and `Get-IshTypeFieldDefinition` over TriDKXmlSetup-FullExport allow a comparison between systems on standard/custom.

## <a name="ISHREMOTE-020"></a>[ISHREMOTE-020]
Overload all `-RequestedMetadata` on `Find-`/`Get-` cmdlets using `IshTypeFieldSetup`
* `-RequestedMetadata` is not mandatory (check all). So when not specified, we can fall back to `IshTypeFieldSetup` descriptive/essential/minimal/basic fields (not all as that includes FISHLINKS, etc). 
* See also [ISHREMOTE-025].

## <a name="ISHREMOTE-021"></a>[ISHREMOTE-021]  *DONE*
Rename `Set-IshValueField`, `Set-IshFilterField` and `Set-IshField` to `Set-IshMetadataField`, `Set-IshMetadataFilterField` and `Set-IshRequestedMetadataField`
* Discovery-wise, cmdlets ask for `-Metadata`, `-FilterMetadata` and `-RequestedMetadata` but the cmdlets to create those `<ishfield>` memory structures are called: `Set-IshValueField`, `Set-IshFilterField` and `Set-IshField`. So rename internal implementation to `IshMetadataField`, `IshMetadataFilterField` and `IshRequestedMetadataField`.  can stay the same.
* Parameter `-RequiredCurrentMetadata` could be done by an alias on `Set-MetadataField` so `Set-IshRequiredCurrentMetadataField` 
* Rename `Get-IshFieldValue` to `Get-IshMetadataField`
* Don't forget a mass-replace to update all documentation.
* Optional
	* Part of breaking compatibility with the past, and make it cleaner for first-time users. See also [ISHREMOTE-011] and [ISHREMOTE-026]
	* This change could match up with the mandatory `-IshSession` parameter introduction, see [ISHREMOTE-023].

## <a name="ISHREMOTE-022"></a>[ISHREMOTE-022]
Refactor to `IshTypeFieldSetup` by replacing hardcoded System and Required fields by dynamic ones. Requires [ISHREMOTE-017].
* ISHRemote abstract class cmdlet offers RemoveSystemFields, AddRequiredFields, AddRequiredDocumentObjFields, AddRequiredPublicationOutputFields
* Functions like `RemoveNonIdentifierFields`/`AddRequiredFields`/`RemoveSystemFields` respect metadata-requests for only logical or version... and the ActionMode
    
## <a name="ISHREMOTE-023"></a>[ISHREMOTE-023]  *DONE*
* Extend `Set-IshMetadataField`, `Set-IshMetadataFilterField` and `Set-IshRequestedMetadataField` cmdlets with `-IshSession` to allow ISHTypeFieldDefinition lookup for fieldnames in that database... Cool demo on complete `-Name` (and `-Level`) regarding field 
* `Set-IshValueField` could be based on the field type to make sure the assigment is right regarding number, datetime, version format. Requires [ISHREMOTE-017].

## <a name="ISHREMOTE-024"></a>[ISHREMOTE-024]
Add `Filter-IshValueField` (alias `Filter-IshFieldValue`, actually `Filter-IshMetadataField`), a bit simpler and cleaner than the `-MetadataFilter` complexity of `Filter-IshObject` in [ISHREMOTE-016].
* Requires [ISHREMOTE-017]
* Filter-ishfield(value) -name -level -eq(operator) -value and ishields/ishobject should be lightweight regarding memorymodel (so no ishfield promotion of a field because you now can know it is a date or multivalue?!). If the field is not incoming then remove the object from the pipeline, same of course if the test fails.
* Respect the the `IshSession` date format `dd/MM/yyy HH:mm:ss` but thanks to `IshTypeFieldSetup` you also know which fields are dates, allowing easier compare operators (so iso format avoiding string compare workaround through format yyyyMMdd.HHmmss). 

## <a name="ISHREMOTE-025"></a>[ISHREMOTE-025]
Add `Get-IshField` that returns `<ishfields>` based `ISHType[]` and switch flags
* `Get-IshField` or better name `Get-IshRequestedMetadataField`, see [ISHREMOTE-021]
* Requires [ISHREMOTE-017].
* Switch flags could be `-Descriptive`/`-System`/`-Basic`/`-All`. Idea needs to align with [ISHREMOTE-011]
* This would allow a `Find-IshDocumentObj -IshSess... -RequestedMetadata (Get-IshField -IshSession $ishSession -IshTypeFilter @('ISHLibrary','ISHModule','ISHMasterDoc') -All)`

## <a name="ISHREMOTE-026"></a>[ISHREMOTE-026]  *DONE*
Break compatiblity, remove all `Trisoft.Automation` artefacts. The goal is to make it easier for first-time users, as little legacy as possible.
* Raise the version number.
* Remove parameters like `-TrisoftSession`
* Remove cmdlet alias like `Retrieve-`/`Cancel-`/...
* Remove deprecated cmdlets like `Show-TrisoftAlias`/`Get-IshFolderChildren`
* Rename cmdlets like [ISHREMOTE-021]
* Don't forget a mass-replace to update all documentation.

## <a name="ISHREMOTE-027"></a>[ISHREMOTE-027]
Add `Test-IshDocumentObj` and `Test-PublicationOutput` through (Try)GetMetadata call to check for object existance

## <a name="ISHREMOTE-028"></a>[ISHREMOTE-028] 
Add `Search-IshDocumentObj` which in turn requires `IshQuery` object with `Set-IshQueryField`
* See [TS-10686]

## <a name="ISHREMOTE-029"></a>[ISHREMOTE-029] 
Overload `Set-IshMetadataField` to accept IshObject/IshFolder/... on the pipeline to mass update in-memory structure
* Requires alignment with [ISHREMOTE-011] and [ISHREMOTE-021]
* Needs to match overloads of `Get-IshFieldValue`
* Can we remove `Set-IshFields` (plural), is there still a use case for that? MergeFields... is `Set-IshValueField` capable of doing that one-by-one? If `Set-IshValueField` takes object as input, it is still missing the group ishfield merge/overwrite option. So leaving `Set-IshFields` for now as an advanced function.

## <a name="ISHREMOTE-030"></a>[ISHREMOTE-030] 
Fix `Stop-IshPublicationOutput` failing with null reference exception on `-WhatIf` usage

## <a name="ISHREMOTE-031"></a>[ISHREMOTE-031]
Extend parameter `-FolderPath` with intellisense
* See also [ISHREMOTE-023] which is based on in-memory lookup. This story is about API-based lookup.
* Probably implementation as cmdlet in ISHRemote.psm1 (see ITProceed slide deck)
* Depending on the success, could also be done for `Get-`/`Find-` on parameter `-LogicalId`
* See [ISHREMOTE-045]

## <a name="ISHREMOTE-032"></a>[ISHREMOTE-032]
Implement Provider for ISH:// usage (like IIS:// or HKLM://) allowing item/container browsing like Remove-Item/Set-Item

Get inspired on the consul.io one, see https://github.com/ntent-ad/consul-ps-provider

## <a name="ISHREMOTE-033"></a>[ISHREMOTE-033]
Add `Get-IshTranslationTemplate`, `Remove-IshTranslationTemplate` and `Add-IshTranslationTemplate` which in turn requires `IshTranslationJobTemplates`
* Especially the cleaning up unused ones seems to be an issue on various environments

## <a name="ISHREMOTE-034"></a>[ISHREMOTE-034]
Add `Get-IshTranslationJob`, `Find-IshTranslationJob` and `Get-IshTranslationJobItem` for analysis purposes
* Add `Get-IshTranslationJob` of a single `IshTranslationJob` always fully initializes by several API calls, and returns 
    1. `IshTranslationJob` respecting `IshTypeFieldSetup`, see [ISHREMOTE-017]
	1. `IshTranslationJob.IshTranslationJobContainer[]`, so no explicit `Get-` cmdlet but still an object with matching `ISHRemote.Format.ps1xml`
	1. `IshTranslationJob.IshTranslationJobWorkflow[]`, so no explicit `Get-` cmdlet but still an object with matching `ISHRemote.Format.ps1xml`
	1. `IshTranslationJob.IshExternalTranslationJob[]`, so no explicit `Get-` cmdlet but still an object with matching `ISHRemote.Format.ps1xml`
* Add `Find-IshTranslationJob` returning `IshTranslationJob[]` only initializes `IshTranslationJob` respecting `IshTypeFieldSetup`, see [ISHREMOTE-017]
* Add `Get-IshTranslationJobItem` of a single `IshTranslationJob` in turn requires `IshTranslationJobItem`
    * `IshTranslationJobItem` respecting `IshTypeFieldSetup`, see [ISHREMOTE-017]
    * Especially columns error/externalstatus allow debugging, available since (TS-10305)
	* Can a `IshTranslationJobItem` be promoted to a `IshDocumentObj`? An extra client-side cmdlet, beware of in-memory `New-IshDocumentObj` consistency.
* None-scope is Creating/Updating/Deleting `IshTranslationJob`
* None-scope is Creating/Updating/Deleting `IshTranslationJobItem`

## <a name="ISHREMOTE-035"></a>[ISHREMOTE-035]
Add `Get-IshSettingWorkflow` which in turn requires `IshSettingWorkflowSetup` and `IshSettingWorkflowTransition`
* Returns `IshSettingWorkflowTransition[]` with matching `ISHRemote.Format.ps1xml`. So workflow as a table with from/to/role (like the UI).
* Should know and respect xml @version attribute
* `IshSettingWorkflowSetup` on `IshSession`, potentially JIT retrieval, could allow future client-side workflow validation including initial states.
* Similar to `IshTypeFieldSetup` and `IshTypeFieldDefinition`, see [ISHREMOTE-017]. 
* None-scope is Adding/Removing `IshSettingWorkflowTransition`
* See also `Admin.XMLStatusConfiguration.xml`

## <a name="ISHREMOTE-036"></a>[ISHREMOTE-036]  *DONE*
Fix `IshSession` by removing Set properties which don't work like Timeout for WCF-proxy

## <a name="ISHREMOTE-037"></a>[ISHREMOTE-037]  *DONE*
Rename `Add-IshValue`/`Get-IshValue`/`Remove-IshValue`/`Set-IshValue` to`Add-IshLovValue`/`Get-IshLovValue`/`Remove-IshLovValue`/`Set-IshLovValue`
* IshValue is a too generic term that most people will not bind to List Of Values. Furthermore when searching for `Value` you also get a confusing hit for `Set-IshValueField` which is about `<ishfield>` and not about `<ishlovvalue>`
* Align with [ISHREMOTE-021] and [ISHREMOTE-026]

## <a name="ISHREMOTE-038"></a>[ISHREMOTE-038]  *DONE*
Fix `Set-IshFilterField` to allow more than one filter on the same field (also verified for `Set-IshValueField` and `Set-IshField`)
* When querying for 1 January 2016 blob changes by `$metadataFilter = Set-IshFilterField -Name 'FISHLASTMODIFIEDON' -Level Lng -FilterOperator GreaterThanOrEqual -Value '01/01/2016' | Set-IshFilterField -Name 'FISHLASTMODIFIEDON' -Level Lng -FilterOperator LessThan -Value '02/01/2016'` the logging on `DocumentObj25.Find` shows `18:13:41.6645	Trace	36	00068	a:UC:Admin2	API25/DocumentObj/Find	Trisoft.InfoShare.API25.DocumentObj.Find	(xmlMetadataFilter=[<?xml version="1.0" encoding="utf-16"?><ishfields><ishfield name="FISHLASTMODIFIEDON" level="lng" ishoperator="lessthan" ishvaluetype="value">02/01/2016</ishfield></ishfields>])	[]	`
* So the second filter was passed, the first one not, eventually pulling a lot of data from the database.
* Best to also check `Set-IshValueField` and `Set-IshField`

## <a name="ISHREMOTE-039"></a>[ISHREMOTE-039]  *DONE*
Fix RemoveSystemFields to not send the new user fields like (TS-11787)
* FISHPASSWORDMODIFIEDON
* FISHFAILEDATTEMPTS
* FISHLOCKEDSINCE
* FISHPASSWORDHISTORY

## <a name="ISHREMOTE-040"></a>[ISHREMOTE-040]
Support ExecutionPolicy AllSigned (SignAndPublish.ps1)
* As an Administrator run `Set-ExecutionPolicy AllSigned` where `Import-Module ISHRemote` will fail because of unsigned text files like `PSM1` and `PS1XML`.
* The full error reads `C:\Users\ddemeyer\Documents\WindowsPowerShell\Modules\ISHRemote\0.1.2119.0\ISHRemote.Format.ps1xml, , C:\Users\ddemeyer\Documents\WindowsPowerShell\Modules\ISHRemote\0.1.2119.0\ISHRemote.Format.ps1xml: The file was skipped because of the following validation exception: File C:\Users\ddemeyer\Documents\WindowsPowerShell\Modules\ISHRemote\0.1.2119.0\ISHRemote.Format.ps1xml cannot be loaded. The file C:\Users\ddemeyer\Documents\WindowsPowerShell\Modules\ISHRemote\0.1.2119.0\ISHRemote.Format.ps1xml is not digitally signed. You cannot run this script on the current system.`

## <a name="ISHREMOTE-041"></a>[ISHREMOTE-041]  *DONE*
Respect single type for `IshObjects`/`IshFolders`/`IshEvents`/`IshLovValues`, so mark `internal`"

## <a name="ISHREMOTE-042"></a>[ISHREMOTE-042]  *DONE*
Respect single type for `IshFeatures`, so mark `internal`, improve `ps1xml` formatting"

## <a name="ISHREMOTE-043"></a>[ISHREMOTE-043]
Respect single type for `IshFields`, so mark `internal`, improve `ps1xml` formatting"
* Verify the `Fields()` usage, should no longer return `IshFields` (plural) but `IshField[]`
* Various `IshField` derivations should print nicer through improved `ps1xml` formatting

## <a name="ISHREMOTE-044"></a>[ISHREMOTE-044]  *DONE*
Respect single type for `IshApplicationSettings` which is used for statistics when `-Count` is specified, `IshApplicationSetting` (single) without `-Count`"

## <a name="ISHREMOTE-045"></a>[ISHREMOTE-045]
Add parameter argument completion for interactive usage.
You can parameter matching for wildcard-cmdlet names. I would first think about IshTypeFieldDefinition derived things like field name and level. Another scenario is element name completion (e.g. Find-... when 5+ characters are there for a GUID). The tooltip could show the readable label while the code actually can continue with the element name. FolderPath autocompletion.
    * 20160614.ITProceed.TheArtofPowerShellToolmaking.pptx
	* https://github.com/thinkbeforecoding/PSCompletion/blob/master/PSCompletion/Register-ParameterCompleter.ps1 hints on "$fakeBoundParameter : A hashtable container names/values of other specified parameters" so only do something when `-IshSession` is specified, then IshField cmdlet parameters like `-Name` and `-Level` could be checked or even suggested
	* http://stackoverflow.com/questions/30923696/add-custom-argument-completer-for-cmdlet is a nice intro, and hints at "Another critical piece was having that script run when the module is loaded. This was done by adding this to the .psd1: ScriptsToProcess = 'ArgumentTabCompletion.ps1'"
	* See [ISHREMOTE-031]

## <a name="ISHREMOTE-046"></a>[ISHREMOTE-046]
More documentation for Get-Help

## <a name="ISHREMOTE-047"></a>[ISHREMOTE-047]
`Set-IshMetadataField`/`Set-IshMetadataFilterField` take `object[]` instead of `string` and convert internally
    * Alter current `public string Value { get; set; }` to `public object[] Value { get; set; }`
	* Concatenate/join the object.ToString() with the IshSession.Seperator (comma-space typically)

## <a name="ISHREMOTE-048"></a>[ISHREMOTE-048]
Merge `Find-` cmdlets into a ParameterSetName on the `Get-` cmdlets. So `Get-` allows retrieval with identifiers and based on Find criterias.
    * The `Find-` cmdlets was built on top of the API Find operation, which triggers a query to find all objects matching the filter criteria.
    * The `Get-` cmdlets was built on top of the API Retrieve/Get operations, which trigger a query given object identifiers and then potentially extra filter criteria.
	* This stories tries to be more PowerShell minded and less API minded. Still needs investigation, but in essence Get is a Find but requires object identifiers.

## <a name="ISHREMOTE-049"></a>[ISHREMOTE-049]
Overload `Set-IshRequestedMetadataField` to accept `IshTypeFieldDefinition[]` to generate `IshMetadataField[]`
    * `Get-IshTypeFieldDefinition` with a couple of `Where-Object` clauses could serve as input to specify requested metadata
	* See also [ISHREMOTE-020] and [ISHREMOTE-025]

## <a name="ISHREMOTE-050"></a>[ISHREMOTE-050]
`Get-IshSetting` should return same output type IshField for `-FieldName` (string) and `-RequestedMetadata`. String output could be achieved by `-Raw` flag for both.
    * `$settings=$configurationTypeFieldDefinition|ForEach-Object { Get-IshSetting -IshSession $ishSession -FieldName $_.Name }` returns string[]
	* `$configurationAllRequestMetadata | ForEach-Object { Get-IshSetting -IshSession $ishSession -RequestedMetadata $_ }` returns IshField[]

## <a name="ISHREMOTE-051"></a>[ISHREMOTE-051]
Mandatory AllowOnCreate fields for `Add-` cmdlets should have their own ParameterSetName `MandatoryParameterGroup`
    * The metadata for an Add-IshUser requires fields FUSERGROUP, PASSWORD (Internal only) which could as well be explicit cmdlet parameters
	* Same goes for other ISHTypes. Alternative could be some `New-IshUser` or `New-IshDocumentObj` that creates an in-memory object. See also [ISHREMOTE-009]

## <a name="ISHREMOTE-052"></a>[ISHREMOTE-052]
Add `Get-IshBaseline`, `Find-IshBaseline` and `Get-IshBaselineItem` for analysis purposes
* Add `Get-IshBaseline` of a single `IshBaseline` always fully initializes by several API calls, and returns 
    1. `IshBaseline` respecting `IshTypeFieldSetup`, see [ISHREMOTE-017]
* Add `Find-IshBaseline` returning `IshBaseline[]` only initializes `IshBaseline` respecting `IshTypeFieldSetup`, see [ISHREMOTE-017]
* Add `Get-IshBaselineItem` of a single `IshBaseline` in turn requires `IshBaselineItem`
    * `IshBaselineItem` respecting `IshTypeFieldSetup`, see [ISHREMOTE-017]
    * Returns all columns/attributes of the <baseline><object> items
	* `IshBaselineItem.IshBaselineReportItem[]`, so no explicit `Get-` cmdlet but still an object with matching `ISHRemote.Format.ps1xml` because <object> returns child <reportitem> entries for reports 
	* `IshBaselineItem` can also be used to update baselines as the API asks these items to be wrapped by <modifications>
* None-scope is Creating/Updating/Deleting `IshBaseline`
* None-scope is Creating/Updating/Deleting `IshBaselineItem`
	
# <a name="ISHREMOTE-999"></a>ISHREMOTE-999