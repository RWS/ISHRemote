BeforeAll {
	$cmdletName = "Get-IshPublicationOutputContent"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshPublicationOutputContent" -Tags "Read" {
	BeforeAll {
		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FNAME" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "FDOCUMENTTYPE" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element
		$ishFolderTestRootOriginal = Get-IshFolder -IShSession $ishSession -FolderPath $folderTestRootPath -RequestedMetadata $requestedMetadata
		$folderIdTestRootOriginal = $ishFolderTestRootOriginal.IshFolderRef
		$folderTypeTestRootOriginal = $ishFolderTestRootOriginal.IshFolderType
		
		Write-Debug("folderIdTestRootOriginal[" + $folderIdTestRootOriginal + "] folderTypeTestRootOriginal[" + $folderTypeTestRootOriginal + "]")
		$ownedByTestRootOriginal = Get-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField
		$readAccessTestRootOriginal = (Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Separator)

		$ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

		# ---- Create a topic ----
		$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Topic $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectTopic = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule `
			-LogicalId ("GETISHPUBLICATIONOUTPUTCONTENT-TOPIC-" + $timestamp) -Version '1' -Lng $ishLng `
			-Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent

		# ---- Create a map referencing the topic ----
		$mapFileContent = $ditaMapWithTopicrefFileContent -replace '<GUID-PLACEHOLDER>', $ishObjectTopic.IshRef
		$ishFolderMap = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "Map" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Map $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectMap = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc `
			-LogicalId ("GETISHPUBLICATIONOUTPUTCONTENT-MAP-" + $timestamp) -Version '2' -Lng $ishLng `
			-Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $mapFileContent
		
		# ---- Create a library topic ----
		$ishFolderLibraryTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHLibrary -FolderName "Lib" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishLibraryTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Topic $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectLibraryTopic = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderLibraryTopic -IshType ISHLibrary `
			-LogicalId ("GETISHPUBLICATIONOUTPUTCONTENT-LIB-" + $timestamp) -Version '3' -Lng $ishLng `
			-Metadata $ishLibraryTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent

		# ---- Create a publication output (Add-IshPublicationOutput implicitly creates a fresh baseline) ----
		$ishFolderPub = Add-IshFolder -IshSession $ishSession -ParentFolderId ($ishFolderCmdlet.IshFolderRef) -FolderType ISHPublication -FolderName "Pub" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishPubMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Pub $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHMASTERREF" -Level Version -ValueType Element -Value $ishObjectMap.IshRef |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHRESOURCES" -Level Version -ValueType Element -Value $ishObjectLibraryTopic.IshRef |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
		$ishObjectPub = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub `
			-LogicalId ("GETISHPUBLICATIONOUTPUTCONTENT-PUB-" + $timestamp) -Version '1' `
			-LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata

		# ---- Retrieve the auto-created baseline ID and pin the map and topic versions in it ----
		# Add-IshPublicationOutput implicitly creates a fresh empty baseline; retrieve its element name.
		$baselineId = $ishObjectPub |
			Get-IshPublicationOutput -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHBASELINE" -Level Version -ValueType Element) |
			Get-IshMetadataField -IshSession $ishSession -Name "FISHBASELINE" -Level Version -ValueType Element
		$ishBaseline = Get-IshBaseline -IshSession $ishSession -Id $baselineId
		$ishBaseline = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishBaseline -LogicalId $ishObjectMap.IshRef -Version '2'
		$ishBaseline = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishBaseline -LogicalId $ishObjectTopic.IshRef -Version '1'
		#$ishBaseline = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishBaseline -LogicalId $ishObjectLibraryTopic.IshRef -Version '3' #deliberately to set up autocomplete
	}

	Context "Get-IshPublicationOutputContent ParameterGroup validation" {
		It "InvalidIshSession" {
			{ Get-IshPublicationOutputContent -IshSession "INVALIDISHSESSION" -IshObject $ishObjectPub } | Should-Throw
		}
	}

	Context "Get-IshPublicationOutputContent IshObjectGroup empty pipeline" {
		It "EmptyIshObject over pipeline" {
			$result = @() | Get-IshPublicationOutputContent -IshSession $ishSession
			$result.Count | Should-Be 0
		}
		It "EmptyIshObject over parameter" {
			$result = Get-IshPublicationOutputContent -IshSession $ishSession -IshObject @()
			$result.Count | Should-Be 0
		}
	}

	Context "Get-IshPublicationOutputContent IshObjectGroup" {
		BeforeAll {
			$result = $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession
		}
		It "Returns IshDocumentObj objects" {
			$result | Should-NotBeNull
			$result.Count -eq 2 | Should-Be $true
		}
		It "GetType returns IshDocumentObj" {
			foreach ($item in $result) {
				$item.GetType().Name | Should-BeString -CaseSensitive "IshDocumentObj"
			}
		}
		It "ResultContainsMap" {
			$mapFound = $result | Where-Object { $_.IshRef -eq $ishObjectMap.IshRef }
			$mapFound | Should-NotBeNull
		}
		It "ResultContainsTopic" {
			$topicFound = $result | Where-Object { $_.IshRef -eq $ishObjectTopic.IshRef }
			$topicFound | Should-NotBeNull
		}
		It "ResultDoesNotContainLibraryTopic" {
			# Library topic was deliberately not pinned in the baseline; expand-only (no -AutoCompleteMode) should not reach it.
			$libFound = $result | Where-Object { $_.IshRef -eq $ishObjectLibraryTopic.IshRef }
			$libFound | Should-BeNull
		}
		It "result[0].IshData" {
			{ $result[0].IshData } | Should -Not -Throw
		}
		It "result[0].IshField" {
			$result[0].IshField | Should-NotBeNull
		}
		It "result[0].IshType" {
			$result[0].IshType | Should-NotBeNull
		}
		It "result[0].ObjectRef" {
			$result[0].ObjectRef | Should-NotBeNull
		}
		It "result[0].VersionRef" {
			$result[0].VersionRef | Should-NotBeNull
		}
		It "result[0].LngRef" {
			$result[0].LngRef | Should-NotBeNull
		}
		It "Explicit IshObject parameter instead of pipeline" {
			$explicitResult = Get-IshPublicationOutputContent -IshSession $ishSession -IshObject $ishObjectPub
			$explicitResult.Count -eq 2 | Should-Be $true
		}
		It "RequestedMetadata returns extra fields" {
			$extraRequestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical
			$requestedResult = $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -RequestedMetadata $extraRequestedMetadata
			$requestedResult.Count -eq 2 | Should-Be $true
			$withTitle = $requestedResult | Where-Object { $_.ftitle_logical_value.Length -ge 1 }
			$withTitle | Should-NotBeNull
		}
	}

	Context "Get-IshPublicationOutputContent IshObjectGroup AutoCompleteMode" {
		It "AutoCompleteMode LatestAvailable returns more objects than expand alone" {
			$expandResult = $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession
			$completeResult = $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -AutoCompleteMode LatestAvailable
			$completeResult.Count -eq 3 | Should-Be $true
			$completeResult.Count -gt $expandResult.Count | Should-Be $true
			($completeResult | Where-Object { $_.IshRef -eq $ishObjectLibraryTopic.IshRef }).version_version_value | Should-BeString -CaseSensitive "3"
			($completeResult | Where-Object { $_.IshRef -eq $ishObjectMap.IshRef }).version_version_value | Should-BeString -CaseSensitive "2"
			($completeResult | Where-Object { $_.IshRef -eq $ishObjectTopic.IshRef }).version_version_value | Should-BeString -CaseSensitive "1"
		}
		It "AutoCompleteMode LatestAvailable ResultContainsLibraryTopic" {
			$result = $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -AutoCompleteMode LatestAvailable
			$libFound = $result | Where-Object { $_.IshRef -eq $ishObjectLibraryTopic.IshRef }
			$libFound | Should-NotBeNull
		}
		It "AutoCompleteMode FirstVersion does not throw" {
			{ $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -AutoCompleteMode FirstVersion } | Should -Not -Throw
		}
		It "AutoCompleteMode LatestReleased does not throw" {
			{ $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -AutoCompleteMode LatestReleased } | Should -Not -Throw
		}
	}

	Context "Get-IshPublicationOutputContent IshObjectGroup Language and Resolution override" {
		It "Language override returns results" {
			$result = $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -Language @($ishLngCombination)
			$result.Count -ge 1 | Should-Be $true
		}
		It "Resolution override returns results" {
			$result = $ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -Resolution @($ishResolution)
			$result.Count -ge 1 | Should-Be $true
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
    try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Where-Object -Property IshFolderType -EQ -Value "ISHPublication" | Get-IshFolderContent -IshSession $ishSession | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}