Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Remove-IshFolder"
try {

Describe “Remove-IshFolder" -Tags "Create" {
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
	$ishFolderFolderIdGroup = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderIdGroup" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderFolderPathGroup = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderPathGroup" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderIshFoldersGroup = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "IshFoldersGroup" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderIshFoldersGroupA = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "IshFoldersGroupA" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderIshFoldersGroupB = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "IshFoldersGroupB" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderIshFoldersGroupC = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "IshFoldersGroupC" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderIshFoldersGroupD = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "IshFoldersGroupD" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderAllYourBase = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "All" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderAllYourBase.IshFolderRef)       -FolderType ISHModule -FolderName "Your" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "Base" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "Belong" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "To" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "Us!" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

	Context “Remove-IshFolder FolderIdGroup" {
		It "Parameter IshSession invalid" {
			{ Remove-IshFolder -IShSession "INVALIDISHSESSION" -FolderId "-7" } | Should Throw
		}
		It "Parameter FolderId 0" {
			{ Remove-IshFolder -IShSession $ishSession -FolderId 0 } | Should Not Throw
		}
		It "Parameter FolderId \General\" {
			{ Remove-IshFolder -IShSession $ishSession -FolderId (Get-IshFolder -IshSession $ishSession -BaseFolder Data).IshFolderRef } | Should Throw
		}
		It "Remove-IshFolder returns nothing" {
			$ishFolder = Remove-IshFolder -IshSession $ishSession -FolderId $ishFolderFolderIdGroup.IshFolderRef 
			$ishFolder -eq $null | Should Be $true
		}
	}

	Context “Remove-IshFolder FolderPathGroup" {
		It "Parameter FolderPathGroup invalid" {
			{ Remove-IshFolder -IShSession $ishSession -FolderPath "INVALIDFOLDERPATH" } | Should Throw
		}
		It "Parameter FolderPathGroup FolderPathGroup" {
			{ Remove-IshFolder -IShSession $ishSession -FolderPath ($folderTestRootPath + $ishSession.FolderPathSeparator + $cmdletName + $ishSession.FolderPathSeparator +  "FolderPathGroup") } | Should Not Throw
		}
	}

	Context “Remove-IshFolder IshFoldersGroup" {
		$ishFolderData = Get-IshFolder -IShSession $ishSession -BaseFolder Data
		$ishFolderSystem = Get-IshFolder -IShSession $ishSession -BaseFolder System
		$ishFolderFavorites = Get-IshFolder -IShSession $ishSession -BaseFolder Favorites
		$ishFolderEditorTemplate = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate
		It "Parameter IshFolder invalid" {
			{ Remove-IshFolder -IShSession $ishSession -IshFolder "INVALIDFOLDERID" } | Should Throw
		}
		It "Parameter IshFolder Single with implicit IshSession" {
			{ Remove-IshFolder -IshFolder $ishFolderIshFoldersGroup } | Should Not Throw
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			{ Remove-IshFolder -IshFolder @($ishFolderIshFoldersGroupA,$ishFolderIshFoldersGroupB) } | Should Not Throw
		}
		It "Pipeline IshFolder" {
			{  @($ishFolderData,$ishFolderSystem,$ishFolderFavorites) | Remove-IshFolder -IshSession $ishSession } | Should Throw
		}
		It "Pipeline IshFolder Multiple" {
			{  @($ishFolderIshFoldersGroupC,$ishFolderIshFoldersGroupD) | Remove-IshFolder -IshSession $ishSession } | Should Not Throw
		}
	}

	Context "Remove-IshFolder Recurse" {
		It "Parameter FolderIdGroup" {
			$ishFolder = Remove-IshFolder -IshSession $ishSession -FolderId $ishFolderAllYourBase.IshFolderRef -Recurse
			$ishFolder -eq $null | Should Be $true
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
