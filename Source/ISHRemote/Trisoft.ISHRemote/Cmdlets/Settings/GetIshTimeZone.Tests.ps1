BeforeAll {
	$cmdletName = "Get-IshTimeZone"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshTimeZone" -Tags "Read" {
	Context "Get-IshTimeZone Parameters" {
		It "Parameter IshSession invalid" {
			{ Get-IshTimeZone -IShSession "INVALIDISHSESSION" -Count 2 } | Should-Throw
		}
		It "Parameter Count invalid" {
			{ Get-IshTimeZone -IShSession $ishSession -Count "INVALIDCOUNT" } | Should-Throw
		}
	}
	Context "Get-IshTimeZone returns IshApplicationSetting (single) object" {
		BeforeAll {
			$ishApplicationSetting = Get-IshTimeZone -IShSession $ishSession
		}
		It "GetType()" {
			$ishApplicationSetting.GetType().Name | Should-BeString -CaseSensitive "IshApplicationSetting"
		}
		It "IshApplicationSetting.TimeElapsedDbServer" {
			$ishApplicationSetting.TimeElapsedDbServer -ge 0 | Should-NotBeNull
		}
		It "IshApplicationSetting.TimeElapsedAppServer" {
			$ishApplicationSetting.TimeElapsedAppServer -ge 0 | Should-NotBeNull
		}
		It "IshApplicationSetting.TimeElapsedWsCall" {
			$ishApplicationSetting.TimeElapsedWsCall -ge 0 | Should-NotBeNull
		}
		It "IshApplicationSetting.TimeZoneDisplayName" {
			$ishApplicationSetting.TimeZoneDisplayName.Length -ge 0 | Should-NotBeNull
		}
		It "IshApplicationSetting.TimeZoneUtcOffset" {
			$ishApplicationSetting.TimeZoneUtcOffset -ge 0 | Should-NotBeNull
		}
		It "IshApplicationSetting.TimeZoneIsdaylightsavingtime" {
			{ $ishApplicationSetting.TimeZoneIsdaylightsavingtime } | Should -Not -Throw
		}
		It "IshApplicationSetting.AppServerComputerName" {
			$ishApplicationSetting.AppServerComputerName.Length -ge 0 | Should-NotBeNull
		}
	}
	Context "Get-IshTimeZone returns IshApplicationSettings (plural) object" {
		It "Parameter IshSession implicit" {
			$ishApplicationSettings = Get-IshTimeZone -IShSession $ishSession -Count 2
			$ishApplicationSettings.GetType().Name | Should-BeString -CaseSensitive "IshApplicationSettings"
			$ishApplicationSettings.TimeZoneId -ge 0 | Should-NotBeNull
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}

