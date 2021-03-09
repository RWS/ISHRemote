Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshBackgroundTask"

function GetLngRefsByInputDataId([long]$inputDataId)
{
	[xml]$xmlBTDataObject = $ishSession.BackgroundTask25.RetrieveDataObjectByIshDataRefs($inputDataId)
	[byte[]]$data = [Convert]::FromBase64String($xmlBTDataObject.ishbackgroundtaskdataobjects.ishbackgroundtaskdataobject."#cdata-section")
	
	#find where xml really starts
	$position = 0
	foreach($byte in $data)
	{
		if($byte -eq "60"){
			break;
		}
		$position ++
	}
	
	#convert to xml
	[xml]$xmlContent = [System.Text.Encoding]::Unicode.GetString($data, $position, $data.Length - $position)

	$retrievedLngRefsArray = @()
	foreach($ishObject in $xmlContent.ishobjects.ishobject) 
	{
		$retrievedLngRefsArray += $ishObject.ishlngRef
	}
	
	return $retrievedLngRefsArray
}

function GetLngRefsByBackgroundTasksArray($arrBackgroundTasks)
{
	if ($arrBackgroundTasks.Count -eq 0)
	{
		return $null
	}
	
	$retrievedLngRefs = @()
	foreach($ishBackgroundTask in $arrBackgroundTasks)
	{
		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name INPUTDATAID 
		$ishBackgroundTask = $ishBackgroundTask | Get-IshBackgroundTask -RequestedMetadata $requestedMetadata
		$retrievedLngRefs += GetLngRefsByInputDataId -inputDataId $ishBackgroundTask.inputdataid
	}
	return $retrievedLngRefs
}

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
	
	Context "IshObjects passed via pipeline (multiple)" {
		$ishBackgroundTaskIshObjectsPipeline = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
		
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishObjects.Count | Should BeExactly 10
			$ishBackgroundTaskIshObjectsPipeline.Count | Should BeExactly 1
			$ishBackgroundTaskIshObjectsPipeline.GetType() | Should BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
			$ishBackgroundTaskIshObjectsPipeline.EventType | Should BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskIshObjectsPipeline.userid | Should BeExactly $ishSession.IshUserName
		}
		
		It "Verify input data contains all expected objects" {
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name INPUTDATAID 
			$ishBackgroundTaskIshObjectsPipeline = $ishBackgroundTaskIshObjectsPipeline | Get-IshBackgroundTask -RequestedMetadata $requestedMetadata
			$retrievedLngRefs = GetLngRefsByInputDataId -inputDataId $ishBackgroundTaskIshObjectsPipeline.inputdataid
			$retrievedLngRefs.Count | Should BeExactly $createdLngRefs.Count
			foreach($lngRef in $retrievedLngRefs)
			{
				$createdLngRefs -contains $lngRef | Should BeExactly $true
			}
		}
	}
	
	Context "IshObjects passed via pipeline (multiple) - divide into batches"{
		$savedMetadataBatchSize = $ishSession.MetadataBatchSize
		
		It "Verify total number of objects under test" {
			$ishObjects.Count | Should BeExactly 10
		}

		It "Verify batch size 2" {
			$ishSession.MetadataBatchSize = 2
			$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
			$ishBackgroundTasks.Count | Should BeExactly 3
			
			$retrievedLngRefs = GetLngRefsByBackgroundTasksArray -arrBackgroundTasks $ishBackgroundTasks
			$retrievedLngRefs.Count | Should BeExactly $createdLngRefs.Count
			foreach($lngRef in $retrievedLngRefs)
			{
				$createdLngRefs -contains $lngRef | Should BeExactly $true
			}
		}
		
		It "Verify batch size 4" {
			$ishSession.MetadataBatchSize = 4
			$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
			$ishBackgroundTasks.Count | Should BeExactly 3
			
			$retrievedLngRefs = GetLngRefsByBackgroundTasksArray -arrBackgroundTasks $ishBackgroundTasks
			$retrievedLngRefs.Count | Should BeExactly $createdLngRefs.Count
			foreach($lngRef in $retrievedLngRefs)
			{
				$createdLngRefs -contains $lngRef | Should BeExactly $true
			}
		}
		
		It "Verify batch size 6" {
			$ishSession.MetadataBatchSize = 6
			$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
			$ishBackgroundTasks.Count | Should BeExactly 2
			
			$retrievedLngRefs = GetLngRefsByBackgroundTasksArray -arrBackgroundTasks $ishBackgroundTasks
			$retrievedLngRefs.Count | Should BeExactly $createdLngRefs.Count
			foreach($lngRef in $retrievedLngRefs)
			{
				$createdLngRefs -contains $lngRef | Should BeExactly $true
			}
		}

		It "Verify batch size 10" {
			$ishSession.MetadataBatchSize = 10
			$ishBackgroundTasks = $ishObjects | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
			$ishBackgroundTasks.Count | Should BeExactly 1
			
			$retrievedLngRefs = GetLngRefsByBackgroundTasksArray -arrBackgroundTasks $ishBackgroundTasks
			$retrievedLngRefs.Count | Should BeExactly $createdLngRefs.Count
			foreach($lngRef in $retrievedLngRefs)
			{
				$createdLngRefs -contains $lngRef | Should BeExactly $true
			}
		}
		
		$ishSession.MetadataBatchSize = $savedMetadataBatchSize
	}

	Context "IshObjects passed via pipeline (single)" {
		$ishBackgroundTaskIshObjectsPipeline = $ishObjectTopic1_1 | Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishObjectTopic1_1.Count | Should BeExactly 1
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
		$ishBackgroundTaskIshObjectsParameter = Add-IshBackgroundTask -IshSession $ishSession -EventType $ishEventTypeToPurge -IshObject $ishObjectTopic1_1
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishObjectTopic1_1.Count | Should BeExactly 1
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
	
	Context "Add-IshBackgroundTask (Parameters group)" {
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
	Context "Add-IshBackgroundTask (Parameters group with StartAfter)" {
		$rawData = "<data><dataExample>Text</dataExample></data>"
		$eventDescription = "Created by Powershell and ISHRemote (startAfter)"
		$dateTomorrow = (Get-Date).AddDays(1)
		$ishBackgroundTaskStartsAfter = Add-IshBackgroundTask -EventType $ishEventTypeToPurge -EventDescription $eventDescription -RawInputData $rawData -StartAfter $dateTomorrow
		
		It "Verify object properties returned by Add-IshBackgroundTask" {
			$ishBackgroundTaskStartsAfter.Count | Should BeExactly 1
			($ishBackgroundTaskStartsAfter.executeafterdate -eq $ishBackgroundTaskStartsAfter.creationdate) | Should Be $false
			$ishBackgroundTaskStartsAfter.GetType() | Should BeExactly Trisoft.ISHRemote.Objects.Public.IshBackgroundTask
			$ishBackgroundTaskStartsAfter.EventType | Should BeExactly $ishEventTypeToPurge
			$ishBackgroundTaskStartsAfter.userid | Should BeExactly $ishSession.IshUserName
		}

		It "Verify retrieved StartsAfter date matches provided StartsAfter" {
			$retrievedExecuteAfter = New-Object DateTime
			$conversionResult = [DateTime]::TryParseExact($ishBackgroundTaskStartsAfter.executeafterdate, "yyyy-MM-ddTHH:mm:ss", [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::None, [ref]$retrievedExecuteAfter)
			$conversionResult | Should BeExactly $true
			$retrievedExecuteAfter.ToString("dd/MM/yyyy") | Should BeExactly $dateTomorrow.ToString("dd/MM/yyyy")	
		}
	}
}

} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession -VersionFilter "" | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
