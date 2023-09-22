BeforeAll {
	$cmdletName = "Test-IshSession"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Test-IshSession" -Tags "Read" {
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
	}

	Context "Test-IshSession ClientIdClientSecret so protocol WcfSoapWithOpenIdConnect or OpenApiWithOpenIdConnect" {
		It "Parameter WsBaseUrl invalid" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				Test-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" | Should -Be $false
			}
		}
		It "Parameter ClientId invalid" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				Test-IshSession -WsBaseUrl $webServicesBaseUrl -ClientId "INVALIDCLIENTID" -ClientSecret "INVALIDCLIENTSECRET" | Should -Be $false
			}
		}
		It "Parameter ClientSecret invalid" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				Test-IshSession -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret "INVALIDCLIENTSECRET" | Should -Be $false
			}
		}
	}

	Context "Test-IshSession Interactive so protocol WcfSoapWithWsTrust, WcfSoapWithOpenIdConnect or OpenApiWithOpenIdConnect" {
		It "Parameter WsBaseUrl invalid" {
			Test-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" | Should -Be $false
		}
		It "Parameter WsBaseUrl invalid and -Timeout exists" {
			Test-IshSession -WsBaseUrl "http:///INVALIDWSBASEURL" -Timeout 5 | Should -Be $false
		}
	}

	Context "Test-IshSession PSCredential so protocol WcfSoapWithWsTrust, WcfSoapWithOpenIdConnect or OpenApiWithOpenIdConnect" {
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
		It "Parameter PSCredential over WcfSoapWithWsTrust" {
			$securePassword = ConvertTo-SecureString $ishPassword -AsPlainText -Force
			$mycredentials = New-Object System.Management.Automation.PSCredential ($ishUserName, $securePassword)
			Test-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials | Should -Be $true
		}
		It "Parameter PSCredential over WcfSoapWithOpenIdConnect" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$secureClientSecret = ConvertTo-SecureString $amClientSecret -AsPlainText -Force
				$mycredentials = New-Object System.Management.Automation.PSCredential ($amClientId, $secureClientSecret)
				Test-IshSession -Protocol WcfSoapWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -PSCredential $mycredentials | Should -Be $true
			}
		}
	}

	Context "Test-IshSession over WcfSoapWithWsTrust returns bool" {
		BeforeAll {
			$ishSessionResult = Test-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
		}
		It "GetType()" {
			$ishSessionResult.GetType().Name | Should -BeExactly "Boolean"
		}
	}

	Context "Test-IshSession over WcfSoapWithOpenIdConnect returns bool" {
		BeforeAll {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$ishSessionResult = Test-IshSession -Protocol WcfSoapWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			}
		}
		It "GetType()" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$ishSessionResult.GetType().Name | Should -BeExactly "Boolean"
			}
		}
	}

	Context "Test-IshSession over OpenApiWithOpenIdConnect returns bool" {
		BeforeAll {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$ishSessionResult = Test-IshSession -Protocol OpenApiWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			}
		}
		It "GetType()" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15) { # new service since 15/15.0.0
				$ishSessionResult.GetType().Name | Should -BeExactly "Boolean"
			}
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
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}