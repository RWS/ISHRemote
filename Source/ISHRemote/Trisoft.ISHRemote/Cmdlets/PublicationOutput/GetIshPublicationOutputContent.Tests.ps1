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
		$ownedByTestRootOriginal = Get-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField
		$readAccessTestRootOriginal = (Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Separator)

		$global:ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

		# Create a topic
		$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Topic $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectTopic = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -LogicalId ("GETISHPUBLICATIONOUTPUTCONTENT-TOPIC-" + $timestamp) -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent

		# Create a map that references the topic
		$ishFolderMap = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "Map" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Map $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectMap = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -LogicalId ("GETISHPUBLICATIONOUTPUTCONTENT-MAP-" + $timestamp) -Version '1' -Lng $ishLng -Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $ditaMapFileContent

		# Create a publication output
		$ishFolderPub = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHPublication -FolderName "Pub" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishPubMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Pub $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHMASTERREF" -Level Version -ValueType Element -Value $ishObjectMap.IshRef |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
		$global:ishObjectPub = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -LogicalId ("GETISHPUBLICATIONOUTPUTCONTENT-PUB-" + $timestamp) -Version '1' -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata
	}

	Context "Get-IshPublicationOutputContent ParameterGroup validation" {
		It "InvalidIshSession" {
			{ Get-IshPublicationOutputContent -IshSession "INVALIDISHSESSION" -IshObject $global:ishObjectPub } | Should-Throw
		}
	}

	Context "Get-IshPublicationOutputContent IshObjectGroup empty pipeline" {
		It "EmptyIshObject" {
			$result = @() | Get-IshPublicationOutputContent -IshSession $ishSession
			$result.Count | Should-Be 0
		}
	}

	Context "Get-IshPublicationOutputContent IshObjectGroup" {
		It "PipelineWithPublicationOutput returns IshDocumentObj" {
			$result = $global:ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession
			# A map with at least itself should be returned
			$result | Should-NotBeNull
			$result.Count -ge 1 | Should-Be $true
		}
		It "GetType returns IshDocumentObj" {
			$result = $global:ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession
			foreach ($item in $result) {
				$item.GetType().Name | Should-BeString -CaseSensitive "IshDocumentObj"
			}
		}
		It "ResultContainsMap" {
			$result = $global:ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession
			$mapFound = $result | Where-Object { $_.IshRef -eq $global:ishObjectPub.IshRef -or $_.IshType -eq "ISHMasterDoc" }
			$mapFound | Should-NotBeNull
		}
		It "RequestedMetadata returns extra fields" {
			$extraRequestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical
			$result = $global:ishObjectPub | Get-IshPublicationOutputContent -IshSession $ishSession -RequestedMetadata $extraRequestedMetadata
			$result.Count -ge 1 | Should-Be $true
			# Spot-check that FTITLE is populated on at least one object
			$withTitle = $result | Where-Object { $_.ftitle_logical_value.Length -ge 1 }
			$withTitle | Should-NotBeNull
		}
	}
}
