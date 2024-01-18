﻿BeforeAll {
	$cmdletName = "New-IshSession"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
	# No longer resetting generic incoming $ishSession to $null to allow version checks, all *-IshSession cmdlets should use $localIShSession
}

Describe "New-IshSession" -Tags "Read" {
	Context "New-IshSession UserNamePassword so protocol WcfSoapWithWsTrust" {
		It "Parameter WsBaseUrl invalid" {
			{ New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -IshUserName "INVALIDISHUSERNAME" -IshPassword "INVALIDISHPASSWORD" } | Should -Throw "Invalid URI: The hostname could not be parsed."
		}
		It "Parameter IshUserName invalid" {
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName "INVALIDISHUSERNAME" -IshPassword "INVALIDISHPASSWORD" } | Should -Throw
		}
		It "Parameter IshPassword invalid" {
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl  -IshUserName $ishUserName -IshPassword "INVALIDISHPASSWORD" } | Should -Throw
		}
		It "Parameter IshUserName empty falls back to NetworkCredential/ActiveDirectory" -Skip:(-Not $isISHRemoteWindowsAuthentication) {
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl  -IshUserName "" -IshPassword "IGNOREISHPASSWORD" } | Should -Not -Throw "Cannot validate argument on parameter 'IshUserName'. The argument is null or empty. Provide an argument that is not null or empty, and then try the command again."
		}
	}

	Context "New-IshSession ClientIdClientSecret so protocol WcfSoapWithOpenIdConnect or OpenApiWithOpenIdConnect" {
		It "Parameter WsBaseUrl invalid" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$exception = { New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -ClientId "INVALIDCLIENTID" -ClientSecret "INVALIDCLIENTSECRET" } | Should -Throw -PassThru
				$exception -like "*Invalid URI: The hostname could not be parsed.*" | Should -Be $true
			}
		}
		It "Parameter ClientId invalid" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$exception = { New-IshSession -WsBaseUrl $webServicesBaseUrl -ClientId "INVALIDCLIENTID" -ClientSecret "INVALIDCLIENTSECRET" } | Should -Throw -PassThru
				$exception -like "*invalid_client*" | Should -Be $true
			}
		}
		It "Parameter ClientSecret invalid" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$exception = { New-IshSession -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret "INVALIDCLIENTSECRET" } | Should -Throw -PassThru
				$exception -like "*invalid_client*" | Should -Be $true
			}
		}
		It "Parameter ClientSecret expired" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$exception = { New-IshSession -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret "INVALIDCLIENTSECRETEXPIRED" } | Should -Throw -PassThru
				$exception -like "*invalid_client*" | Should -Be $true
			}
		}
	}

	Context "New-IshSession Interactive so protocol WcfSoapWithWsTrust, WcfSoapWithOpenIdConnect or OpenApiWithOpenIdConnect" {
		It "Parameter WsBaseUrl invalid" {
			{ New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" } | Should -Throw "Invalid URI: The hostname could not be parsed."
		}
		It "Parameter WsBaseUrl invalid and -Timeout exists" {
			{ New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -Timeout 5 } | Should -Throw "Invalid URI: The hostname could not be parsed."
		}
	}

	Context "New-IshSession PSCredential so protocol WcfSoapWithWsTrust, WcfSoapWithOpenIdConnect or OpenApiWithOpenIdConnect" {
		It "Parameter WsBaseUrl invalid" {
			{ 
				$securePassword = ConvertTo-SecureString $ishPassword -AsPlainText -Force
				$mycredentials = New-Object System.Management.Automation.PSCredential ($ishUserName, $securePassword)
				New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -PSCredential $mycredentials
			} | Should -Throw "Invalid URI: The hostname could not be parsed."
		}
		It "Parameter PSCredential invalid" {
			$securePassword = ConvertTo-SecureString "INVALIDPASSWORD" -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ("INVALIDISHUSERNAME", $securePassword)
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials } | Should -Throw
		}
		It "Parameter PSCredential over WcfSoapWithWsTrust" {
			$securePassword = ConvertTo-SecureString $ishPassword -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ($ishUserName, $securePassword)
			{ New-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials } | Should -Not -Throw
		}
		It "Parameter PSCredential over WcfSoapWithOpenIdConnect" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$secureClientSecret = ConvertTo-SecureString $amClientSecret -AsPlainText -Force
				$mycredentials = New-Object System.Management.Automation.PSCredential ($amClientId, $secureClientSecret)
				{ New-IshSession -Protocol WcfSoapWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials } | Should -Not -Throw
			}
		}
	}

	Context "New-IshSession over WcfSoapWithWsTrust returns IshSession object" {
		BeforeAll {
			$localIShSession = New-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
		}
		It "Protocol" {
			$localIShSession.Protocol | Should -BeExactly "WcfSoapWithWsTrust"
		}
		It "GetType()" {
			$localIShSession.GetType().Name | Should -BeExactly "IshSession"
		}
		It "IshSession.AuthenticationContext" {
			$localIShSession.AuthenticationContext | Should -Not -BeNullOrEmpty
		}
		It "IshSession.BlobBatchSize" {
			$localIShSession.BlobBatchSize -gt 0 | Should -Be $true
		}
		It "IshSession.ChunkSize" {
			$localIShSession.ChunkSize -gt 0 | Should -Be $true
		}
		It "IshSession.ClientVersion" {
			$localIShSession.ClientVersion | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ClientVersion not 0.0.0.0" {
			$localIShSession.ClientVersion | Should -Not -Be "0.0.0.0"
		}
		It "IshSession.FolderPathSeparator" {
			$localIShSession.FolderPathSeparator | Should -Be "\"
		}
		It "IshSession.IshUserName" {
			$localIShSession.IshUserName | Should -Not -BeNullOrEmpty
		}
		It "IshSession.UserName" {
			$localIShSession.UserName | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ClientAppId" {
			$localIShSession.ClientAppId | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ClientId" {
			$localIShSession.ClientId | Should -BeNullOrEmpty
		}
		It "IshSession.AccessToken" {
			$localIShSession.AccessToken | Should -BeNullOrEmpty
		}
		It "IshSession.IshTypeFieldDefinition" {
			$localIShSession.IshTypeFieldDefinition | Should -Not -BeNullOrEmpty
		}
		It "IshSession.IshTypeFieldDefinition.Count" {
			$localIShSession.IshTypeFieldDefinition.Count -gt 460 | Should -Be $true
		}
		It "IshSession.IshTypeFieldDefinition.GetType().Name" {
			$localIShSession.IshTypeFieldDefinition[0].GetType().Name | Should -BeExactly "IshTypeFieldDefinition"
			$localIShSession.IshTypeFieldDefinition[0].ISHType | Should -Not -BeNullOrEmpty
			$localIShSession.IshTypeFieldDefinition[0].Level | Should -Not -BeNullOrEmpty
			$localIShSession.IshTypeFieldDefinition[0].Name | Should -Not -BeNullOrEmpty
			$localIShSession.IshTypeFieldDefinition[0].DataType | Should -Not -BeNullOrEmpty
		}
		It "IshSession.MetadataBatchSize" {
			$localIShSession.MetadataBatchSize -gt 0 | Should -Be $true
		}
		It "IshSession.Separator" {
			$localIShSession.Separator | Should -Be ", "
		}
		It "IshSession.ServerVersion empty (ISHWS down?)" {
			$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ServerVersion not 0.0.0.0" {
			$localIShSession.ServerVersion | Should -Not -Be "0.0.0.0"
		}
		It "IshSession.ServerVersion contains 4 dot-seperated parts" {
			$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
		}
		It "IshSession.Timeout defaults to 30m" {
			$localIShSession.Timeout.TotalMinutes | Should -Be 30
		}
		It "IshSession.StrictMetadataPreference" {
			$localIShSession.StrictMetadataPreference | Should -Be "Continue"
		}
		It "IshSession.PipelineObjectPreference" {
			$localIShSession.PipelineObjectPreference | Should -Be "PSObjectNoteProperty"
		}
		It "IshSession.DefaultRequestedMetadata" {
			$localIShSession.DefaultRequestedMetadata | Should -Be "Basic"
		}
		It "IshSession.WebServicesBaseUrl" {
			$localIShSession.WebServicesBaseUrl | Should -Not -BeNullOrEmpty
		}
	}

	Context "New-IshSession over WcfSoapWithOpenIdConnect returns IshSession object" {
		BeforeAll {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession = New-IshSession -Protocol WcfSoapWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			}
		}
		It "Protocol" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Protocol | Should -BeExactly "WcfSoapWithOpenIdConnect"
			}
		}
		It "GetType()" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.GetType().Name | Should -BeExactly "IshSession"
			}
		}
		It "IshSession.AuthenticationContext" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.AuthenticationContext | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.BlobBatchSize" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.BlobBatchSize -gt 0 | Should -Be $true
			}
		}
		It "IshSession.ChunkSize" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ChunkSize -gt 0 | Should -Be $true
			}
		}
		It "IshSession.ClientVersion" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientVersion | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ClientVersion not 0.0.0.0" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientVersion | Should -Not -Be "0.0.0.0"
			}
		}
		It "IshSession.FolderPathSeparator" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.FolderPathSeparator | Should -Be "\"
			}
		}
		It "IshSession.IshUserName" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshUserName | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.UserName" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.UserName | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ClientAppId" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientAppId | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ClientId" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientId | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.AccessToken" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.AccessToken | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.IshTypeFieldDefinition" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshTypeFieldDefinition | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.IshTypeFieldDefinition.Count" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshTypeFieldDefinition.Count -gt 460 | Should -Be $true
			}
		}
		It "IshSession.IshTypeFieldDefinition.GetType().Name" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshTypeFieldDefinition[0].GetType().Name | Should -BeExactly "IshTypeFieldDefinition"
				$localIShSession.IshTypeFieldDefinition[0].ISHType | Should -Not -BeNullOrEmpty
				$localIShSession.IshTypeFieldDefinition[0].Level | Should -Not -BeNullOrEmpty
				$localIShSession.IshTypeFieldDefinition[0].Name | Should -Not -BeNullOrEmpty
				$localIShSession.IshTypeFieldDefinition[0].DataType | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.MetadataBatchSize" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.MetadataBatchSize -gt 0 | Should -Be $true
			}
		}
		It "IshSession.Separator" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Separator | Should -Be ", "
			}
		}
		It "IshSession.ServerVersion empty (ISHWS down?)" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ServerVersion not 0.0.0.0" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ServerVersion | Should -Not -Be "0.0.0.0"
			}
		}
		It "IshSession.ServerVersion contains 4 dot-seperated parts" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
			}
		}
		It "IshSession.Timeout defaults to 30m" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Timeout.TotalMinutes | Should -Be 30
			}
		}
		It "IshSession.StrictMetadataPreference" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.StrictMetadataPreference | Should -Be "Continue"
			}
		}
		It "IshSession.PipelineObjectPreference" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.PipelineObjectPreference | Should -Be "PSObjectNoteProperty"
			}
		}
		It "IshSession.DefaultRequestedMetadata" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.DefaultRequestedMetadata | Should -Be "Basic"
			}
		}
		It "IshSession.WebServicesBaseUrl" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.WebServicesBaseUrl | Should -Not -BeNullOrEmpty
			}
		}
	}

	Context "New-IshSession over OpenApiWithOpenIdConnect returns IshSession object" {
		BeforeAll {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession = New-IshSession -Protocol OpenApiWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			}
		}
		It "Protocol" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Protocol | Should -BeExactly "OpenApiWithOpenIdConnect"
			}
		}
		It "GetType()" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.GetType().Name | Should -BeExactly "IshSession"
			}
		}
		It "IshSession.AuthenticationContext" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.AuthenticationContext | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.BlobBatchSize" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.BlobBatchSize -gt 0 | Should -Be $true
			}
		}
		It "IshSession.ChunkSize" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ChunkSize -gt 0 | Should -Be $true
			}
		}
		It "IshSession.ClientVersion" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientVersion | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ClientVersion not 0.0.0.0" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientVersion | Should -Not -Be "0.0.0.0"
			}
		}
		It "IshSession.FolderPathSeparator" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.FolderPathSeparator | Should -Be "\"
			}
		}
		It "IshSession.IshUserName" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshUserName | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.UserName" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.UserName | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ClientAppId" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientAppId | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ClientId" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ClientId | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.AccessToken" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.AccessToken | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.IshTypeFieldDefinition" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshTypeFieldDefinition | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.IshTypeFieldDefinition.Count" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshTypeFieldDefinition.Count -gt 460 | Should -Be $true
			}
		}
		It "IshSession.IshTypeFieldDefinition.GetType().Name" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.IshTypeFieldDefinition[0].GetType().Name | Should -BeExactly "IshTypeFieldDefinition"
				$localIShSession.IshTypeFieldDefinition[0].ISHType | Should -Not -BeNullOrEmpty
				$localIShSession.IshTypeFieldDefinition[0].Level | Should -Not -BeNullOrEmpty
				$localIShSession.IshTypeFieldDefinition[0].Name | Should -Not -BeNullOrEmpty
				$localIShSession.IshTypeFieldDefinition[0].DataType | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.MetadataBatchSize" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.MetadataBatchSize -gt 0 | Should -Be $true
			}
		}
		It "IshSession.Separator" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Separator | Should -Be ", "
			}
		}
		It "IshSession.ServerVersion empty (ISHWS down?)" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ServerVersion not 0.0.0.0" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ServerVersion | Should -Not -Be "0.0.0.0"
			}
		}
		It "IshSession.ServerVersion contains 4 dot-seperated parts" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
			}
		}
		It "IshSession.Timeout defaults to 30m" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Timeout.TotalMinutes | Should -Be 30
			}
		}
		It "IshSession.StrictMetadataPreference" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.StrictMetadataPreference | Should -Be "Continue"
			}
		}
		It "IshSession.PipelineObjectPreference" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.PipelineObjectPreference | Should -Be "PSObjectNoteProperty"
			}
		}
		It "IshSession.DefaultRequestedMetadata" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.DefaultRequestedMetadata | Should -Be "Basic"
			}
		}
		It "IshSession.WebServicesBaseUrl" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.WebServicesBaseUrl | Should -Not -BeNullOrEmpty
			}
		}
	}

	Context "New-IshSession WsBaseUrl without ending slash" {
		It "WsBaseUrl without ending slash" {
			# .NET throws unhandy "Reference to undeclared entity 'raquo'." error
			$webServicesBaseUrlWithoutEndingSlash = $webServicesBaseUrl.Substring(0,$webServicesBaseUrl.Length-1)
			{ New-IshSession -WsBaseUrl $webServicesBaseUrlWithoutEndingSlash -IshUserName $ishUserName -IshPassword $ishPassword } | Should -Not -Throw
		}
	}

	Context "New-IshSession Timeout" {
		It "Parameter Timeout Invalid" {
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout "INVALIDTIMEOUT" } | Should -Throw
		}
		It "IshSession.Timeout set to 30s" {
			$localIShSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout (New-TimeSpan -Seconds 60) -WarningAction Ignore -ErrorAction Ignore
			$localIShSession.Timeout.TotalMilliseconds  | Should -Be "60000"
		}
		It "IshSession.Timeout on INVALID url set to 1ms execution" {
			# TaskCanceledException: A task was canceled.
			{
				$invalidWebServicesBaseUrl = $webServicesBaseUrl -replace "://", "://INVALID"
				New-IshSession -WsBaseUrl $invalidWebServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout (New-Object TimeSpan(0,0,0,0,1))
			} | Should -Throw
		}
	}

	Context "New-IshSession IgnoreSslPolicyErrors" {
		It "Parameter IgnoreSslPolicyErrors specified positive flow" {
			$localIShSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors -WarningAction Ignore
			$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
			$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
		}
		It "Parameter IgnoreSslPolicyErrors specified negative flow like host IPv4 address" {
			# replace hostname like machinename.somedomain.com to ipaddress only as often certificates are valid for machinename/localhost as well
			$slash1Position = $webServicesBaseUrl.IndexOf("/")
			$slash2Position = $webServicesBaseUrl.IndexOf("/",$slash1Position+1)
			$slash3Position = $webServicesBaseUrl.IndexOf("/",$slash2Position+1)
			$hostname = $webServicesBaseUrl.Substring($slash2Position+1,$slash3Position-$slash2Position-1)
			$ipv4Addresses = [System.Net.Dns]::GetHostAddresses($hostname) | 
			                 Where-Object -Property IsIPv6LinkLocal -ne $true | 
							 Select-Object -Property IPAddressToString  # returning @(192.168.1.160,10.100.139.126)
			foreach ($ipv4Address in $ipv4Addresses)
			{
				$webServicesBaseUrlWithIpAddress = $webServicesBaseUrl.Replace($hostname,$ipv4Address.IPAddressToString)
				$localIShSession = New-IshSession -WsBaseUrl $webServicesBaseUrlWithIpAddress -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
				$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
				$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
			}
		}
		It "Parameter IgnoreSslPolicyErrors specified negative flow like hostname (segment-one-url)" -Skip {
			# replace hostname like machinename.somedomain.com to machinename only, marked as skipped for non-development machines
			$slash1Position = $webServicesBaseUrl.IndexOf("/")
			$slash2Position = $webServicesBaseUrl.IndexOf("/",$slash1Position+1)
			$slash3Position = $webServicesBaseUrl.IndexOf("/",$slash2Position+1)
			$hostname = $webServicesBaseUrl.Substring($slash2Position+1,$slash3Position-$slash2Position-1)
			$computername = $hostname.Substring(0,$hostname.IndexOf("."))
			$webServicesBaseUrlToComputerName = $webServicesBaseUrl.Replace($hostname,$computername)
			$localIShSession = New-IshSession -WsBaseUrl $webServicesBaseUrlToComputerName -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
			$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
			$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
			$localIShSession.Dispose()
		}
		<# It "Parameter IgnoreSslPolicyErrors specified negative flow (Resolve-DnsName)" -Skip {
			# replace hostname like example.com with ip-address
			$slash1Position = $webServicesBaseUrl.IndexOf("/")
			$slash2Position = $webServicesBaseUrl.IndexOf("/",$slash1Position+1)
			$slash3Position = $webServicesBaseUrl.IndexOf("/",$slash2Position+1)
			$hostname = $webServicesBaseUrl.Substring($slash2Position+1,$slash3Position-$slash2Position)
			$ipAddress = Resolve-DnsName –Name $hostname  # only available on Windows Server 2012 R2 and Windows 8.1
			$webServicesBaseUrlToIpAddress = $webServicesBaseUrl.Replace($hostname,$ipAddress)
			$localIShSession = New-IshSession -WsBaseUrl $webServicesBaseUrlToIpAddress -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
			$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
			$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
			$localIShSession.Dispose()
		} #>
	}

	Context "New-IshSession over WcfSoapWithWsTrust returns IshSession ServiceReferences" {
		BeforeAll {
			$localIShSession = New-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
		}
		It "IshSession.OpenApiISH30Service" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				 $localIShSession.OpenApiISH30Service | Should -BeNullOrEmpty
			}
		}
		It "IshSession.Annotation25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 14) { # new service since 14/14.0.0
				 $localIShSession.Annotation25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Application25" {
			$localIShSession.Application25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.BackgroundTask25" { # new service since 13SP2/13.0.2
			if (([Version]$ishSession.ServerVersion).Major -ge 14 -or (([Version]$ishSession.ServerVersion).Major -ge 13 -and ([Version]$ishSession.ServerVersion).Revision -ge 2)) { 
				$localIShSession.BackgroundTask25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Baseline25" {
			$localIShSession.Baseline25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.DocumentObj25" {
			$localIShSession.DocumentObj25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.EDT25" {
			$localIShSession.EDT25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.EventMonitor25" {
			$localIShSession.EventMonitor25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.Folder25" {
			$localIShSession.Folder25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ListOfValues25" {
			$localIShSession.ListOfValues25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.MetadataBinding25" {
			$localIShSession.MetadataBinding25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.OutputFormat25" {
			$localIShSession.OutputFormat25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.PublicationOutput25" {
			$localIShSession.PublicationOutput25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.Search25" {
			$localIShSession.Search25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.Settings25" {
			$localIShSession.Settings25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.TranslationJob25" {
			$localIShSession.TranslationJob25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.TranslationTemplate25" {
			$localIShSession.TranslationTemplate25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.User25" {
			$localIShSession.User25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.UserGroup25" {
			$localIShSession.UserGroup25 | Should -Not -BeNullOrEmpty
		}
		It "IshSession.UserRole25" {
			$localIShSession.UserRole25 | Should -Not -BeNullOrEmpty
		}
	}

	Context "New-IshSession over WcfSoapWithOpenIdConnect returns IshSession ServiceReferences" {
		BeforeAll {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession = New-IshSession -Protocol WcfSoapWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			}
		}
		It "IshSession.OpenApiISH30Service" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.OpenApiISH30Service | Should -BeNullOrEmpty
			}
		}
		It "IshSession.Annotation25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Annotation25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Application25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Application25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.BackgroundTask25" { # new service since 13SP2/13.0.2
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.BackgroundTask25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Baseline25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Baseline25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.DocumentObj25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.DocumentObj25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.EDT25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.EDT25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.EventMonitor25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.EventMonitor25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Folder25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Folder25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ListOfValues25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ListOfValues25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.MetadataBinding25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.MetadataBinding25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.OutputFormat25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.OutputFormat25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.PublicationOutput25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.PublicationOutput25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Search25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Search25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Settings25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Settings25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.TranslationJob25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.TranslationJob25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.TranslationTemplate25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.TranslationTemplate25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.User25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.User25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.UserGroup25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.UserGroup25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.UserRole25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.UserRole25 | Should -Not -BeNullOrEmpty
			}
		}
	}

	Context "New-IshSession over OpenApiWithOpenIdConnect returns IshSession ServiceReferences" {
		BeforeAll {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession = New-IshSession -Protocol OpenApiWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			}
		}
		It "IshSession.OpenApiISH30Service" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$json = $localIShSession.OpenApiISH30Service.GetApplicationVersionAsync()
				$json.Result | Should -Be $ishSession.ServerVersion
			}
		}
		It "IshSession.Annotation25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Annotation25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Application25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Application25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.BackgroundTask25" { # new service since 13SP2/13.0.2
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.BackgroundTask25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Baseline25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Baseline25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.DocumentObj25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.DocumentObj25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.EDT25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.EDT25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.EventMonitor25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.EventMonitor25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Folder25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Folder25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.ListOfValues25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.ListOfValues25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.MetadataBinding25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.MetadataBinding25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.OutputFormat25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.OutputFormat25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.PublicationOutput25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.PublicationOutput25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Search25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Search25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.Settings25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.Settings25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.TranslationJob25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.TranslationJob25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.TranslationTemplate25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.TranslationTemplate25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.User25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.User25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.UserGroup25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.UserGroup25 | Should -Not -BeNullOrEmpty
			}
		}
		It "IshSession.UserRole25" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$localIShSession.UserRole25 | Should -Not -BeNullOrEmpty
			}
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}