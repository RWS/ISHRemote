Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshUserGroup"
try {

Describe “Add-IshUserGroup" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
	
	Context “Add-IshUserGroup ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Add-IshUserGroup -IShSession "INVALIDISHSESSION" -Name "INVALIDUSERGROUPNAME" } | Should Throw
		}
	}

	Context “Add-IshUserGroup ParameterGroup" {
		It "GetType().Name" {
			$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$ishObject = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName
			$ishObject.GetType().Name | Should BeExactly "IshObject"
			$ishObject.Count | Should Be 1
		}
		It "Parameter Metadata" {
			$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FDESCRIPTION" -Level None -Value "Description of $userGroupName"
			$ishObject = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName -Metadata $metadata
			$ishObject.Count | Should Be 1
			$ishObject.IshRef -Like "VUSER*" | Should Be $true
		}
		It "Parameter Metadata return descriptive metadata" {
			$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FDESCRIPTION" -Level None -Value "Description of $userGroupName"
			$ishObject = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName -Metadata $metadata
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Name FDESCRIPTION -Level None).Length -gt 1 | Should Be $true
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Name FISHUSERGROUPNAME -Level None).Length -gt 1 | Should Be $true
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			$ishObject.fishobjectactive.Length -ge 1 | Should Be $true 
			$ishObject.fishusergroupname.Length -ge 1 | Should Be $true 
			$ishObject.fishusergroupname_none_element.StartsWith('VUSERGROUP') | Should Be $true 
		}
		It "Parameter Metadata StrictMetadataPreference=Off" {
			$strictMetadataPreference = $ishSession.StrictMetadataPreference
			$ishSession.StrictMetadataPreference = "Off"
			$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "CREATED-ON" -Level None -Value "12/03/2017" | 
						Set-IshMetadataField -IshSession $ishSession -Name "MODIFIED-ON" -Level None -Value "12/03/2017" |
						Set-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -Level None -Value "SomethingReadAccess"  |
						Set-IshMetadataField -IshSession $ishSession -Name "OWNER" -Level None -Value "SomethingOwner" |
						Set-IshMetadataField -IshSession $ishSession -Name "INVALIDFIELDNAME" -Level None -Value "SomethingInvalidFieldName"
			{ Add-IshUserGroup -IshSession $ishSession -Name $userGroupName -Metadata $metadata } | Should Throw
			$ishSession.StrictMetadataPreference = $strictMetadataPreference
		}
	}

	Context “Add-IshUserGroup IshObjectsGroup" {
		$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
		$ishObjectA = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName
		$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
		$ishObjectB = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName | 
		              Get-IshUserGroup -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERGROUPNAME")
		$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
		$ishObjectC = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName | 
		              Get-IshUserGroup -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERGROUPNAME")
		$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
		$ishObjectD = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName | 
		              Get-IshUserGroup -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERGROUPNAME")
		$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " E")
		$ishObjectE = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName
		$userGroupName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " F")
		$ishObjectF = Add-IshUserGroup -IshSession $ishSession -Name $userGroupName | 
		              Get-IshUserGroup -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERGROUPNAME")
		Remove-IshUserGroup -IshSession $ishSession -IshObject @($ishObjectA,$ishObjectB,$ishObjectC,$ishObjectD,$ishObjectE,$ishObjectF)
		Start-Sleep -Milliseconds 1000  # Avoids uniquesness error which only up to the second " Cannot insert duplicate key row in object 'dbo.CARD' with unique index 'CARD_NAME_I1'. The duplicate key value is (...A12/10/2016 16:47:16)."
		It "Parameter IshObject invalid" {
			{ Add-IshUserGroup -IShSession $ishSession -IshObject "INVALIDUSERGROUP" } | Should Throw
		}
		It "Parameter IshObject Single with implicit IshSession" {
			$ishObjects = Add-IshUserGroup -IshObject $ishObjectA
			$ishObjects | Remove-IshUserGroup
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple with implicit IshSession" {
			$ishObjects = Add-IshUserGroup -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects | Remove-IshUserGroup
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$ishObjects = $ishObjectD | Add-IshUserGroup -IshSession $ishSession
			$ishObjects | Remove-IshUserGroup -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishObjects = @($ishObjectE,$ishObjectF) | Add-IshUserGroup -IshSession $ishSession
			$ishObjects | Remove-IshUserGroup -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$userGroups = Find-IshUserGroup -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHUSERGROUPNAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshUserGroup -IshSession $ishSession -IshObject $userGroups } catch { }
}
