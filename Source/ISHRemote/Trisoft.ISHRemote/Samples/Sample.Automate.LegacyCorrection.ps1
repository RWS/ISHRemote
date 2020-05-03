<#
.SYNOPSIS
	For all the content objects after filtering re-submit blob.

.DESCRIPTION
		
	The powershell script executes the following scenario:
	1) Get all folders(where type is "ISHModule", "ISHMasterDoc" or "ISHLibrary") recursively under the Data base folder
    2) Iterate over folders and get objects(where FSTATUS is "To be translated", FISHLASTMODIFIEDBY is not current user and FSOURCELANGUAGE is not empty)
	3) For every object get blob and submit it again
	
	-Log file is provided continuously as the script executes
	-ISHRemote 0.11.0.1 minimal version is required to run this script.
	
	-You can re-run the script and avoid processing of the objects that were already processed. For this task it is important 
		to fill in values in the "User defined variables" section. This example of saving time on re-run is designed to work for InfoshareSTS authentication,
		for ADFS systems you will need to: 
		1) Either temporarily change authentication type using ISHDeploy
		2) Or create a new user and temporarily reassing one of the existing external ids.
	
.PARAMETER WsBaseUrl
	Tridion Docs web services url

.PARAMETER IshUserName
    Tridion Docs user name

.PARAMETER IshPassword
     Tridion Docs user password
#>
Param(
	[Parameter(Mandatory=$true)]
	[string]$WsBaseUrl,
	[Parameter(Mandatory=$false)]
	[string]$IshUserName,
   	[Parameter(Mandatory=$false)]
	[string]$IshPassword
)

Function CreateOrUpdateInternalUser{
<#
.DESCRIPTION
	Create or update internal SDL Tridion Docs user.
	By default all existing user groups are added to the user in "Create" and "Update" operations
#>
param(
	[Parameter(Mandatory=$true)] [Trisoft.ISHRemote.Objects.Public.IshSession]$ishSession,
	[Parameter(Mandatory=$true)] [string]$userName,
	[Parameter(Mandatory=$false)] [string]$userPassword,
	[Parameter(Mandatory=$false)] [string]$userRoles
)
	# Get all user groups and add them to the user
	$ishUserGroups = Find-IshUserGroup -IshSession $ishSession -ActivityFilter "None" -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERGROUPNAME" -Level "None")
	[array]$arrUserGroups = @()
	$ishUserGroups | % {$arrUserGroups += $_.fishusergroupname}
	$userGroups = $arrUserGroups -join ", "

	# Find if user already exists
	$ishUserLovs = Get-IshLovValue -IshSession $ishsession -LovId "USERNAME"
	$foundUserLov = $ishUserLovs | where {[string]$_.Label -eq $userName}
	if($foundUserLov -ne $null)
	{
		Write-Host "Updating user $userName"
		$userMetadataUpdate = Set-IshMetadataField -IshSession $ishsession -Name "NAME" -Level None -Value $userName |
							  Set-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -Level "none" -ValueType "Value" -Value $userGroups 
		if($userPassword -ne "")
		{
			$userMetadataUpdate = $userMetadataUpdate | Set-IshMetadataField -IshSession $ishsession -Name "PASSWORD" -Level None -Value $userPassword
		}
		if($userRoles -ne "")
		{
			$userMetadataUpdate = $userMetadataUpdate | Set-IshMetadataField -IshSession $ishsession -Name "FISHUSERROLES" -Level None -Value $userRoles
		}
		$ishUserUpdate = Set-IshUser -IshSession $ishsession -Id $foundUserLov.IshRef -Metadata $userMetadataUpdate
		return $ishUserUpdate
	}else{
		Write-Host "Creating user $userName"
		
		$userMetadataCreate = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERTYPE" -Level "none" -Value "Internal" `
								| Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERDISPLAYNAME" -Level "none" -Value "$userName" `
								| Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERLANGUAGE" -Level "none" -Value "en" `
								| Set-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -Level "none" -ValueType "Value" -Value $userGroups `
								| Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERROLES" -Level "none" -ValueType "Value" -Value $userRoles `
								| Set-IshMetadataField -IshSession $ishSession -Name "PASSWORD" -Level "none" -Value $userPassword
		
		$ishUserCreate = Add-IshUser -IshSession $ishsession `
									-Name $userName `
									-Metadata $userMetadataCreate
		return $ishUserCreate
	}
}

Function Log-IshCustomDocumentObj{
<#
.DESCRIPTION
	Log information about the provided ishDocumentObj and action taken on it to the file in the .csv format.
#>
param(
	[Parameter(Mandatory=$true,ValueFromPipeline=$true)] [Trisoft.ISHRemote.Objects.Public.IshDocumentObj]$ishDocumentObj,
	[Parameter(Mandatory=$true)] [string]$actionName,
	[Parameter(Mandatory=$true)] [string]$logFileFullPath,
	[Parameter(Mandatory=$false)] [string]$exceptionMessage
)
	$logObject = new-object PSObject
	$logObject | Add-Member -MemberType NoteProperty -Name "Action" -Value $actionName
	$logObject | Add-Member -MemberType NoteProperty -Name "LogicalId" -Value $ishDocumentObj.IshRef
	$logObject | Add-Member -MemberType NoteProperty -Name "Version" -Value $ishDocumentObj.version_version_value
	$logObject | Add-Member -MemberType NoteProperty -Name "Language" -Value $ishDocumentObj.doclanguage
	$logObject | Add-Member -MemberType NoteProperty -Name "Status" -Value $ishDocumentObj.fstatus
	$logObject | Add-Member -MemberType NoteProperty -Name "LastModifiedBy" -Value $ishDocumentObj.fishlastmodifiedby
	$logObject | Add-Member -MemberType NoteProperty -Name "LastModifiedOn" -Value $ishDocumentObj.fishlastmodifiedon
	$logObject | Add-Member -MemberType NoteProperty -Name "Exception" -Value $exceptionMessage
	
	try{
		$logObject | Export-Csv -Path $logFileFullPath -NoTypeInformation -Append -Confirm:$false
	}catch{
		Write-Host "Exception on appending to the log file $logFileFullPath"
        Write-Host $_.Exception.Message
	}
}

Function Convert-ISHCustomDocumentObj{
<#
.DESCRIPTION
	Generic template Convert cmdlet to handle the incoming pipeline objects to your desire. Best practice is to have a WhatIf implementation to see which objects you are handling in what way.

	This could be a custom implementation to get the file and resubmit it you over (new) IWriteMetadata*-plugin or simply purge pretranslation tags. Or to remove certain language cards.
#>
[cmdletbinding(SupportsShouldProcess=$true)]
param(
	[Parameter(Mandatory=$true)] [Trisoft.ISHRemote.Objects.Public.IshSession]
	$ishSession,
	[Parameter(Mandatory=$true,ValueFromPipeline=$true)] 
	[Trisoft.IshRemote.Objects.Public.IshDocumentObj] $ishDocumentObj,
	[Parameter(Mandatory=$true)] 
	[string] $logFileFullPath
)
	$documentDescription = "$($ishDocumentObj.IshRef)=$($ishDocumentObj.version_version_value)=$($ishDocumentObj.doclanguage)"
	Write-Host "$documentDescription"
	
	# additional requested metadata for the retrieved document objects
	$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHLASTMODIFIEDBY" -Level Lng -ValueType Value |
						 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHLASTMODIFIEDON" -Level Lng -ValueType Value |
						 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Value
	try{
		$ishDocumentObj = $ishDocumentObj | Get-IshDocumentObj -IshSession $ishSession -RequestedMetadata $requestedMetadata -IncludeData
		if ($PSCmdlet.ShouldProcess("$documentDescription", $MyInvocation.MyCommand) -eq $true)
    	{
			$ishDocumentObjUpdated = $ishDocumentObj | Set-IshDocumentObj -IshSession $ishSession -Confirm:$false
		}
		$ishDocumentObjUpdated | Log-IshCustomDocumentObj -actionName $MyInvocation.MyCommand `
														  -logFileFullPath $logFileFullPath												
	}
	catch{
		Write-Host "Exception"
	    Write-Host "========="
	    Write-Host $_.Exception.Message
	    Write-Host "========="
		$ishDocumentObj | Log-IshCustomDocumentObj -actionName $MyInvocation.MyCommand `
												 -logFileFullPath $logFileFullPath `
												 -exceptionMessage $_.Exception.Message
	}
}

CLS
Import-Module ISHRemote -MinimumVersion 0.11.0.1 -DisableNameChecking
# Create session
$ishSession = New-IshSession -wsBaseUrl $WsBaseUrl -IshUserName $IshUserName -IshPassword $IshPassword -IgnoreSslPolicyErrors

#region User defined variables
$folderTypeFilter = @("ISHModule", "ISHMasterDoc", "ISHLibrary")
$documentObjectStatusFilter = "To be translated"

# For InfoshareSTS authentication create a new internal dedicated user
$dedicatedIshUserName = "SDLDocsConversionUser"
$dedicatedIshUserPassword = "SDLDocsConversionUser"
$dedicatedIshUserRoles = "Reviewer, Author, Translator"
#endregion

#log file location and timestamp
$logFileLocation = Split-Path -parent $MyInvocation.MyCommand.Definition
$logTimestamp = get-date -Format "yyyyMMddHHmmss"
$logFileFullPath = "$logFileLocation\$($logTimestamp)ObjUpdateLog.csv"
$logFile = New-Item -Path $logFileFullPath -ItemType File -Force

# Get all folders
$ishFolders = Get-IshFolder -IshSession $ishSession -BaseFolder Data -Recurse -FolderTypeFilter $folderTypeFilter

if($ishFolders.Count -eq 0)
{
	Write-Host "No folders matching filter were found."
	Exit
}
else{
	Write-Host "Found" $ishFolders.count "folders of type: $folderTypeFilter"
}

# Filtering on content objects (Get-IshFolderContent)
$metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name "FSTATUS" -Level Lng -Value $documentObjectStatusFilter -ValueType Value |
				  Set-IshMetadataFilterField -IshSession $ishSession -Name "FSOURCELANGUAGE" -Level Lng -ValueType Value -FilterOperator NotEmpty

#region Additional filtering to save time when re-running the script
# typically means InfoshareSTS authentication, ISHRemote can create user and log in
# Create dedicated user
$dedicatedIshUser = CreateOrUpdateInternalUser -ishSession $ishSession -userName $dedicatedIshUserName -userPassword $dedicatedIshUserPassword -userRoles $dedicatedIshUserRoles
if($dedicatedIshUser -eq $null)
{
	Write-Host "Error on Create/Update of the dedicated SDL Tridion Docs user" 
	Exit
}

# Create session for the dedicated user
$ishSession = New-IshSession -wsBaseUrl $WsBaseUrl -IshUserName $dedicatedIshUserName -IshPassword $dedicatedIshUserPassword -IgnoreSslPolicyErrors

# additional filtering on LastModifiedBy
$metadataFilter = $metadataFilter | Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHLASTMODIFIEDBY" -Level Lng -Value $dedicatedIshUserName -ValueType Value -FilterOperator NotEqual
#endregion

# Limit the number of objects retrieved per one web service call 
$ishSession.MetadataBatchSize = 100
# Decrease number of default metadata fields requested
$ishSession.DefaultRequestedMetadata = "Descriptive"

# Loop over folders
foreach($ishFolder in $ishFolders)
{
	$ishObjects = Get-IshFolderContent -IshSession $ishSession `
									   -IshFolder $ishFolder `
									   -MetadataFilter $metadataFilter
	
	$folderPath = ($ishFolder.fishfolderpath).Replace($ishSession.Separator, $ishSession.FolderPathSeparator) + $ishSession.FolderPathSeparator + $ishFolder.fname
	if($ishObjects.Count -eq 0)
	{
		Write-Host "$folderPath : no objects returned"
		continue;
	}
	
	Write-Host "$folderPath : $($ishObjects.Count) object(s)"
	foreach($ishObject in $ishObjects)
	{
		$ishObject | Convert-ISHCustomDocumentObj -ishSession $ishSession -logFileFullPath $logFileFullPath
		#$ishObject | Convert-ISHCustomDocumentObj -ishSession $ishSession -logFileFullPath $logFileFullPath -WhatIf
		#$ishObject | Convert-ISHCustomDocumentObj -ishSession $ishSession -logFileFullPath $logFileFullPath -Confirm
	}
}

Remove-Module ISHRemote
Write-Host "End.`r`nLog file location: $logFileFullPath"
