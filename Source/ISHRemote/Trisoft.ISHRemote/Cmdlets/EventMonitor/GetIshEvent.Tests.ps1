BeforeAll {
	$cmdletName = "Get-IshEvent"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshEvent" -Tags "Create" {
	BeforeAll {
		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FNAME" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "FDOCUMENTTYPE" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element 
		$ishFolderTestRootOriginal = Get-IshFolder -IShSession $ishSession -FolderPath $folderTestRootPath -RequestedMetadata $requestedMetadata
		$folderIdTestRootOriginal = $ishFolderTestRootOriginal.IshFolderRef
		$folderTypeTestRootOriginal = $ishFolderTestRootOriginal.IshFolderType
		Write-Debug ("folderIdTestRootOriginal[" + $folderIdTestRootOriginal + "] folderTypeTestRootOriginal[" + $folderTypeTestRootOriginal + "]")
		$ownedByTestRootOriginal = Get-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField
		$readAccessTestRootOriginal = (Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Separator)

		$global:ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
							Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		# Forcing a status transition to release, triggers Translation Management which means a BackgroundTask and EventMonitor entry
		$ishObject = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
					Set-IshDocumentObj -IshSession $ishSession -Metadata (Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusReleased)
		$allProgressMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name PROGRESSID | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name EVENTID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name CREATIONDATE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name MODIFICATIONDATE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name EVENTTYPE | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name DESCRIPTION |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name STATUS -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name STATUS -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name USERID -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name USERID -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name PARENTPROGRESSID | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name MAXIMUMPROGRESS |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name CURRENTPROGRESS
		$allDetailMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name DETAILID | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name PROGRESSID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name CREATIONDATE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name HOSTNAME |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name ACTION | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name DESCRIPTION |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name STATUS -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name STATUS -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTLEVEL -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTLEVEL -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name PROCESSID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name THREADID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTDATATYPE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTDATASIZE
		$allMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name PROGRESSID | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name EVENTID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name CREATIONDATE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name MODIFICATIONDATE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name EVENTTYPE | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name DESCRIPTION |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name STATUS -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name STATUS -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name USERID -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name USERID -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name PARENTPROGRESSID | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name MAXIMUMPROGRESS |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name CURRENTPROGRESS |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name DETAILID | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name PROGRESSID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name CREATIONDATE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name HOSTNAME |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name ACTION | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name DESCRIPTION |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name STATUS -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name STATUS -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTLEVEL -ValueType Value |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTLEVEL -ValueType Element |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name PROCESSID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name THREADID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTDATATYPE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Detail -Name EVENTDATASIZE
	}
	Context "Get-IshEvent" {
		BeforeAll {
			$metadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name PROGRESSID | 
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name EVENTID |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name EVENTTYPE |
						Set-IshRequestedMetadataField -IshSession $ishSession -Level Progress -Name STATUS
			$ishEvent = (Get-IshEvent -IshSession $ishSession -UserFilter All -RequestedMetadata $metadata)[0]
		}
		It "GetType().Name" {
			$ishEvent.GetType().Name | Should -BeExactly "IshEvent"
		}
		It "ishObject.IshField" {
			$ishEvent.IshField | Should -Not -BeNullOrEmpty
		}
		It "ishObject.IshRef" {
			$ishEvent.IshRef | Should -Not -BeNullOrEmpty
		}
		# Double check following 2 ReferenceType enum usage 
		It "ishEvent.ProgressRef" {
			$ishEvent.ProgressRef | Should -Not -BeNullOrEmpty
		}
		#It "ishEvent.DetailRef" {  # Requires BackgroundTask to be running to get detail entries
		#	$ishEvent.DetailRef | Should -Not -BeNullOrEmpty
		#}
		It "ishEvent ConvertTo-Json" {
			(ConvertTo-Json $ishEvent).Length -gt 2 | Should -Be $true
		}
		It "Parameter IshSession/ModifiedSince/UserFilter invalid" {
			{ Get-IshEvent -IShSession "INVALIDISHSESSION" -ModifiedSince "INVALIDDATE" -UserFilter "INVALIDUSERFILTER" } | Should -Throw
		}
		It "Parameter RequestedMetadata/MetadataFile invalid" {
			{ Get-IshEvent -IShSession $ishSession -RequestedMetadata "INVALIDMETADATA" -MetadataFilter "INVALIDFILTER"  } | Should -Throw
		}
		It "Parameter IshSession/UserFilter/MetadataFilter are optional" {
			$ishEvent = (Get-IshEvent -ModifiedSince ((Get-Date).AddMinutes(-10)) -RequestedMetadata $allProgressMetadata)[0]
			($ishEvent | Get-IshMetadataField -IshSession $ishSession -Level Progress -Name EVENTID -ValueType Value).Length -gt 0 | Should -Be $true
			#($ishEvent | Get-IshMetadataField -IshSession $ishSession -Level Progress -Name USERID -ValueType Element).StartsWith('VUSER') | Should -Be $false  # unexpected but ValueType Element is not returned by the API call
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$oldDefaultRequestedMetadata = $ishSession.DefaultRequestedMetadata
			$ishSession.DefaultRequestedMetadata = "Descriptive"
			$ishEvent = (Get-IshEvent -IShSession $ishSession)[0]
			$ishEvent.IshField.Count | Should -Be 2
			$ishSession.DefaultRequestedMetadata = "Basic"
			$ishEvent = (Get-IshEvent -IShSession $ishSession)[0]
			$ishEvent.status.Length -gt 0 | Should -Be $true
			if((([Version]$ishSession.ServerVersion).Major -eq 15 -and ([Version]$ishSession.ServerVersion).Minor -ge 1) -or ([Version]$ishSession.ServerVersion).Major -ge 16) {
				$ishEvent.IshField.Count | Should -Be 10
			} else {
				$ishEvent.IshField.Count | Should -Be 9
			}
			$ishSession.DefaultRequestedMetadata = "All"
			$ishEvent = (Get-IshEvent -IShSession $ishSession)[0]
			if((([Version]$ishSession.ServerVersion).Major -eq 15 -and ([Version]$ishSession.ServerVersion).Minor -ge 1) -or ([Version]$ishSession.ServerVersion).Major -ge 16) {
				$ishEvent.IshField.Count | Should -Be 11
			} else {
				$ishEvent.IshField.Count | Should -Be 10
			}
			$ishSession.DefaultRequestedMetadata = $oldDefaultRequestedMetadata
		}
		It "Parameter ModifiedSince is now" {
			(Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(1)) -UserFilter All).Count | Should -Be 0
		}
		It "Parameter RequestedMetadata only all of Progress level" {
			$ishEvent = (Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter All -RequestedMetadata $allProgressMetadata)[0]
			$ishEvent.ProgressRef -gt 0 | Should -Be $true
			#$ishEvent.DetailRef -gt 0 | Should -Be $true
			if((([Version]$ishSession.ServerVersion).Major -eq 15 -and ([Version]$ishSession.ServerVersion).Minor -ge 1) -or ([Version]$ishSession.ServerVersion).Major -ge 16) {
				$ishEvent.IshField.Count | Should -Be 12
			} else {
				$ishEvent.IshField.Count | Should -Be 10
			}
		}
		It "Parameter RequestedMetadata only all of Detail level" {
			$ishEvent = (Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter All -RequestedMetadata $allDetailMetadata)[0]
			$ishEvent.ProgressRef -gt 0 | Should -Be $true
			$ishEvent.DetailRef -gt 0 | Should -Be $true
			$ishEvent.IshField.Count -ge 20 | Should -Be $true  # Perhaps expected 10 Progress level fields, but Get-IshEvent currently always retrieves details as well
		}
		It "Parameter RequestedMetadata PipelineObjectPreference=PSObjectNoteProperty" {
			$ishSession.PipelineObjectPreference | Should -Be "PSObjectNoteProperty"
			$ishEvent = (Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter All -RequestedMetadata $allMetadata)[0]
			$ishEvent.GetType().Name | Should -BeExactly "IshEvent"  # and not PSObject
			[bool]($ishEvent.PSobject.Properties.name -match "status") | Should -Be $true
			[bool]($ishEvent.PSobject.Properties.name -match "userid") | Should -Be $true
			[bool]($ishEvent.PSobject.Properties.name -match "modificationdate") | Should -Be $true
			[bool]($ishEvent.PSobject.Properties.name -match "status_detail_value") | Should -Be $true
			$ishEvent.modificationdate -like "*/*" | Should -Be $false  # It should be sortable date format: yyyy-MM-ddTHH:mm:ss
		}
		It "Parameter RequestedMetadata PipelineObjectPreference=Off" {
		    $pipelineObjectPreference = $ishSession.PipelineObjectPreference
			$ishSession.PipelineObjectPreference = "Off"
			$ishEvent = (Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter All -RequestedMetadata $allMetadata)[0]
			$ishEvent.GetType().Name | Should -BeExactly "IshEvent"
			[bool]($ishEvent.PSobject.Properties.name -match "status") | Should -Be $false
			[bool]($ishEvent.PSobject.Properties.name -match "userid") | Should -Be $false
			[bool]($ishEvent.PSobject.Properties.name -match "modificationdate") | Should -Be $false
			[bool]($ishEvent.PSobject.Properties.name -match "status_detail_value") | Should -Be $false
			$ishSession.PipelineObjectPreference = $pipelineObjectPreference
		}
		It "Parameter MetadataFilter" {
			$ishEvent = (Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter All -RequestedMetadata $allMetadata)[0]
			$filterMetadata = Set-IshMetadataFilterField -IshSession $ishSession -Level Progress -Name EVENTID -Value ($ishEvent | Get-IshMetadataField -IshSession $ishSession -Level Progress -Name EVENTID)
			                # | Set-IshMetadataFilterField -IshSession $ishSession -Level Progress -Name USERID -Value ($ishEvent | Get-IshMetadataField -IshSession $ishSession -Level Progress -Name USERID)  # Seems just like higher that USERID by valuetype retrieval and filtering are not working
			$ishEventArray = Get-IshEvent -IshSession $ishSession -MetadataFilter $filterMetadata
			#Write-Host ("ishEvent.IshRef["+ $ishEvent.IshRef + "] ishEventArray.IshRef["+ $ishEvent.IshRef + "]")
			$ishEventArray.Count -ge 1 | Should -Be $true
		}
		It "Parameter IshEvent invalid" {
			{ Get-IshEvent -IshSession $ishSession -IshEvent "INVALIDISHEVENT" } | Should -Throw
		}
		It "Parameter IshEvent Single" {
			$ishEvent = (Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter Current)[0]
			$eventId = $ishEvent | Get-IshMetadataField -IshSession $ishSession -Level Progress -Name EVENTID
			$ishEventArray = Get-IshEvent -IshSession $ishSession -IshEvent $ishEvent
			$ishEventArray.Count -ge 1 | Should -Be $true
			$ishEventArray.IshRef | Should -Be $eventId
		}
		<# TODO [Could] It "Parameter IshEvent Multiple" {
		}
		#>
		It "Pipeline IshEvent Single" {
			$ishEvent = (Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter Current)[0]
			$eventId = $ishEvent | Get-IshMetadataField -IshSession $ishSession -Level Progress -Name EVENTID
			$ishEventArray = $ishEvent | Get-IshEvent -IshSession $ishSession
			$ishEventArray.Count -ge 1 | Should -Be $true
			$ishEventArray.IshRef | Should -Be $eventId
		}
		<# TODO [Could] It "Pipeline IshEvent Multiple" {
		}
		#>
	}
	#>
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}

