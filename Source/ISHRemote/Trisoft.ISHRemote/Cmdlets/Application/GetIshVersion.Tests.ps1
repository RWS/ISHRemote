BeforeAll {
	$cmdletName = "Get-IshVersion"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshVersion" -Tags "Read" {
	Context "Get-IshVersion Parameters" {
		It "Parameter IshSession invalid" {
			{ Get-IshVersion -IshSession "INVALIDISHSESSION" } | Should -Throw
		}
	}

	Context "Get-IshVersion returns IshVersion object" {
		BeforeAll {
			$ishVersion = Get-IshVersion -IshSession $ishSession
		}
		It "GetType()" {
			$ishVersion.GetType().Name | Should -BeExactly "IshVersion"
		}
		It "IshVersion.MajorVersion" {
			$ishVersion.MajorVersion -ge 0 | Should -Be $true
		}
		It "IshVersion.MinorVersion" {
			$ishVersion.MinorVersion -ge 0 | Should -Be $true
		}
		It "IshVersion.BuildVersion" {
			$ishVersion.BuildVersion -ge 0 | Should -Be $true
		}
		It "IshSession.RevisionVersion" {
			$ishVersion.RevisionVersion -ge 0 | Should -Be $true
		}
	}

	Context "Get-IshVersion without IshSession returns IshVersion object" {
		BeforeAll {
			$ishVersion = Get-IshVersion
		}
		It "GetType()" {
			$ishVersion.GetType().Name | Should -BeExactly "IshVersion"
		}
		It "IshVersion.MajorVersion" {
			$ishVersion.MajorVersion -ge 0 | Should -Be $true
		}
		It "IshVersion.MinorVersion" {
			$ishVersion.MinorVersion -ge 0 | Should -Be $true
		}
		It "IshVersion.BuildVersion" {
			$ishVersion.BuildVersion -ge 0 | Should -Be $true
		}
		It "IshSession.RevisionVersion" {
			$ishVersion.RevisionVersion -ge 0 | Should -Be $true
		}
	}
}
	
AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}
