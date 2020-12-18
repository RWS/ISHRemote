Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Move-IshFolder"
try {

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
	$readAccessTestRootOriginal = (Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Separator)
	$ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$folderCmdletRootPath = Join-Path $folderTestRootPath $cmdletName
	$ishFolderA = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderA" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderB = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderB" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderC = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderC" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderD = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderD" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderE = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderE" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderF = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderF" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderG = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderG" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderH = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderH" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

Describe “Move-IshFolder" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"

	Context “Move-IshFolder ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Move-IshFolder -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Move-IshFolder returns IshFolder object" {
		$ishFolderData = Move-IshFolder -IShSession $ishSession -FolderId $ishFolderB.IshFolderRef -ToFolderId $ishFolderA.IshFolderRef
		It "GetType().Name" {
			$ishFolderData.GetType().Name | Should BeExactly "IshFolder"
		}
		It "$ishFolderData.IshFolderRef" {
			$ishFolderData.IshFolderRef -eq $ishFolderB.IshFolderRef | Should Be $true
		}
		It "$ishFolderData.IshFolderType" {
			$ishFolderData.IshFolderType | Should Not BeNullOrEmpty
		}
		It "$ishFolderData.IshField" {
			$ishFolderData.IshField | Should Not BeNullOrEmpty
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			$ishFolderData.name.Length -ge 1 | Should Be $true 
			$ishFolderData.fdocumenttype.Length -ge 1 | Should Be $true 
			$ishFolderData.fdocumenttype_none_element.StartsWith('VDOCTYPE') | Should Be $true 
		}
	}

	Context “Move-IshFolder IshFoldersGroup" {
		It "Parameter IshFolder invalid" {
			{ Move-IshFolder -IShSession $ishSession -IshFolder "INVALIDFOLDERID" -ToFolderId $ishFolderA.IshFolderRef } | Should Throw
		}
		It "Parameter IshFolder Single with implicit IshSession" {
			$ishFolders = Move-IshFolder -IshFolder $ishFolderC -ToFolderId $ishFolderA.IshFolderRef
			$ishFolders.Count | Should Be 1
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			$ishFolders = Move-IshFolder -IshFolder @($ishFolderD,$ishFolderE) -ToFolderId $ishFolderA.IshFolderRef
			$ishFolders.Count | Should Be 2
		}
		It "Pipeline IshFolder Single" {
			$ishFolders = $ishFolderF | Move-IshFolder -IshSession $ishSession -ToFolderId $ishFolderA.IshFolderRef
			$ishFolders.Count | Should Be 1
		}
		It "Pipeline IshFolder Multiple" {
			$ishFolders = @($ishFolderG,$ishFolderH) | Move-IshFolder -IshSession $ishSession -ToFolderId $ishFolderA.IshFolderRef
			$ishFolders.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
