﻿BeforeAll {
	$cmdletName = "TestPrerequisite.Tests.ps1"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Test-Prerequisite" -Tags "Read" {
	Context "IshSession - Required overwrite in ISHRemote.PesterSetup.Debug.ps1" {
		BeforeAll {
			$ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
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
		It "Current IShSession user should be part of VUSERGROUPSYSTEMMANAGEMENT UserGroup" {
			$ishUser = Get-IshUser
			$ishUser.fusergroup_none_element -like "*VUSERGROUPSYSTEMMANAGEMENT*"
		}
	}

	Context "IshFolder - Manual clean up required, probably a lock blocked the previous test clean-up" {
		It "ISHRemote root folder exists" {
			$ishFolder = Get-IshFolder -IshSession $ishSession -FolderPath $folderTestRootPath
			$ishFolder.IshFolderRef -ge 0 | Should -Be $true
		}
		It "ISHRemote root folder has no failed test folders" {
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
		It "Status Transition from ishStatusDraft to exists ishStatusReleased" -Skip {
			# Direct status transition from $ishStatusDraft (D) to $ishStatusReleased (R) is required by the executing user
			# Test over xml settings
		}
	}

	Context "User - Potential overwrite in ISHRemote.PesterSetup.Debug.ps1" {
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
		It "EventType ishEventTypeToPurge for BackgroundTask - Configure purge in Xml Settings BackgroundTask" -Skip {
			# Event to be raised by BackgroundTasks tests that is automatically purged by the BackgroundTask service thanks to its Xml Settings configuration
			# $ishEventTypeToPurge = "PUSHTRANSLATIONS"
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