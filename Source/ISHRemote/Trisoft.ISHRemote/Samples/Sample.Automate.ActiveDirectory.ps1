# http://technet.microsoft.com/en-us/library/dd378937(WS.10).aspx Active Directory Administration with Windows PowerShell
# https://technet.microsoft.com/en-us/library/ee617195.aspx Active Directory Cmdlets in Windows PowerShell
#
# EXTRACT FROM TRISOFT MANUALS: Provisioning users through the API
# ----------------------------------------------------------------
# The API allows you CRUD and Disabling of user profiles. The following algorithm can guide you in sync'ing your user systems. 
# Delete or Disable Trisoft User Profiles that no longer exist in the central system. 
#   List all Trisoft user profiles that have FISHUSERTYPE set to External and FISHUSERDISABLED set to No 
#   For every user in the trisoft-user-list find the external user profile by FISHEXTERNALID 
#     If none exists, delete the Trisoft user profile if not referenced otherwise disable the Trisoft user profile 
#     If one or more exists; check if disabled, possibly disable the Trisoft user profile 
# Create or Update Trisoft User Profiles in the Trisoft system. 
#   List all external users required to have a matching profile in Trisoft (e.g. limited by LDAP role,…) 
#   For every user in the external-user-list find the Trisoft User Profile by FISHEXTERNALID 
#     If multiple hits; throw exception as multiple profile hits will never grant a login 
#     If none exists; create the user profile with required roles and user groups 
#     If one exists; enable, skip or possibly update the user profile 
#     CAUTION: Beware that update could overwrite explicitly set values. 
#

CLS
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

### AUX FUNCTIONS BEGIN ###
# Is this a 64 bit process
function ProcessArchitecture {
	if ([IntPtr]::size -eq 8) {
		return "x64"
	}
	elseif ([IntPtr]::size -eq 4) {
		return "x86"
	}
	else {
		return "unknown"
	}
}
### AUX FUNCTIONS END ###

try
{
	Write-Host ("`r`nStarting on architecture[" + (ProcessArchitecture) + "]...")
	Write-Host "`r`nCreate a IshSession through Login..."
    $ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -TrisoftUserName $trisoftUserName -TrisoftPassword $trisoftPassword
	$ishSession

	$domain = "exampledomain"
	$groupToHaveForTrisoft = "exampledomaingroup"

	##########################################################
	# [A] Disable/Deactivate Trisoft User Profiles that no longer exist in the central system. 
	#   List all Trisoft user profiles that have FISHUSERTYPE set to External and FISHUSERDISABLED set to No 
	#   For every user in the trisoft-user-list find the external user profile by FISHEXTERNALID 
	#     If none exists, disable the Trisoft user profile 
	#     If one or more exists; check if disabled, disable/deactivate the Trisoft user profile 
	##########################################################
	
	Write-Host "`r`n[A] Disable/Deactivate Trisoft User Profiles that no longer exist in the central system."
	Write-Host "Listing all Trisoft user profiles that have FISHUSERTYPE set to External and FISHUSERDISABLED set to No "
	$ishMetadataFilterFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHUSERTYPE" -Level "None" -ValueType "Element" -Value "VUSERTYPEEXTERNAL" | `
	                   Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHUSERDISABLED" -Level "None" -ValueType "Element" -Value "FALSE" | `
					   Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHEXTERNALID" -Level "None" -FilterOperator "Like" -Value "$domain\%"
	$ishMetadataFields = Set-IshMetadataField -IshSession $ishSession -Name "FISHEXTERNALID" -Level "none" 
	$ishobjectsFind = Find-IshUser -IshSession $ishSession -ActivityFilter "None" -MetadataFilter $ishMetadataFilterFields -RequestedMetadata $ishMetadataFields
	foreach ($ishobject in $ishobjectsFind)
	{
		$externalId = $ishobject.IshField.RetrieveFirst("FISHEXTERNALID", "None").Value
		Write-Host "Verifying if $externalId is disabled..."
		$externalUserArray = $externalId.Split('\');
		$accountName = $externalUserArray[$externalUserArray.Length-1]
		$adUser = Get-ADUser -Filter {(Enabled -eq "True") -and (SamAccountName -eq $accountName)}
		if ($adUser -ne $null)
		{
			# Enabled domain user found
			Write-Host "Trisoft user identified by $externalId is still valid"
		}
		else
		{
			# No enabled domain user found, disabling Trisoft user
			Write-Host "Disabling and Deactivating Trisoft user identified by $externalId"
			$ishMetadataFieldsAction = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERDISABLED" -Level "none" -ValueType "Element" -Value "TRUE" `
			                      | Set-IshMetadataField -IshSession $ishSession -Name "FISHOBJECTACTIVE" -Level "none" -ValueType "Element" -Value "FALSE"
			$ishobject = Write-Output $ishobject `
			           | Set-IshField -IshSession $ishSession -MergeFields $ishMetadataFieldsAction -ValueAction "Overwrite" `
			           | Set-IshUser -IshSession $ishSession 
		}
	}
	
	##########################################################
	# [B] Create or Update Trisoft User Profiles in the Trisoft system.
	#   List all external users required to have a matching profile in Trisoft (e.g. limited by group) 
	#   For every user in the external-user-list find the Trisoft User Profile by FISHEXTERNALID 
	#     If multiple hits; throw exception as multiple profile hits will never grant a login 
	#     If none exists; create the user profile with required roles and user groups 
	#     If one exists; enable, skip or possibly update the user profile 
	#     CAUTION: Beware that update could overwrite explicitly set values. 
	##########################################################

	Write-Host "`r`n[B] Create or Update User Profiles in Trisoft"
	Write-Host "Listing all enabled users part of $groupToHaveForTrisoft ..."
	$usersForTrisoft = Get-ADGroupMember -Identity $groupToHaveForTrisoft
	foreach ($userForTrisoft in $usersForTrisoft)
	{
		$userDetail = Get-ADUser -Identity ($userForTrisoft).SID -Properties mail

		$enabled = ($userDetail).Enabled
		if ($enabled -eq "True")
		{
			$externalId = $domain + "\" + ($userDetail).SamAccountName
			$externalName = ($userDetail).name
			$externalMail = ($userDetail).mail

			Write-Host "Verifying if $externalId is already part of a Trisoft Profile..."
			$ishMetadataFilterFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHEXTERNALID" -Level "None" -Value "$externalId"
			$ishobjectsFind = Find-IshUser -IshSession $ishSession -ActivityFilter "None" -MetadataFilter $ishMetadataFilterFields
			if ($ishobjectsFind)
			{
				# Trisoft Profile exists
				Write-Host "Trisoft user identified by $externalId will not be updated"
			}
			else
			{
				# Create Trisoft Profile
				Write-Host "Creating Trisoft user identified by $externalId..."
				$ishMetadataFieldsAdd = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERTYPE" -Level "none" -Value "External" `
								   | Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERDISPLAYNAME" -Level "none" -Value "$externalName" `
								   | Set-IshMetadataField -IshSession $ishSession -Name "FISHEXTERNALID" -Level "none" -Value $externalId `
								   | Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERLANGUAGE" -Level "none" -Value "en" `
								   | Set-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -Level "none" -ValueType "Element" -Value "VUSERGROUPDEFAULTDEPARTMENT" `
								   | Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERROLES" -Level "none" -ValueType "Element" -Value "VUSERROLEREVIEWER" `
								   | Set-IshMetadataField -IshSession $ishSession -Name "FISHEMAIL" -Level "none" -Value $externalMail

				$ishobject = Add-IshUser -IshSession $ishSession -Name "$externalName ($externalMail)" -Metadata $ishMetadataFieldsAdd
			}
		}
	}
	
	Write-Host "`r`nUsers with $domain ExternalId:"
	$ishMetadataFilterFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHUSERTYPE" -Level "None" -FilterOperator "In" -Value "Internal, External" `
	                 | Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHEXTERNALID" -Level "None" -FilterOperator "Like" -Value "$domain%"
	$ishobjectsFind = Find-IshUser -IshSession $ishSession -ActivityFilter "None" -MetadataFilter $ishMetadataFilterFields
	$ishobjectsFind | Out-String
}
catch
{
	Write-Host "`r`nException"
    Write-Host "========="
    $Error[0].Exception.Message
    Write-Host "========="
}
finally
{
    Write-Host "`r`nRemove-Module ISHRemote..."
	Remove-Module ISHRemote
}