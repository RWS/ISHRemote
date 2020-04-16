$DebugPreference="SilentlyContinue"
$VerbosePreference="SilentlyContinue"

# Preferably create a user specifically for this conversion operation, this way a restart is possible
# Provide simple Add-IshUser based on admin user example here





#
# AUXILIARY FUNCTIONS
#
Function Convert-ISHCustomDocumentObj {
<#
.DESCRIPTION
	Generic template Convert cmdlet to handle the incoming pipeline objects to your desire. Best practice is to have a WhatIf implementation to see which objects you are handling in what way.

	This could be a custom implementation to get the file and resubmit it you over (new) IWriteMetadata*-plugin or simply purge pretranslation tags. Or to remove certain language cards.
	
.PARAMETER TestDescription
	Mandatory parameter where you should describe what you are trying to get out of the test. Suggestions are client and source descriptions, hardware changes, network changes, etc

.EXAMPLE
	Provides a valid ISHRemote IshSession. Triggers all available 'MeasureCmdlet' tests available and export them to a CSV file.

	Set-ISHMeasureVariable -Name ([ISHMeasureVariableEnum]::ISHMeasureIshSession) -Value (New-IshSession -WsBaseUrl 'https://example.com/ISHWS/' -IshUserName 'admin' -IshPassword 'admin')
	Invoke-ISHMeasure -TestDescription "All localhost testing" -MeasureType 'All' -CsvFilePath "C:\temp\ISHMeasure.csv"

#>
[cmdletbinding(SupportsShouldProcess=$True)]
param(
	[Parameter(Mandatory=$True,ValueFromPipeline=$True,ParameterSetName='IshObjectsGroup')]
	$IshObject
)
Process
{
    $ishObjectName = ($IshObject.ftitle_logical_value+"="+$IshObject.IshRef+"="+$IshObject.version_version_value+"="+$IshObject.doclanguage)
    $ishObjectStatus = ($IshObject.fstatus_lng_element)
    Write-Host ("Handling object["+$ishObjectName+"] of status["+$ishObjectStatus+"]")
    if (-not $PSCmdlet.ShouldProcess($IshObject))
    {
        Write-Host ("What if: Performing the operation on object["+$ishObjectName+"] of status["+$ishObjectStatus+"]")
        return
    }

    #try {
    #} catch 
    {
        # Write-Error like transactions or checkedout
        # write to log file
    }# at least log with exact error

    #get blob, submit blob #pretranslation or legacy conversion
    #remove-documentobj In Translation/To be Translated

    #Get-IshDocumentObj -IncludeData | Set-IshDocumentObj
}
}


#
# MAIN
#

<# Querying DocumentObjs to find hanging In Translations that were send out for 'Quoting' but never came back
$metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FSTATUS -ValueType Element -FilterOperator In -Value VSTATUSINTRANSLATION
Get-IshFolder -FolderPath "\General\Mobile Phones Demo" -Recurse | 
Get-IshFolderContent -VersionFilter LATEST -LanguagesFilter ('en','de') -MetadataFilter $metadataFilter |
Convert-ISHCustomDocumentObj -WhatIf
#>

<# Querying DocumentObjs to remove target language stubs
$metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FSTATUS -ValueType Element -FilterOperator In -Value 'VSTATUSTOBETRANSLATED, VSTATUSINTRANSLATION' |
                  Set-IshMetadataFilterField -Level Lng -Name FSOURCELANGUAGE -FilterOperator NotEmpty
Get-IshFolder -FolderPath "\General\Mobile Phones Demo" -Recurse | 
Get-IshFolderContent -MetadataFilter $metadataFilter |
Convert-ISHCustomDocumentObj -WhatIf
#>

<# Querying DocumentObjs that are currently checked out
#>

<# Querying DocumentObjs that are changed last week
#>

# Querying DocumentObjs for LatestVersion source language if you have a (new) IWriteMetadata*-plugin to update logical metadata
$metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FSOURCELANGUAGE -FilterOperator Empty |
                  Set-IshMetadataFilterField -Level Lng -Name FISHLASTMODIFIEDBY -FilterOperator My user
Get-IshFolder -FolderPath "\General\Mobile Phones Demo" -Recurse | 
Get-IshFolderContent -VersionFilter LATEST -MetadataFilter $metadataFilter |
Convert-ISHCustomDocumentObj
