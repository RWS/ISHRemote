Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshUserRole"
try {

Describe "Add-IshUserRole" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
	
	Context "Add-IshUserRole ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Add-IshUserRole -IShSession "INVALIDISHSESSION" -Name "INVALIDUSERROLENAME" } | Should Throw
		}
	}

	Context "Add-IshUserRole ParameterGroup" {
		It "GetType().Name" {
			$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$ishObject = Add-IshUserRole -IshSession $ishSession -Name $userRoleName
			$ishObject.GetType().Name | Should BeExactly "IshObject"
			$ishObject.Count | Should Be 1
		}
		It "Parameter Metadata" {
			$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FDESCRIPTION" -Level None -Value "Description of $userRoleName"
			$ishObject = Add-IshUserRole -IshSession $ishSession -Name $userRoleName -Metadata $metadata
			$ishObject.Count | Should Be 1
			$ishObject.IshRef -Like "VUSER*" | Should Be $true
		}
	}

	Context "Add-IshUserRole IshObjectsGroup" {
		$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
		$ishObjectA = Add-IshUserRole -IshSession $ishSession -Name $userRoleName
		$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
		$ishObjectB = Add-IshUserRole -IshSession $ishSession -Name $userRoleName | 
		              Get-IshUserRole -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERROLENAME")
		$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
		$ishObjectC = Add-IshUserRole -IshSession $ishSession -Name $userRoleName
		$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
		$ishObjectD = Add-IshUserRole -IshSession $ishSession -Name $userRoleName | 
		              Get-IshUserRole -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERROLENAME")
		$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " E")
		$ishObjectE = Add-IshUserRole -IshSession $ishSession -Name $userRoleName
		$userRoleName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " F")
		$ishObjectF = Add-IshUserRole -IshSession $ishSession -Name $userRoleName | 
		              Get-IshUserRole -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERROLENAME")
		Remove-IshUserRole -IshSession $ishSession -IshObject @($ishObjectA,$ishObjectB,$ishObjectC,$ishObjectD,$ishObjectE,$ishObjectF)
		Start-Sleep -Milliseconds 1000  # Avoids uniquesness error which only up to the second " Cannot insert duplicate key row in object 'dbo.CARD' with unique index 'CARD_NAME_I1'. The duplicate key value is (VUSERROLEADD-ISHUSERROLE20161012164716068A12/10/2016 16:47:16)."
		It "Parameter IshObject invalid" {
			{ Add-IshUserRole -IShSession $ishSession -IshObject "INVALIDUSERROLE" } | Should Throw
		}
		It "Parameter IshObject Single" {
			$ishObjects = Add-IshUserRole -IshSession $ishSession -IshObject $ishObjectA
			$ishObjects | Remove-IshUserRole -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple" {
			$ishObjects = Add-IshUserRole -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects | Remove-IshUserRole -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$ishObjects = $ishObjectD | Add-IshUserRole -IshSession $ishSession
			$ishObjects | Remove-IshUserRole -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishObjects = @($ishObjectE,$ishObjectF) | Add-IshUserRole -IshSession $ishSession
			$ishObjects | Remove-IshUserRole -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$userRoles = Find-IshUserRole -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHUSERROLENAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshUserRole -IshSession $ishSession -IshObject $userRoles } catch { }
}
