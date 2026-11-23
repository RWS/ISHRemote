<#
.SYNOPSIS
	Converts the string value of one problem field on publication outputs into the typed value of a new field.

.DESCRIPTION
	Scenario: a STRING field holds the label of a value that belongs to a list of values (DOMAIN)
	or to a card type like users. Client Tools search with the identifier of that value, never
	with the label, so searching the STRING field finds nothing. The supported design is a typed
	field. This script moves the values of one old STRING field to one new typed field, on
	publication outputs (ISHPublication). Call the script once per field.

	The sample works on any standard CMS setup, including with -WhatIf, by using standard fields:
		OldFieldName  FCHANGES (STRING), holding for example 'true' or 'Admin'
		NewFieldName  FPUBINCLUDECOMMENTS (BOOLEAN domain), 'true' becomes the 'True' domain value
		NewFieldName  FISHPUBLISHER (IShUser), 'Admin' becomes the user with that name
	Both new fields are system fields, so the real update is expected to fail while -WhatIf works.
	Replace the names with your own fields, like the legacy STRING field FDLTOUTPUTISRELEASED and
	its new DOMAIN field, to tweak it to your case.

	Prerequisites, all outside of this script:
	- The new field (and its list of values entries) is created up front, before running this
	  script, using the DatabaseUpgradeTool (DBUT) and added to the metadata configuration.
	- The user running this script holds the Administrator user role, plus access (read and
	  write) to all UserGroups that protect the folders being converted. Released, frozen or
	  completed objects can only be updated with these elevated permissions.

	Flow:
	- Get-IshTypeFieldDefinition resolves the level and data type of both fields, nothing is assumed.
	- The allowed values of the new field are retrieved once: Get-IshLovValue on the DataSource for
	  an ISHLov field, Find-IshUser for an IShUser field. Other data types are not supported.
	- Convert-IshProblemField receives one object, matches the old value (case-insensitive) with
	  the allowed labels, and returns the object holding the refreshed new field. No match throws,
	  and that object is skipped with a warning.
	- The refreshed object is passed to Set-IshPublicationOutput, which only submits the new field.

	The old field is NOT emptied by default, so the script can be run multiple times, overwriting
	the new values when the conversion rules changed. A comment inside the convert function shows
	how to empty the old field for people that want that.

	As performance is key, IshSession.DefaultRequestedMetadata is set to Descriptive (minimal)
	for the duration of the script and restored afterwards. Every field needed for conversion or
	logging is therefore explicitly requested through RequestedMetadata.

.PARAMETER IShSession
	New-IshSession result. Defaults to the session held in
	$ISHRemoteSessionStateIshSession.

.PARAMETER FolderPath
	The CMS folder path to scan, including subfolders, for ISHPublication folders.
	Defaults to 'General'.

.PARAMETER OldFieldName
	The STRING field holding the value to convert.

.PARAMETER NewFieldName
	The typed field receiving the converted value.

.EXAMPLE
	.\Sample.Automate.ConvertFields.ps1 -FolderPath 'General\DataSets\PajeroDemo' -OldFieldName FCHANGES -NewFieldName FISHPUBLISHER -WhatIf

	Shows which publication outputs would get FISHPUBLISHER filled from FCHANGES. To test put a USERNAME in the Publication Version Changes field.

.NOTES
	Writes only the new field to the CMS. Test on a non-production environment first.
#>

[CmdletBinding(SupportsShouldProcess)]
param(
	$IShSession = $ISHRemoteSessionStateIshSession,
	[string] $FolderPath = 'General',
	[Parameter(Mandatory)]
	[string] $OldFieldName,
	[Parameter(Mandatory)]
	[string] $NewFieldName
)

$ishType = 'ISHPublication'

# 1. Field definitions: resolve level, data type and domain of both fields
$fieldDefinitions = Get-IshTypeFieldDefinition -IshSession $IShSession | Where-Object { $_.ISHType -eq $ishType }
$oldDefinition = $fieldDefinitions | Where-Object { $_.Name -eq $OldFieldName } | Select-Object -First 1
if ($null -eq $oldDefinition) { throw "OldFieldName[$OldFieldName] is not known for ISHType[$ishType]" }
$newDefinition = $fieldDefinitions | Where-Object { $_.Name -eq $NewFieldName } | Select-Object -First 1
if ($null -eq $newDefinition) { throw "NewFieldName[$NewFieldName] is not known for ISHType[$ishType]" }
$oldLevel = [string]$oldDefinition.Level
$newLevel = [string]$newDefinition.Level

# 2. Allowed values of the new field: label -> element
$labelToElement = @{}
if ($newDefinition.DataType -eq 'ISHLov') {
	Get-IshLovValue -IshSession $IShSession -LovId $newDefinition.DataSource | ForEach-Object { $labelToElement[$PSItem.Label] = $PSItem.IshRef }
} elseif ($newDefinition.DataType -eq 'ISHType' -and $newDefinition.DataSource -eq 'ISHUser') {
	Find-IshUser -IshSession $IShSession | ForEach-Object { $labelToElement[$PSItem.username] = $PSItem.IshRef }
} else {
	throw "NewFieldName[$NewFieldName] has DataType[$($newDefinition.DataType)] DataSource[$($newDefinition.DataSource)] which this sample cannot convert to"
}
Write-Verbose ("Old[$OldFieldName/$oldLevel] New[$NewFieldName/$newLevel $($newDefinition.DataType) $($newDefinition.DataSource)] AllowedValues[$($labelToElement.Count)]")

# 3. Explicit RequestedMetadata, needed because DefaultRequestedMetadata is Descriptive
$requestedMetadata = @(
	(Set-IshRequestedMetadataField -IshSession $IShSession -Level $oldLevel -Name $OldFieldName -ValueType Value),
	(Set-IshRequestedMetadataField -IshSession $IShSession -Level $newLevel -Name $NewFieldName -ValueType Element)
)

# Receives one object, converts the old value into the new field and returns the refreshed
# object, ready for Set-IshPublicationOutput.
function Convert-IshProblemField {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, ValueFromPipeline)]
		$IshObject
	)

	process {
		$oldValue = ([string](Get-IshMetadataField -IshSession $IShSession -IshObject $IshObject -Level $oldLevel -Name $OldFieldName -ValueType Value)).Trim()
		# Hashtable keys are case-insensitive, so 'true' matches 'True'. Adapt the rules to your data.
		if (-not $labelToElement.ContainsKey($oldValue)) {
			throw "Value[$oldValue] of field[$OldFieldName] is not an allowed value of field[$NewFieldName]"
		}
		$IshObject = Set-IshMetadataField -IshSession $IShSession -IshObject $IshObject -Level $newLevel -Name $NewFieldName -ValueType Element -Value $labelToElement[$oldValue]

		# Optional, disabled by default so the script can be rerun. Uncomment to empty the old field.
		# $IshObject = Set-IshMetadataField -IshSession $IShSession -IshObject $IshObject -Level $oldLevel -Name $OldFieldName -ValueType Value -Value ''

		Write-Output $IshObject
	}
}

$originalDefaultRequestedMetadata = $IShSession.DefaultRequestedMetadata
$IShSession.DefaultRequestedMetadata = 'Descriptive'
$converted = 0
$failed = 0
try {
	# Server-side filter, so only objects with something to convert are returned
	$metadataFilter = Set-IshMetadataFilterField -IshSession $IShSession -Level $oldLevel -Name $OldFieldName -FilterOperator NotEmpty
	Get-IshFolder -IshSession $IShSession -FolderPath $FolderPath -FolderTypeFilter @($ishType) -RequestedMetadata (Set-IshRequestedMetadataField -Name 'FISHFOLDERPATH') -Recurse |
	ForEach-Object {
		$ishFolder = $PSItem
		Write-Host ("Handling folder[$($ishFolder.fishfolderpath.Split(', ') -Join $IShSession.FolderPathSeparator)]...")
		Get-IshFolderContent -IshSession $IShSession -IshFolder $ishFolder -VersionFilter '' -MetadataFilter $metadataFilter -RequestedMetadata $requestedMetadata |
		ForEach-Object {
			$ishObject = $PSItem
			$key = "$($ishObject.IshRef)/$($ishObject.LngRef)"
			try {
				$ishObject = Convert-IshProblemField -IshObject $ishObject
				# Submit only the new field, never the old one
				$oldValue = Get-IshMetadataField -IshSession $IShSession -IshObject $ishObject -Level $oldLevel -Name $OldFieldName -ValueType Value
				$newValue = Get-IshMetadataField -IshSession $IShSession -IshObject $ishObject -Level $newLevel -Name $NewFieldName -ValueType Element
				$newMetadata = Set-IshMetadataField -IshSession $IShSession -Level $newLevel -Name $NewFieldName -ValueType Element -Value $newValue
				if ($PSCmdlet.ShouldProcess($key, "Set-IshPublicationOutput $OldFieldName[$oldValue] -> $NewFieldName[$newValue]")) {
					$null = $ishObject | Set-IshPublicationOutput -IshSession $IShSession -Metadata $newMetadata
				}
				Write-Verbose ("Converted object[$key]")
				$converted++
			} catch {
				$failed++
				Write-Warning ("Skipping object[$key] message[$($_.Exception.Message)]")
			}
		}
	}
} finally {
	$IShSession.DefaultRequestedMetadata = $originalDefaultRequestedMetadata
}
Write-Host ("Converted[$converted] Failed[$failed]")
