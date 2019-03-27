Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshBackgroundTask"
try {

Describe “Get-IshBackgroundTask" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
	$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FNAME" |
	                     Set-IshRequestedMetadataField -IshSession $ishSession -Name "FDOCUMENTTYPE" |
	                     Set-IshRequestedMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element |
	                     Set-IshRequestedMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element 
	$ishFolderTestRootOriginal = Get-IshFolder -IShSession $ishSession -FolderPath $folderTestRootPath -RequestedMetadata $requestedMetadata
	$folderIdTestRootOriginal = $ishFolderTestRootOriginal.IshFolderRef
	$folderTypeTestRootOriginal = $ishFolderTestRootOriginal.IshFolderType
	Write-Debug ("folderIdTestRootOriginal[" + $folderIdTestRootOriginal + "] folderTypeTestRootOriginal[" + $folderTypeTestRootOriginal + "]")
	$ownedByTestRootOriginal = Get-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField
	$readAccessTestRootOriginal = (Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Seperator)

	$global:ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObject = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
	             Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusReleased |
				 Set-IshDocumentObj -IshSession $ishSession


    $allTaskMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name CREATIONDATE  | 
			           Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name CURRENTATTEMPT |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name EVENTTYPE |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name EXECUTEAFTERDATE |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name HASHID | 
			           Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name INPUTDATAID |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name LEASEDBY |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name LEASEDON |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name MODIFICATIONDATE | 
			           Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name OUTPUTDATAID |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name PROGRESSID |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name STATUS -ValueType Value |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name STATUS -ValueType Element |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name TASKID |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name TRACKINGID |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name USERID -ValueType Element
    $allHistMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ENDDATE | 
			           Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ERROR |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ERRORNUMBER |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name EXITCODE |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name HISTORYID | 
			           Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name HOSTNAME |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name OUTPUT |
					   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name STARTDATE
    $allMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name CREATIONDATE  | 
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name CURRENTATTEMPT |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name EVENTTYPE |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name EXECUTEAFTERDATE |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name HASHID | 
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name INPUTDATAID |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name LEASEDBY |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name LEASEDON |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name MODIFICATIONDATE | 
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name OUTPUTDATAID |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name PROGRESSID |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name STATUS -ValueType Value |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name STATUS -ValueType Element |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name TASKID |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name TRACKINGID |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name USERID -ValueType Element |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ENDDATE | 
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ERROR |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ERRORNUMBER |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name EXITCODE |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name HISTORYID | 
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name HOSTNAME |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name OUTPUT |
                   Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name STARTDATE


	Context "Get-IshBackgroundTask" {
		$metadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name TASKID | 
		            Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name HISTORYID |
					Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name EVENTTYPE |
					Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name PROGRESSID 
		$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -UserFilter All -RequestedMetadata $metadata)[0]
		It "GetType().Name" {
			$ishBackgroundTask.GetType().Name | Should BeExactly "IshBackgroundTask"
		}
		It "ishObject.IshField" {
			$ishBackgroundTask.IshField | Should Not BeNullOrEmpty
		}
		It "ishObject.IshRef" {
			$ishBackgroundTask.IshRef | Should Not BeNullOrEmpty
		}
		# Double check following 2 ReferenceType enum usage 
		It "ishBackgroundTask.TaskRef" {
			$ishBackgroundTask.TaskRef | Should Not BeNullOrEmpty
		}
		#It "ishBackgroundTask.HistoryRef" {
		#	$ishBackgroundTask.HistoryRef | Should Not BeNullOrEmpty
		#}
		It "ishBackgroundTask ConvertTo-Json" {
			(ConvertTo-Json $ishBackgroundTask).Length -gt 2 | Should Be $true
		}
		It "Parameter IshSession/ModifiedSince/UserFilter invalid" {
			{ Get-IshBackgroundTask -IShSession "INVALIDISHSESSION" -ModifiedSince "INVALIDDATE" -UserFilter "INVALIDUSERFILTER" } | Should Throw
		}
		It "Parameter RequestedMetadata/MetadataFile invalid" {
			{ Get-IshBackgroundTask -IShSession $ishSession -RequestedMetadata "INVALIDMETADATA" -MetadataFilter "INVALIDFILTER"  } | Should Throw
		}
		It "Parameter IshSession/UserFilter/MetadataFilter are optional" {
			$ishBackgroundTask = (Get-IshBackgroundTask -ModifiedSince ((Get-Date).AddSeconds(-10)) -RequestedMetadata $allTaskMetadata)[0]
			($ishBackgroundTask | Get-IshMetadataField -IshSession $ishSession -Level Task -Name USERID -ValueType Element).StartsWith('VUSER') | Should Be $true
			($ishBackgroundTask | Get-IshMetadataField -IshSession $ishSession -Level Task -Name STATUS -ValueType Element).StartsWith('VBACKGROUNDTASK') | Should Be $true
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$oldDefaultRequestedMetadata = $ishSession.DefaultRequestedMetadata
			$ishSession.DefaultRequestedMetadata = "Descriptive"
			$ishBackgroundTask = (Get-IshBackgroundTask -IShSession $ishSession)[0]
			(($ishBackgroundTask.IshField.Count -eq 1) -or ($ishBackgroundTask.IshField.Count -eq 2)) | Should Be $true  # Either BackgroundTask has run and you get taskid/historyid, or it didn't and you only get taskid
			$ishSession.DefaultRequestedMetadata = "Basic"
			$ishBackgroundTask = (Get-IshBackgroundTask -IShSession $ishSession)[0]
			$ishBackgroundTask.status.Length -ge 1 | Should Be $true
			$ishBackgroundTask.status_task_element.StartsWith('VBACKGROUNDTASKSTATUS') | Should Be $true 
			(($ishBackgroundTask.IshField.Count -eq 13) -or ($ishBackgroundTask.IshField.Count -eq 19)) | Should Be $true
			$ishSession.DefaultRequestedMetadata = "All"
			$ishBackgroundTask = (Get-IshBackgroundTask -IShSession $ishSession)[0]
			(($ishBackgroundTask.IshField.Count -eq 18) -or ($ishBackgroundTask.IshField.Count -eq 26)) | Should Be $true
			$ishSession.DefaultRequestedMetadata = $oldDefaultRequestedMetadata
		}
		It "Parameter ModifiedSince is now" {
			(Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(1)) -UserFilter All).Count | Should Be 0
		}
		It "Parameter RequestedMetadata only all of Task level" {
			$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-10)) -UserFilter All -RequestedMetadata $allTaskMetadata)[0]
			$ishBackgroundTask.TaskRef -gt 0 | Should Be $true
			$ishBackgroundTask.IshField.Count -ge 16 | Should Be $true
		}
		It "Parameter RequestedMetadata only all of History level" {
			$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-1)) -UserFilter All -RequestedMetadata $allHistMetadata)[0]
			$ishBackgroundTask.TaskRef -gt 0 | Should Be $true
			#$ishBackgroundTask.HistoryRef -gt 0 | Should Be $true
			$ishBackgroundTask.IshField.Count -ge 1 | Should Be $true  # At least 1 entries returned if BackgroundTask service is not running, otherwise more
		}
		It "Parameter RequestedMetadata PipelineObjectPreference=PSObjectNoteProperty" {
			$ishSession.PipelineObjectPreference | Should Be "PSObjectNoteProperty"
			$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-1)) -UserFilter All -RequestedMetadata $allMetadata)[0]
			$ishBackgroundTask.GetType().Name | Should BeExactly "IshBackgroundTask"  # and not PSObject
			[bool]($ishBackgroundTask.PSobject.Properties.name -match "status_task_element") | Should Be $true
			[bool]($ishBackgroundTask.PSobject.Properties.name -match "userid_task_element") | Should Be $true
			[bool]($ishBackgroundTask.PSobject.Properties.name -match "modificationdate") | Should Be $true
			$ishBackgroundTask.modificationdate -like "*/*" | Should Be $false  # It should be sortable date format: yyyy-MM-ddTHH:mm:ss
		}
		It "Parameter RequestedMetadata PipelineObjectPreference=Off" {
		    $pipelineObjectPreference = $ishSession.PipelineObjectPreference
			$ishSession.PipelineObjectPreference = "Off"
			$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-1)) -UserFilter All -RequestedMetadata $allMetadata)[0]
			$ishBackgroundTask.GetType().Name | Should BeExactly "IshBackgroundTask"
			[bool]($ishBackgroundTask.PSobject.Properties.name -match "status_task_element") | Should Be $false
			[bool]($ishBackgroundTask.PSobject.Properties.name -match "userid_task_element") | Should Be $false
			[bool]($ishBackgroundTask.PSobject.Properties.name -match "modificationdate") | Should Be $false
			$ishSession.PipelineObjectPreference = $pipelineObjectPreference
		}
		It "Parameter MetadataFilter Filter to exactly one" {
			$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-1)) -UserFilter All -RequestedMetadata $allTaskMetadata)[0]
			$filterMetadata = Set-IshMetadataFilterField -IshSession $ishSession -Level Task -Name USERID -ValueType Element -Value ($ishBackgroundTask | Get-IshMetadataField -IshSession $ishSession -Level Task -Name USERID -ValueType Element) |
			                  Set-IshMetadataFilterField -IshSession $ishSession -Level Task -Name TASKID -ValueType Element -Value ($ishBackgroundTask | Get-IshMetadataField -IshSession $ishSession -Level Task -Name TASKID)
			$ishBackgroundTaskArray = Get-IshBackgroundTask -IshSession $ishSession -MetadataFilter $filterMetadata
			#Write-Host ("ishBackgroundTask.IshRef["+ $ishBackgroundTask.IshRef + "] ishBackgroundTaskArray.IshRef["+ $ishBackgroundTask.IshRef + "]")
			$ishBackgroundTaskArray.Count -ge 1 | Should Be $true  # earlier on, also filter on HISTORYID, but that is not always filled in, even with the windows service off
		}
		It "Parameter IshBackgroundTask invalid" {
			{ Get-IshBackgroundTask -IshSession $ishSession -IshBackgroundTask "INVALIDISHBACKGROUNDTASK" } | Should Throw
		}
		It "Parameter IshBackgroundTask Single" {
			$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-1)) -UserFilter Current)[0]
			$taskId = $ishBackgroundTask | Get-IshMetadataField -IshSession $ishSession -Level Task -Name TASKID
			$ishBackgroundTaskArray = Get-IshBackgroundTask -IshSession $ishSession -IshBackgroundTask $ishBackgroundTask
			$ishBackgroundTaskArray.Count -ge 1 | Should Be $true
			$ishBackgroundTaskArray.IshRef | Should Be $taskId
		}
		<# TODO It "Parameter IshBackgroundTask Multiple" {
		}
		#>
		It "Pipeline IshBackgroundTask Single" {
			$ishBackgroundTask = (Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-1)) -UserFilter Current)[0]
			$taskId = $ishBackgroundTask | Get-IshMetadataField -IshSession $ishSession -Level Task -Name TASKID
			$ishBackgroundTaskArray = $ishBackgroundTask | Get-IshBackgroundTask -IshSession $ishSession
			$ishBackgroundTaskArray.Count -ge 1 | Should Be $true
			$ishBackgroundTaskArray.IshRef | Should Be $taskId
		}
		<# TODO It "Pipeline IshBackgroundTask Multiple" {
		}
		#>
	}
	#>
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
