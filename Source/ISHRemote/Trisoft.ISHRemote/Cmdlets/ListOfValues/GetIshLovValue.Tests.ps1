Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshLovValue"
try {
	
Describe “Get-IshLovValue" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"

	Context “Get-IshLovValue ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshLovValue -IShSession "INVALIDISHSESSION" -LovId $ishLovId -IshLovValue "ISHREMOTE$ishLovId" } | Should Throw
		}
	}

	Context "Get-IshLovValue returns one IshLovValue object" {
		$ishLovValueId = $ishLng
		$ishLovValue = Get-IshLovValue -IShSession $ishSession -LovId $ishLovId -LovValueId $ishLovValueId
		It "GetType().Name" {
			$ishLovValue.GetType().Name | Should BeExactly "IshLovValue"
		}
		It "ishLovValue.IshLovValueRef" {
			$ishLovValue.IshLovValueRef -ge 0 | Should Be $true
		}
		It "ishLovValue.LovId" {
			$ishLovValue.LovId | Should Be "$ishLovId"
		}
		It "ishLovValue.IshRef" {
			$ishLovValue.IshRef | Should be $ishLovValueId
		}
		It "ishLovValue.Label" {
			$ishLovValue.Label -ge 0 | Should Be $true
		}
		It "ishLovValue.Description" {
			$ishLovValue.Description -ge 0 | Should Be $true
		}
		It "ishLovValue.Active" {
			$ishLovValue.Active -ge 0 | Should Be $true
		}
	}

	Context "Get-IshLovValue returns two IshLovValue object" {
		$ishLovValues = Get-IshLovValue -IShSession $ishSession -LovId ($ishLovId,$ishLovId2) -LovValueId ($ishLng,$ishResolution)
		It "Count" {
			$ishLovValues.Count | Should Be 2
		}
	}

	Context "Get-IshLovValue by LovId returns many IshLovValue objects" {
		$ishLovValues = Get-IshLovValue -IShSession $ishSession -LovId $ishLovId 
		It "Count" {
			$ishLovValues.Count -ge 3 | Should Be $true
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
}
