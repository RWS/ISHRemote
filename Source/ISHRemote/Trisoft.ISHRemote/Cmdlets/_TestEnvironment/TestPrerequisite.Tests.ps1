#pester:no-parallel # Checks if the remote system is ready which will fail if other tests are already creating folders
BeforeAll {
	$cmdletName = "TestPrerequisite.Tests.ps1"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Test-Prerequisite" -Tags "Read" {
	Context "Package ISHRemote verification" {
		It "Folder ISHRemote exists" {
			Test-Path -Path $moduleFolder | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "ISHRemote.psm1") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "ISHRemote.psd1") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "ISHRemote.Format.ps1xml") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "Trisoft.ISHRemote.dll-Help.xml") | Should-Be $true
		}
		It "Folder ISHRemote/Scripts exists" {
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "Scripts") | Should-Be $true
		}
		It "Folder ISHRemote/Scripts/Public exists" {
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "Scripts/Public") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "Scripts/Public/Expand-ISHParameter.ps1") | Should-Be $true
		}
		It "Folder ISHRemote/Scripts/Private exists" {
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "Scripts/Private") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "Scripts/Private/Register-IshAuxParameterCompleter.ps1") | Should-Be $true
		}
		It "Folder ISHRemote/net48 exists" {
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net48") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net48/Trisoft.ISHRemote.dll") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net48/Trisoft.ISHRemote.xml") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net48/Trisoft.ISHRemote.dll-Help.xml") | Should-Be $true
		}
		It "Folder ISHRemote/net6.0 exists" {
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net6.0") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net6.0/Trisoft.ISHRemote.dll") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net6.0/Trisoft.ISHRemote.xml") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net6.0/Trisoft.ISHRemote.dll-Help.xml") | Should-Be $true
		}
		It "Folder ISHRemote/net10.0 exists" {
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net10.0") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net10.0/Trisoft.ISHRemote.dll") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net10.0/Trisoft.ISHRemote.xml") | Should-Be $true
			Test-Path -Path (Join-Path -Path $moduleFolder -ChildPath "net10.0/Trisoft.ISHRemote.dll-Help.xml") | Should-Be $true
		}
	}

	Context "Assembly loading - No Global Assembly Cache (GAC) fallback on PowerShell Core" {
		It "No assemblies loaded from GAC_MSIL/GAC_32/GAC_64 (only applicable to pwsh.exe/PowerShell Core; Windows PowerShell 5.1/Desktop legitimately uses the GAC)" {
			# Regression test for a customer issue where a Microsoft.IdentityModel.Tokens assembly registered
			# in the GAC (e.g. by Microsoft Intune Management Extension) got resolved instead of ISHRemote's
			# own bundled copy, causing 'FileLoadException: The located assembly's manifest definition does
			# not match the assembly reference' during New-IshSession (WcfSoapWithOpenIdConnect protocol).
			# ISHRemote force-loads its own copies early in AppDomainModuleAssemblyInitializer.OnImport() to
			# win that race; this test asserts that mitigation keeps holding for the whole session, including
			# after Import-Module and New-IshSession ran as part of ISHRemote.PesterSetup.ps1 above.
			if ($PSVersionTable.PSEdition -eq 'Core') {
				$gacAssemblies = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.Location -like '*\Microsoft.NET\assembly\GAC_*' }
				# Fold the offending assembly identity/location into the actual value under test, so a
				# failure reports e.g. "Expected [string] '', but got [string] 'Duende.IdentityModel.OidcClient,
				# Version=7.0.0.0,... from C:\WINDOWS\...\GAC_MSIL\...'" instead of a bare count mismatch
				# that gives no lead on which library or where it came from.
				$offendingAssemblies = ($gacAssemblies | ForEach-Object { "$($_.FullName) from $($_.Location)" }) -join "`r`n"
				$offendingAssemblies | Should-BeEmptyString
			}
		}
	}

	Context "ISHRemote.PesterSetup.Debug.ps1 minimal overwrites" {
		It "baseUrl" {
			$baseUrl | Should-NotBe 'https://ish.example.com'
			$baseUrl | Should-NotBe ''
		}
		It "webServicesBaseUrl" {
			$webServicesBaseUrl | Should-NotBe '$baseUrl/ISHWS/'
			$webServicesBaseUrl | Should-NotBe ''
		}
		It "ishUserName" {
			$ishUserName | Should-NotBe 'myusername'
			$ishUserName | Should-NotBe ''
		}
		It "ishPassword" {
			$ishPassword | Should-NotBe 'mypassword'
			$ishPassword | Should-NotBe ''
		}
		It "amClientId" {
			$amClientId | Should-NotBe 'myserviceaccountclientid'
			$amClientId | Should-NotBe ''
		}
		It "amClientSecret" {
			$amClientSecret | Should-NotBe 'myserviceaccountclientsecret'
			$amClientSecret | Should-NotBe ''
		}
	}

	Context "IshSession (-eq 15) - Validating overwrites of ISHRemote.PesterSetup.Debug.ps1" {
		BeforeAll {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			$ishUser = Get-IshUser
		}
		It "IshSession.Protocol WcfSoapWithOpenIdConnect" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$IshSession.Protocol | Should-Be 'WcfSoapWithOpenIdConnect'
			}
		}
		It "Current IShSession user over ClientId/ClientSecret should match UserName parameter so all tests run under the same account" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$ishUser.UserName | Should-Be $ishUserName
			}
		}
		It "Current IShSession user should be part of VUSERGROUPSYSTEMMANAGEMENT UserGroup" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$ishUser.fusergroup_none_element -like "*VUSERGROUPSYSTEMMANAGEMENT*" | Should-Be $true
			}
		}
		It "Current IShSession user identified over CliendId/ClientSecret should match the IshUserName/IshPassword" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$ishUser.username | Should-Be $ishUserName
			}
		}
		It "IshSession.AuthenticationContext" {
			$ishSession.AuthenticationContext | Should-NotBeNull
		}
		It "IshSession.ClientVersion" {
			$ishSession.ClientVersion | Should-NotBeNull
		}
		It "IshSession.ClientVersion not 0.0.0.0" {
			$ishSession.ClientVersion | Should-NotBe "0.0.0.0"
		}
		It "IshSession.ServerVersion empty (ISHWS down?)" {
			$ishSession.ServerVersion | Should-NotBeNull
		}
		It "IshSession.ServerVersion not 0.0.0.0" {
			$ishSession.ServerVersion | Should-NotBe "0.0.0.0"
		}
	}

	Context "IshFolder - Manual clean up required, probably a lock blocked the previous test clean-up" {
		It "ISHRemote root folder exists" {
			$ishFolder = Get-IshFolder -IshSession $ishSession -FolderPath $folderTestRootPath
			$ishFolder.IshFolderRef -ge 0 | Should-Be $true
		}
		It "ISHRemote root folder should be empty, probably clean up failed test folders and data" {
			$commands = (Get-Command -Module ISHRemote).Name
			$subIshFolders = Get-IshFolder -IshSession $ishSession -FolderPath $folderTestRootPath -Recurse -Depth 2
			foreach($subIshFolderName in $subIshFolders.name)
			{
				if ($commands -contains $subIshFolderName) { $subIshFolderName | Should-Be "" }				 
			}
		}
	}

	Context "ListOfValues - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "Parameter DLANGUAGE labels exist" {
			$ishLovValues = Get-IshLovValue -IshSession $ishSession -LovId DLANGUAGE
			$ishLovValues.Label -contains $ishLngLabel | Should-Be $true
			$ishLovValues.Label -contains $ishLngTarget1Label | Should-Be $true
			$ishLovValues.Label -contains $ishLngTarget2Label | Should-Be $true
			$ishLovValues.Label -contains $ishLngCombination | Should-Be $true
		}
		It "Parameter DLANGUAGE ishLngTarget1 exists" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DLANGUAGE -LovValueId $ishLngTarget1
			$ishLovValue.IshRef | Should-Be $ishLngTarget1
		}
		It "Parameter DLANGUAGE ishLngTarget2 exists" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DLANGUAGE -LovValueId $ishLngTarget2
			$ishLovValue.IshRef | Should-Be $ishLngTarget2
		}
	}

	Context "Statuses - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "Parameter DSTATUS ishStatusDraft exist" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DSTATUS -LovValueId $ishStatusDraft
			$ishLovValue.IshRef | Should-Be $ishStatusDraft
		}
		It "Parameter DSTATUS ishStatusReleased exist" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DSTATUS -LovValueId $ishStatusReleased
			$ishLovValue.IshRef | Should-Be $ishStatusReleased
		}
		It "Status Transition from ishStatusDraft to exists ishStatusReleased" {
			# Direct status transition from $ishStatusDraft (D) to $ishStatusReleased (R) is required by the executing user
			[xml]$stateConfiguration = Get-IshSetting -FieldName FSTATECONFIGURATION
			$fromStatusDraft = $stateConfiguration.InfoShareStates.Transitions.FromStatus | Where-Object ref -eq $ishStatusDraft 
			$toStatusReleased = $fromStatusDraft.ToStatus | Where-Object ref -eq $ishStatusReleased
			if ($toStatusReleased -is [array]) {
				$toStatusReleased.Ref -contains $ishStatusReleased | Should-Be $true
			}
			else {
				$toStatusReleased.ref | Should-Be $ishStatusReleased
			}
		}
	}

	Context "User - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		BeforeAll {
			if ($isLinuxContainerized) {
				# Detecting Containerization; Windows .NET-Framework-based WcfSoapWithWsTrust not supported, so forcing newer route
				$ishSession = New-IshSession -Protocol WcfSoapWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
				$ishUser = Get-IshUser -RequestedMetadata (Set-IshRequestedMetadataField -Level None -Name FUSERGROUP)				
			}
			else {
				$ishSession = New-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
				$ishUser = Get-IshUser -RequestedMetadata (Set-IshRequestedMetadataField -Level None -Name FUSERGROUP)
			}
		}
		It "Current User has UserRole Administrator access" {
			$ishUser.fishuserroles_none_element -like '*VUSERROLEADMINISTRATOR*' | Should-Be $true
		}
		It "Current User has UserGroup System Management access" {
			# Otherwise error [-102009] Unable to complete your request, you are not allowed to alter folder "System".
			$ishUser.fusergroup_none_element -like '*VUSERGROUPSYSTEMMANAGEMENT*' | Should-Be $true
		}
		It "Current User has user role to do Status Transition from ishStatusDraft to ishStatusReleased" {
			# Direct status transition from $ishStatusDraft (D) to $ishStatusReleased (R) is required by the executing user
			# Some systems put Draft to Released transition under user role Administrator and other under  _Testing
			[xml]$stateConfiguration = Get-IshSetting -FieldName FSTATECONFIGURATION
			$fromStatusDraft = $stateConfiguration.InfoShareStates.Transitions.FromStatus | Where-Object ref -eq $ishStatusDraft 
			$fromStatusDraftToReleased = $fromStatusDraft | Where-Object {$_.ToStatus.ref -eq $ishStatusReleased}
			$userRolesRequiredForStatusTransition = $fromStatusDraftToReleased.userrole 
			foreach ($userRole in $userRolesRequiredForStatusTransition) {
				$ishUser.fishuserroles -like "*$($userRole)*" | Should-Be $true
			}
		}
		It "Parameter Author ishUserAuthor exist" {
			$ishUser = Get-IshUser -IshSession $ishSession -Id $ishUserAuthor
			$ishUser.IshRef | Should-Be $ishUserAuthor
		}
	}

	Context "OutputFormat - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "Parameter DITA XML ishOutputFormatDitaXml exist" {
			$ishOUtputFormat = Get-IshOutputFormat -IshSession $ishSession -Id $ishOutputFormatDitaXml
			$ishOUtputFormat.IshRef | Should-Be $ishOutputFormatDitaXml
		}
	}

	Context "BackgroundTask - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "EventType ishEventTypeToPurge for BackgroundTask - Configure purge in Xml Settings BackgroundTask" {
			# Event to be raised by BackgroundTasks tests that is automatically purged by the BackgroundTask service thanks to its Xml Settings configuration
			# $ishEventTypeToPurge = "PUSHTRANSLATIONS"
			[xml]$backgroundTaskConfiguration = Get-IshSetting -FieldName FISHBACKGROUNDTASKCONFIG
			$handler = $backgroundTaskConfiguration.infoShareBackgroundTaskConfig.handlers.handler | Where-Object eventType -eq $ishEventTypeToPurge
			$handler.eventType | Should-Be $ishEventTypeToPurge
		}
	}

	Context "Search - SolrLucene Windows Service should be running, and Crawler service not to avoid lock race conditions" {
		It "SolrLucene query service is running" {
			{ Search-IshDocumentObj -SimpleQuery "*" -MaxHitsToReturn 1 } | Should -Not -Throw
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}