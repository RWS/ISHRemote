Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Set-IshRequestedMetadataField"
try {

Describe “Set-IshRequestedMetadataField" -Tags "Read" {
	Write-Host "Initializing Test Data and Variables"

	Context "Set-IshRequestedMetadataField -IshSession $ishSession returns IshRequestedMetadataField" {
		It "GetType().Name" {
			(Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE").GetType().Name | Should BeExactly "IshRequestedMetadataField"
		}
	}

	Context “Set-IshRequestedMetadataField" {
		It "Parameter IshSession/Name/Level invalid" {
			{ Set-IshRequestedMetadataField -IshSession "INVALIDISHSESSION" -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" } | Should Throw
		}
		It "Parameter ValueType invalid" {
			{ Set-IshRequestedMetadataField -IshSession $ishSession -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" -ValueType "INVALIDFIELDVALUETYPE" } | Should Throw
		}
		It "Parameter Name" {
			(Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE").Length | Should Be 1
		}
		It "Parameter Name Level" {
			(Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical).Length | Should Be 1
		}
		It "Parameter Name Level ValueType" {
			(Set-IshRequestedMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element).Length | Should Be 1
		}
		It "Parameter Name Level ValueType Overwrite" {
			(Set-IshRequestedMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element | 
			 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element).Length | Should Be 1
		}
		It "Parameter Name Level ValueType Twice" {
			(Set-IshRequestedMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Value | 
			 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Id | 
			 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element).Length | Should Be 3
		}
		It "Parameter IshFields Single Add" {
			$ishFields = $null
			(Set-IshRequestedMetadataField -IshSession $ishSession -IshField $ishFields -Name "FAUTHOR" -Level Lng -ValueType Element).Length | Should Be 1
		}
		It "Parameter IshFields Single Update" {
			$ishFields = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element
			(Set-IshRequestedMetadataField -IshSession $ishSession -IshField $ishFields -Name "FTITLE" -Level Logical -ValueType Element).Length | Should Be 1
		}
		It "Parameter IshFields Multiple duplicate entries will be removed" {
			$ishFieldA =Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element
			$ishFieldB =Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element
			(Set-IshRequestedMetadataField -IshSession $ishSession -IshField @($ishFieldA,$ishFieldB) -Name "FTITLE" -Level Logical -ValueType Element).Length | Should Be 1
		}
		It "Pipeline IshFields Single" {
			$ishFields = $null
			($ishFields | Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element).Length | Should Be 1
		}
		It "Pipeline IshFields Multiple duplicate entries will be removed" {
			$ishFieldA =Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element
			$ishFieldB =Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element
			(@($ishFieldA,$ishFieldB) | Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element).Length | Should Be 1
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
}
