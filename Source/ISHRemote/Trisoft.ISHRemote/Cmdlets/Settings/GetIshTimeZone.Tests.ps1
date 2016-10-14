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
		It "IshApplicationSetting.TimeZoneId" {
			$ishApplicationSetting.TimeZoneId -ge 0 | Should Not BeNullOrEmpty
		}
	}

	Context "Get-IshTimeZone returns IshApplicationSettings (plural) object" {
		$ishApplicationSettings = Get-IshTimeZone -IShSession $ishSession -Count 2
		It "GetType()" {
			$ishApplicationSettings.GetType().Name | Should BeExactly "IshApplicationSettings"
		}
		It "IshApplicationSettings.TimeZoneId" {
			$ishApplicationSettings.TimeZoneId -ge 0 | Should Not BeNullOrEmpty
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
}
