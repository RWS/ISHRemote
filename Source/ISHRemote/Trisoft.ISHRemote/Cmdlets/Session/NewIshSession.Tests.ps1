Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "New-IshSession"
try {

Describe "New-IshSession" -Tags "Read" {
	Write-Host "Initializing Test Data and Variables"
	$ishSession = $null  # Resetting generic $ishSession

	<#
	Context "New-IshSession ISHDeploy::Enable-ISHIntegrationSTSInternalAuthentication/Prepare-SupportAccess.ps1" {
		It "Parameter WsBaseUrl contains 'SDL' (legacy script)" {
			$ishSession = New-IshSession -WsBaseUrl https://example.com/ISHWS/SDL/ -IshUserName x -IshPassword y
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
		}
		It "Parameter WsBaseUrl contains 'Internal' (ISHDeploy)" {
			$ishSession = New-IshSession -WsBaseUrl https://example.com/ISHWS/Internal/ -IshUserName x -IshPassword y
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
		}
	}
	#>

	Context “New-IshSession UserNamePassword" {
		It "Parameter WsBaseUrl invalid" {
			{ New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -IshUserName "INVALIDISHUSERNAME" -IshPassword "INVALIDISHPASSWORD" } | Should Throw "Invalid URI"
		}
		It "Parameter IshUserName invalid" {
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName "INVALIDISHUSERNAME" -IshPassword "INVALIDISHPASSWORD" } | Should Throw
		}
		It "Parameter IshPassword specified" {
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl  -IshUserName $ishUserName -IshPassword "INVALIDISHPASSWORD" } | Should Throw
		}
	}

	Context “New-IshSession ActiveDirectory" {
		It "Parameter WsBaseUrl invalid" {
			{ New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" } | Should Throw "Invalid URI"
		}
	}

	Context “New-IshSession PSCredential" {
		It "Parameter WsBaseUrl invalid" {
			{ 
				$securePassword = ConvertTo-SecureString $ishPassword -AsPlainText -Force
				$mycredentials = New-Object System.Management.Automation.PSCredential ($ishUserName, $securePassword)
				New-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -PSCredential $mycredentials
			} | Should Throw "Invalid URI"
		}
		It "Parameter PSCredential invalid" {
			$securePassword = ConvertTo-SecureString "INVALIDPASSWORD" -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ("INVALIDISHUSERNAME", $securePassword)
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials } | Should Throw
		}
		It "Parameter PSCredential" {
			$securePassword = ConvertTo-SecureString $ishPassword -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ($ishUserName, $securePassword)
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials } | Should Not Throw
		}
	}

	Context "New-IshSession returns IshSession object" {
		$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
		It "GetType()" {
			$ishSession.GetType().Name | Should BeExactly "IshSession"
		}
		It "IshSession.AuthenticationContext" {
			$ishSession.AuthenticationContext | Should Not BeNullOrEmpty
		}
		It "IshSession.BlobBatchSize" {
			$ishSession.BlobBatchSize -gt 0 | Should Be $true
		}
		It "IshSession.ChunkSize" {
			$ishSession.ChunkSize -gt 0 | Should Be $true
		}
		It "IshSession.ClientVersion" {
			$ishSession.ClientVersion | Should Not BeNullOrEmpty
		}
		It "IshSession.ClientVersion not 0.0.0.0" {
			$ishSession.ClientVersion | Should Not Be "0.0.0.0"
		}
		It "IshSession.FolderPathSeparator" {
			$ishSession.FolderPathSeparator | Should Be "\"
		}
		It "IshSession.IshUserName" {
			$ishSession.IshUserName | Should Not BeNullOrEmpty
		}
		It "IshSession.UserName" {
			$ishSession.UserName | Should Not BeNullOrEmpty
		}
		It "IshSession.IshTypeFieldDefinition" {
			$ishSession.IshTypeFieldDefinition | Should Not BeNullOrEmpty
		}
		It "IshSession.IshTypeFieldDefinition.Count" {
			$ishSession.IshTypeFieldDefinition.Count -gt 460 | Should Be $true
		}
		It "IshSession.IshTypeFieldDefinition.GetType().Name" {
			$ishSession.IshTypeFieldDefinition[0].GetType().Name | Should BeExactly "IshTypeFieldDefinition"
			$ishSession.IshTypeFieldDefinition[0].ISHType | Should Not BeNullOrEmpty
			$ishSession.IshTypeFieldDefinition[0].Level | Should Not BeNullOrEmpty
			$ishSession.IshTypeFieldDefinition[0].Name | Should Not BeNullOrEmpty
			$ishSession.IshTypeFieldDefinition[0].DataType | Should Not BeNullOrEmpty
		}
		It "IshSession.MetadataBatchSize" {
			$ishSession.MetadataBatchSize -gt 0 | Should Be $true
		}
		It "IshSession.Seperator" {
			$ishSession.Seperator | Should Be ", "
		}
		It "IshSession.ServerVersion empty (ISHWS down?)" {
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
		}
		It "IshSession.ServerVersion not 0.0.0.0" {
			$ishSession.ServerVersion | Should Not Be "0.0.0.0"
		}
		It "IshSession.ServerVersion contains 4 dot-seperated parts" {
			$ishSession.ServerVersion.Split(".").Length | Should Be 4
		}
		It "IshSession.Timeout defaults to 20s" {
			$ishSession.Timeout.TotalMilliseconds -eq 20000 | Should Be $true
		}
		It "IshSession.TimeoutIssue" {
			$ishSession.TimeoutIssue.TotalMilliseconds -gt 0 | Should Be $true
		}
		It "IshSession.TimeoutService" {
			$ishSession.TimeoutService.TotalMilliseconds -gt 0 | Should Be $true
		}
		It "IshSession.StrictMetadataPreference" {
			$ishSession.StrictMetadataPreference | Should Be "Continue"
		}
		It "IshSession.WebServicesBaseUrl" {
			$ishSession.WebServicesBaseUrl | Should Not BeNullOrEmpty
		}
	}

	Context "New-IshSession WsBaseUrl without ending slash" {
		# .NET throws unhandy "Reference to undeclared entity 'raquo'." error
		$webServicesBaseUrlWithoutEndingSlash = $webServicesBaseUrl.Substring(0,$webServicesBaseUrl.Length-1)
		It "WsBaseUrl without ending slash" {
			{ $ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrlWithoutEndingSlash -IshUserName $ishUserName -IshPassword $ishPassword } | Should Not Throw
		}
	}

	Context "New-IshSession Timeout" {
		It "Parameter Timeout Invalid" {
			{ $ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout "INVALIDTIMEOUT" } | Should Throw
		}
		It "IshSession.Timeout set to 30s" {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout (New-TimeSpan -Seconds 60)
			$ishSession.Timeout.TotalMilliseconds  | Should Be "60000"
		}
		It "IshSession.Timeout on INVALID url set to 1ms execution" {
			# TaskCanceledException: A task was canceled.
			{
				$invalidWebServicesBaseUrl = $webServicesBaseUrl -replace "://", "://INVALID"
				$ishSession = New-IshSession -WsBaseUrl $invalidWebServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout (New-Object TimeSpan(0,0,0,0,1))
			} | Should Throw
		}
	}

	Context "New-IshSession TimeoutIssue" {
		It "Parameter TimeoutIssue Invalid" {
			{ $ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutIssue "INVALIDTimeoutIssue" } | Should Throw
		}
		It "IshSession.TimeoutIssue set to 30s" {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutIssue (New-TimeSpan -Seconds 30)
			$ishSession.TimeoutIssue.TotalMilliseconds  | Should Be "30000"
		}
		It "IshSession.TimeoutIssue set to 1ms execution" {
			# The request channel timed out while waiting for a reply after 00:00:00.0000017. Increase the timeout value passed to the call to Request or increase the SendTimeout value on the Binding. The time allotted to this operation may have been a portion of a longer timeout.
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutIssue (New-Object TimeSpan(0,0,0,0,1)) } | Should Throw
		}
	}
	
	Context "New-IshSession TimeoutService" {
		It "Parameter TimeoutService Invalid" {
			{ $ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutService "INVALIDTIMEOUTSERVICE" } | Should Throw
		}
		It "IshSession.TimeoutService set to 40s" {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutService (New-TimeSpan -Seconds 40)
			$ishSession.TimeoutService.TotalMilliseconds  | Should Be "40000"
		}
		<# It "IshSession.TimeoutService set to 1 tickout execution" {
			# The request channel timed out attempting to send after 00:00:00.0010000. Increase the timeout value passed to the call to Request or increase the SendTimeout value on the Binding. The time allotted to this operation may have been a portion of a longer timeout.
			{ 
				$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutService (New-Object TimeSpan(1)) 
				# Forcing a GetVersion web service call, probably needs a better call because GetVersion can be too fast, so nothing is thrown
				$version = $ishSession.ServerVersion
			} | Should Throw
		} #>
	}

	Context "New-IshSession IgnoreSslPolicyErrors" {
		It "Parameter IgnoreSslPolicyErrors specified positive flow" {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
			$ishSession.ServerVersion.Split(".").Length | Should Be 4
		}
		It "Parameter IgnoreSslPolicyErrors specified negative flow (segment-one-url)" -skip {
			# replace hostname like machinename.somedomain.com to machinename only, marked as skipped for non-development machines
			$slash1Position = $webServicesBaseUrl.IndexOf("/")
			$slash2Position = $webServicesBaseUrl.IndexOf("/",$slash1Position+1)
			$slash3Position = $webServicesBaseUrl.IndexOf("/",$slash2Position+1)
			$hostname = $webServicesBaseUrl.Substring($slash2Position+1,$slash3Position-$slash2Position-1)
			$computername = $hostname.Substring(0,$hostname.IndexOf("."))
			$webServicesBaseUrlToComputerName = $webServicesBaseUrl.Replace($hostname,$computername)
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrlToComputerName -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
			$ishSession.ServerVersion.Split(".").Length | Should Be 4
			$ishSession.Dispose()
		}
		<# It "Parameter IgnoreSslPolicyErrors specified negative flow (Resolve-DnsName)" -skip {
			# replace hostname like example.com with ip-address
			$slash1Position = $webServicesBaseUrl.IndexOf("/")
			$slash2Position = $webServicesBaseUrl.IndexOf("/",$slash1Position+1)
			$slash3Position = $webServicesBaseUrl.IndexOf("/",$slash2Position+1)
			$hostname = $webServicesBaseUrl.Substring($slash2Position+1,$slash3Position-$slash2Position)
			$ipAddress = Resolve-DnsName –Name $hostname  # only available on Windows Server 2012 R2 and Windows 8.1
			$webServicesBaseUrlToIpAddress = $webServicesBaseUrl.Replace($hostname,$ipAddress)
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrlToIpAddress -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
			$ishSession.ServerVersion.Split(".").Length | Should Be 4
			$ishSession.Dispose()
		} #>
	}
	Context "New-IshSession ExplicitIssuer" {
		It "Parameter WsTrustIssuerUrl and WsTrustIssuerMexUrl are using full hostname" {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -WsTrustIssuerUrl $wsTrustIssuerUrl -WsTrustIssuerMexUrl $wsTrustIssuerMexUrl -IshUserName $ishUserName -IshPassword $ishPassword
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
			$ishSession.ServerVersion.Split(".").Length | Should Be 4
		}
		It "Parameter WsTrustIssuerUrl and WsTrustIssuerMexUrl are using localhost" -skip {
			$ishSession = New-IshSession -WsBaseUrl $localWebServicesBaseUrl -WsTrustIssuerUrl $localWsTrustIssuerUrl -WsTrustIssuerMexUrl $localWsTrustIssuerMexUrl -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors
			$ishSession.ServerVersion | Should Not BeNullOrEmpty
			$ishSession.ServerVersion.Split(".").Length | Should Be 4
		}
	}

	Context "New-IshSession returns IshSession ServiceReferences" {
		$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
		It "IshSession.Application25" {
			$ishSession.Application25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.BackgroundTask25" -skip { # Only available starting 13SP2/13.0.2
			$ishSession.BackgroundTask25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.Baseline25" {
			$ishSession.Baseline25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.DocumentObj25" {
			$ishSession.DocumentObj25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.EDT25" {
			$ishSession.EDT25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.EventMonitor25" {
			$ishSession.EventMonitor25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.Folder25" {
			$ishSession.Folder25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.ListOfValues25" {
			$ishSession.ListOfValues25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.MetadataBinding25" {
			$ishSession.MetadataBinding25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.OutputFormat25" {
			$ishSession.OutputFormat25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.PublicationOutput25" {
			$ishSession.PublicationOutput25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.Search25" {
			$ishSession.Search25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.Settings25" {
			$ishSession.Settings25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.TranslationJob25" {
			$ishSession.TranslationJob25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.TranslationTemplate25" {
			$ishSession.TranslationTemplate25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.User25" {
			$ishSession.User25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.UserGroup25" {
			$ishSession.UserGroup25 -ne $null | Should Not BeNullOrEmpty
		}
		It "IshSession.UserRole25" {
			$ishSession.UserRole25 -ne $null | Should Not BeNullOrEmpty
		}
	}
}

	
} finally {
	Write-Host "Cleaning Test Data and Variables"
}