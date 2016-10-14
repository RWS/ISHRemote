Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshVersion"
try {
	Write-Host "Initializing Test Data and Variables"

Describe “Get-IshVersion" -Tags "Read" {
	Context “Get-IshVersion Parameters" {
		It "Parameter IshSession invalid" {
			{ Get-IshVersion -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Get-IshVersion returns IshVersion object" {
		$ishVersion = Get-IshVersion -IShSession $ishSession
		It "GetType()" {
			$ishVersion.GetType().Name | Should BeExactly "IshVersion"
		}
		It "IshVersion.MajorVersion" {
			$ishVersion.MajorVersion -ge 0 | Should Be $true
		}
		It "IshVersion.MinorVersion" {
			$ishVersion.MinorVersion -ge 0 | Should Be $true
		}
		It "IshVersion.BuildVersion" {
			$ishVersion.BuildVersion -ge 0 | Should Be $true
		}
		It "IshSession.RevisionVersion" {
			$ishVersion.RevisionVersion -ge 0 | Should Be $true
		}
	}
}

	
} finally {
	Write-Host "Cleaning Test Data and Variables"
}