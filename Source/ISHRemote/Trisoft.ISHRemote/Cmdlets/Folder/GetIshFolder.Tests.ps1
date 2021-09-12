BeforeAll {
	$cmdletName = "Get-IshFolder"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshFolder" -Tags "Read" {
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
		$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "aLL yOUR bASE bELONG tO uS!" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "All" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "Your" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "Base" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "Belong" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "To" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolder = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolder.IshFolderRef)       -FolderType ISHModule -FolderName "Us!" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderData = Get-IshFolder -IShSession $ishSession -BaseFolder Data
	}
	Context "Get-IshFolder ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshFolder -IShSession "INVALIDISHSESSION" } | Should -Throw
		}
	}
	Context "Get-IshFolder returns IshFolder object" {
		It "GetType()" {
			$ishFolderData.GetType().Name | Should -BeExactly "IshFolder"
		}
		It "ishFolderData.IshFolderRef" {
			$ishFolderData.IshFolderRef -ge 0 | Should -Be $true
		}
		It "ishFolderData.IshFolderType" {
			$ishFolderData.IshFolderType | Should -Not -BeNullOrEmpty
		}
		It "ishFolderData.IshField" {
			$ishFolderData.IshField | Should -Not -BeNullOrEmpty
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should -Be "Basic"
			$ishFolderData.name.Length -ge 1 | Should -Be $true 
			$ishFolderData.fdocumenttype.Length -ge 1 | Should -Be $true 
			$ishFolderData.fdocumenttype_none_element.StartsWith('VDOCTYPE') | Should -Be $true 
		}
	}
	Context "Get-IshFolder BaseFolderGroup" {
		It "Parameter BaseFolder invalid" {
			{ Get-IshFolder -IShSession $ishSession -BaseFolder None } | Should -Throw
		}		
		It "Parameter BaseFolder Data" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder Data).IshFolderRef -ge 0 | Should -Be $true
		}
		It "Parameter BaseFolder System" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder System).IshFolderRef -ge 0 | Should -Be $true
		}
		It "Parameter BaseFolder Favorites" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder Favorites).IshFolderRef -ge 0 | Should -Be $true
		}
		It "Parameter BaseFolder EditorTemplate" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate).IshFolderRef -ge 0 | Should -Be $true
		}
	}
	Context "Get-IshFolder FolderPathGroup" {
		It "Parameter FolderPath invalid" {
			$exception = { Get-IshFolder -IShSession $ishSession -FolderPath "INVALIDFOLDERPATH" } | Should -Throw -PassThru
			# 14.0.4 message is:  [-102001] The folder 'INVALIDFOLDERPATH' does not exist. [name:"'INVALIDFOLDERPATH'"] [102001;InvalidObject]
			$exception -like "*102001*" | Should -Be $true 
			$exception -like "*InvalidObject*" | Should -Be $true
		}
		It "Parameter FolderPath $folderTestRootPath" {
			(Get-IshFolder -IshSession $ishSession -FolderPath $folderTestRootPath).IshFolderRef -ge 0 | Should -Be $true
		}
	}
	Context "Get-IshFolder FolderIdsGroup" {
		BeforeAll {
			[long]$ishFolderDataFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Data).IshFolderRef
			[long]$ishFolderSystemFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder System).IshFolderRef
			[long]$ishFolderFavoritesFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Favorites).IshFolderRef
			[long]$ishFolderEditorTemplateFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate).IshFolderRef
		}
		It "Parameter FolderId invalid" {
			{ Get-IshFolder -IShSession $ishSession -FolderId "INVALIDFOLDERID" } | Should -Throw
		}
		It "Parameter FolderId zero" {
			{ Get-IshFolder -IShSession $ishSession -FolderId 0 } | Should -Throw
		}
		It "Parameter FolderId from Data" {
			(Get-IshFolder -IshSession $ishSession -FolderId $ishFolderDataFolderRef).IshFolderRef -ge 0 | Should -Be $true
		}
		<# FolderId is defined as long, not long[], so don't even know if PowerShell automatically converts
		It "Parameter FolderId from Data.System" {
			(Get-IshFolder -IshSession $ishSession -FolderId [long[]]@($ishFolderDataFolderRef,$ishFolderSystemFolderRef)).Count -eq 2 | Should -Be $true
		}
		#>
		It "Pipeline FolderId" {
			$ishFolders = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolder -IshSession $ishSession
			$ishFolders.Count -eq 4 | Should -Be $true
		}
		It "Pipeline FolderId MetadataBatchSize[1]" {
			$ishSession.MetadataBatchSize = 1
			$ishFolders = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolder -IshSession $ishSession
			$ishFolders.Count -eq 4 | Should -Be $true
		}
	}
	Context "Get-IshFolder IshFoldersGroup" {
		BeforeAll {
			$ishFolderData = Get-IshFolder -IShSession $ishSession -BaseFolder Data
			$ishFolderSystem = Get-IshFolder -IShSession $ishSession -BaseFolder System
			$ishFolderFavorites = Get-IshFolder -IShSession $ishSession -BaseFolder Favorites
			$ishFolderEditorTemplate = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate
		}
		It "Parameter IshFolder invalid" {
			{ Get-IshFolder -IShSession $ishSession -IshFolder "INVALIDFOLDERID" } | Should -Throw
		}
		It "Parameter IshFolder Single with implicit IshSession" {
			(Get-IshFolder -IshFolder $ishFolderData).IshFolderRef -ge 0 | Should -Be $true
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			(Get-IshFolder -IshFolder @($ishFolderData,$ishFolderSystem,$ishFolderFavorites,$ishFolderEditorTemplate)).Count -eq 4 | Should -Be $true
		}
		It "Pipeline IshFolder Single" {
			$ishFolders = $ishFolderData | Get-IshFolder -IshSession $ishSession
			$ishFolders.Count -eq 1 | Should -Be $true
		}
		It "Pipeline IshFolder Multiple" {
			$ishSession.MetadataBatchSize = 2
			$ishFolders = @($ishFolderData,$ishFolderSystem,$ishFolderFavorites,$ishFolderEditorTemplate) | Get-IshFolder -IshSession $ishSession
			$ishFolders.Count -eq 4 | Should -Be $true
		}
	}
	Context "Get-IshFolder EditorTemplate Recurse" {
		BeforeAll {
			$ishFolderEditorTemplateRecursive = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -Recurse
		}
		It "GetType()" {
			$ishFolderEditorTemplateRecursive[0].GetType().Name | Should -BeExactly "IshFolder"
		}
		It "Get-IshFolder EditorTemplate Count" {
			$ishFolderEditorTemplateRecursive.Count -ge 4 | Should -Be $true
		}
		It "Get-IshFolder EditorTemplate Count Depth=-1" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -Recurse -Depth -1).Count | Should -Be 0
		}
		It "Get-IshFolder EditorTemplate Count Depth=0" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -Recurse -Depth 0).Count | Should -Be 0
		}
		It "Get-IshFolder EditorTemplate Count Depth=1" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -Recurse -Depth 1).Count | Should -Be 1
		}
		It "Get-IshFolder EditorTemplate Count Depth=2" {
			(Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -Recurse -Depth 2).Count  -ge 4 | Should -Be $true
		}
	}
	Context "Get-IshFolder Recurse Sort and Traversal" {
		BeforeAll {
			$ishFolderEditorTemplateRecursive = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -Recurse
		}
		It "GetType()" {
			$ishFolderEditorTemplateRecursive[0].GetType().Name | Should -BeExactly "IshFolder"
		}
		It "Get-IshFolder ishFolderCmdlet Count" {
			$ishFolderEditorTemplateRecursive.Count -ge 6 | Should -Be $true
		}
		It "Get-IshFolder ishFolderCmdlet Count Depth=2" {
			(Get-IshFolder -IShSession $ishSession -IshFolder $ishFolderCmdlet -Recurse -Depth 2).Count | Should -Be 3
		}
		It "Get-IshFolder ishFolderCmdlet Traversal 'All'" {
			Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -IshField((Get-IshFolder -IShSession $ishSession -IshFolder $ishFolderCmdlet -Recurse)[1].IshField) | Should -MatchExactly "All"
		}
		It "Get-IshFolder ishFolderCmdlet Traversal 'aLL'" {
			Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -IshField((Get-IshFolder -IShSession $ishSession -IshFolder $ishFolderCmdlet -Recurse)[7].IshField) | Should -MatchExactly "aLL yOUR bASE bELONG tO uS!"
		}
		It "Get-IshFolder ishFolderCmdlet Traversal 'All' with FolderTypeFilter" {
			Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -IshField((Get-IshFolder -IShSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHNone") -Recurse)[1].IshField) | Should -MatchExactly "All"
		}
		It "Get-IshFolder ishFolderCmdlet Traversal 'aLL' with FolderTypeFilter" {
			Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -IshField((Get-IshFolder -IShSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHNone") -Recurse)[7].IshField) | Should -MatchExactly "aLL yOUR bASE bELONG tO uS!"
		}
	}
	Context "Get-IshFolder System Recurse Pipeline" {
		It "Get-IshFolder System Recurse Pipeline to Get-IshFolderContent" {
			# Check Fiddler that Folder calls alternate with DocumentObj calls confirming the pipeline
			$ishObjects = Get-IshFolder -IShSession $ishSession -BaseFolder System -Recurse | Get-IshFolderContent -IshSession $ishSession
			$ishObjects.Count -ge 1 | Should -Be $true
		}
	}
    Context "Get-IshFolder with requesting FolderTypeFilter and Recursion" {
        It "Get-IshFolder with incorrect FolderTypeFilter" {
           {Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter "ISHWrongType" -Recurse} | Should -Throw
        }
		It "Get-IshFolder with incorrect value in array FolderTypeFilter" {
            {Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHWrongType") -Recurse} | Should -Throw
        }
		It "Get-IshFolder with empty FolderTypeFilter" {
            {Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter "" -Recurse} | Should -Throw
        }
        It "Get-IshFolder with filtering on all Xml content folders" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary", "ISHTemplate") -Recurse).Count | Should -Be 7
        }
        It "Get-IshFolder with filtering on all Xml content folders with Depth=2" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary", "ISHTemplate") -Recurse -Depth 2).Count | Should -Be 2
        }
        It "Get-IshFolder with filtering on all folders with Depth=2" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary", "ISHTemplate", "ISHNone", "ISHIllustration") -Recurse -Depth 2).Count | Should -Be 3
        }
		It "Get-IshFolder without filtering on FolderType" {
			(Get-IshFolder -IShSession $ishSession -IshFolder $ishFolderCmdlet -Recurse).Count | Should -Be 8
        }
		It "Get-IshFolder with filtering on only ISHModule folders" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule") -Recurse).Count | Should -Be 7
        }
        It "Get-IshFolder with filtering on only ISHNone folders" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHNone") -Recurse).Count | Should -Be 1
        }
        It "Get-IshFolder with filtering out all folders" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHIllustration") -Recurse).Count | Should -Be 0
		}
    }
    Context "Get-IshFolder with requesting FolderTypeFilter and with no Recursion" {
        It "Get-IshFolder with incorrect FolderTypeFilter" {
           {Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter "ISHWrongType" } | Should -Throw
        }
		It "Get-IshFolder with incorrect value in array FolderTypeFilter" {
            {Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHWrongType") } | Should -Throw
        }
		It "Get-IshFolder with empty FolderTypeFilter" {
            {Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter "" } | Should -Throw
        }
        It "Get-IshFolder with filtering on only ISHNone folders" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHNone")).Count | Should -Be 1
        }
        It "Get-IshFolder with filtering out all folders" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHIllustration")).Count | Should -Be 0
		}
        It "Get-IshFolder with filtering out all folders(Filter array)" {
			(Get-IshFolder -IshSession $ishSession -IshFolder $ishFolderCmdlet -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary", "ISHTemplate")).Count | Should -Be 0
        }
    }
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}

