Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshBackgroundTask"
try {

Describe “Add-IshBackgroundTask" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
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
	Context "IshObjects passed via pipeline (multiple)" {
		$ishBackgroundTaskIshObjectsPipeline = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishObjects.Count | Should BeExactly 2
			$ishBackgroundTaskIshObjectsPipeline.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsPipeline.GetType() | Should BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
			$ishBackgroundTaskIshObjectsPipeline.EventType | Should BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskIshObjectsPipeline.userid | Should BeExactly $ishSession.IshUserName
		}
	}

	Context "IshObjects passed via pipeline (single)" {
		$ishBackgroundTaskIshObjectsPipeline = $ishObjectTopic1 | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishObjectTopic1.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsPipeline.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsPipeline.GetType() | Should BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
			$ishBackgroundTaskIshObjectsPipeline.EventType | Should BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskIshObjectsPipeline.userid | Should BeExactly $ishSession.IshUserName
		}
	}

	Context "IshObjects passed as a parameter (multiple)" {
		$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge -IshObject $ishObjects
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishBackgroundTaskIshObjectsParameter.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsParameter.GetType() | Should BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
			$ishBackgroundTaskIshObjectsParameter.EventType | Should BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskIshObjectsParameter.userid | Should BeExactly $ishSession.IshUserName
		}
	}

	Context "IshObjects passed as a parameter (single)" {
		$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge -IshObject $ishObjectTopic1
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishObjectTopic1.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsParameter.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsParameter.GetType() | Should BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
			$ishBackgroundTaskIshObjectsParameter.EventType | Should BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskIshObjectsParameter.userid | Should BeExactly $ishSession.IshUserName
		}
		
		It "Verify mandatory parameters" {
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -IshObject $null} | Should Throw
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $null -IshObject $ishObjects} | Should Throw
		}
	}
	
	Context "Add-IshBackgroundTask by providing parameters" {
		$rawData = "<data><dataExample>Text</dataExample></data>"
		$eventDescription = "Created by Powershell and ISHRemote"
		$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $eventDescription -RawInputData $rawData
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishBackgroundTaskParameters.Count | Should BeExactly 1
			$ishBackgroundTaskParameters.GetType() | Should BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
			$ishBackgroundTaskParameters.EventType | Should BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskParameters.userid | Should BeExactly $ishSession.IshUserName
		}
         
		It "Verify mandatory parameters" {
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $null -RawInputData $rawData} | Should Throw
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $null -EventDescription $eventDescription -RawInputData $rawData} | Should Throw
			{$ishBackgroundTaskParameters = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $eventDescription -RawInputData $null} | Should Throw
		}
	}
}

} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
