Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshBackgroundTask"
try {

Describe “Add-IshBackgroundTask" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
	$eventType = "PUSHTRANSLATIONS"
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
    $ishObjectTopic1 = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
    $ishObjectTopic1 = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent

	$ishObjects = $ishFolderTopic | Get-IshFolderContent -IshSession $ishSession
	Context "Add-IshBackgroundTask IshObjects passed via pipeline" {
		$ishBackgroundTaskIshObjectsPipeline = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $eventType
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishBackgroundTaskIshObjectsPipeline.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsPipeline.status_task_element | Should BeExactly "VBACKGROUNDTASKSTATUSPENDING"
			$ishBackgroundTaskIshObjectsPipeline.EventType | Should BeExactly $eventType
			$ishBackgroundTaskIshObjectsPipeline.userid | Should BeExactly $ishSession.IshUserName
		}
		It "Pipe returned BackgroundTask to Get-IshBackgroundTask"{
			$ishBackgroundTask = $ishBackgroundTaskIshObjectsPipeline | Get-IshBackgroundTask -IshSession $ishSession
			$ishBackgroundTask.Count | Should BeExactly 1
			$ishBackgroundTask.EventType | Should BeExactly $eventType
			$ishBackgroundTask.userid | Should BeExactly $ishSession.IshUserName
		}
	}

	Context "Add-IshBackgroundTask IshObjects passed as parameter" {
		$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -IshSession $ishSession -EventType $eventType -IshObject $ishObjects
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishBackgroundTaskIshObjectsParameter.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsParameter.status_task_element | Should BeExactly "VBACKGROUNDTASKSTATUSPENDING"
			$ishBackgroundTaskIshObjectsParameter.EventType | Should BeExactly $eventType
			$ishBackgroundTaskIshObjectsParameter.userid | Should BeExactly $ishSession.IshUserName
		}
		It "Pipe returned BackgroundTask to Get-IshBackgroundTask"{
			$ishBackgroundTask = $ishBackgroundTaskIshObjectsParameter | Get-IshBackgroundTask -IshSession $ishSession
			$ishBackgroundTask.Count | Should BeExactly 1
			$ishBackgroundTask.EventType | Should BeExactly $eventType
			$ishBackgroundTask.userid | Should BeExactly $ishSession.IshUserName
		}
		It "Verify mandatory parameters" {
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $eventType -IshObject $null} | Should Throw
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $null -IshObject $ishObjects} | Should Throw
		}
	}
	
	Context "Add-IshBackgroundTask by providing parameters" {
		$rawData = [System.Byte[]]::CreateInstance([System.Byte], 1)
		$eventDescription = "Created by Powershell and ISHRemote"
		$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $eventType -EventDescription $eventDescription -RawInputData $rawData
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishBackgroundTaskParameters.Count | Should BeExactly 1
			$ishBackgroundTaskParameters.status_task_element | Should BeExactly "VBACKGROUNDTASKSTATUSPENDING"
			$ishBackgroundTaskParameters.EventType | Should BeExactly $eventType
			$ishBackgroundTaskParameters.userid | Should BeExactly $ishSession.IshUserName
		}
		It "Verify mandatory parameters" {
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $eventType -EventDescription $null -RawInputData $rawData} | Should Throw
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $null -EventDescription $eventDescription -RawInputData $rawData} | Should Throw
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $eventType -EventDescription $eventDescription -RawInputData $null} | Should Throw
		}
	}
}

} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
