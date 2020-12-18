Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshTimeZone"
try {

Describe “Get-IshTimeZone" -Tags "Read" {
	Context “Get-IshTimeZone Parameters" {
		It "Parameter IshSession invalid" {
			{ Get-IshTimeZone -IShSession "INVALIDISHSESSION" -Count 2 } | Should Throw
		}
		It "Parameter Count invalid" {
			{ Get-IshTimeZone -IShSession $ishSession -Count "INVALIDCOUNT" } | Should Throw
		}
	}

	Context "Get-IshTimeZone returns IshApplicationSetting (single) object" {
		$ishApplicationSetting = Get-IshTimeZone -IShSession $ishSession
		It "GetType()" {
			$ishApplicationSetting.GetType().Name | Should BeExactly "IshApplicationSetting"
		}
		It "IshApplicationSetting.TimeElapsedDbServer" {
			$ishApplicationSetting.TimeElapsedDbServer -ge 0 | Should Not BeNullOrEmpty
		}
		It "IshApplicationSetting.TimeElapsedAppServer" {
			$ishApplicationSetting.TimeElapsedAppServer -ge 0 | Should Not BeNullOrEmpty
		}
		It "IshApplicationSetting.TimeElapsedWsCall" {
			$ishApplicationSetting.TimeElapsedWsCall -ge 0 | Should Not BeNullOrEmpty
		}
		It "IshApplicationSetting.TimeZoneDisplayName" {
			$ishApplicationSetting.TimeZoneDisplayName.Length -ge 0 | Should Not BeNullOrEmpty
		}
		It "IshApplicationSetting.TimeZoneUtcOffset" {
			$ishApplicationSetting.TimeZoneUtcOffset -ge 0 | Should Not BeNullOrEmpty
		}
		It "IshApplicationSetting.TimeZoneIsdaylightsavingtime" {
			{ $ishApplicationSetting.TimeZoneIsdaylightsavingtime } | Should Not Throw
		}
		It "IshApplicationSetting.AppServerComputerName" {
			$ishApplicationSetting.AppServerComputerName.Length -ge 0 | Should Not BeNullOrEmpty
		}
	}

	Context "Get-IshTimeZone returns IshApplicationSettings (plural) object" {
		It "Parameter IshSession implicit" {
			$ishApplicationSettings = Get-IshTimeZone -IShSession $ishSession -Count 2
			$ishApplicationSettings.GetType().Name | Should BeExactly "IshApplicationSettings"
			$ishApplicationSettings.TimeZoneId -ge 0 | Should Not BeNullOrEmpty
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
}
