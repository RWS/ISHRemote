Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshFolderLocation"
try {

Describe “Get-IshFolderLocation" -Tags "Read" {
	Write-Host "Initializing Test Data and Variables"

	Context “Get-IshFolderLocation ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshFolderLocation -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Get-IshFolderLocation returns string object" {
		$ishObjects = Get-IshFolderLocation -IShSession $ishSession -BaseFolder Data
		It "GetType().Name" {
			$ishObjects.GetType().Name | Should BeExactly "String"
		}
	}

	Context “Get-IshFolderLocation BaseFolderGroup" {
		It "Parameter BaseFolder invalid" {
			{ Get-IshFolderLocation -IShSession $ishSession -BaseFolder None } | Should Throw
		}
		It "Parameter BaseFolder Data" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder Data | Should Not BeNullOrEmpty
		}
		It "Parameter BaseFolder System" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder System | Should Not BeNullOrEmpty
		}
		It "Parameter BaseFolder Favorites" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder Favorites | Should Not BeNullOrEmpty
		}
		It "Parameter BaseFolder EditorTemplate" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder EditorTemplate | Should Not BeNullOrEmpty
		}
	}

	Context “Get-IshFolderLocation FolderPathGroup" {
		It "Parameter FolderPath invalid" {
			{ Get-IshFolderLocation -IShSession $ishSession -FolderPath "INVALIDFOLDERPATH" } | Should Throw "-102001"
		}
		It "Parameter FolderPath $folderTestRootPath" {
			Get-IshFolderLocation -IshSession $ishSession -FolderPath $folderTestRootPath | Should Be $folderTestRootPath
		}
	}

	Context “Get-IshFolderLocation FolderIdsGroup" {
		[long]$ishFolderDataFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Data).IshFolderRef
		[long]$ishFolderSystemFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder System).IshFolderRef
		[long]$ishFolderFavoritesFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Favorites).IshFolderRef
		[long]$ishFolderEditorTemplateFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate).IshFolderRef
		It "Parameter FolderId invalid" {
			{ Get-IshFolderLocation -IShSession $ishSession -FolderId "INVALIDFOLDERID" } | Should Throw
		}
		It "Parameter FolderId from EditorTemplate" {
			Get-IshFolderLocation -IshSession $ishSession -FolderId $ishFolderEditorTemplateFolderRef | Should Not BeNullOrEmpty
		}
		It "Pipeline FolderId" {
			$ishObjects = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should Be 4
		}
		It "Pipeline FolderId MetadataBatchSize[1]" {
			$ishSession.MetadataBatchSize = 1
			$ishObjects = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should Be 4
		}
	}

	Context “Get-IshFolderLocation IshFoldersGroup" {
		$ishFolderData = Get-IshFolder -IShSession $ishSession -BaseFolder Data
		$ishFolderSystem = Get-IshFolder -IShSession $ishSession -BaseFolder System
		$ishFolderFavorites = Get-IshFolder -IShSession $ishSession -BaseFolder Favorites
		$ishFolderEditorTemplate = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate
		It "Parameter IshFolder invalid" {
			{ Get-IshFolderLocation -IShSession $ishSession -IshFolder "INVALIDFOLDERID" } | Should Throw
		}
		It "Parameter IshFolder Single with implicit IshSession" {
			$ishObjects = Get-IshFolderLocation -IshFolder $ishFolderEditorTemplate
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			$ishObjects = Get-IshFolderLocation -IshFolder @($ishFolderData,$ishFolderSystem)
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline IshFolder Single" {
			$ishObjects = @($ishFolderData) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshFolder Multiple" {
			$ishObjects = @($ishFolderData,$ishFolderSystem,$ishFolderFavorites,$ishFolderEditorTemplate) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should Be 4
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }

}
