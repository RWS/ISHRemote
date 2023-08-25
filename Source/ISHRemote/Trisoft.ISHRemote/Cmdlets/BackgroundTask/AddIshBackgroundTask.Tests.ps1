BeforeAll {
	$cmdletName = "Add-IshBackgroundTask"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Add-IshBackgroundTask" -Tags "Create" {
	BeforeAll {
		$ishFolderTestRootOriginal = Get-IshFolder -IShSession $ishSession -FolderPath $folderTestRootPath
		$folderIdTestRootOriginal = $ishFolderTestRootOriginal.IshFolderRef
		$folderTypeTestRootOriginal = $ishFolderTestRootOriginal.IshFolderType
		$ownedByTestRootOriginal = $ishFolderTestRootOriginal.fusergroup_none_element
		$readAccessTestRootOriginal = $ishFolderTestRootOriginal.readaccess_none_element
		
		Write-Debug("folderIdTestRootOriginal[" +  $ishFolderTestRootOriginal.IshFolderRef + "] folderTypeTestRootOriginal[" + $folderTypeTestRootOriginal + "]")
		$global:ishBackgroundTaskCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishBackgroundTaskCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
							Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

		$ishObjectTopic1_1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT1" -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
		$ishObjectTopic1_2 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT1" -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
		$ishObjectTopic1_3 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT1" -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent

		$ishObjectTopic2_1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT2" -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
		$ishObjectTopic2_2 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT2" -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
		$ishObjectTopic2_3 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT2" -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent

		$ishObjectTopic3_1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT3" -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
		$ishObjectTopic3_2 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT3" -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
		$ishObjectTopic3_3 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT3" -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
		$ishObjectTopic3_4 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "ISHREMOTE-LOGICALID-TOPIC-FORADDBT3" -Version '4' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent

		$ishObjects = $ishFolderTopic | Get-IshFolderContent -IshSession $ishSession -VersionFilter ""
		$createdLngRefs = $ishObjects | select -ExpandProperty LngRef
	}
	Context "Add-IshBackgroundTask IshObjectsGroup Parameter IshObject with implicit IshSession since 14SP4/14.0.4 =< $(([Version]$ishSession.ServerVersion))" {
		if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
			It "Parameter IshObject invalid" {
				{ Add-IshBackgroundTask -EventType $ishEventTypeToPurge -IshObject "INVALIDISHOBJECT" } | Should -Throw
			}
			It "Parameter EventType null" {
				{ Add-IshBackgroundTask -EventType $null -IshObject  $ishObjects } | Should -Throw
			}
			It "Pipeline IshObject Single" {
				$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -IshObject $ishObjectTopic1_1
				$ishObjectTopic1_1.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsParameter.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsParameter.GetType() | Should -BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
				$ishBackgroundTaskIshObjectsParameter.EventType | Should -BeExactly $ishEventTypeToPurge
				$ishBackgroundTaskIshObjectsParameter.userid | Should -BeExactly $ishSession.UserName
			}
			It "Pipeline IshObject Multiple" {
				$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -IshObject $ishObjects
				$ishBackgroundTaskIshObjectsParameter.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsParameter.GetType() | Should -BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
				$ishBackgroundTaskIshObjectsParameter.EventType | Should -BeExactly $ishEventTypeToPurge
				$ishBackgroundTaskIshObjectsParameter.userid | Should -BeExactly $ishSession.UserName
			}
		}
	}

	Context "Add-IshBackgroundTask IshObjectsGroup Pipeline IshObject since 14SP4/14.0.4 =< $(([Version]$ishSession.ServerVersion))" {
		BeforeAll {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishBackgroundTaskIshObjectsPipeline = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
			}
			$savedMetadataBatchSize = $ishSession.MetadataBatchSize
		}
		It "Add-IshBackgroundTask returns IshBackgroundTask object" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishObjects.Count | Should -BeExactly 10
				$ishBackgroundTaskIshObjectsPipeline.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsPipeline.GetType().Name | Should -BeExactly "IshBackgroundTask"
				$ishBackgroundTaskIshObjectsPipeline.EventType | Should -BeExactly $ishEventTypeToPurge
				$ishBackgroundTaskIshObjectsPipeline.userid | Should -BeExactly $ishSession.UserName
			}
		}
		It "Pipeline IshObject Single" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishBackgroundTaskIshObjectsPipeline = $ishObjectTopic1_1 | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
				$ishObjectTopic1_1.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsPipeline.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsPipeline.GetType().Name | Should -BeExactly "IshBackgroundTask"
				$ishBackgroundTaskIshObjectsPipeline.EventType | Should -BeExactly $ishEventTypeToPurge
				$ishBackgroundTaskIshObjectsPipeline.userid | Should -BeExactly $ishSession.UserName
			}
		}
		It "Pipeline IshObject MetadataBatchSize[2] with LogicalId grouping" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishSession.MetadataBatchSize = 2
				$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
				$ishBackgroundTasks.Count | Should -BeExactly 3
			}
		}
		It "Pipeline IshObject MetadataBatchSize[4] with LogicalId grouping" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishSession.MetadataBatchSize = 4
				$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
				$ishBackgroundTasks.Count | Should -BeExactly 3
			}
		}
		It "Pipeline IshObject MetadataBatchSize[6] with LogicalId grouping" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishSession.MetadataBatchSize = 6
				$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
				$ishBackgroundTasks.Count | Should -BeExactly 2
			}
		}
		It "Pipeline IshObject MetadataBatchSize[10] with LogicalId grouping" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishSession.MetadataBatchSize = 10
				$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
				$ishBackgroundTasks.Count | Should -BeExactly 1
			}
		}
		AfterAll {
			$ishSession.MetadataBatchSize = $savedMetadataBatchSize
		}
	}
	
	Context "Add-IshBackgroundTask ParameterGroup" {
		BeforeAll {
			# If you get the below error, it means you configured default purge operation $ishEventTypetoPurge (defaults to PUSHTRANSLATIONS in ISHRemote.PesterSetup.ps1) away
			# FaultException`1: [-105001] The parameter eventType with value "PUSHTRANSLATIONS" is invalid. Make sure a handler with the eventType is configured in the Background Task Configuration XML [105001;InvalidParameter]
			$rawData = "<data><dataExample>Text</dataExample></data>"
			$eventDescription = "Created by Powershell and ISHRemote"
			$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $eventDescription -RawInputData $rawData
		}
		It "Add-IshBackgroundTask returns IshBackgroundTask object" {
			$ishBackgroundTaskParameters.Count | Should -BeExactly 1
			$ishBackgroundTaskParameters.GetType().Name | Should -BeExactly "IshBackgroundTask"
			$ishBackgroundTaskParameters.EventType | Should -BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskParameters.userid | Should -BeExactly $ishSession.UserName
		}
		It "Parameter EventDescription null" {
			{ Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $null -RawInputData $rawData } | Should -Throw
		}
		It "Parameter EventType null" {
			{ Add-IshBackgroundTask -EventType $null -EventDescription $eventDescription -RawInputData $rawData } | Should -Throw
		}
		It "Parameter RawInputData null" {
			{ Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $eventDescription -RawInputData $null } | Should -Throw
		}
		It "Parameter StartAfter Tommorrow" {
			$dateTomorrow = (Get-Date).AddDays(1)
			$eventDescription = $eventDescription `
			                  +" StartAfter["+$dateTomorrow+"]" `
							  +" CultureInfo.LCID["+([System.Globalization.CultureInfo]::CurrentCulture).LCID+"]" `
							  +" CultureInfo.Name["+([System.Globalization.CultureInfo]::CurrentCulture).Name+"]" `
							  +" CultureInfo...ShortDatePattern["+([System.Globalization.CultureInfo]::CurrentCulture).DateTimeFormat.ShortDatePattern+"]"
			$ishBackgroundTaskStartsAfter = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $eventDescription -RawInputData $rawData -StartAfter $dateTomorrow
			$ishBackgroundTaskStartsAfter.Count | Should -BeExactly 1
			($ishBackgroundTaskStartsAfter.executeafterdate -eq $ishBackgroundTaskStartsAfter.creationdate) | Should -Be $false
			$ishBackgroundTaskStartsAfter.GetType().Name | Should -BeExactly "IshBackgroundTask"
			$ishBackgroundTaskStartsAfter.EventType | Should -BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskStartsAfter.userid | Should -BeExactly $ishSession.UserName
			# Verify returned submitted IshBackgroundTask.StartsAfter date matches provided tomorrow StartsAfter
			$ishBackgroundTaskStartsAfter.executeafterdate -like "*-*-*T*:*:*" | Should -Be $true
			$ishBackgroundTaskStartsAfter.executeafterdate.Substring(0, 11) | Should -Be ($dateTomorrow.ToString("yyyy-MM-ddT"))
			$retrievedExecuteAfter = New-Object DateTime
			$conversionResult = [DateTime]::TryParseExact($ishBackgroundTaskStartsAfter.executeafterdate, "yyyy-MM-ddTHH:mm:ss", [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::None, [ref]$retrievedExecuteAfter)
			$conversionResult | Should -BeExactly $true
			$retrievedExecuteAfter.Year | Should -Be $dateTomorrow.Year
			$retrievedExecuteAfter.Month | Should -Be $dateTomorrow.Month
			$retrievedExecuteAfter.Day | Should -Be $dateTomorrow.Day
			# Hour and Minute check are skipped; if Client and Server are in different timezone, you get different hours or minutes back
			# $retrievedExecuteAfter.Hour | Should -Be $dateTomorrow.Hour
			# $retrievedExecuteAfter.Minute | Should -Be $dateTomorrow.Minute
			$retrievedExecuteAfter.Second | Should -Be $dateTomorrow.Second
			$retrievedExecuteAfter.ToString("dd/MM/yyyy") | Should -BeExactly $dateTomorrow.ToString("dd/MM/yyyy")	
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession -VersionFilter "" | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}

