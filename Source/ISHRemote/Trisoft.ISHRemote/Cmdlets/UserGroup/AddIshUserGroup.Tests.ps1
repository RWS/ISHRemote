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
		It "Parameter IshObject Single" {
			$ishObjects = Add-IshUserGroup -IshSession $ishSession -IshObject $ishObjectA
			$ishObjects | Remove-IshUserGroup -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple" {
			$ishObjects = Add-IshUserGroup -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects | Remove-IshUserGroup -IshSession $ishSession
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
