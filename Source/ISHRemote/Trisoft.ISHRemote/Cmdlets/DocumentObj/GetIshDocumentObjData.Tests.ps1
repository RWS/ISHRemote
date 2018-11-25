Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshDocumentObjData"
try {

Describe “Get-IshDocumentObjData" -Tags "Read" {
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
	$ishFolderLib = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHLibrary -FolderName "Library" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

	$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectTopicA = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
	$ishObjectLibraryA = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderLib.IshFolderRef -IshType ISHLibrary -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent

	Context "Get-IshDocumentObjData FolderPathGroup" {
		It "Parameter IshSession/IshObject invalid" {
			{ Get-IshDocumentObjData -IShSession "INVALIDISHSESSION" -IshObject "INVALIDISHOBJECT" } | Should Throw
		}
		$fileInfo = Get-IshDocumentObjData -IshSession $ishSession -IshObject $ishObjectTopicA -FolderPath (Join-Path $env:TEMP $cmdletName)
		It "GetType().Name" {
			$fileInfo.GetType().Name | Should BeExactly "FileInfo"
		}
		It "FileInfo.Name contains 4 = signs" {
			$fileInfo.Name -like "*=*=*=*=*.*" | Should Be $true
		}
		It "Parameter IshFeature matching features (so everything equals source file)" {
			$ishFeatures = Set-IshFeature -Name "ISHRemoteStringCond" -Value "StringOne" |
			               Set-IshFeature -Name "ISHRemoteVersionCond" -Value "12.0.1"
			$fileInfo = Get-IshDocumentObjData -IshSession $ishSession -IshObject $ishObjectTopicA -FolderPath (Join-Path $env:TEMP $cmdletName) -IshFeature $ishFeatures
			$fileContent = Get-Content $fileInfo
			Write-Debug ("fileContent.Length[" + $fileContent.Length + "] fileContent.GetType()[" + $fileContent.GetType() + "] fileContent[" +$fileContent+"]")
			($fileContent -like "*ISHRemoteStringCond*") | Should Be $true
		}
		It "Parameter IshFeature not matching features (so everything filtered away)" {
			$ishFeatures = Set-IshFeature -Name "INVALIDFEATURE" -Value "INVALIDVALUE"
			$fileInfo = Get-IshDocumentObjData -IshSession $ishSession -IshObject $ishObjectTopicA -FolderPath (Join-Path $env:TEMP $cmdletName) -IshFeature $ishFeatures
			$fileContent = Get-Content $fileInfo
			Write-Debug ("fileContent.Length[" + $fileContent.Length + "] fileContent.GetType()[" + $fileContent.GetType() + "] fileContent[" +$fileContent+"]")
			$fileContent -notlike "*ISHRemoteStringCond*" | Should Be $true
		}
	}

	Context "Get-IshDocumentObjData IshObjectGroup" {
		It "GetType().Name" {
			$ishobjects = Get-IshDocumentObjData -IshSession $ishSession -IshObject @($ishObjectTopicA,$ishObjectLibraryA)
			$ishobjects.GetType().Name | Should BeExactly "Object[]"
		}
		It "Parameter IshObject Single with implicit IshSession" {
			$ishobjects = Get-IshDocumentObjData -IshObject $ishObjectTopicA
			$ishobjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple with implicit IshSession" {
			$ishobjects = Get-IshDocumentObjData -IshObject @($ishObjectTopicA,$ishObjectLibraryA)
			$ishobjects.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$ishobjects = $ishObjectTopicA | Get-IshDocumentObjData -IshSession $ishSession
			$ishobjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishobjects = @($ishObjectTopicA,$ishObjectLibraryA) | Get-IshDocumentObjData -IshSession $ishSession
			$ishobjects.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	try { Remove-Item (Join-Path $env:TEMP $cmdletName) -Recurse -Force } catch { }
}
