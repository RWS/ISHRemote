Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshDocumentObjFolderLocation"
try {

Describe “Get-IshDocumentObjFolderLocation" -Tags "Read" {
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

	Context "Get-IshDocumentObjFolderLocation ParameterGroup" {
		It "Parameter IshSession/LogicalId invalid" {
			{ Get-IshDocumentObjFolderLocation -IShSession "INVALIDISHSESSION" -LogicalId "INVALIDLOGICALID" } | Should Throw
		}
		$folderPath = Get-IShDocumentObjFolderLocation -IshSession $ishSession -LogicalId $ishObjectTopicA.IshRef
		It "GetType().Name" {
			$folderPath.GetType().Name | Should BeExactly "String"
		}
		It "Parameter LogicalId Single" {
			$folderPath | Should BeExactly (Join-Path (Join-Path $folderTestRootPath $cmdletName) "Topic")
		}
		It "Leading IshSession.FolderPathSeparator" {
			$folderPath[0] | Should Be $ishSession.FolderPathSeparator
		}
	}

	Context "Get-IshDocumentObjFolderLocation IshObjectGroup" {
		It "GetType().Name" {
			$folderPathArray = Get-IShDocumentObjFolderLocation -IshSession $ishSession -IshObject @($ishObjectTopicA,$ishObjectLibraryA)
			$folderPathArray.GetType().Name | Should BeExactly "Object[]"
		}
		It "Parameter IshObject Single with implicit IshSession" {
			$folderPathArray = Get-IShDocumentObjFolderLocation -IshObject $ishObjectTopicA
			$folderPathArray.Count | Should Be 1
		}
		It "Parameter IshObject Multiple with implicit IshSession" {
			$folderPathArray = Get-IShDocumentObjFolderLocation -IshObject @($ishObjectTopicA,$ishObjectLibraryA)
			$folderPathArray.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$folderPathArray = $ishObjectTopicA | Get-IShDocumentObjFolderLocation -IshSession $ishSession
			$folderPathArray.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$folderPathArray = @($ishObjectTopicA,$ishObjectLibraryA) | Get-IShDocumentObjFolderLocation -IshSession $ishSession
			$folderPathArray.Count | Should Be 2
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
