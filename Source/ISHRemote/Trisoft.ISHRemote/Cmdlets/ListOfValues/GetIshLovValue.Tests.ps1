BeforeAll {
	$cmdletName = "Get-IshLovValue"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}
	
Describe "Get-IshLovValue" -Tags "Create" {
	Context "Get-IshLovValue ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshLovValue -IShSession "INVALIDISHSESSION" -LovId $ishLovId -IshLovValue "ISHREMOTE$ishLovId" } | Should -Throw
		}
	}
	Context "Get-IshLovValue returns one IshLovValue object" {
		BeforeAll {
			$ishLovValueId = $ishLng
			$ishLovValue = Get-IshLovValue -IShSession $ishSession -LovId $ishLovId -LovValueId $ishLovValueId
		}
		It "GetType().Name" {
			$ishLovValue.GetType().Name | Should -BeExactly "IshLovValue"
		}
		It "ishLovValue.IshLovValueRef" {
			$ishLovValue.IshLovValueRef -ge 0 | Should -Be $true
		}
		It "ishLovValue.LovId" {
			$ishLovValue.LovId | Should -Be "$ishLovId"
		}
		It "ishLovValue.IshRef" {
			$ishLovValue.IshRef | Should -Be $ishLovValueId
		}
		It "ishLovValue.Label" {
			$ishLovValue.Label -ge 0 | Should -Be $true
		}
		It "ishLovValue.Description" {
			$ishLovValue.Description -ge 0 | Should -Be $true
		}
		It "ishLovValue.Active" {
			$ishLovValue.Active -ge 0 | Should -Be $true
		}
	}
	Context "Get-IshLovValue returns two IshLovValue object" {
		It "Count" {
			$ishLovValues = Get-IshLovValue -IShSession $ishSession -LovId ($ishLovId,$ishLovId2) -LovValueId ($ishLng,$ishResolution)
			$ishLovValues.Count | Should -Be 2
		}
	}
	Context "Get-IshLovValue by LovId returns many IshLovValue objects" {
		It "Count" {
			$ishLovValues = Get-IshLovValue -IShSession $ishSession -LovId $ishLovId 
			$ishLovValues.Count -ge 3 | Should -Be $true
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}

