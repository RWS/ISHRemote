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
	$readAccessTestRootOriginal = (Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Separator)

	$ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishTopicCount = 3
	for($current=1;$current -le $ishTopicCount;$current++)
	{
		# Create topic
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $current" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
		# Create extra version
		Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject.IshRef -Version "2" -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
	}

	Context “Get-IshFolderContent ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshFolderContent -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Get-IshFolderContent returns latest IshObject[] object" {
		$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic
		It "GetType().Name" {
			$ishObjects.GetType().Name | Should BeExactly "Object[]"
		}
		It "ishObjects.Count" {
			$ishObjects.Length | Should BeExactly $ishTopicCount  
		}
		It "[0]GetType().BaseType.Name" {
			# Used to be IshObject, but more specific ISHType like IshDocumentObj or IshPublicationOutput is put on the pipeline
			$ishObjects[0].GetType().Name | Should BeExactly "IshDocumentObj"  
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
		It "ishObjects[0].ObjectRef" {
			$ishObjects[0].ObjectRef | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].VersionRef" {
			$ishObjects[0].VersionRef | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].LngRef" {
			$ishObjects[0].LngRef | Should Not BeNullOrEmpty
		}
		It "ishObjects[0] ConvertTo-Json" {
			(ConvertTo-Json $ishObjects[0]).Length -gt 2 | Should Be $true
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			#logical
			$ishObjects[0].ftitle_logical_value.Length -ge 1 | Should Be $true 
			#version
			$ishObjects[0].version_version_value.Length -ge 1 | Should Be $true 
			$ishObjects[0].version_version_value -ge 2 | Should Be $true 
			#language
			$ishObjects[0].fstatus.Length -ge 1 | Should Be $true 
			$ishObjects[0].fstatus_lng_element.StartsWith('VSTATUS') | Should Be $true 
		}
	}

	Context "Get-IshFolderContent with empty VersionFilter returns IshObject[] object" {
		$ishObjects = Get-IshFolderContent -IShSession $ishSession -VersionFilter "" -IshFolder $ishFolderTopic
		It "GetType().Name" {
			$ishObjects.GetType().Name | Should BeExactly "Object[]"
		}
		It "ishObjects.Count" {
            $totalLngObjects = $ishTopicCount*2
			$ishObjects.Length | Should BeExactly $totalLngObjects
		}
		It "[0]GetType().BaseType.Name" {
			# Used to be IshObject, but more specific ISHType like IshDocumentObj or IshPublicationOutput is put on the pipeline
			$ishObjects[0].GetType().Name | Should BeExactly "IshDocumentObj"  
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
		It "ishObjects[0].ObjectRef" {
			$ishObjects[0].ObjectRef | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].VersionRef" {
			$ishObjects[0].VersionRef | Should Not BeNullOrEmpty
		}
		It "ishObjects[0].LngRef" {
			$ishObjects[0].LngRef | Should Not BeNullOrEmpty
		}
		It "ishObjects[0] ConvertTo-Json" {
			(ConvertTo-Json $ishObjects[0]).Length -gt 2 | Should Be $true
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			#logical
			$ishObjects[0].ftitle_logical_value.Length -ge 1 | Should Be $true 
			#version
			$ishObjects[0].version_version_value.Length -ge 1 | Should Be $true 
			#language
			$ishObjects[0].fstatus.Length -ge 1 | Should Be $true 
			$ishObjects[0].fstatus_lng_element.StartsWith('VSTATUS') | Should Be $true 
        }
        It "ishObjects[0].version_version_value" { 
            # First version
            ($ishobjects | Where-Object version_version_value -eq 1 | Select-Object).Length | Should BeExactly $ishTopicCount 
            # Second version
            ($ishobjects | Where-Object version_version_value -eq 2 | Select-Object).Length | Should BeExactly $ishTopicCount 
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
			(Get-IshFolderContent -IshSession $ishSession -FolderPath $folderPath).Count | Should Be $ishTopicCount
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
		It "Parameter IshFolder Single with implicit IshSession" {
			(Get-IshFolderContent -IshFolder $ishFolderSystem).Count -eq 0 | Should Be $true
		}
		It "Parameter IshFolder Multiple with implicit IshSession" {
			$ishObjects = Get-IshFolderContent -IshFolder @($ishFolderData,$ishFolderSystem)
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

	Context “Get-IshFolderContent IshFoldersGroup mixing MetadataFilter and VersionFilter/LanguagesFilter" {
		$ishDocumentObjsVersionOne = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -VersionFilter 1 -LanguagesFilter ($ishLng,$ishLngTarget1)
		$ishDocumentObjsVersionLatest = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -VersionFilter LATEST -LanguagesFilter ($ishLng,$ishLngTarget1)
		$ishDocumentObjsAllLanguages = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic 
		$ishDocumentObjsExplicitLanguage = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter $ishLng
		$metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Level Lng -Name FSTATUS -ValueType Element -FilterOperator Equal -Value $ishStatusDraft
		$ishDocumentObjsExplicitLanguageAndStatus = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter $ishLng -MetadataFilter $metadataFilter
		$metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Level Lng -Name FSTATUS -ValueType Element -FilterOperator NotEqual -Value $ishStatusDraft
		$ishDocumentObjsExplicitLanguageAndWrongStatus = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter $ishLng -MetadataFilter $metadataFilter
		$metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Level Lng -Name DOC-LANGUAGE -ValueType Element -FilterOperator In -Value $ishLng
		$ishDocumentObjsExplicitLanguageAndLanguage = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter $ishLng -MetadataFilter $metadataFilter
		$metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Level Lng -Name DOC-LANGUAGE -ValueType Element -FilterOperator NotEqual -Value $ishLng
		$ishDocumentObjsLanguageFilterOverridesMetadatafilter = Get-IshFolderContent -IshSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter $ishLng -MetadataFilter $metadataFilter
		It "Parameter VersionFilter" {
			$ishDocumentObjsVersionOne.Count | Should Be $ishTopicCount
			$ishDocumentObjsVersionLatest.Count | Should Be $ishTopicCount
			$ishDocumentObjsVersionOne.Count -eq $ishDocumentObjsVersionLatest.Count | Should Be $true
		}
		It "Parameter LanguagesFilter" {
			$ishDocumentObjsAllLanguages.Count | Should Be $ishTopicCount
			$ishDocumentObjsExplicitLanguage.Count | Should Be $ishTopicCount
			$ishDocumentObjsAllLanguages.Count -eq $ishDocumentObjsExplicitLanguage.Count | Should Be $true
		}
		It "Parameter LanguagesFilter and matching status MetadataFilter" {
			$ishDocumentObjsExplicitLanguageAndStatus.Count -eq $ishDocumentObjsExplicitLanguage.Count | Should Be $true
		}
		It "Parameter LanguagesFilter and non-matching status MetadataFilter" {
			$ishDocumentObjsExplicitLanguageAndWrong.Count | Should Be 0
		}
		It "Parameter LanguagesFilter and matching language MetadataFilter" {
			$ishDocumentObjsExplicitLanguageAndLanguage.Count | Should Be $ishTopicCount
		}
		It "Parameter LanguagesFilter overrides MetadataFilter (non-matching language filter)" {
			$ishDocumentObjsLanguageFilterOverridesMetadatafilter.Count | Should Be 3
		}
	}
	Context "Get-IshFolderContent use LanguagesFilter" {
		It "LanguagesFilter on '$ishLngLabel' language" {
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter "$ishLngLabel"  # en
			$ishObjects.Count | Should Be $ishTopicCount
		}
		It "LanguagesFilter on '$ishLngTarget2Label, $ishLngLabel' languages" {
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter @("$ishLngTarget2Label", "$ishLngLabel")  # de, en
			$ishObjects.Count | Should Be $ishTopicCount
		}
		It "LanguagesFilter on '$ishLngTarget1Label' language (filtering out results)" {
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -LanguagesFilter @("$ishLngTarget1Label")  # es
			$ishObjects.Count | Should Be 0
		}
		It "LanguagesFilter overrides MetadataFilter" {
			$metadataFilter = Set-IshMetadataFilterField -Name "DOC-LANGUAGE" -Level Lng -Value "$ishLngTarget1Label" -ValueType Value  # es
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -MetadataFilter $metadataFilter -LanguagesFilter @("$ishLngTarget2Label", "$ishLngLabel")  # de, en
			$ishObjects.Count | Should Be $ishTopicCount
		}
	}
	
	Context "Get-IshFolderContent use MetadataFilter" {
		It "Metadata filter on FSTATUS" {
			$metadataFilter = Set-IshMetadataFilterField -Name "FSTATUS" -Level Lng -Value $ishStatusDraft -ValueType Element
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -MetadataFilter $metadataFilter
			$ishObjects.Count | Should Be $ishTopicCount
		}
		
		It "Metadata filter on FSTATUS (filtering out results)" {
			$metadataFilter = Set-IshMetadataFilterField -Name "FSTATUS" -Level Lng -Value $ishStatusReleased -ValueType Element
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -MetadataFilter $metadataFilter
			$ishObjects.Count | Should Be 0
		}
		
		It "Metadata filter on FSTATUS,DOC-LANGUAGE" {
			$metadataFilter = Set-IshMetadataFilterField -Name "FSTATUS" -Level Lng -Value $ishStatusDraft -ValueType Element |
							  Set-IshMetadataFilterField -Name "DOC-LANGUAGE" -Level Lng -Value "$ishLngLabel" -ValueType Value  # en
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -MetadataFilter $metadataFilter
			$ishObjects.Count | Should Be $ishTopicCount
		}
		It "Metadata filter on (FSTATUS, DOC-LANGUAGE) with LanguagesFilter override" {
			$metadataFilter = Set-IshMetadataFilterField -Name "FSTATUS" -Level Lng -Value $ishStatusDraft -ValueType Element |
							  Set-IshMetadataFilterField -Name "DOC-LANGUAGE" -Level Lng -Value "$ishLngLabel" -ValueType Value  # en
			$ishObjects = Get-IshFolderContent -IShSession $ishSession -IshFolder $ishFolderTopic -MetadataFilter $metadataFilter -LanguagesFilter @("$ishLngTarget2Label", "$ishLngTarget1Label")  # de, es
			$ishObjects.Count | Should Be 0
		}		
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession -VersionFilter "" | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
