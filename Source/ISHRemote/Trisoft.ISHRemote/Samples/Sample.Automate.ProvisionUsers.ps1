Write-Host "`r`nImport-Module ISHRemote..."
Import-Module ISHRemote -DisableNameChecking 

Write-Host "`r`nSetting preferences..."
$DebugPreference   = "SilentlyContinue"   # Continue or SilentlyContinue
$VerbosePreference = "SilentlyContinue"   # Continue or SilentlyContinue
$WarningPreference = "Continue"   # Continue or SilentlyContinue or Stop
$ProgressPreference= "Continue"   # Continue or SilentlyContinue

Write-Host "`r`nSetting variables..."
$webServicesBaseUrl = 'https://example.com/InfoShareWS/'
$trisoftUserName = ''
$trisoftPassword = ''
$ishSession = ''

Write-Host "`r`nHere we go..."
try
{
	Write-Host "`r`nStarting..."
	Write-Host "`r`nCreate a IshSession through Login..."
    $ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -TrisoftUserName $trisoftUserName -TrisoftPassword $trisoftPassword
	$ishSession

	# Domain / User to provision
	$domain = "exampledomain"
	$usersToCreate = @("user1", "user2", "user3", "user4", "user5")
	$emailDomain = "@example.com"

	Write-Host "`r`nRetrieving all user roles"
	$userRoleIshMetadataFields = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERROLENAME" -Level "none" -ValueType "Element" 
	$userRoleIshObjects = Find-IshUserRole -IshSession $ishSession -ActivityFilter "None" -RequestedMetadata $userRoleIshMetadataFields
	foreach ($userRoleIshObject in $userRoleIshObjects)
	{
		$userRoles = $userRoles + $ishSession.Seperator + $userRoleIshObject.IshField.RetrieveFirst("FISHUSERROLENAME", "None", "Element").Value
	}
	$userRoles = $userRoles.TrimStart(", ")
	$userRoles

	Write-Host "`r`nRetrieving all user groups"
	$userGroupIshMetadataFields = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERGROUPNAME" -Level "none" -ValueType "Element" 
	$userGroupIshObjects = Find-IshUserGroup -IshSession $ishSession -ActivityFilter "None" -RequestedMetadata $userGroupIshMetadataFields
	foreach ($userGroupIshObject in $userGroupIshObjects)
	{
		$userGroups = $userGroups + $ishSession.Seperator + $userGroupIshObject.IshField.RetrieveFirst("FISHUSERGROUPNAME", "None", "Element").Value
	}
	$userGroups = $userGroups.TrimStart(", ")
	$userGroups

	Write-Host "`r`nIf user exists, then update, otherwise create"
	foreach ($user in $usersToCreate)
	{
		$userLowerCase = ("$domain".ToLower()) + "\$user"
		$userUpperCase = ("$domain".ToUpper()) + "\$user"
		Write-Host "Finding $userLowerCase or $userUpperCase"
		$userIshMetadataFilterFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -Level "None" -FilterOperator "like" -Value "$userLowerCase"
		$userIshObjectsLowerCase = Find-IshUser -IshSession $ishSession -ActivityFilter "None" -MetadataFilter $userIshMetadataFilterFields
		$userIshMetadataFilterFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -Level "None" -FilterOperator "like" -Value "$userUpperCase"
		$userIshObjectsUpperCase = Find-IshUser -IshSession $ishSession -ActivityFilter "None" -MetadataFilter $userIshMetadataFilterFields
		
		if(($userIshObjectsLowerCase.Length -eq 0) -and ($userIshObjectsUpperCase.Length -eq 0))
		{
			Write-Host "Adding '$user'"
			$ishMetadataFieldsAdd = Set-IshMetadataField -IshSession $ishSession -Name "FISHEMAIL" -Level "none" -Value "$user$emailDomain" `
			                   | Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERTYPE" -Level "none" -Value "External" `
							   | Set-IshMetadataField -IshSession $ishSession -Name "FISHEXTERNALID" -Level "none" -Value "$domain\$user" `
							   | Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERLANGUAGE" -Level "none" -Value "en" `
							   | Set-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -Level "none" -Value "$userGroups" -ValueType "Element" `
							   | Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERROLES" -Level "none" -Value "$userRoles" -ValueType "Element"
			$ishobjectsAdd = Add-IshUser -IshSession $ishSession -Name "$domain\$user" -Metadata $ishMetadataFieldsAdd
		}
		else
		{
			Write-Host "Skipping '$user'"
		}
	}
}
catch
{
	Write-Host "`r`nException"
    Write-Host "========="
    $Error[0].Exception.Message # $_.Message;
    Write-Host "========="
}
finally
{
    Write-Host "`r`nRemove-Module ISHRemote..."
	Remove-Module ISHRemote
}