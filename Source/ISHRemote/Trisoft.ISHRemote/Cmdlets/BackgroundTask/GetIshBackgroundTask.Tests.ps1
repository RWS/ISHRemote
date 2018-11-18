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

	Context "Get-IshBackgroundTask returns IshBackgroundTask object " {
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
		It "ishBackgroundTask.EventType" {
			$ishBackgroundTask.EventType | Should Not BeNullOrEmpty
		}
		# Double check following 2 ReferenceType enum usage 
		It "ishBackgroundTask.ObjectRef[Enumerations.ReferenceType.BackgroundTask]" {
			$ishBackgroundTask.ObjectRef["BackgroundTask"] | Should Not BeNullOrEmpty
		}
		It "ishBackgroundTask.ObjectRef[Enumerations.ReferenceType.BackgroundTaskHistory]" {
			$ishBackgroundTask.ObjectRef["BackgroundTaskHistory"] | Should Not BeNullOrEmpty
		}
	}

		<#
	Context “Add-IshDocumentObj ParameterGroupFileContent" {
		It "Parameter IshSession/Lng/FileContent invalid" {
			{ Add-IshDocumentObj -IShSession "INVALIDISHSESSION" -Lng "INVALIDLANGUAGE" -FileContent "INVALIDFILECONTENT" } | Should Throw
		}
		It "Parameter Lng/FileContent invalid" {
			{ Add-IshDocumentObj -IShSession $ishSession -Lng "INVALIDLANGUAGE" -FileContent "INVALIDFILECONTENT" } | Should Throw
		}
		It "Parameter FileContent invalid" {
			{ Add-IshDocumentObj -IShSession $ishSession -Lng $ishLng -FileContent "INVALIDFILECONTENT" } | Should Throw
		}
		It "All Parameters (Topic)" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Topic $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId "MYOWNGENERATEDLOGICALIDTOPIC" -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject.ObjectRef["Lng"] -gt 0 | Should Be $true
		}
		It "All Parameters (Map)" {
			$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Map $timestamp" |
						      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -LogicalId "MYOWNGENERATEDLOGICALIDMAP" -Version '3' -Lng $ishLng -Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $ditaMapFileContent
			$ishObject.ObjectRef["Lng"] -gt 0 | Should Be $true
		}
		It "All Parameters (Lib)" {
			$ishLibMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Lib $timestamp" |
						      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderLib -IshType ISHLibrary -LogicalId "MYOWNGENERATEDLOGICALIDLIB" -Version '4' -Lng $ishLng -Metadata $ishLibMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject.ObjectRef["Lng"] -gt 0 | Should Be $true
		}
	}
	
	Context “Add-IshDocumentObj ParameterGroupFilePath" {
		It "Parameter IshSession/Lng/FilePath invalid" {
			{ Add-IshDocumentObj -IShSession "INVALIDISHSESSION" -Lng "INVALIDLANGUAGE" -FilePath "INVALIDFILEPATH" } | Should Throw
		}
		It "All Parameters (Image like EDTJPEG)" {
			$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Image $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$tempFilePath = (New-TemporaryFile).FullName
			Add-Type -AssemblyName "System.Drawing"
			$bmp = New-Object -TypeName System.Drawing.Bitmap(100,100)
			for ($i = 0; $i -lt 100; $i++)
			{
				for ($j = 0; $j -lt 100; $j++)
				{
					$bmp.SetPixel($i, $j, 'Red')
				}
			}
			$bmp.Save($tempFilePath, [System.Drawing.Imaging.ImageFormat]::Jpeg)
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderImage -IshType ISHIllustration -LogicalId "MYOWNGENERATEDLOGICALIDIMAGE" -Version '5' -Lng $ishLng -Resolution $ishResolution -Metadata $ishImageMetadata -Edt "EDTJPEG" -FilePath $tempFilePath
			$ishObject.ObjectRef["Lng"] -gt 0 | Should Be $true
		}
		It "All Parameters (Other like EDT-TEXT)" {
			$ishOtherMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Other $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			Get-Process | Out-File $tempFilePath
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderOther -IshType ISHTemplate -LogicalId "MYOWNGENERATEDLOGICALIDOTHER" -Version '6' -Lng $ishLng -Metadata $ishOtherMetadata -Edt "EDT-TEXT" -FilePath $tempFilePath
			$ishObject.ObjectRef["Lng"] -gt 0 | Should Be $true
		}
	}

	Context “Add-IshDocumentObj IshObjectsGroup" {
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		It "Parameter IshObject invalid" {
			{ Add-IshDocumentObj -IshSession $ishSession -IshFolder "INVALIDISHFOLDER" -IshObject "INVALIDISHOBJECT" } | Should Throw
		}
		It "Parameter IshObject Single" {
			# Create an object, Delete it, Recreate it using parameter IshObject as if the incoming object came from another repository
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			             Get-IshDocumentObjData -IshSession $ishSession
			Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObject
			$ishObjectArray = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshObject $ishObject
			$ishObjectArray.Count | Should Be 1
		}
		It "Parameter IshObject Multiple" {
			# Create an object, Delete it, Recreate it using parameter IshObject as if the incoming object came from another repository
			$ishObjectA = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			              Get-IshDocumentObjData -IshSession $ishSession
			Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObjectA
			$ishObjectB = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			              Get-IshDocumentObjData -IshSession $ishSession
			Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObjectB
			$ishObjectArray = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshObject @($ishObjectA,$ishObjectB)
			$ishObjectArray.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			# Create an object, Delete it, Recreate it using parameter IshObject as if the incoming object came from another repository
			$ishObjectC = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			              Get-IshDocumentObjData -IshSession $ishSession
			Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObjectC
			$ishObjectArray = $ishObjectC | Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic
			$ishObjectArray.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			# Create an object, Delete it, Recreate it using parameter IshObject as if the incoming object came from another repository
			$ishObjectD = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			              Get-IshDocumentObjData -IshSession $ishSession
			$ishObjectE = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			              Get-IshDocumentObjData -IshSession $ishSession
			$ishObjects = @($ishObjectD,$ishObjectE)
			$ishObjects | Remove-IshDocumentObj -IshSession $ishSession
			$ishObjectArray = $ishObjects | Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic
			$ishObjects.Count -eq $ishObjectArray.Count | Should Be $true
		}
	}
	#>
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
