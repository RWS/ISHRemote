BeforeAll {
	$cmdletName = "Test-IshSession"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Test-IshSession" -Tags "Read" {
	Context "Test-IshSession ISHDeploy::Enable-ISHIntegrationSTSInternalAuthentication/Prepare-SupportAccess.ps1" {
		It "Parameter WsBaseUrl contains 'SDL' (legacy script)" -Skip {
			Test-IshSession -WsBaseUrl https://example.com/ISHWS/SDL/ -IshUserName x -IshPassword y | Should -Be $true
		}
		It "Parameter WsBaseUrl contains 'Internal' (ISHDeploy)" -Skip {
			Test-IshSession -WsBaseUrl https://example.com/ISHWS/Internal/ -IshUserName x -IshPassword y | Should -Be $true
		}
	}

	Context "Test-IshSession UserNamePassword" {
		It "Parameter WsBaseUrl invalid" {
			Test-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -IshUserName "INVALIDISHUSERNAME" -IshPassword "INVALIDISHPASSWORD" | Should -Be $false
		}
		It "Parameter IshUserName invalid" {
			Test-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName "INVALIDISHUSERNAME" -IshPassword "INVALIDISHPASSWORD" | Should -Be $false
		}
		It "Parameter IshPassword specified" {
			Test-IshSession -WsBaseUrl $webServicesBaseUrl  -IshUserName $ishUserName -IshPassword "INVALIDISHPASSWORD" | Should -Be $false
		}
		It "Parameter IshUserName empty falls back to NetworkCredential/ActiveDirectory" -Skip:(-Not $isISHRemoteWindowsAuthentication) {
			{ New-IshSession -WsBaseUrl $webServicesBaseUrl  -IshUserName "" -IshPassword "IGNOREISHPASSWORD" } | Should -Not -Throw "Cannot validate argument on parameter 'IshUserName'. The argument is null or empty. Provide an argument that is not null or empty, and then try the command again."
		}
	}

	Context “Test-IshSession ActiveDirectory" {
		It "Parameter WsBaseUrl invalid" {
			Test-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" | Should -Be $false
		}
	}

	Context "Test-IshSession PSCredential" {
		It "Parameter WsBaseUrl invalid" {
			$securePassword = ConvertTo-SecureString $ishPassword -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ($ishUserName, $securePassword)
			Test-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -PSCredential $mycredentials | Should -Be $false
		}
		It "Parameter PSCredential invalid" {
			$securePassword = ConvertTo-SecureString "INVALIDPASSWORD" -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ("INVALIDISHUSERNAME", $securePassword)
			Test-IshSession -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials | Should -Be $false
		}
		It "Parameter PSCredential" {
			$securePassword = ConvertTo-SecureString $ishPassword -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ($ishUserName, $securePassword)
			Test-IshSession -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials | Should -Be $true
		}
	}

	Context "Test-IshSession returns bool" {
		BeforeAll {
			$ishSessionResult = Test-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
		}
		It "GetType()" {
			$ishSessionResult.GetType().Name | Should -BeExactly "Boolean"
		}
	}

	Context "Test-IshSession WsBaseUrl without ending slash" {
		It "WsBaseUrl without ending slash" {
			# .NET throws unhandy "Reference to undeclared entity 'raquo'." error
			$webServicesBaseUrlWithoutEndingSlash = $webServicesBaseUrl.Substring(0,$webServicesBaseUrl.Length-1)
			Test-IshSession -WsBaseUrl $webServicesBaseUrlWithoutEndingSlash -IshUserName $ishUserName -IshPassword $ishPassword | Should -Be $true
		}
	}

	Context "Test-IshSession Timeout" {
		It "Parameter Timeout Invalid" {
			{ Test-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout "INVALIDTIMEOUT" } | Should -Throw
		}
		It "IshSession.Timeout on INVALID url set to 1ms execution" {
			# TaskCanceledException: A task was canceled.
			$invalidWebServicesBaseUrl = $webServicesBaseUrl -replace "://", "://INVALID"
			Test-IshSession -WsBaseUrl $invalidWebServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -Timeout (New-Object TimeSpan(0,0,0,0,1)) | Should -Be $false
		}
	}

	Context "Test-IshSession TimeoutIssue" {
		It "Parameter TimeoutIssue Invalid" {
			{ Test-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutIssue "INVALIDTimeoutIssue" } | Should -Throw
		}
		It "IshSession.TimeoutIssue set to 1ms execution" {
			# The request channel timed out while waiting for a reply after 00:00:00.0000017. Increase the timeout value passed to the call to Request or increase the SendTimeout value on the Binding. The time allotted to this operation may have been a portion of a longer timeout.
			Test-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutIssue (New-Object TimeSpan(0,0,0,0,1)) | Should -Be $false
		}
	}
	
	Context "Test-IshSession TimeoutService" {
		It "Parameter TimeoutService Invalid" {
			{ Test-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -TimeoutService "INVALIDTIMEOUTSERVICE" } | Should -Throw
		}
	}

	Context "Test-IshSession IgnoreSslPolicyErrors" {
		It "Parameter IgnoreSslPolicyErrors specified negative flow (segment-one-url)" -Skip {
			# replace hostname like machinename.somedomain.com to machinename only, marked as skipped for non-development machines
			$slash1Position = $webServicesBaseUrl.IndexOf("/")
			$slash2Position = $webServicesBaseUrl.IndexOf("/",$slash1Position+1)
			$slash3Position = $webServicesBaseUrl.IndexOf("/",$slash2Position+1)
			$hostname = $webServicesBaseUrl.Substring($slash2Position+1,$slash3Position-$slash2Position-1)
			$computername = $hostname.Substring(0,$hostname.IndexOf("."))
			$webServicesBaseUrlToComputerName = $webServicesBaseUrl.Replace($hostname,$computername)
			Test-IshSession -WsBaseUrl $webServicesBaseUrlToComputerName -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors -WarningAction Ignore | Should -Be $true
		}
	}

	Context "New-IshSession ExplicitIssuer" {
		It "Parameter WsTrustIssuerUrl and WsTrustIssuerMexUrl are using full hostname" {
			Test-IshSession -WsBaseUrl $webServicesBaseUrl -WsTrustIssuerUrl $wsTrustIssuerUrl -WsTrustIssuerMexUrl $wsTrustIssuerMexUrl -IshUserName $ishUserName -IshPassword $ishPassword | Should -BeExactly $true
		}
		It "Parameter WsTrustIssuerUrl and WsTrustIssuerMexUrl are using localhost" -Skip {
			Test-IshSession -WsBaseUrl $localWebServicesBaseUrl -WsTrustIssuerUrl $localWsTrustIssuerUrl -WsTrustIssuerMexUrl $localWsTrustIssuerMexUrl -IshUserName $ishUserName -IshPassword $ishPassword -IgnoreSslPolicyErrors -WarningAction Ignore | Should -BeExactly $true
		}
	}

}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}