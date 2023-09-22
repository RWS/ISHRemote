BeforeAll {
	$cmdletName = "Set-IshMetadataField"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Set-IshMetadataField" -Tags "Read" {
	BeforeAll {
		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FNAME" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "FDOCUMENTTYPE" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element
		$ishFolderDataOriginal = Get-IshFolder -IShSession $ishSession -BaseFolder Data -RequestedMetadata $requestedMetadata
	}
	Context "Set-IshMetadataField returns IshMetadataField" {
		It "GetType().Name" {
			(Set-IshMetadataField -IshSession $ishSession -Name "FTITLE").GetType().Name | Should -BeExactly "IshMetadataField"
		}
		It "Parameter IshSession/Name/Level invalid" {
			{ Set-IshMetadataField -IshSession "INVALIDISHSESSION" -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" } | Should -Throw
		}
		It "Parameter ValueType invalid" {
			{ Set-IshMetadataField -IshSession $ishSession -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" -ValueType "INVALIDFIELDVALUETYPE" } | Should -Throw
		}
		It "Parameter Name" {
			Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" | Should -Be ""
		}
		It "Parameter Name Level" {
			Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical | Should -Be ""
		}
		It "Parameter Name Level Value" {
			$value = "MyString"
			(Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical) -eq $value | Should -Be $true
		}
		It "Parameter Name Level ValueType Value" {
			$value = "MYSTRING"
			(Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element) -eq $value | Should -Be $true
		}
		It "Parameter Name Level ValueType Value Overwrite" {
			$value = "MYSTRING"
			(Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value "ValueThatWillBeOverwritten" | 
			 Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value).Length | Should -Be 1
		}
		It "Parameter IshFields Single Add" {
			$ishFields = $null
			$value = "SingleString"
			(Set-IshMetadataField -IshSession $ishSession -IshField $ishFields -Name "FTITLE" -Level Logical -ValueType Element -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element) -eq $value | Should -Be $true
		}
		It "Parameter IshFields Single Update" {
			$value = "OrginalString"
			$ishFields = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$value = "NewString"
			(Set-IshMetadataField -IshSession $ishSession -IshField $ishFields -Name "FTITLE" -Level Logical -ValueType Element -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element) -eq $value | Should -Be $true
		}
		It "Parameter IshField Multiple duplicate entries will be removed" {
			$value = "OrginalString"
			$ishFieldA =Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$ishFieldB =Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$value = "SingleString"
			(Set-IshMetadataField -IshSession $ishSession -IshField @($ishFieldA,$ishFieldB) -Name "FTITLE" -Level Logical -ValueType Element -value $value).Length | Should -Be 1
		}
		It "Pipeline IshFields Single with implicit IshSession" {
			$value = "OrginalString"
			$ishFieldA =Set-IshMetadataField -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$value = "SingleString"
			($ishFieldA | Set-IshMetadataField -Name "FTITLE" -Level Logical -ValueType Element -value $value | Get-IshMetadataField -Name "FTITLE" -Level Logical -ValueType Element) -eq $value | Should -Be $true
		}
		It "Pipeline IshFields Multiple with implicit IshSession" {
			$value = "OrginalString"
			$ishFieldA =Set-IshMetadataField -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$ishFieldB =Set-IshMetadataField -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$value = "SingleString"
			(@($ishFieldA,$ishFieldB) | Set-IshMetadataField -Name "FTITLE" -Level Logical -ValueType Element -value $value).Length | Should -Be 1
		}
	}
	Context "Set-IshMetadataField returns IshFolder" {
		It "GetType().Name" {
			(Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshFolder $ishFolderDataOriginal).GetType().Name | Should -BeExactly "IshFolder"
		}
		It "Parameter Name Level ValueType Value" {
			$value = "MYSTRING"
			($ishFolderDataOriginal |
			 Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -ValueType Element -value $value |
			 Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -ValueType Element) -eq $value | Should -Be $true
		}
	}
	Context "Alias Set-IshRequiredCurrentMetadataField" {
		It "Parameter IshFields Single" {
			$ishFields = $null
			$value = "SingleString"
			(Set-IshRequiredCurrentMetadataField -IshSession $ishSession -IshField $ishFields -Name "FTITLE" -Level Logical -ValueType Element -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element) -eq $value | Should -Be $true
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}

