<#
.SYNOPSIS
	Finds source-language documents with invalid @ishcondition syntax.

.DESCRIPTION
	The script scans the selected folder and its subfolders for modules, master
	documents, and libraries. It requests only source-language objects, exports
	each object to a local folder, parses the exported XML, and validates every
	@ishcondition attribute with the Trisoft.Utilities.ConditionFilter validator.

	When all conditions in a document are valid, the exported local file is
	removed. The CMS document is not modified or deleted. When a condition is
	invalid, the exported file is kept and the original document object is
	written to the output collection. Documents that cannot be exported or
	parsed are skipped with a warning.

	The validator assembly is loaded from the GAC first. ConditionFilterPath is
	used only as a fallback when the assembly is not available there.

.PARAMETER IShSession
	New-IshSession result. Defaults to the session held in
	$ISHRemoteSessionStateIshSession.

.PARAMETER FolderPath
	The CMS folder path to scan. The scan includes subfolders and is limited to
	ISHModule, ISHMasterDoc, and ISHLibrary objects. Defaults to
	'\General\TestKD'.

.PARAMETER ExportFolderPath
	The local folder where Get-IshDocumentObjData writes temporary XML files.
	Valid files are deleted after validation; invalid files remain for review.
	Defaults to $env:TEMP.

.PARAMETER ConditionFilterPath
	The fallback path to Trisoft.Utilities.ConditionFilter.dll when the
	assembly cannot be loaded from the GAC. Defaults to the standard
	InfoShare Web installation path.

.EXAMPLE
	.\Sample.Automate.CheckConditionSyntax.ps1 -IShSession $ishSession -FolderPath '\General\Issues'

	Scans the Issues folder recursively using the active session and the
	default temporary export folder.

.EXAMPLE
	.\Sample.Automate.CheckConditionSyntax.ps1 -IShSession $ishSession `
		-FolderPath '\General\Issues' -ExportFolderPath 'C:\Temp\ConditionScan'

	Scans the Issues folder and keeps invalid documents' exported XML in the
	specified local folder for inspection.

.NOTES
	This script performs read operations against the CMS and removes only local
	exported files. It does not update or delete CMS content.

.LINK
	https://github.com/RWS/ISHRemote/blob/master/Doc/ReleaseNotes-ISHRemote-0.13.md#sample---custom-actions-across-folder-and-subfolders
#>

param(
	$IShSession = $ISHRemoteSessionStateIshSession,
	[string] $FolderPath = '\General\TestKD',
	[string] $ExportFolderPath = $env:TEMP,
	[string] $ConditionFilterPath = 'C:\InfoShare\Web\InfoShareWS\Api\Trisoft.Utilities.ConditionFilter.dll'
)


try {
	Add-Type -AssemblyName 'Trisoft.Utilities.ConditionFilter' -ErrorAction Stop
} catch {
	if (-not (Test-Path -LiteralPath $ConditionFilterPath -PathType Leaf)) {
		throw "ConditionFilterPath[$($ConditionFilterPath)] does not exist"
	}
	Add-Type -Path $ConditionFilterPath
}

# Example:
# Test-IshConditionSyntax '((MODEL=330) or (MODEL=990))'
# Test-IshConditionSyntax '((MODEL=330) or (MODEL=990)'
function Test-IshConditionSyntax {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory)]
		[AllowEmptyString()]
		[string] $Condition
	)

	try {
		[Trisoft.Utilities.ConditionFilter.ConditionFilter]::ValidateConditionSyntax($Condition)
		[pscustomobject]@{
			Condition = $Condition
			IsValid   = $true
			Warning   = $null
		}
	}
	catch {
		[pscustomobject]@{
			Condition = $Condition
			IsValid   = $false
			Warning   = $_.Exception.Message
		}
	}
}

function Invoke-IshDocumentObjTransform {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, ValueFromPipeline)]
		$IshObject,
		[Parameter(Mandatory)]
		$IShSession,
		[Parameter(Mandatory)]
		[string] $ExportFolderPath
	)

	process {
		$fileInfo = $null
		try {
			$fileInfo = Get-IshDocumentObjData -IshSession $IShSession -IshObject $IshObject -FolderPath $ExportFolderPath
			Write-Host ("Handling file[$($fileInfo.FullName)]...")
			[xml]$xml = Get-Content -Path $fileInfo.FullName
			if ($null -eq $xml) {
				Write-Warning ("Skipping file[$($fileInfo.FullName)] because of bad or empty xml")
				return
			}
			$ishConditions = $xml.SelectNodes('//@ishcondition')
			$invalidCondition = $null
			foreach ($ishCondition in $ishConditions) {
				$result = Test-IshConditionSyntax -Condition $ishCondition.Value
				if (-not $result.IsValid) {
					$invalidCondition = $result
					break
				}
			}
			if ($null -eq $invalidCondition) {
				# Remove the file only when every condition is valid.
				Write-Verbose ("Removing file[$($fileInfo.FullName)]")
				try { Remove-Item -LiteralPath $fileInfo.FullName -Force } catch { Write-Warning ("Could not remove file[$($fileInfo.FullName)] message[$($_.Exception.Message)]") }
			} else {
				Write-Warning ("Keeping file[$($fileInfo.FullName)] condition[$($invalidCondition.Condition)] message[$($invalidCondition.Warning)]")
				Write-Output $IshObject
			}
		} catch {
			Write-Warning ("Skipping file[$($fileInfo.FullName)] message[$($_.Exception.Message)]")
		}
	}
}


$metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FSOURCELANGUAGE -FilterOperator Empty
$ishProblemObjects = Get-IshFolder -IshSession $IShSession -FolderPath $FolderPath -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary")  -Recurse | 
Foreach-Object {
	Write-Host ("Handling folder[$($PSItem.fishfolderpath.Split(", ") -Join $IShSession.FolderPathSeparator)]...")
	Get-IshFolderContent -IshSession $IShSession -IshFolder $PSItem -VersionFilter '' -MetadataFilter $metadataFilter |
		Invoke-IshDocumentObjTransform -IShSession $IShSession -ExportFolderPath $ExportFolderPath
}
$ishProblemObjects | Out-GridView