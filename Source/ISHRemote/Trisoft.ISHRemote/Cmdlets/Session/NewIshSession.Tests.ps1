BeforeAll {
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
			                 Where-Object -Property AddressFamily -eq 'InterNetwork' |
			                 Where-Object -Property IsIPv6LinkLocal -ne $true | 
							 Select-Object -Property IPAddressToString  # returning @(192.168.1.160,10.100.139.126)
			foreach ($ipv4Address in $ipv4Addresses)
			{
				$webServicesBaseUrlWithIpAddress = $webServicesBaseUrl.Replace($hostname,$ipv4Address.IPAddressToString)
				$localIShSession = New-IshSession -WsBaseUrl $webServicesBaseUrlWithIpAddress -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
				$localIShSession.ServerVersion | Should -Not -BeNullOrEmpty
				$localIShSession.ServerVersion.Split(".").Length | Should -Be 4
			}
			# Remember that ipv6 addresses need to go in between square brackets to avoid port confusion
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
		It "IshSession.OpenApiISH30Client" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				 $localIShSession.OpenApiISH30Client | Should -BeNullOrEmpty
			}
		}
		It "IshSession.OpenApiAM10Client" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				 $localIShSession.OpenApiAM10Client | Should -BeNullOrEmpty
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
		It "IshSession.OpenApiISH30Client" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$json = $localIShSession.OpenApiISH30Client.GetApplicationVersionAsync()
				$json.Result | Should -Be $ishSession.ServerVersion
			}
		}
		It "IshSession.OpenApiAM10Client" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				 $json = $localIShSession.OpenApiAM10Client.IdentityProviders_GetAsync()
				 $json.Result.Count -ge 1 | Should -Be $true
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
				# OpenApiISH30Client
				# below array generated with: "@( '"+$(((Get-Member -InputObject $ishSession.OpenApiISH30Client -MemberType Method).Name) -join "','")+"' )"
				# Correct HATEOS Placeholder 'GetBackgroundTaskByIdAsync' which was only implemented in 15.2.0 and renamed to 'GetBackgroundTaskAsync'
				$openApiISH30ClientMembers20240404 = @( 'AddShortcutToFolderAsync','CancelPublicationPublishAsync','CheckInDocumentObjectByLanguageCardIdAsync','CheckOutDocumentObjectByLanguageCardIdAsync','CompleteBaselineAsync','CreateBackgroundTaskAsync','CreateDocumentObjectAsync','CreateDocumentObjectLanguageAsync','CreateDocumentObjectVersionAsync','CreateFolderAsync','CreateLovValueAsync','CreatePublicationAsync','CreatePublicationLanguageAsync','CreatePublicationVersionAsync','CreateStatusDefinitionAsync','CreateUserAsync','CreateUserGroupAsync','DeleteDocumentObjectByLanguageCardIdAsync','DeleteDocumentObjectByLogicalIdAsync','DeleteFolderAsync','DeleteLovValueAsync','DeletePublicationByLanguageCardIdAsync','DeletePublicationByLogicalIdAsync','DeleteStatusDefinitionAsync','DeleteUserAsync','DeleteUserGroupAsync','Equals','ExpandBaselineAsync','GetApplicationVersionAsync','GetBackgroundTaskAsync','GetBaselineListAsync','GetCurrentUserAsync','GetDocumentObjectByLanguageCardIdAsync','GetDocumentObjectByLogicalIdAsync','GetDocumentObjectContentByLanguageCardIdAsync','GetDocumentObjectListAsync','GetDocumentObjectListByLanguageCardIdAsync','GetDocumentObjectListByLogicalIdAsync','GetDocumentObjectListUsingDocumentObjectAsync','GetDocumentObjectLocationAsync','GetDocumentObjectPossibleTargetStatusListAsync','GetDocumentObjectPossibleTargetStatusListByLanguageCardIdAsync','GetDocumentObjectReferenceListByLanguageCardIdAsync','GetFolderAsync','GetFolderListAsync','GetFolderLocationAsync','GetFolderObjectListAsync','GetHashCode','GetInboxListAsync','GetInboxObjectListAsync','GetLovListAsync','GetLovValueAsync','GetLovValueListAsync','GetMetadataBindingTagListAsync','GetMetadataBindingTagStructureAsync','GetMyPreferencesAsync','GetMyPrivilegesAsync','GetObjectAsync','GetPublicationByLanguageCardIdAsync','GetPublicationByLogicalIdAsync','GetPublicationContentByLanguageCardIdAsync','GetPublicationListAsync','GetPublicationListByLanguageCardIdAsync','GetPublicationListByLogicalIdAsync','GetPublicationListUsingDocumentObjectAsync','GetPublicationLocationAsync','GetRootFolderListAsync','GetSettingsAsync','GetStatusDefinitionAsync','GetStatusDefinitionListAsync','GetTranslationReportSettingsAsync','GetType','GetUserAsync','GetUserGroupListAsync','GetUserListAsync','GetUserPreferencesAsync','GetWorkflowReportSettingsAsync','MoveFolderAsync','MoveObjectToFolderAsync','PublishPublicationAsync','ReleasePublicationAsync','RemoveShortcutFromFolderAsync','SearchAsync','SetMyPreferencesAsync','SetUserPreferencesAsync','ToString','UndoCheckoutDocumentObjectByLanguageCardIdAsync','UnpublishPublicationAsync','UnreleasePublicationAsync','UpdateCurrentUserAsync','UpdateDocumentObjectByLanguageCardIdAsync','UpdateDocumentObjectByLogicalIdAsync','UpdateDocumentObjectContentByLanguageCardIdAsync','UpdateFolderAsync','UpdateLovValueAsync','UpdatePublicationByLanguageCardIdAsync','UpdatePublicationByLogicalIdAsync','UpdateSettingsAsync','UpdateStatusDefinitionAsync','UpdateUserAsync','UpdateUserGroupAsync' )
				$openApiISH30ClientMembers20251210 = @( 'AddShortcutToFolderAsync','AddTranslationJobSourceObjectsAsync','CancelPublicationPublishAsync','CancelTranslationJobAsync','CheckInDocumentObjectByLanguageCardIdAsync','CheckOutDocumentObjectByLanguageCardIdAsync','CleanUpBaselineAsync','CompleteBaselineAsync','CompleteBaselineReportAsync','CopyBaselineAsync','CreateAnnotationAsync','CreateAnnotationReplyAsync','CreateBackgroundTaskAsync','CreateBackgroundTaskForDocumentObjectByLanguageCardIdAsync','CreateBaselineAsync','CreateDocumentObjectAsync','CreateDocumentObjectLanguageAsync','CreateDocumentObjectVersionAsync','CreateElectronicDocumentTypeAsync','CreateEventAsync','CreateEventDetailAsync','CreateFolderAsync','CreateLovValueAsync','CreateOutputFormatAsync','CreateProjectAssigneeAsync','CreateProjectAsync','CreatePublicationAsync','CreatePublicationLanguageAsync','CreatePublicationVersionAsync','CreateStatusDefinitionAsync','CreateTranslationJobAsync','CreateUserAsync','CreateUserGroupAsync','CreateUserRoleAsync','DeleteAnnotationByAnnotationIdAsync','DeleteAnnotationByReplyCardIdAsync','DeleteBaselineAsync','DeleteDocumentObjectByLanguageCardIdAsync','DeleteDocumentObjectByLogicalIdAsync','DeleteElectronicDocumentTypeAsync','DeleteEventAsync','DeleteFolderAsync','DeleteLovValueAsync','DeleteMyPreferencesAsync','DeleteOutputFormatAsync','DeleteProjectAssigneeAsync','DeleteProjectAsync','DeletePublicationByLanguageCardIdAsync','DeletePublicationByLogicalIdAsync','DeleteStatusDefinitionAsync','DeleteTranslationJobAsync','DeleteTranslationJobSourceObjectAsync','DeleteUserAsync','DeleteUserGroupAsync','DeleteUserPreferencesAsync','DeleteUserRoleAsync','Equals','ExpandBaselineAsync','ExpandBaselineReportAsync','ExtendBaselineReportByBaselineAsync','ExtendBaselineReportByCandidateAsync','FreezeBaselineAsync','GetAnnotationByAnnotationIdAsync','GetAnnotationByReplyCardIdAsync','GetAnnotationListAsync','GetAnnotationListByAnnotationIdAsync','GetApplicationVersionAsync','GetBackgroundTaskAsync','GetBackgroundTaskContentAsync','GetBackgroundTaskListAsync','GetBaselineAsync','GetBaselineEntryListAsync','GetBaselineListAsync','GetCurrentUserAsync','GetDocumentObjectByLanguageCardIdAsync','GetDocumentObjectByLogicalIdAsync','GetDocumentObjectContentByLanguageCardIdAsync','GetDocumentObjectContentByLogicalIdAsync','GetDocumentObjectContentInfoByLanguageCardIdAsync','GetDocumentObjectListAsync','GetDocumentObjectListByLanguageCardIdAsync','GetDocumentObjectListByLogicalIdAsync','GetDocumentObjectListByVersionAndLanguageFilterAsync','GetDocumentObjectListByVersionFilterAsync','GetDocumentObjectListUsingConditionNameAsync','GetDocumentObjectListUsingConditionValueAsync','GetDocumentObjectListUsingDocumentObjectAsync','GetDocumentObjectListUsingVariableAsync','GetDocumentObjectLocationAsync','GetDocumentObjectPossibleTargetStatusListAsync','GetDocumentObjectPossibleTargetStatusListByLanguageCardIdAsync','GetDocumentObjectReferenceListByLanguageCardIdAsync','GetDocumentObjectSmartTagListByLanguageCardIdAsync','GetElectronicDocumentTypeAsync','GetElectronicDocumentTypeListAsync','GetEventByDetailIdAsync','GetEventByEventIdAsync','GetEventByProgressIdAsync','GetEventByProgressIdOverviewAsync','GetEventContentByDetailIdAsync','GetEventGroupsAsync','GetEventListByParentProgressIdAsync','GetEventListByProgressIdAsync','GetEventOverviewAsync','GetFieldDefinitionsSettingsAsync','GetFolderAsync','GetFolderByFolderPathAsync','GetFolderListAsync','GetFolderLocationAsync','GetFolderObjectListAsync','GetHashCode','GetInboxListAsync','GetInboxObjectListAsync','GetLovListAsync','GetLovValueAsync','GetLovValueListAsync','GetMetadataBindingSmartTagsAsync','GetMetadataBindingTagListAsync','GetMetadataBindingTagStructureAsync','GetMyPreferencesAsync','GetMyPrivilegesAsync','GetObjectAsync','GetOutputFormatAsync','GetOutputFormatListAsync','GetProjectByAssigneeCardIdAsync','GetProjectByProjectIdAsync','GetProjectListAsync','GetProjectListByProjectIdAsync','GetPublicationByLanguageCardIdAsync','GetPublicationByLogicalIdAsync','GetPublicationContentByLanguageCardIdAsync','GetPublicationListAsync','GetPublicationListByLanguageCardIdAsync','GetPublicationListByLogicalIdAsync','GetPublicationListByVersionFilterAsync','GetPublicationListUsingBaselineAsync','GetPublicationListUsingDocumentObjectAsync','GetPublicationLocationAsync','GetPublicationPublishReportAsync','GetRootFolderListAsync','GetSettingsAsync','GetStatusDefinitionAsync','GetStatusDefinitionListAsync','GetTranslationJobAsync','GetTranslationJobListAsync','GetTranslationJobSourceObjectListAsync','GetTranslationJobTargetObjectListAsync','GetTranslationReportSettingsAsync','GetTranslationStatusListSettingsAsync','GetTranslationTemplateListAsync','GetType','GetUserAsync','GetUserGroupAsync','GetUserGroupListAsync','GetUserListAsync','GetUserPreferencesAsync','GetUserRoleAsync','GetUserRoleListAsync','GetWorkflowReportSettingsAsync','MoveFolderAsync','MoveObjectToFolderAsync','PublishPublicationAsync','ReleasePublicationAsync','RemoveShortcutFromFolderAsync','RetryTranslationJobAsync','SearchAsync','SearchInPublicationAsync','SendTranslationJobToTranslationAsync','SetMyPreferencesAsync','SetUserPreferencesAsync','TerminateEventAsync','ToString','UndoCheckoutDocumentObjectByLanguageCardIdAsync','UnpublishPublicationAsync','UnreleasePublicationAsync','UpdateAnnotationByAnnotationIdAsync','UpdateAnnotationByReplyCardIdAsync','UpdateBaselineAsync','UpdateBaselineEntryListAsync','UpdateCurrentUserAsync','UpdateDocumentObjectByLanguageCardIdAsync','UpdateDocumentObjectByLogicalIdAsync','UpdateDocumentObjectContentByLanguageCardIdAsync','UpdateElectronicDocumentTypeAsync','UpdateFolderAsync','UpdateLovValueAsync','UpdateOutputFormatAsync','UpdateProjectAssigneeAsync','UpdateProjectAsync','UpdatePublicationByLanguageCardIdAsync','UpdatePublicationByLogicalIdAsync','UpdateSettingsAsync','UpdateStatusDefinitionAsync','UpdateTranslationJobAsync','UpdateUserAsync','UpdateUserGroupAsync','UpdateUserRoleAsync' )
				# OpenApiAM10Client
				# below array generated with: "@( '"+$(((Get-Member -InputObject $ishSession.OpenApiAM10Client -MemberType Method).Name) -join "','")+"' )"
				$openApiAM10ClientMembers20240404 = @( 'ApiResourcesGetAsync','ApiResourcesGetByIdAsync','ApplicationsCreateAsync','ApplicationsDeleteAsync','ApplicationsGetAsync','ApplicationsGetByIdAsync','ApplicationsUpdateAsync','AuditLogsGetByFilterAsync','AuditLogsGetByIdAsync','Equals','GetHashCode','GetType','IdentityProvidersCreateAsync','IdentityProvidersDeleteAsync','IdentityProvidersGetAsync','IdentityProvidersGetAvailableIdpTypesAsync','IdentityProvidersGetByIdAsync','IdentityProvidersGetIconAsync','IdentityProvidersGetLoginOptionsAsync','IdentityProvidersGetParametersForIdentityProviderTypeAsync','IdentityProvidersUpdateAsync','ServiceAccountsCreateAsync','ServiceAccountsDeleteAsync','ServiceAccountsDeleteClientSecretAsync','ServiceAccountsGenerateClientSecretAsync','ServiceAccountsGetAsync','ServiceAccountsGetByIdAsync','ServiceAccountsUpdateAsync','ServiceAccountsUpdateClientSecretAsync','SystemGetLoginStatusAsync','SystemGetSupportedCulturesAsync','SystemGetUserInfoAsync','ToString','UsersActivateUserAsync','UsersDeactivateUserAsync','UsersDeleteAsync','UsersDeleteClientSecretAsync','UsersGenerateClientSecretAsync','UsersGetAsync','UsersGetByIdAsync','UsersUpdateAsync','UsersUpdateClientSecretAsync' ) # Before v8.2
				$openApiAM10ClientMembers20251210 = @( 'ApiResources_GetAsync','ApiResources_GetByIdAsync','Applications_CreateAsync','Applications_DeleteAsync','Applications_GetAsync','Applications_GetByIdAsync','Applications_UpdateAsync','AuditLogs_GetByFilterAsync','AuditLogs_GetByIdAsync','Equals','GetHashCode','GetType','IdentityProviders_CreateAsync','IdentityProviders_DeleteAsync','IdentityProviders_GetAsync','IdentityProviders_GetAvailableIdpTypesAsync','IdentityProviders_GetByIdAsync','IdentityProviders_GetIconAsync','IdentityProviders_GetLoginOptionsAsync','IdentityProviders_GetParametersForIdentityProviderTypeAsync','IdentityProviders_UpdateAsync','ServiceAccounts_CreateAsync','ServiceAccounts_DeleteAsync','ServiceAccounts_DeleteClientSecretAsync','ServiceAccounts_GenerateClientSecretAsync','ServiceAccounts_GetAsync','ServiceAccounts_GetByIdAsync','ServiceAccounts_UpdateAsync','ServiceAccounts_UpdateClientSecretAsync','System_GetLoginStatusAsync','System_GetSupportedCulturesAsync','System_GetUserInfoAsync','ToString','Users_ActivateUserAsync','Users_DeactivateUserAsync','Users_DeleteAsync','Users_DeleteClientSecretAsync','Users_GenerateClientSecretAsync','Users_GetAsync','Users_GetByIdAsync','Users_UpdateAsync','Users_UpdateClientSecretAsync' ) # Since v8.2
				# Up to ISHRemote v8.2 $ishSession.OpenApiAM10Client was CamelCase, chose to break and align to Snake_Case as delivered by Access Management since v2.0.0 so Tridion Docs 15.0.0
				$openApiAM10ClientMembers20240404 = $openApiAM10ClientMembers20251210
			}
		}
		It "IshSession.OpenApiISH30Client" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$json = $localIShSession.OpenApiISH30Client.GetApplicationVersionAsync()
				$json.Result | Should -Be $ishSession.ServerVersion
			}
		}
		It "IshSession.OpenApiISH30Client OperationIds (15.1 20240404 methods exists)" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				foreach ($member in $openApiISH30ClientMembers20240404) {
					$localIShSession.OpenApiISH30Client.PSObject.Methods.Name | Should -Contain $member
				}
			}
		}
		It "IshSession.OpenApiISH30Client OperationIds (15.3 20251210 methods exists)" {
			if ((([Version]$ishSession.ServerVersion).Major -ge 15) -and (([Version]$ishSession.ServerVersion).Minor -ge 2)) { # new service since 15.2/15.2.0
				foreach ($member in $openApiISH30ClientMembers20251210) {
					$localIShSession.OpenApiISH30Client.PSObject.Methods.Name | Should -Contain $member
				}
			}
		}
		It "IshSession.OpenApiISH30Client OperationIds (new methods found, adapt this test)" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				foreach ($member in $localIShSession.OpenApiISH30Client.PSObject.Methods.Name) {
					if ($member -notin @('Equals','GetHashCode','GetType','ToString','get_BaseUrl','set_BaseUrl','get_ReadResponseAsString','set_ReadResponseAsString')) {
						$openApiISH30ClientMembers20251210 | Should -Contain $member
					}
				}
			}
		}
		It "IshSession.OpenApiAM10Client" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				 $json = $localIShSession.OpenApiAM10Client.IdentityProviders_GetAsync()
				 $json.Result.Count -ge 1 | Should -Be $true
			}
		}
		It "IshSession.OpenApiAM10Client OperationIds (20240404 methods exists)" {
			foreach ($member in $openApiAM10ClientMembers20240404) {
				$localIShSession.OpenApiAM10Client.PSObject.Methods.Name | Should -Contain $member
			}
		}
		It "IshSession.OpenApiAM10Client OperationIds (new methods found, adapt this test)" {
			foreach ($member in $localIShSession.OpenApiAM10Client.PSObject.Methods.Name) {
				if ($member -notin @('Equals','GetHashCode','GetType','ToString','get_BaseUrl','set_BaseUrl','get_ReadResponseAsString','set_ReadResponseAsString')) {
					$openApiAM10ClientMembers20240404 | Should -Contain $member
				}
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