# Release Notes of ISHRemote v0.7

Actual detailed release notes are on [Github](https://github.com/sdl/ISHRemote/releases/tag/v0.7), below some code samples.

Remember
* All C# source code of the ISHRemote library is online at [Source](https://github.com/sdl/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote), including handling of WS-Trust protocol.
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/sdl/ISHRemote/blob/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/sdl/ISHRemote/blob/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

## Sample - Create Session and List Pending Background Task

Showing implicit IshSession behavior, avoiding explicit `-IshSession` and `-RequestedMetadata` parameters. Less typing and a lot of information. Note that explicit parameters are still possible like before.

```powershell
New-IshSession -WsBaseUrl https://medevddemeyer10.global.sdl.corp/InfoShareWSDita/ -PSCredential Admin2
Get-IshLovValue -LovId DSTATUS
Find-IshEDT
Get-IshBackgroundTask | Where-Object -Property status -EQ "Pending"
Get-IshBackgroundTask | Out-GridView
```

![ISHRemote-0.7--Session-DStatus-EDT-BackgroundTaskPending-BackgroundTaskGridview](./Images/ISHRemote-0.7--Session-DStatus-EDT-BackgroundTaskPending-BackgroundTaskGridview2.gif)

## Sample - Descriptive, Basic and All Fields

Showing the implicit defaulting of the `-RequestedMetadata` parameter. By defaulting to `Basic` with matching default table rendering, you get a clean overview of your requested objects. Think about listing User Profiles (`Find-IshUser`) or Event (`Get-IshEvent`).

```powershell
$ishSession = New-IshSession -WsBaseUrl https://medevddemeyer10.global.sdl.corp/InfoShareWSDita/ -PSCredential Admin2
$ishSession.DefaultRequestedMetadata = 'Descriptive' # v0.6 and before, only identifying fields
Find-IshOutputFormat 
$ishSession.DefaultRequestedMetadata = 'Basic' # v0.7, new default readable fields
Find-IshOutputFormat
$ishSession.DefaultRequestedMetadata = 'All'  # v0.7, new optional all fields
Find-IshOutputFormat | Select-Object -Property * | Out-GridView
```

![ISHRemote-0.7--Session-BackgroundTask-DescriptiveBasicAllField](./Images/ISHRemote-0.7--Session-BackgroundTask-DescriptiveBasicAllField.gif)

## Sample - Descriptive, Basic and All Fields Performance

Showing the performance effect of implicit defaulting of the `-RequestedMetadata` parameter. And also the option to return to the previous behavior. Usability is high for the negliable overhead of retrieving a lot more info, generating read only object properties (`PSNoteProperty`) and table style rendering.

```powershell
$ishSession = New-IshSession -WsBaseUrl https://medevddemeyer10.global.sdl.corp/InfoShareWSDita/ -PSCredential Admin2
$metadataFilter = Set-IshMetadataFilterField -Level Lng -Name MODIFIED-ON -FilterOperator GreaterThanOrEqual -Value "01/01/2016" |
                  Set-IshMetadataFilterField -Level Lng -Name MODIFIED-ON -FilterOperator LessThan -Value "01/01/2017" 
$ishSession.DefaultRequestedMetadata = 'Descriptive' # v0.6 and before, only identifying fields
$ishSession.PipelineObjectPreference = 'Off'         # v0.6 and before
(Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
$ishSession.DefaultRequestedMetadata = 'Basic'                # v0.7, new default readable fields
$ishSession.PipelineObjectPreference = 'PSObjectNoteProperty' # v0.7, new default readonly object properties
(Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
$ishSession.DefaultRequestedMetadata = 'All'                  # v0.7, new optional all fields
$ishSession.PipelineObjectPreference = 'PSObjectNoteProperty' # v0.7, new default readonly object properties
(Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
```

![ISHRemote-0.7--Session-DocumentObj-DescriptiveBasicAllFieldPerformance](./Images/ISHRemote-0.7--Session-DocumentObj-DescriptiveBasicAllFieldPerformance.gif)

## Sample - JSON

Showing a _potential_ JSON version derived from the public objects.

```powershell
Get-IshUser | ConvertTo-JsonEx -Depth 1 -AsArray
```
results in
```
[
  {
    "IshRef": "VUSERADMIN2",
    "IshType": 8,
    "IshField": "Set-IshMetadataField -Level none -Name CREATED-ON -ValueType value -Value \"13/03/2007
 11:19:12\" Set-IshMetadataField -Level none -Name FISHEMAIL -ValueType value -Value \"ishremote@sdl.co
m\" Set-IshMetadataField -Level none -Name FISHEXTERNALID -ValueType value -Value \"Admin2\" Set-IshMet
adataField -Level none -Name FISHFAILEDATTEMPTS -ValueType value -Value \"0\" Set-IshMetadataField -Lev
el none -Name FISHLASTLOGINON -ValueType value -Value \"23/03/2019 14:11:47\" Set-IshMetadataField -Lev
el none -Name FISHLOCKED -ValueType value -Value \"No\" Set-IshMetadataField -Level none -Name FISHLOCK
ED -ValueType element -Value \"FALSE\" Set-IshMetadataField -Level none -Name FISHLOCKEDSINCE -ValueTyp
e value -Value \"\" Set-IshMetadataField -Level none -Name FISHOBJECTACTIVE -ValueType value -Value \"Y
es\" Set-IshMetadataField -Level none -Name FISHOBJECTACTIVE -ValueType element -Value \"TRUE\" Set-Ish
MetadataField -Level none -Name FISHPASSWORDMODIFIEDON -ValueType value -Value \"13/03/2007 11:19:12\" 
Set-IshMetadataField -Level none -Name FISHUSERDISABLED -ValueType value -Value \"No\" Set-IshMetadataF
ield -Level none -Name FISHUSERDISABLED -ValueType element -Value \"FALSE\" Set-IshMetadataField -Level
 none -Name FISHUSERDISPLAYNAME -ValueType value -Value \"Admin2 Display Name\" Set-IshMetadataField -L
evel none -Name FISHUSERTYPE -ValueType value -Value \"Internal\" Set-IshMetadataField -Level none -Nam
e FISHUSERTYPE -ValueType element -Value \"VUSERTYPEINTERNAL\" Set-IshMetadataField -Level none -Name M
ODIFIED-ON -ValueType value -Value \"23/03/2019 14:11:47\" Set-IshMetadataField -Level none -Name NAME 
-ValueType value -Value \"Admin2\" Set-IshMetadataField -Level none -Name OSUSER -ValueType value -Valu
e \"GLOBAL\\Admin2\" Set-IshMetadataField -Level none -Name FISHUSERLANGUAGE -ValueType value -Value \"
en\" Set-IshMetadataField -Level none -Name FISHUSERLANGUAGE -ValueType element -Value \"VLANGUAGEEN\" 
Set-IshMetadataField -Level none -Name FISHUSERROLES -ValueType value -Value \"Administrator, Author, P
lanning, Reviewer, Translator\" Set-IshMetadataField -Level none -Name FISHUSERROLES -ValueType element
 -Value \"VUSERROLEADMINISTRATOR, VUSERROLEAUTHOR, VUSERROLEPLANNING, VUSERROLEREVIEWER, VUSERROLETRANS
LATOR\" Set-IshMetadataField -Level none -Name FUSERGROUP -ValueType value -Value \"Default Department,
 Project team, Research and Development, Sales Marketing, Support, System management\" Set-IshMetadataF
ield -Level none -Name FUSERGROUP -ValueType element -Value \"VUSERGROUPDEFAULTDEPARTMENT, VUSERGROUPPR
OJECTTEAM, VUSERGROUPRESEARCHANDDEVELOPMENT, VUSERGROUPSALESMARKETING, VUSERGROUPSUPPORT, VUSERGROUPSYS
TEMMANAGEMENT\" Set-IshMetadataField -Level none -Name RIGHTS -ValueType value -Value \"Default Departm
ent, Project team, Research and Development, Sales Marketing, Support, System management\" Set-IshMetad
ataField -Level none -Name RIGHTS -ValueType element -Value \"VUSERGROUPDEFAULTDEPARTMENT, VUSERGROUPPR
OJECTTEAM, VUSERGROUPRESEARCHANDDEVELOPMENT, VUSERGROUPSALESMARKETING, VUSERGROUPSUPPORT, VUSERGROUPSYS
TEMMANAGEMENT\" Set-IshMetadataField -Level none -Name USERNAME -ValueType value -Value \"Admin2\" Set-
IshMetadataField -Level none -Name USERNAME -ValueType element -Value \"VUSERADMIN2\"",
    "IshData": "Trisoft.ISHRemote.Objects.Public.IshData",
    "ObjectRef": "System.Collections.Generic.Dictionary`2[Trisoft.ISHRemote.Objects.Enumerations+ReferenceType,System.String]",
    "createdon": "2007-03-13T11:19:12",
    "fishemail": "ishremote@sdl.com",
    "fishexternalid": "Admin2",
    "fishfailedattempts": "0",
    "fishlastloginon": "2019-03-23T14:11:47",
    "fishlocked": "No",
    "fishlocked_none_element": "FALSE",
    "fishlockedsince": "",
    "fishobjectactive": "Yes",
    "fishobjectactive_none_element": "TRUE",
    "fishpasswordmodifiedon": "2007-03-13T11:19:12",
    "fishuserdisabled": "No",
    "fishuserdisabled_none_element": "FALSE",
    "fishuserdisplayname": "Admin2 Display Name",
    "fishusertype": "Internal",
    "fishusertype_none_element": "VUSERTYPEINTERNAL",
    "modifiedon": "2019-03-23T14:11:47",
    "name": "Admin2",
    "osuser": "GLOBAL\\Admin2",
    "fishuserlanguage": "en",
    "fishuserlanguage_none_element": "VLANGUAGEEN",
    "fishuserroles": "Administrator, Author, Planning, Reviewer, Translator",
    "fishuserroles_none_element": "VUSERROLEADMINISTRATOR, VUSERROLEAUTHOR, VUSERROLEPLANNING, VUSERROLEREVIEWER, VUSERROLETRANSLATOR",
    "fusergroup": "Default Department, Project team, Research and Development, Sales Marketing, Support, System management",
    "fusergroup_none_element": "VUSERGROUPDEFAULTDEPARTMENT, VUSERGROUPPROJECTTEAM, VUSERGROUPRESEARCHANDDEVELOPMENT, VUSERGROUPSALESMARKETING, VUSERGROUPSUPPORT, VUSERGROUPSYSTEMMANAGEMENT",
    "rights": "Default Department, Project team, Research and Development, Sales Marketing, Support, System management",
    "rights_none_element": "VUSERGROUPDEFAULTDEPARTMENT, VUSERGROUPPROJECTTEAM, VUSERGROUPRESEARCHANDDEVELOPMENT, VUSERGROUPSALESMARKETING, VUSERGROUPSUPPORT, VUSERGROUPSYSTEMMANAGEMENT",
    "username": "Admin2",
    "username_none_element": "VUSERADMIN2"
  }
]
```
