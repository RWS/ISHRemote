BeforeAll {
	$cmdletName = "Get-IshUser"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshUser" -Tags "Create" {
	Context "Get-IshUser ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshUser -IShSession "INVALIDISHSESSION" -Name "INVALIDUSERNAME" } | Should -Throw
		}
	}
	Context "Get-IshUser ParameterGroup" {
		BeforeAll {
			$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name FISHUSERLANGUAGE -Level None -ValueType Element -Value "VLANGUAGEEN" |
						Set-IshMetadataField -IshSession $ishSession -Name FUSERGROUP -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
						Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "SomethingSecret"
			$ishObject = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
		}
		It "Parameter IshSession Implicit" {
			$ishObject = Get-IshUser -Id $ishObject.IshRef
			$ishObject.GetType().Name | Should -BeExactly "IshUser"
			$ishObject.Count | Should -Be 1
		}
		It "Parameter Metadata StrictMetadataPreference=Off with PASSWORD" {
			$strictMetadataPreference = $ishSession.StrictMetadataPreference
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name PASSWORD -Level None
			$ishSession.StrictMetadataPreference = "Off"
			{ Get-IshUser -IshSession $ishSession -Id $ishObject.IshRef -RequestedMetadata $requestedMetadata } | Should -Throw
			$ishSession.StrictMetadataPreference = "Continue"
			{ Get-IshUser -IshSession $ishSession -Id $ishObject.IshRef -RequestedMetadata $requestedMetadata } | Should -Not -Throw
			$ishSession.StrictMetadataPreference = $strictMetadataPreference
		}
		It "Parameter Metadata StrictMetadataPreference=Continue with PASSWORD" {
			$strictMetadataPreference = $ishSession.StrictMetadataPreference
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name PASSWORD -Level None
			$ishSession.StrictMetadataPreference = "Continue"
			{ Get-IshUser -IshSession $ishSession -Id $ishObject.IshRef -RequestedMetadata $requestedMetadata } | Should -Not -Throw
			$ishSession.StrictMetadataPreference = $strictMetadataPreference
		}
		It "Parameter IshSession.DefaultRequestedMetadata=Descriptive on My-Metadata" {
			$oldDefaultRequestedMetadata = $ishSession.DefaultRequestedMetadata
			$ishSession.DefaultRequestedMetadata = "Descriptive"
			$ishObject = Get-IshUser -IshSession $ishSession
			$ishSession.DefaultRequestedMetadata = $oldDefaultRequestedMetadata
			$ishObject.GetType().Name | Should -BeExactly "IshUser"
			$ishObject.IshField.Length | Should -Be 10
		}
		It "Parameter IshSession.DefaultRequestedMetadata=Basic on My-Metadata" {
			$oldDefaultRequestedMetadata = $ishSession.DefaultRequestedMetadata
			$ishSession.DefaultRequestedMetadata = "Basic"
			$ishObject = Get-IshUser -IshSession $ishSession
			$ishSession.DefaultRequestedMetadata = $oldDefaultRequestedMetadata
			$ishObject.GetType().Name | Should -BeExactly "IshUser"
			$ishObject.IshField.Length | Should -Be 25
		}
		It "Parameter IshSession.DefaultRequestedMetadata=All on My-Metadata" {
			$oldDefaultRequestedMetadata = $ishSession.DefaultRequestedMetadata
			$ishSession.DefaultRequestedMetadata = "All"
			$ishObject = Get-IshUser -IshSession $ishSession
			$ishSession.DefaultRequestedMetadata = $oldDefaultRequestedMetadata
			$ishObject.GetType().Name | Should -BeExactly "IshUser"
			$ishObject.IshField.Length | Should -Be 29
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$users = Find-IshUser -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "USERNAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshUser -IshSession $ishSession -IshObject $users } catch { }
}

