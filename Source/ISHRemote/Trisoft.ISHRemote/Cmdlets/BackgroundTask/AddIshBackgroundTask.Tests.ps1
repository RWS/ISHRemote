BeforeAll {
	$cmdletName = "Add-IshBackgroundTask"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
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
	Context "Add-IshBackgroundTask IshObjectsGroup Parameter IshObject with implicit IshSession since 14SP4/14.0.4" {
		It "Parameter IshObject invalid" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				{ Add-IshBackgroundTask -EventType $ishEventTypeToPurge -IshObject "INVALIDISHOBJECT" } | Should -Throw
			}
		}
		It "Parameter EventType null" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				{ Add-IshBackgroundTask -EventType $null -IshObject  $ishObjects } | Should -Throw
			}
		}
		It "Pipeline IshObject Single" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -IshObject $ishObjectTopic1_1
				$ishObjectTopic1_1.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsParameter.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsParameter.GetType() | Should -BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
				$ishBackgroundTaskIshObjectsParameter.EventType | Should -BeExactly $ishEventTypeToPurge
				$ishBackgroundTaskIshObjectsParameter.userid | Should -BeExactly $ishSession.UserName
			}
		}
		It "Pipeline IshObject Multiple" {
			if (([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) { 
				$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -IshObject $ishObjects
				$ishBackgroundTaskIshObjectsParameter.Count | Should -BeExactly 1
				$ishBackgroundTaskIshObjectsParameter.GetType() | Should -BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
				$ishBackgroundTaskIshObjectsParameter.EventType | Should -BeExactly $ishEventTypeToPurge
				$ishBackgroundTaskIshObjectsParameter.userid | Should -BeExactly $ishSession.UserName
			}
		}
	}

	Context "Add-IshBackgroundTask IshObjectsGroup Pipeline IshObject since 14SP4/14.0.4" {
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
    
    Context "Add-IshBackgroundTask IshObjectsGroup Pipeline IshObject with InputDataTemplate" {
        BeforeAll {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name INPUTDATAID
        }
        It "Pipeline IshObject with InputDataTemplate IshObjectsWithLngRef" {
            if(([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) {
                # Get-IshBackgroundTask is called to get the system field 'INPUTDATAID'
                $backgroundTask = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge -InputDataTemplate IshObjectsWithLngRef |
                Get-IshBackgroundTask -IshSession $ishSession -RequestedMetadata $requestedMetadata
				$backgroundTask.INPUTDATAID -ge 0 | Should -Be $true

                # inputData looks like <ishobjects><ishobject ishtype='ISHMasterDoc' ishref='GUID-X' ishlngref='45679'>...
				$inputData = $ishSession.BackgroundTask25.RetrieveDataObjectByIshDataRefs($backgroundTask.INPUTDATAID)
                $xml = [xml]$inputData
                $cdataNode = $xml.ishbackgroundtaskdataobjects.ishbackgroundtaskdataobject.'#cdata-section'
                $rawCdataContent = $cdataNode
                $decodedContent = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($rawCdataContent))
                $ishObjectsFromInputData = [xml]$decodedContent

                $ishObjectsFromInputData.ishObjects -ne $null | Should -Be $true
				$ishObjectsFromInputData.ishobjects.ChildNodes.Count | Should -Be $ishObjects.LngRef.Count  # all language cards are passed
                $ishObjectsFromInputData.ishObjects.ishObject.Count -ge 0 | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishtype -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishref -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishlogicalref -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishversionref -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishlngref -ne $null | Should -Be $true
            }
        }
        It "Pipeline IshObject with InputDataTemplate IshObjectWithLngRef" {
            if(([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) {
                # Get-IshBackgroundTask is called to get the system field 'INPUTDATAID'
                $backgroundTask = Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge -InputDataTemplate IshObjectWithLngRef -IshObject $ishObjectTopic1_1 |
                Get-IshBackgroundTask -IshSession $ishSession -RequestedMetadata $requestedMetadata
				$backgroundTask.INPUTDATAID -ge 0 | Should -Be $true

                # inputData looks like <ishobject ishtype='ISHMasterDoc' ishref='GUID-X' ishlogicalref='45677' ishversionref='45678' ishlngref='45679'> or <ishobject ishtype='ISHBaseline' ishref='GUID-X' ishbaselineref='45798'>
				$inputData = $ishSession.BackgroundTask25.RetrieveDataObjectByIshDataRefs($backgroundTask.INPUTDATAID)
                $xml = [xml]$inputData
                $cdataNode = $xml.ishbackgroundtaskdataobjects.ishbackgroundtaskdataobject.'#cdata-section'
                $rawCdataContent = $cdataNode
                $decodedContent = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($rawCdataContent))
                $ishObjectFromInputData = [xml]$decodedContent
        
                $ishObjectFromInputData.ChildNodes.Count | Should -Be 1  # first-and-only IshObject will be passed
				$ishObjectFromInputData.ishObject -ne $null | Should -Be $true
                $ishObjectFromInputData.ishObject.ishtype | Should -Be "ISHModule"
                $ishObjectFromInputData.ishObject.ishref | Should -Be "ISHREMOTE-LOGICALID-TOPIC-FORADDBT1"
                $ishObjectFromInputData.ishObject.ishlogicalref -ne $null | Should -Be $true
                $ishObjectFromInputData.ishObject.ishversionref -ne $null | Should -Be $true
                $ishObjectFromInputData.ishObject.ishlngref -ne $null | Should -Be $true
            }
        }
        It "Pipeline IshObject with InputDataTemplate IshObjectsWithIshRef" {
            if(([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) {
				# Using explicit Get-IshFolderContent which does 3 implicit API calls to compare to direct Folder25.GetContents of next test
				$localIshObjects = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -VersionFilter ""

                # Get-IshBackgroundTask is called to get the system field 'INPUTDATAID'
                $backgroundTask = $localIshObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge -InputDataTemplate IshObjectsWithIshRef |
                Get-IshBackgroundTask -IshSession $ishSession -RequestedMetadata $requestedMetadata
				$backgroundTask.INPUTDATAID -ge 0 | Should -Be $true

                # inputData looks like <ishobjects><ishobject ishtype='ISHMasterDoc' ishref='GUID-X'>...
				$inputData = $ishSession.BackgroundTask25.RetrieveDataObjectByIshDataRefs($backgroundTask.INPUTDATAID)
                $xml = [xml]$inputData
                $cdataNode = $xml.ishbackgroundtaskdataobjects.ishbackgroundtaskdataobject.'#cdata-section'
                $rawCdataContent = $cdataNode
                $decodedContent = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($rawCdataContent))
                $ishObjectsFromInputData = [xml]$decodedContent
        
                $ishObjectsFromInputData.ishObjects -ne $null | Should -Be $true
				$ishObjectsFromInputData.ishobjects.ChildNodes.Count | Should -Be ($ishObjects.IshRef | Select-Object -Unique).Count  # all unique LogicalIds are passed
                $ishObjectsFromInputData.ishObjects.ishObject.Count -ge 0 | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishtype -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishref -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishlogicalref -eq $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishversionref -eq $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishlngref -eq $null | Should -Be $true
            }
        }
		It "Pipeline IShObject with InputDataTemplate IshObjectsWithIshRef over Folder25.GetContents [SCTCM-3506]" {
			if(([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) {
				# As Get-IshFolderContent implicitly does 3 API calls:
				# 1. Folder25.GetContents(returnFolderId);
				# 2. DocumentObj25.RetrieveVersionMetadata(logicalIdBatch.ToArray(), VersionFilter, "");
				# 3. DocumentObj25.RetrieveMetadataByIshVersionRefs(versionRefs.ToArray(), DocumentObj25ServiceReference.StatusFilter.ISHNoStatusFilter, metadataFilterFields.ToXml(), requestedMetadata.ToXml());
				# The idea rose to bypass the 2nd and 3rd call to optimize for throughput. This however does introduce Public.IshObject dependency which we try to test here.
                [xml]$xmlIshObjects = $ishSession.Folder25.GetContents($ishFolderTopic.IshFolderRef)
				$localIshObjects = @()
				foreach($xmlIshObject in $xmlIshObjects.ishobjects.ishobject)
				{
					$ishObject = New-Object Trisoft.ISHRemote.Objects.Public.IshObject -ArgumentList @(,$xmlIshObject)
					$localIshObjects += $ishObject
				}
				# Regular ISHRemote would have fully initialized (logical-version-language) $ishObjects,
				# these $localIshObjects only contain logical information, sufficient for the Add-IShBackgroundTask cmdlet

				# Get-IshBackgroundTask is called to get the system field 'INPUTDATAID'
                $backgroundTask = $localIshObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge -InputDataTemplate IshObjectsWithIshRef |
                Get-IshBackgroundTask -IshSession $ishSession -RequestedMetadata $requestedMetadata
				$backgroundTask.INPUTDATAID -ge 0 | Should -Be $true

                # inputData looks like <ishobjects><ishobject ishtype='ISHMasterDoc' ishref='GUID-X'>...
				$inputData = $ishSession.BackgroundTask25.RetrieveDataObjectByIshDataRefs($backgroundTask.INPUTDATAID)
                $xml = [xml]$inputData
                $cdataNode = $xml.ishbackgroundtaskdataobjects.ishbackgroundtaskdataobject.'#cdata-section'
                $rawCdataContent = $cdataNode
                $decodedContent = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($rawCdataContent))
                $ishObjectsFromInputData = [xml]$decodedContent
        
                $ishObjectsFromInputData.ishObjects -ne $null | Should -Be $true
				$ishObjectsFromInputData.ishobjects.ChildNodes.Count | Should -Be ($ishObjects.IshRef | Select-Object -Unique).Count  # all unique LogicalIds are passed
                $ishObjectsFromInputData.ishObjects.ishObject.Count -ge 0 | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishtype -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishref -ne $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishlogicalref -eq $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishversionref -eq $null | Should -Be $true
                $ishObjectsFromInputData.ishObjects.ishObject[0].ishlngref -eq $null | Should -Be $true
            }
        }
        It "Pipeline IShObject with InputDataTemplate EventDataWithIshLngRefs, typically skipped to avoid server file artefacts" -Skip {
            if(([Version]$ishSession.ServerVersion).Major -ge 15 -or (([Version]$ishSession.ServerVersion).Major -ge 14 -and ([Version]$ishSession.ServerVersion).Revision -ge 4)) {
                # Test is correct but skipped as it would generate server-side export folders like 'C:\InfoShare\Data\ExportService\Data\DataExports\20240704134839851Z\en' that nobody would clean up
                # And BackgroundTask service would most likely pick up the export which this Pester test already deleted the data for resulting in errors like 'Object with language card id 516319 could not be found.'

                # Get-IshBackgroundTask is called to get the system field 'INPUTDATAID'
                $backgroundTask = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType "FOLDEREXPORT" -InputDataTemplate EventDataWithIshLngRefs |
                Get-IshBackgroundTask -IshSession $ishSession -RequestedMetadata $requestedMetadata
			    $backgroundTask.INPUTDATAID -ge 0 | Should -Be $true

                # inputData looks like <eventdata><lngcardids>13043819, 13058357, 14246721, 13058260</lngcardids></eventdata>, decided to drop optional <foldername>
			    $inputData = $ishSession.BackgroundTask25.RetrieveDataObjectByIshDataRefs($backgroundTask.INPUTDATAID)
                $xml = [xml]$inputData
                $cdataNode = $xml.ishbackgroundtaskdataobjects.ishbackgroundtaskdataobject.'#cdata-section'
                $rawCdataContent = $cdataNode
                $decodedContent = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($rawCdataContent))
                $eventdataFromInputData = [xml]$decodedContent

                $eventdataFromInputData.eventdata -ne $null | Should -Be $true
                $eventdataFromInputData.eventdata.lngcardids -ne $null | Should -Be $true
            }
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

