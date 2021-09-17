BeforeAll {
	$cmdletName = "Get-IshFolderLocation"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshFolderLocation" -Tags "Read" {
	Context "Get-IshFolderLocation ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshFolderLocation -IShSession "INVALIDISHSESSION" } | Should -Throw
		}
	}
	Context "Get-IshFolderLocation returns string object" {
		It "GetType().Name" {
			$ishObjects = Get-IshFolderLocation -IShSession $ishSession -BaseFolder Data
			$ishObjects.GetType().Name | Should -BeExactly "String"
		}
	}
	Context "Get-IshFolderLocation BaseFolderGroup" {
		It "Parameter BaseFolder invalid" {
			{ Get-IshFolderLocation -IShSession $ishSession -BaseFolder None } | Should -Throw
		}
		It "Parameter BaseFolder Data" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder Data | Should -Not -BeNullOrEmpty
		}
		It "Parameter BaseFolder System" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder System | Should -Not -BeNullOrEmpty
		}
		It "Parameter BaseFolder Favorites" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder Favorites | Should -Not -BeNullOrEmpty
		}
		It "Parameter BaseFolder EditorTemplate" {
			Get-IshFolderLocation -IShSession $ishSession -BaseFolder EditorTemplate | Should -Not -BeNullOrEmpty
		}
	}
	Context "Get-IshFolderLocation FolderPathGroup" {
		It "Parameter FolderPath invalid" {
			$exception = { Get-IshFolderLocation -IShSession $ishSession -FolderPath "INVALIDFOLDERPATH" } | Should -Throw -PassThru
			# 14.0.4 message is:  [-102001] The folder 'INVALIDFOLDERPATH' does not exist. [name:"'INVALIDFOLDERPATH'"] [102001;InvalidObject]
			$exception -like "*102001*" | Should -Be $true 
			$exception -like "*InvalidObject*" | Should -Be $true
		}
		It "Parameter FolderPath $folderTestRootPath" {
			Get-IshFolderLocation -IshSession $ishSession -FolderPath $folderTestRootPath | Should -Be $folderTestRootPath
		}
	}
	Context "Get-IshFolderLocation FolderIdsGroup" {
		BeforeAll {
			[long]$ishFolderDataFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Data).IshFolderRef
			[long]$ishFolderSystemFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder System).IshFolderRef
			[long]$ishFolderFavoritesFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Favorites).IshFolderRef
			[long]$ishFolderEditorTemplateFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate).IshFolderRef
		}
		It "Parameter FolderId invalid" {
			{ Get-IshFolderLocation -IShSession $ishSession -FolderId "INVALIDFOLDERID" } | Should -Throw
		}
		It "Parameter FolderId from EditorTemplate" {
			Get-IshFolderLocation -IshSession $ishSession -FolderId $ishFolderEditorTemplateFolderRef | Should -Not -BeNullOrEmpty
		}
		It "Parameter FolderId 0 no longer returns BaseFolder" {
			$exception = { Get-IshFolderLocation -FolderId 0 } | Should -Throw -PassThru
			# 14.0.4 message is:  [-105001] The parameter folderRef with value "0" is invalid. Zero is not allowed as valid value. [105001;InvalidParameter]
			$exception -like "*105001*" | Should -Be $true 
			$exception -like "*InvalidParameter*" | Should -Be $true
		}
		It "Pipeline FolderId" {
			$ishObjects = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should -Be 4
		}
		It "Pipeline FolderId MetadataBatchSize[1]" {
			$ishSession.MetadataBatchSize = 1
			$ishObjects = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should -Be 4
		}
	}
	Context "Get-IshFolderLocation IshFoldersGroup" {
		BeforeAll {
			$ishFolderData = Get-IshFolder -IShSession $ishSession -BaseFolder Data
			$ishFolderSystem = Get-IshFolder -IShSession $ishSession -BaseFolder System
			$ishFolderFavorites = Get-IshFolder -IShSession $ishSession -BaseFolder Favorites
			$ishFolderEditorTemplate = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate
		}
		It "Parameter IshFolder invalid" {
			{ Get-IshFolderLocation -IShSession $ishSession -IshFolder "INVALIDFOLDERID" } | Should -Throw
		}
		It "Parameter IshFolder Single with implicit IshSession" {
			$ishObjects = Get-IshFolderLocation -IshFolder $ishFolderEditorTemplate
			$ishObjects.Count | Should -Be 1
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			$ishObjects = Get-IshFolderLocation -IshFolder @($ishFolderData,$ishFolderSystem)
			$ishObjects.Count | Should -Be 2
		}
		It "Pipeline IshFolder Single" {
			$ishObjects = @($ishFolderData) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should -Be 1
		}
		It "Pipeline IshFolder Multiple" {
			$ishObjects = @($ishFolderData,$ishFolderSystem,$ishFolderFavorites,$ishFolderEditorTemplate) | Get-IshFolderLocation -IshSession $ishSession
			$ishObjects.Count | Should -Be 4
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }

}

