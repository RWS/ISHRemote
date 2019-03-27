Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshDocumentObj"
try {

Describe “Add-IshDocumentObj" -Tags "Create" {
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
	$ishFolderMap = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "Map" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderLib = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHLibrary -FolderName "Library" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderImage = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHIllustration -FolderName "Image" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderOther = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHTemplate -FolderName "Other" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

	$tempFilePath = (New-TemporaryFile).FullName

	Context "Add-IshDocumentObj returns IshObject object (Topic)" {
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObject = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
		It "GetType().Name" {
			$ishObject.GetType().Name | Should BeExactly "IshDocumentObj"
		}
		It "ishObject.IshData" {
			{ $ishObject.IshData } | Should Not Throw
		}
		It "ishObject.IshField" {
			$ishObject.IshField | Should Not BeNullOrEmpty
		}
		It "ishObject.IshRef" {
			$ishObject.IshRef | Should Not BeNullOrEmpty
		}
		It "ishObject.IshType" {
			$ishObject.IshType | Should Not BeNullOrEmpty
		}
		# Double check following 3 ReferenceType enum usage 
		It "ishObject.ObjectRef" {
			$ishObject.ObjectRef | Should Not BeNullOrEmpty
		}
		It "ishObject.VersionRef" {
			$ishObject.VersionRef | Should Not BeNullOrEmpty
		}
		It "ishObject.LngRef" {
			$ishObject.LngRef | Should Not BeNullOrEmpty
		}
		It "ishObject ConvertTo-Json" {
			(ConvertTo-Json $ishObject).Length -gt 2 | Should Be $true
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			#logical
			$ishObject.ftitle_logical_value.Length -ge 1 | Should Be $true 
			#version
			$ishObject.version_version_value.Length -ge 1 | Should Be $true 
			#language
			$ishObject.fstatus.Length -ge 1 | Should Be $true 
			$ishObject.fstatus_lng_element.StartsWith('VSTATUS') | Should Be $true 
			$ishObject.doclanguage.Length -ge 1 | Should Be $true  # Field names like DOC-LANGUAGE get stripped of the hyphen, otherwise you get $ishObject.'doc-language' and now you get the more readable $ishObject.doclanguage
			$ishObject.doclanguage_lng_element.StartsWith('VLANGUAGE') | Should Be $true 
		}
	}

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
			$ishObject.LngRef -gt 0 | Should Be $true
		}
		It "All Parameters (Map)" {
			$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Map $timestamp" |
						      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -LogicalId "MYOWNGENERATEDLOGICALIDMAP" -Version '3' -Lng $ishLng -Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $ditaMapFileContent
			$ishObject.LngRef -gt 0 | Should Be $true
		}
		It "All Parameters (Lib)" {
			$ishLibMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Lib $timestamp" |
						      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderLib -IshType ISHLibrary -LogicalId "MYOWNGENERATEDLOGICALIDLIB" -Version '4' -Lng $ishLng -Metadata $ishLibMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject.LngRef -gt 0 | Should Be $true
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
			$ishObject.LngRef -gt 0 | Should Be $true
		}
		It "All Parameters (Other like EDT-TEXT)" {
			$ishOtherMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Other $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			Get-Process | Out-File $tempFilePath
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderOther -IshType ISHTemplate -LogicalId "MYOWNGENERATEDLOGICALIDOTHER" -Version '6' -Lng $ishLng -Metadata $ishOtherMetadata -Edt "EDT-TEXT" -FilePath $tempFilePath
			$ishObject.LngRef -gt 0 | Should Be $true
		}
	}

	Context “Add-IshDocumentObj IshObjectsGroup" {
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		It "Parameter IshObject invalid" {
			{ Add-IshDocumentObj -IshSession $ishSession -IshFolder "INVALIDISHFOLDER" -IshObject "INVALIDISHOBJECT" } | Should Throw
		}
		It "Parameter IshObject Single with implicit IshSession" {
			# Create an object, Delete it, Recreate it using parameter IshObject as if the incoming object came from another repository
			$ishObject = Add-IshDocumentObj -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			             Get-IshDocumentObjData
			Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObject
			$ishObjectArray = Add-IshDocumentObj -IshFolder $ishFolderTopic -IshObject $ishObject
			$ishObjectArray.Count | Should Be 1
		}
		It "Parameter IshObject Multiple with implicit IshSession" {
			# Create an object, Delete it, Recreate it using parameter IshObject as if the incoming object came from another repository
			$ishObjectA = Add-IshDocumentObj -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			              Get-IshDocumentObjData 
			Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObjectA
			$ishObjectB = Add-IshDocumentObj -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent |
			              Get-IshDocumentObjData
			Remove-IshDocumentObj -IshObject $ishObjectB
			$ishObjectArray = Add-IshDocumentObj -IshFolder $ishFolderTopic -IshObject @($ishObjectA,$ishObjectB)
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

}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	try { Remove-Item $tempFilePath -Force } catch { }
}
