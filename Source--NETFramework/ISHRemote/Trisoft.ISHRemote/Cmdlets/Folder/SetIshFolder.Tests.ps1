Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Set-IshFolder"
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
	$ishFolderI = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderI" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderJ = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderJ" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderK = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderK" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderL = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderL" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderM = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderM" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderN = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderN" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderO = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderO" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderP = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "FolderP" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal


Describe “Set-IshFolder" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"

	Context “Set-IshFolder ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Set-IshFolder -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Set-IshFolder returns IshFolder object" {
		$ishFolderData = Set-IshFolder -IShSession $ishSession -IshFolder $ishFolderA
		It "GetType().Name" {
			$ishFolderData.GetType().Name | Should BeExactly "IshFolder"
		}
		It "ishFolderData.IshFolderRef" {
			$ishFolderData.IshFolderRef -ge 0 | Should Be $true
		}
		It "ishFolderData.IshFolderType" {
			$ishFolderData.IshFolderType | Should Not BeNullOrEmpty
		}
		It "ishFolderData.IshField" {
			$ishFolderData.IshField | Should Not BeNullOrEmpty
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			$ishFolderData.name.Length -ge 1 | Should Be $true 
			$ishFolderData.fdocumenttype.Length -ge 1 | Should Be $true 
			$ishFolderData.fdocumenttype_none_element.StartsWith('VDOCTYPE') | Should Be $true 
		}
	}

	# Order in renames matters
	Context “Set-IshFolder FolderPathGroup" {
		It "Parameter FolderPath invalid" {
			{ Set-IshFolder -IShSession $ishSession -FolderPath "INVALIDFOLDERPATH" -NewFolderName "FolderPathGroup INVALIDFOLDERPATH" } | Should Throw "-102001"
		}
		It "Parameter FolderPath $folderTestRootPath" {
			Set-IshFolder -IshSession $ishSession -FolderPath (Join-Path $folderCmdletRootPath "FolderB") -NewFolderName "FolderPathGroup $cmdletName" | Should Not BeNullOrEmpty
		}
	}

	Context “Set-IshFolder BaseFolderGroup" {
		$ishFolderDataOrginalName = Get-IshFolder -IShSession $ishSession -BaseFolder Data | Get-IshMetadataField -IshSession $ishSession -Name "FNAME"
		$ishFolderSystemOriginalName = Get-IshFolder -IShSession $ishSession -BaseFolder System | Get-IshMetadataField -IshSession $ishSession -Name "FNAME"
		$ishFolderFavoritesOriginalName = Get-IshFolder -IShSession $ishSession -BaseFolder Favorites | Get-IshMetadataField -IshSession $ishSession -Name "FNAME"
		$ishFolderEditorTemplateOriginalName = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate | Get-IshMetadataField -IshSession $ishSession -Name "FNAME"
		It "Parameter BaseFolder invalid" {
			{ Set-IshFolder -IShSession $ishSession -BaseFolder None } | Should Throw
		}
		It "Parameter BaseFolder Data" {
			Set-IshFolder -IShSession $ishSession -BaseFolder Data -NewFolderName "Set-IshFolder BaseFolderGroup Data"
			Set-IshFolder -IShSession $ishSession -BaseFolder Data -NewFolderName $ishFolderDataOrginalName | Should Not BeNullOrEmpty
		}
		It "Parameter BaseFolder System" {
			Set-IshFolder -IShSession $ishSession -BaseFolder System -NewFolderName "Set-IshFolder BaseFolderGroup System"
			Set-IshFolder -IShSession $ishSession -BaseFolder System -NewFolderName $ishFolderSystemOriginalName | Should Not BeNullOrEmpty
		}
		It "Parameter BaseFolder Favorites" {
			Set-IshFolder -IShSession $ishSession -BaseFolder Favorites -NewFolderName "Set-IshFolder BaseFolderGroup Favorites"
			Set-IshFolder -IShSession $ishSession -BaseFolder Favorites -NewFolderName $ishFolderFavoritesOriginalName | Should Not BeNullOrEmpty
		}
		It "Parameter BaseFolder EditorTemplate" {
			Set-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -NewFolderName "Set-IshFolder BaseFolderGroup EditorTemplate" 
			Set-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -NewFolderName $ishFolderEditorTemplateOriginalName | Should Not BeNullOrEmpty
		}
	}

	Context “Set-IshFolder FolderIdsGroup" {
		[long]$ishFolderCFolderRef = (Get-IshFolder -IShSession $ishSession -FolderPath (Join-Path $folderCmdletRootPath "FolderC")).IshFolderRef
		[long]$ishFolderDFolderRef = (Get-IshFolder -IShSession $ishSession -FolderPath (Join-Path $folderCmdletRootPath "FolderD")).IshFolderRef
		[long]$ishFolderEFolderRef = (Get-IshFolder -IShSession $ishSession -FolderPath (Join-Path $folderCmdletRootPath "FolderE")).IshFolderRef
		[long]$ishFolderFFolderRef = (Get-IshFolder -IShSession $ishSession -FolderPath (Join-Path $folderCmdletRootPath "FolderF")).IshFolderRef
		[long]$ishFolderGFolderRef = (Get-IshFolder -IShSession $ishSession -FolderPath (Join-Path $folderCmdletRootPath "FolderG")).IshFolderRef
		It "Parameter FolderId invalid" {
			{ Set-IshFolder -IShSession $ishSession -FolderId "INVALIDFOLDERID" } | Should Throw
		}
		It "Parameter FolderId Single" {
			Set-IshFolder -IshSession $ishSession -FolderId $ishFolderCFolderRef -NewFolderName "FolderIdsGroup C Rename" | Should Not BeNullOrEmpty
		}
		It "Parameter FolderId Multiple" {
			{ Set-IshFolder -IshSession $ishSession -FolderId $ishFolderDFolderRef,$ishFolderEFolderRef -NewFolderName "FolderIdsGroup D,E Rename" } | Should Throw
		}
		It "Pipeline FolderId Single" {
			$ishObjects = @($ishFolderDFolderRef,$ishFolderEFolderRef) | Set-IshFolder -IshSession $ishSession -ReadAccess $readAccessTestRootOriginal
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline FolderId Multiple" {
			$ishObjects = @($ishFolderFFolderRef,$ishFolderGFolderRef) | Set-IshFolder -IshSession $ishSession -ReadAccess $readAccessTestRootOriginal
			$ishObjects.Count | Should Be 2
		}
	}

	# Most cmdldets take IshFolder pipeline to apply on, this one will apply the incoming IshFolder
	Context “Set-IshFolder IshFoldersGroup" {
		It "Parameter IshFolder invalid" {
			{ Set-IshFolder -IShSession $ishSession -IshFolder "INVALIDFOLDERID" } | Should Throw
		}
		It "Parameter IshFolder Single with implicit IshSession" {
			$ishFolders = Set-IshFolder -IshFolder $ishFolderK
			$ishFolders.Count | Should Be 1
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			$ishFolders = Set-IshFolder -IshFolder @($ishFolderL,$ishFolderM)
			$ishFolders.Count | Should Be 2
		}
		It "Pipeline IshFolder Single" {
			$ishFolders = $ishFolderN | Set-IshFolder -IshSession $ishSession
			$ishFolders.Count | Should Be 1
		}
		It "Pipeline IshFolder Multiple" {
			$ishFolders = @($ishFolderO,$ishFolderP) | Set-IshFolder -IshSession $ishSession 
			$ishFolders.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
