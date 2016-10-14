Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshFolderContent"
try {

Describe “Get-IshFolderContent" -Tags "Read" {
	# TODO explicit count test, so initialize Get-IshFolderContent test folders with accurate count for -eq testing
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

	$ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$count = 3
	for($current=1;$current -le $count;$current++)
	{
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $current" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value "VUSERADMIN" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value "VSTATUSDRAFT"
		Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng VLANGUAGEEN -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
	}

	Context “Get-IshFolderContent ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshFolderContent -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Get-IshFolderContent returns IshObject[] object" {
		$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic
		It "GetType().Name" {
			$ishObjects.GetType().Name | Should BeExactly "Object[]"
		}
		It "[0]GetType().BaseType.Name" {
			$ishObjects[0].GetType().Name | Should BeExactly "IshObject"
		}
		It "ishObjects[0].IshData" {
			{ $ishObjects[0].IshData } | Should Not Throw
		}
		It "ishObjects[0].IshField" {
			$ishObjects[0].IshField | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].IshRef" {
			$ishObjects[0].IshRef | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].IshType" {
			$ishObjects[0].IshType | Should Not BeNullOrEmpty
		}
		# Double check following 3 ReferenceType enum usage 
		It "ishObjects[0].ObjectRef[Enumerations.ReferenceType.Logical]" {
			$ishObjects[0].ObjectRef["Logical"] | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].ObjectRef[Enumerations.ReferenceType.Version]" {
			$ishObjects[0].ObjectRef["Version"] | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].ObjectRef[Enumerations.ReferenceType.Lng]" {
			$ishObjects[0].ObjectRef["Lng"] | Should Not BeNullOrEmpty
		}
	}

	Context “Get-IshFolderContent BaseFolderGroup" {
		It "Parameter BaseFolder invalid" {
			{ Get-IshFolderContent -IShSession $ishSession -BaseFolder None } | Should Throw
		}
		It "Parameter BaseFolder Data" {
			(Get-IshFolderContent -IShSession $ishSession -BaseFolder Data).Count -eq 0 | Should Be $true
		}
		It "Parameter BaseFolder System" {
			(Get-IshFolderContent -IShSession $ishSession -BaseFolder System).Count -eq 0 | Should Be $true
		}
		It "Parameter BaseFolder Favorites" {
			{ Get-IshFolderContent -IShSession $ishSession -BaseFolder Favorites } | Should Not Throw
		}
		It "Parameter BaseFolder EditorTemplate" {
			(Get-IshFolderContent -IShSession $ishSession -BaseFolder EditorTemplate).Count -eq 0 | Should Be $true
		}
	}

	Context “Get-IshFolderContent FolderPathGroup" {
		It "Parameter FolderPath invalid" {
			{ Get-IshFolderContent -IShSession $ishSession -FolderPath "INVALIDFOLDERPATH" } | Should Throw "-102001"
		}
		It "Parameter FolderPath $folderTestRootPath" {
			(Get-IshFolderContent -IshSession $ishSession -FolderPath $folderTestRootPath).Count -ge 0 | Should Be $true
		}
		It "Parameter FolderPath Topic" {
			$folderPath = Get-IshFolderLocation -IshSession $ishSession -IshFolder $ishFolderTopic
			(Get-IshFolderContent -IshSession $ishSession -FolderPath $folderPath).Count | Should Be $count
		}
	}

	Context “Get-IshFolderContent FolderIdsGroup" {
		[long]$ishFolderDataFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Data).IshFolderRef
		[long]$ishFolderSystemFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder System).IshFolderRef
		[long]$ishFolderFavoritesFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder Favorites).IshFolderRef
		[long]$ishFolderEditorTemplateFolderRef = (Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate).IshFolderRef
		It "Parameter FolderId invalid" {
			{ Get-IshFolderContent -IShSession $ishSession -FolderId "INVALIDFOLDERID" } | Should Throw
		}
		It "Parameter FolderId from System" {
			(Get-IshFolderContent -IshSession $ishSession -FolderId $ishFolderSystemFolderRef).Count -eq 0 | Should Be $true
		}
		<# FolderId[] ValueFromPipeline is not implemented
		It "Pipeline FolderId" {
			$ishObjects = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolderContent -IshSession $ishSession
			$ishObjects.Count -ge 0 | Should Be $true
		}
		It "Pipeline FolderId MetadataBatchSize[1]" {
			$ishSession.MetadataBatchSize = 1
			$ishObjects = @($ishFolderDataFolderRef,$ishFolderSystemFolderRef,$ishFolderFavoritesFolderRef,$ishFolderEditorTemplateFolderRef) | Get-IshFolderContent -IshSession $ishSession
			$ishObjects.Count -ge 0 | Should Be $true
		}
		#>
	}

	Context “Get-IshFolderContent IshFoldersGroup" {
		$ishFolderData = Get-IshFolder -IShSession $ishSession -BaseFolder Data
		$ishFolderSystem = Get-IshFolder -IShSession $ishSession -BaseFolder System
		It "Parameter IshFolder invalid" {
			{ Get-IshFolderContent -IShSession $ishSession -IshFolder "INVALIDFOLDERID" } | Should Throw
		}
		It "Parameter IshFolder Single" {
			(Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderSystem).Count -eq 0 | Should Be $true
		}
		It "Parameter IshFolder Multiple" {
			$ishObjects = Get-IshFolderContent -IshSession $ishSession -IshFolder @($ishFolderData,$ishFolderSystem)
			$ishObjects.Count -eq 0| Should Be $true
		}
		It "Pipeline IshFolder Single" {
			$ishObjects = $ishFolderData | Get-IshFolderContent -IshSession $ishSession
			$ishObjects.Count -eq 0 | Should Be $true
		}
		It "Pipeline IshFolder Multiple" {
			$ishObjects = @($ishFolderData,$ishFolderSystem) | Get-IshFolderContent -IshSession $ishSession
			$ishObjects.Count -eq 0| Should Be $true
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
