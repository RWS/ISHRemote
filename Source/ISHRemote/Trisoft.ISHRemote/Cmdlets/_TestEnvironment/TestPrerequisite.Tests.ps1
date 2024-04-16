BeforeAll {
	$cmdletName = "TestPrerequisite.Tests.ps1"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Test-Prerequisite" -Tags "Read" {
	Context "ISHRemote.PesterSetup.Debug.ps1 minimal overwrites" {
		It "baseUrl" {
			$baseUrl | Should -Not -Be 'https://ish.example.com'
			$baseUrl | Should -Not -Be ''
		}
		It "webServicesBaseUrl" {
			$webServicesBaseUrl | Should -Not -Be '$baseUrl/ISHWS/'
			$webServicesBaseUrl | Should -Not -Be ''
		}
		It "ishUserName" {
			$ishUserName | Should -Not -Be 'myusername'
			$ishUserName | Should -Not -Be ''
		}
		It "ishPassword" {
			$ishPassword | Should -Not -Be 'mypassword'
			$ishPassword | Should -Not -Be ''
		}
		It "amClientId" {
			$amClientId | Should -Not -Be 'myserviceaccountclientid'
			$amClientId | Should -Not -Be ''
		}
		It "amClientSecret" {
			$amClientSecret | Should -Not -Be 'myserviceaccountclientsecret'
			$amClientSecret | Should -Not -Be ''
		}
	}

	Context "IshSession (-lt 16) - Validating overwrites of ISHRemote.PesterSetup.Debug.ps1" {
		BeforeAll {
			$ishSession = New-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
			$ishUser = Get-IshUser
		}
		It "IshSession.Protocol WcfSoapWithWsTrust" {
			$IshSession.Protocol | Should -Be 'WcfSoapWithWsTrust'
		}
		It "Current IShSession user should be part of VUSERGROUPSYSTEMMANAGEMENT UserGroup" {
			$ishUser.fusergroup_none_element -like "*VUSERGROUPSYSTEMMANAGEMENT*" | Should -Be $true
		}
		It "IshSession.AuthenticationContext" {
			$ishSession.AuthenticationContext | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ClientVersion" {
			$ishSession.ClientVersion | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ClientVersion not 0.0.0.0" {
			$ishSession.ClientVersion | Should -Not -Be "0.0.0.0"
		}
		It "IshSession.ServerVersion empty (ISHWS down?)" {
			$ishSession.ServerVersion | Should -Not -BeNullOrEmpty
		}
		It "IshSession.ServerVersion not 0.0.0.0" {
			$ishSession.ServerVersion | Should -Not -Be "0.0.0.0"
		}
	}

	Context "IshSession (-eq 15) - Validating overwrites of ISHRemote.PesterSetup.Debug.ps1" {
		BeforeAll {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret
			$ishUser = Get-IshUser
		}
		It "IshSession.Protocol WcfSoapWithOpenIdConnect" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$IshSession.Protocol | Should -Be 'WcfSoapWithOpenIdConnect'
			}
		}
		It "Current IShSession user over ClientId/ClientSecret should match UserName parameter so all tests run under the same account" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$ishUser.UserName | Should -Be $ishUserName
			}
		}
		It "Current IShSession user should be part of VUSERGROUPSYSTEMMANAGEMENT UserGroup" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$ishUser.fusergroup_none_element -like "*VUSERGROUPSYSTEMMANAGEMENT*" | Should -Be $true
			}
		}
		It "Current IShSession user identified over CliendId/ClientSecret should match the IshUserName/IshPassword" {
			if (([Version]$ishSession.ServerVersion).Major -eq 15) { 
				$ishUser.username | Should -Be $ishUserName
			}
		}
	}

	Context "IshFolder - Manual clean up required, probably a lock blocked the previous test clean-up" {
		It "ISHRemote root folder exists" {
			$ishFolder = Get-IshFolder -IshSession $ishSession -FolderPath $folderTestRootPath
			$ishFolder.IshFolderRef -ge 0 | Should -Be $true
		}
		It "ISHRemote root folder should be empty, probably clean up failed test folders and data" {
			$commands = (Get-Command -Module ISHRemote).Name
			$subIshFolders = Get-IshFolder -IshSession $ishSession -FolderPath $folderTestRootPath -Recurse -Depth 2
			foreach($subIshFolderName in $subIshFolders.name)
			{
				if ($commands -contains $subIshFolderName) { $subIshFolderName | Should -Be "" }				 
			}
		}
	}

	Context "ListOfValues - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "Parameter DLANGUAGE labels exist" {
			$ishLovValues = Get-IshLovValue -IshSession $ishSession -LovId DLANGUAGE
			$ishLovValues.Label -contains $ishLngLabel | Should -Be $true
			$ishLovValues.Label -contains $ishLngTarget1Label | Should -Be $true
			$ishLovValues.Label -contains $ishLngTarget2Label | Should -Be $true
			$ishLovValues.Label -contains $ishLngCombination | Should -Be $true
		}
		It "Parameter DLANGUAGE ishLngTarget1 exists" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DLANGUAGE -LovValueId $ishLngTarget1
			$ishLovValue.IshRef | Should -Be $ishLngTarget1
		}
		It "Parameter DLANGUAGE ishLngTarget2 exists" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DLANGUAGE -LovValueId $ishLngTarget2
			$ishLovValue.IshRef | Should -Be $ishLngTarget2
		}
	}

	Context "Statuses - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "Parameter DSTATUS ishStatusDraft exist" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DSTATUS -LovValueId $ishStatusDraft
			$ishLovValue.IshRef | Should -Be $ishStatusDraft
		}
		It "Parameter DSTATUS ishStatusReleased exist" {
			$ishLovValue = Get-IshLovValue -IshSession $ishSession -LovId DSTATUS -LovValueId $ishStatusReleased
			$ishLovValue.IshRef | Should -Be $ishStatusReleased
		}
		It "Status Transition from ishStatusDraft to exists ishStatusReleased" {
			# Direct status transition from $ishStatusDraft (D) to $ishStatusReleased (R) is required by the executing user
			[xml]$stateConfiguration = Get-IshSetting -FieldName FSTATECONFIGURATION
			$fromStatusDraft = $stateConfiguration.InfoShareStates.Transitions.FromStatus | Where-Object ref -eq $ishStatusDraft 
			$toStatusReleased = $fromStatusDraft.ToStatus | Where-Object ref -eq $ishStatusReleased
			if ($toStatusReleased -is [array]) {
				$toStatusReleased.Ref -contains $ishStatusReleased | Should -Be $true
			}
			else {
				$toStatusReleased.ref | Should -Be $ishStatusReleased
			}
		}
	}

	Context "User - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		BeforeAll {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
			$ishUser = Get-IshUser -RequestedMetadata (Set-IshRequestedMetadataField -Level None -Name FUSERGROUP)
		}
		It "Current User has UserRole Administrator access" {
			$ishUser.fishuserroles_none_element -like '*VUSERROLEADMINISTRATOR*' | Should -Be $true
		}
		It "Current User has UserGroup System Management access" {
			# Otherwise error [-102009] Unable to complete your request, you are not allowed to alter folder "System".
			$ishUser.fusergroup_none_element -like '*VUSERGROUPSYSTEMMANAGEMENT*' | Should -Be $true
		}
		It "Parameter Author ishUserAuthor exist" {
			$ishUser = Get-IshUser -IshSession $ishSession -Id $ishUserAuthor
			$ishUser.IshRef | Should -Be $ishUserAuthor
		}
	}

	Context "OutputFormat - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "Parameter DITA XML ishOutputFormatDitaXml exist" {
			$ishOUtputFormat = Get-IshOutputFormat -IshSession $ishSession -Id $ishOutputFormatDitaXml
			$ishOUtputFormat.IshRef | Should -Be $ishOutputFormatDitaXml
		}
	}

	Context "BackgroundTask - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		It "EventType ishEventTypeToPurge for BackgroundTask - Configure purge in Xml Settings BackgroundTask" {
			# Event to be raised by BackgroundTasks tests that is automatically purged by the BackgroundTask service thanks to its Xml Settings configuration
			# $ishEventTypeToPurge = "PUSHTRANSLATIONS"
			[xml]$backgroundTaskConfiguration = Get-IshSetting -FieldName FISHBACKGROUNDTASKCONFIG
			$handler = $backgroundTaskConfiguration.infoShareBackgroundTaskConfig.handlers.handler | Where-Object eventType -eq $ishEventTypeToPurge
			$handler.eventType | Should -Be $ishEventTypeToPurge
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