BeforeAll {
	$cmdletName = "Add-IshFolder"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Add-IshFolder" -Tags "Create" {
	BeforeAll {
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
	}
	Context "Add-IshFolder ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Add-IshFolder -IShSession "INVALIDISHSESSION" -ParentFolderId "-6" -FolderType ISHNone -FolderName "INVALIDFOLDERNAME" -OwnedBy "INVALIDUSERGROUP" -ReadAccess @("INVALIDUSERGROUPA","INVALIDUSERGROUPB") } | Should -Throw
		}
	}
	Context "Add-IshFolder returns IshFolder object" {
		It "GetType().Name" {
			$ishFolderCmdlet.GetType().Name | Should -BeExactly "IshFolder"
		}
		It "ishFolderCmdlet.IshFolderRef" {
			$ishFolderCmdlet.IshFolderRef -ge 0 | Should -Be $true
		}
		It "ishFolderCmdlet.IshFolderType" {
			$ishFolderCmdlet.IshFolderType | Should -Not -BeNullOrEmpty
		}
		It "ishFolderCmdlet.IshField" {
			$ishFolderCmdlet.IshField | Should -Not -BeNullOrEmpty
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should -Be "Basic"
			$ishFolderCmdlet.name.Length -ge 1 | Should -Be $true 
			$ishFolderCmdlet.fdocumenttype.Length -ge 1 | Should -Be $true 
			$ishFolderCmdlet.fdocumenttype_none_element.StartsWith('VDOCTYPE') | Should -Be $true 
		}
	}
	Context "Add-IshFolder ParameterGroup" {
		It "Parameter ParentFolderId invalid" {
			{ Add-IshFolder -IShSession $ishSession -ParentFolderId "INVALIDFOLDERID" } | Should -Throw
		}
		It "Parameter ParentFolderId invalid" {
			{ Add-IshFolder -IShSession $ishSession -ParentFolderId "INVALIDFOLDERID" } | Should -Throw
		}
		It "Parameter FolderType ISHIllustration" {
			$ishFolders = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHIllustration -FolderName "ParameterGroup ISHIllustration" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter FolderType ISHLibrary" {
			$ishFolders = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHLibrary -FolderName "ParameterGroup ISHLibrary" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter FolderType ISHMasterDoc" {
			$ishFolders = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "ParameterGroup ISHMasterDoc" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter FolderType ISHModule" {
			$ishFolders = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "ParameterGroup ISHModule" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter FolderType ISHNone" {
			$ishFolders = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHNone -FolderName "ParameterGroup ISHNone" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter FolderType ISHPublication" {
			$ishFolders = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHPublication -FolderName "ParameterGroup ISHPublication" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter FolderType ISHTemplate" {
			$ishFolders = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHTemplate -FolderName "ParameterGroup ISHTemplate" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter FolderType ISHQuery" {
			{ Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHQuery -FolderName "ParameterGroup ISHQuery" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal }  | Should -Throw
		}
		It "Parameter FolderType ISHReference" {
			{ Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHReference -FolderName "ParameterGroup ISHReference" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal }  | Should -Throw
		}
	}

	# Most cmdldets take IshFolder pipeline to apply on, this one will apply the incoming IshFolder to become subfolders of the ParentFolder
	Context "Add-IshFolder IshFoldersGroup" {
		BeforeAll {
			$ishFolderData = Get-IshFolder -IShSession $ishSession -BaseFolder Data
			$ishFolderSystem = Get-IshFolder -IShSession $ishSession -BaseFolder System
			$ishFolderFavorites = Get-IshFolder -IShSession $ishSession -BaseFolder Favorites
			$ishFolderEditorTemplate = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate
		}
		It "Parameter IshFolder invalid" {
			{ Add-IshFolder -IShSession $ishSession -IshFolder "INVALIDFOLDERID" } | Should -Throw
		}
		It "Parameter IshFolder Single with implicit IshSession" {
			$ishFolderEditorTemplate = $ishFolderEditorTemplate | Set-IshMetadataField -Name "FNAME" -Level None -Value "EditorTemplate IshFoldersGroup Parameter IshFolder Single"
			$ishFolders = Add-IshFolder -IshFolder $ishFolderEditorTemplate -ParentFolderId $ishFolderCmdlet.IshFolderRef
			$ishFolders.Count | Should -Be 1
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			$ishFolderEditorTemplate = $ishFolderEditorTemplate | Set-IshMetadataField -Name "FNAME" -Level None -Value "EditorTemplate IshFoldersGroup Parameter IshFolder Multiple"
			$ishFolderFavorites = $ishFolderFavorites | Set-IshMetadataField -Name "FNAME" -Level None -Value "Favorites IshFoldersGroup Parameter IshFolder Multiple"
			$ishFolders = Add-IshFolder -IshFolder @($ishFolderEditorTemplate,$ishFolderFavorites) -ParentFolderId $ishFolderCmdlet.IshFolderRef
			$ishFolders.Count | Should -Be 2
		}
		It "Pipeline IshFolder Single" {
			$ishFolderData = $ishFolderData | Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -Value "EditorTemplate IshFoldersGroup Pipeline IshFolder Single"
			$ishFolders = $ishFolderData | Add-IshFolder -IshSession $ishSession -ParentFolderId $ishFolderCmdlet.IshFolderRef
			$ishFolders.Count | Should -Be 1
		}
		It "Pipeline IshFolder Multiple" {
			$ishFolderData = $ishFolderData | Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -Value "EditorTemplate IshFoldersGroup Pipeline IshFolder Multiple"
			$ishFolderSystem = $ishFolderSystem | Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -Value "System IshFoldersGroup Pipeline IshFolder Multiple"
			$ishFolders = @($ishFolderData,$ishFolderSystem) | Add-IshFolder -IshSession $ishSession -ParentFolderId $ishFolderCmdlet.IshFolderRef
			$ishFolders.Count | Should -Be 2
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}

