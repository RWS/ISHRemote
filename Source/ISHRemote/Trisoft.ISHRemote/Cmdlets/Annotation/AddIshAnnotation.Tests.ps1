BeforeAll {
	$cmdletName = "Add-IshAnnotation"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Add-IshAnnotation" -Tags "Create" {
	BeforeAll {
		$ishFolderTestRootOriginal = Get-IshFolder -IShSession $ishSession -FolderPath $folderTestRootPath
		$folderIdTestRootOriginal = $ishFolderTestRootOriginal.IshFolderRef
		$folderTypeTestRootOriginal = $ishFolderTestRootOriginal.IshFolderType
		$ownedByTestRootOriginal = $ishFolderTestRootOriginal.fusergroup_none_element
		$readAccessTestRootOriginal = $ishFolderTestRootOriginal.readaccess_none_element

		Write-Debug("folderIdTestRootOriginal[" +  $ishFolderTestRootOriginal.IshFolderRef + "] folderTypeTestRootOriginal[" + $folderTypeTestRootOriginal + "]")
		$global:ishAnnotationCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishAnnotationCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderMap = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishAnnotationCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "Map" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderPub = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishAnnotationCmdlet.IshFolderRef) -FolderType ISHPublication -FolderName "Pub" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		
		#add topic
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
								Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
								Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectTopic = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent `
						| Get-IshDocumentObj -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "ED" -Level Lng)
		#add map
		$ditaMap = $ditaMapWithTopicrefFileContent.Replace("<GUID-PLACEHOLDER>", $ishObjectTopic.IshRef)
		$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Map $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectMap = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -Version '1' -Lng $ishLng -Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $ditaMap
		
		#add publication
		$ishPubMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Pub $timestamp" |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHMASTERREF" -Level Version -ValueType Element -Value $ishObjectMap.IshRef |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
						Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
		$ishObjectPub = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -Version '1' -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata
	}
	
	Context "Add-IshAnnotation MetadataGroup" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationText = "by ISHRemote Pester [MetadataGroup] on $timestamp"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$ishAnnotationMetadata = Set-IshMetadataField -Name "FISHREVISIONID" -Level Annotation -Value $revisionId |
				Set-IshMetadataField -Name "FISHPUBLOGICALID" -Level Annotation -Value $ishObjectPub.IshRef |
				Set-IshMetadataField -Name "FISHPUBVERSION" -Level Annotation -Value $ishObjectPub.version_version_value |
				Set-IshMetadataField -Name "FISHPUBLANGUAGE" -Level Annotation -Value $ishObjectPub.fishpubsourcelanguages_version_value |
				Set-IshMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -ValueType Element -Value "VANNOTATIONSTATUSUNSHARED" |
				Set-IshMetadataField -Name "FISHANNOTATIONADDRESS" -Level Annotation -Value $annotationAddress |
				Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationText |
				Set-IshMetadataField -Name "FISHANNOTATIONCATEGORY" -Level Annotation -Value $annotationCategory |
				Set-IshMetadataField -Name "FISHANNOTATIONTYPE" -Level Annotation -Value $annotationType
			$ishAnnotation = Add-IshAnnotation -IshSession $ishsession -Metadata $ishAnnotationMetadata
		}
		It "GetType().Name" {
			$ishAnnotation.GetType().Name | Should -BeExactly "IshAnnotation"
		}
		It "ishAnnotation.IshField" {
			$ishAnnotation.IshField | Should -Not -BeNullOrEmpty
		}
		It "ishAnnotation.IshRef" {
			$ishAnnotation.IshRef | Should -Not -BeNullOrEmpty
		}
		It "ishAnnotation.IshType" {
			$ishAnnotation.IshType | Should -Not -BeNullOrEmpty
		}
		It "ishAnnotation.ObjectRef" {
			$ishAnnotation.ObjectRef | Should -Not -BeNullOrEmpty
		}
        It "ishAnnotation PubLogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
        It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}
        It "ishAnnotation RevisionId" {
			$ishAnnotation.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
		It "ishAnnotation Address" {
			$ishAnnotation.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotation.fishannotationtext_annotation_value | Should -BeExactly $annotationText
		}
		It "ishAnnotation Category" {
			$ishAnnotation.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotation.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
		It "Metadata is null" {
			{Add-IshAnnotation -IshSession $ishsession -Metadata $null} | Should -Throw
		}
	}

	Context "Add-IshAnnotation ParametersGroup" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationText = "by ISHRemote Pester [ParametersGroup] on $timestamp"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-LogicalId $ishObjectTopic.IshRef `
								-Version $ishObjectTopic.version_version_value `
								-Lng $ishObjectTopic.doclanguage `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress
        }
		It "ishAnnotation RevisionId" {
			$ishAnnotation.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
        It "ishAnnotation PubLogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
        It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}
		It "ishAnnotation LogicalId" {
			$ishAnnotation.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
		}
		It "ishAnnotation Version" {
			$ishAnnotation.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
		}
		It "ishAnnotation Lng" {
			$ishAnnotation.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
		}		
		It "ishAnnotation Status" {
			$ishAnnotation.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
		}
		It "ishAnnotation Address" {
			$ishAnnotation.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotation.fishannotationtext_annotation_value | Should -BeExactly $annotationText
		}
		It "ishAnnotation Category" {
			$ishAnnotation.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotation.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
	}
	
	Context "Add-IshAnnotation ParametersGroup override metadata matching parameters" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationText = "by ISHRemote Pester [ParametersGroup] on $timestamp"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			# this should not be overrided
			$proposedChangeText = "Proposed change in this annotation"
			
			$metadataProvided = Set-IshMetadataField -Name "FISHREVISIONID" -Level Annotation -Value "GUID-123-REVISION-ID" |
				Set-IshMetadataField -Name "FISHPUBLOGICALID" -Level Annotation -Value "GUID-123-PUBLICATION-ID" |
				Set-IshMetadataField -Name "FISHPUBVERSION" -Level Annotation -Value "100" |
				Set-IshMetadataField -Name "FISHPUBLANGUAGE" -Level Annotation -Value "aa" |
				Set-IshMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value "Metadata status" |
				Set-IshMetadataField -Name "FISHANNOTATIONADDRESS" -Level Annotation -Value "Metadata address" |
				Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "Metadata text" |
				Set-IshMetadataField -Name "FISHANNOTATIONCATEGORY" -Level Annotation -Value "Metadata comment" |
				Set-IshMetadataField -Name "FISHANNOTATIONTYPE" -Level Annotation -Value "Metadata type" |
				Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeText
			
			$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-LogicalId $ishObjectTopic.IshRef `
								-Version $ishObjectTopic.version_version_value `
								-Lng $ishObjectTopic.doclanguage `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress `
								-Metadata $metadataProvided
		}                       
		It "ishAnnotation Address" {
			$ishAnnotation.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotation.fishannotationtext_annotation_value | Should -BeExactly $annotationText
		}
		It "ishAnnotation Category" {
			$ishAnnotation.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotation.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
		It "ishAnnotation Status" {
			$ishAnnotation.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
		}
		It "ishAnnotation PublogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
		It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}
		It "ishAnnotation RevisionId" {
			$ishAnnotation.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
		It "ishAnnotation LogicalId" {
			$ishAnnotation.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
		}
		It "ishAnnotation Version" {
			$ishAnnotation.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
		}
		It "ishAnnotation Lng" {
			$ishAnnotation.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
		}
		It "ishAnnotation ProposedChngText" {
			$ishAnnotation.fishannotproposedchngtxt_annotation_value | Should -BeExactly $proposedChangeText
		}
	}	
	
	Context "Add-IshAnnotation IshObjectGroup" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationText = "by ISHRemote Pester [IshObjectGroup] on $timestamp"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-IshObject $ishObjectTopic `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress
		}                
		It "ishAnnotation RevisionId" {
			$ishAnnotation.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
		It "ishAnnotation LogicalId" {
			$ishAnnotation.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
		}
		It "ishAnnotation Version" {
			$ishAnnotation.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
		}
		It "ishAnnotation Lng" {
			$ishAnnotation.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
		}		
		It "ishAnnotation PublogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
		It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}
		It "ishAnnotation Status" {
			$ishAnnotation.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
		}
		It "ishAnnotation Address" {
			$ishAnnotation.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotation.fishannotationtext_annotation_value | Should -BeExactly $annotationText
		}
		It "ishAnnotation Category" {
			$ishAnnotation.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotation.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
		It "IshObject is of invalid type (IshPublication)" {
			{$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
                            -PubLogicalId $ishObjectPub.IshRef `
                            -PubVersion $ishObjectPub.version_version_value `
                            -PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
							-IshObject $ishObjectPub `
                            -Type $annotationType `
                            -Text $annotationText `
                            -Status $annotationStatus `
                            -Category $annotationCategory `
                            -Address $annotationAddress
			} | Should -Throw
		}
	}

	Context "Add-IshAnnotation IshObjectGroup override metadata matching parameters" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationText = "by ISHRemote Pester [IshObjectGroup] on $timestamp"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			# this should not be overrided
			$proposedChangeText = "Proposed change in this annotation"
			$metadataProvided = Set-IshMetadataField -Name "FISHREVISIONID" -Level Annotation -Value "GUID-123-REVISION-ID" | `
				Set-IshMetadataField -Name "FISHPUBLOGICALID" -Level Annotation -Value "GUID-123-PUBLICATION-ID" |
				Set-IshMetadataField -Name "FISHPUBVERSION" -Level Annotation -Value "100" |
				Set-IshMetadataField -Name "FISHPUBLANGUAGE" -Level Annotation -Value "aa" |
				Set-IshMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value "Metadata status" |
				Set-IshMetadataField -Name "FISHANNOTATIONADDRESS" -Level Annotation -Value "Metadata address" |
				Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "Metadata text" |
				Set-IshMetadataField -Name "FISHANNOTATIONCATEGORY" -Level Annotation -Value "Metadata comment" |
				Set-IshMetadataField -Name "FISHANNOTATIONTYPE" -Level Annotation -Value "Metadata type" |
				Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeText
			
			$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-IshObject $ishObjectTopic `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress `
								-Metadata $metadataProvided
		}                       
		It "ishAnnotation RevisionId" {
			$ishAnnotation.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
		It "ishAnnotation LogicalId" {
			$ishAnnotation.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
		}
		It "ishAnnotation Version" {
			$ishAnnotation.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
		}
		It "ishAnnotation Lng" {
			$ishAnnotation.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
		}	
		It "ishAnnotation PublogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
		It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}
		It "ishAnnotation Status" {
			$ishAnnotation.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
		}
		It "ishAnnotation Address" {
			$ishAnnotation.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotation.fishannotationtext_annotation_value | Should -BeExactly $annotationText
		}
		It "ishAnnotation Category" {
			$ishAnnotation.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotation.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
		It "ishAnnotation ProposedChngText" {
			$ishAnnotation.fishannotproposedchngtxt_annotation_value | Should -BeExactly $proposedChangeText
		}
	}
	
	Context "Add-IshAnnotation IshObjectGroup IshObject from pipeline" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationText = "by ISHRemote Pester [IshObjectGroup] pipeline on $timestamp"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			$ishAnnotation = $ishObjectTopic | Add-IshAnnotation -IshSession $ishsession `
												-PubLogicalId $ishObjectPub.IshRef `
												-PubVersion $ishObjectPub.version_version_value `
												-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
												-Type $annotationType `
												-Text $annotationText `
												-Status $annotationStatus `
												-Category $annotationCategory `
												-Address $annotationAddress
		}		                               
		It "ishAnnotation RevisionId" {
			$ishAnnotation.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
		It "ishAnnotation LogicalId" {
			$ishAnnotation.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
		}
		It "ishAnnotation PublogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
		It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}		
        It "ishAnnotation Version" {
			$ishAnnotation.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
		}
		It "ishAnnotation Lng" {
			$ishAnnotation.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
		}		
		It "ishAnnotation Status" {
			$ishAnnotation.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
		}
		It "ishAnnotation Address" {
			$ishAnnotation.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotation.fishannotationtext_annotation_value | Should -BeExactly $annotationText
		}
		It "ishAnnotation Category" {
			$ishAnnotation.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotation.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
	}

	Context "Add-IshAnnotation IshAnnotationGroup" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationText = "by ISHRemote Pester [IshAnnotationGroup] on $timestamp"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			$proposedChangeText = "Proposed change in this annotation"
			$metadata = Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeText
			
			$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-IshObject $ishObjectTopic `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress `
								-Metadata $metadata
			
			$proposedChangeTextUpdated = $proposedChangeText + "updated"
			$annotationTextUpdated = $annotationText + "updated"
			$ishAnnotation = $ishAnnotation | Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeTextUpdated |
											Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
			
			$ishAnnotationUpdated = Add-IshAnnotation -IshSession $ishsession `
													-IshAnnotation $ishAnnotation
		}
		It "ishAnnotation RevisionId" {
			$ishAnnotationUpdated.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
		It "ishAnnotation LogicalId" {
			$ishAnnotationUpdated.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
		}
		It "ishAnnotation PublogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
		It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}
		It "ishAnnotation Version" {
			$ishAnnotationUpdated.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
		}
		It "ishAnnotation Lng" {
			$ishAnnotationUpdated.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
		}		
		It "ishAnnotation Status" {
			$ishAnnotationUpdated.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
		}
		It "ishAnnotation Address" {
			$ishAnnotationUpdated.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotationUpdated.fishannotationtext_annotation_value | Should -BeExactly $annotationTextUpdated
		}
		It "ishAnnotation Category" {
			$ishAnnotationUpdated.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotationUpdated.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
		It "ishAnnotation ProposedChngText" {
			$ishAnnotationUpdated.fishannotproposedchngtxt_annotation_value | Should -BeExactly $proposedChangeTextUpdated
		}
	}

	Context "Add-IshAnnotation IshAnnotationGroup array from pipeline" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationText = "by ISHRemote Pester [IshAnnotationGroup] pipeline on $timestamp"
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			$proposedChangeText = "Proposed change in this annotation"
			$metadata = Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeText
			$ishAnnotation1 = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-IshObject $ishObjectTopic `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress `
								-Metadata $metadata

			$ishAnnotation2 = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-IshObject $ishObjectTopic `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress `
								-Metadata $metadata

			
			$proposedChangeTextUpdated = $proposedChangeText + "updated"
			$annotationTextUpdated = $annotationText + "updated"
			$ishAnnotation1 = $ishAnnotation1 | Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeTextUpdated |
												Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
			$ishAnnotation2 = $ishAnnotation2 | Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeTextUpdated |
												Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
			
			$ishAnnotationsUpdated = @($ishAnnotation1, $ishAnnotation2) | Add-IshAnnotation -IshSession $ishsession
		}
		It "ishAnnotations array count" {
			$ishAnnotationsUpdated.Count | Should -BeExactly 2
		}
		
		foreach($ishAnnotationUpdated in $ishAnnotationsUpdated)
		{
			It "ishAnnotation RevisionId" {
				$ishAnnotationUpdated.fishrevisionid_annotation_value | Should -BeExactly $revisionId
			}
			It "ishAnnotation LogicalId" {
				$ishAnnotationUpdated.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
			}
			It "ishAnnotation Version" {
				$ishAnnotationUpdated.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
			}
			It "ishAnnotation Lng" {
				$ishAnnotationUpdated.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
			}	
		    It "ishAnnotation PublogicalId" {
			    $ishAnnotationUpdated.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		    }
		    It "ishAnnotation PubVersion" {
			    $ishAnnotationUpdated.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		    }
            It "ishAnnotation PubLng" {
			    $ishAnnotationUpdated.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		    }	
			It "ishAnnotation Status" {
				$ishAnnotationUpdated.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
			}
			It "ishAnnotation Address" {
				$ishAnnotationUpdated.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
			}
			It "ishAnnotation Text" {
				$ishAnnotationUpdated.fishannotationtext_annotation_value | Should -BeExactly $annotationTextUpdated
			}
			It "ishAnnotation Category" {
				$ishAnnotationUpdated.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
			}
			It "ishAnnotation Type" {
				$ishAnnotationUpdated.fishannotationtype_annotation_value | Should -BeExactly $annotationType
			}
			It "ishAnnotation ProposedChngText" {
				$ishAnnotationUpdated.fishannotproposedchngtxt_annotation_value | Should -BeExactly $proposedChangeTextUpdated
			}
		}
	}
	
	Context "Add-IshAnnotation IshAnnotationGroup single object from pipeline" {
		BeforeAll {
			$revisionId = $ishObjectTopic.ed
			$annotationText = "by ISHRemote Pester [IshAnnotationGroup] pipeline on $timestamp"
			$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
			$annotationCategory = (Get-IshLovValue -LovId DANNOTATIONCATEGORY -LovValueId VANNOTATIONCATEGORYCOMMENT).Label
			$annotationType = (Get-IshLovValue -LovId DANNOTATIONTYPE -LovValueId VANNOTATIONTYPEGENERAL).Label
			$annotationStatus = (Get-IshLovValue -LovId DANNOTATIONSTATUS -LovValueId VANNOTATIONSTATUSUNSHARED).Label
			$proposedChangeText = "Proposed change in this annotation"
			$metadata = Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeText
			$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
								-PubLogicalId $ishObjectPub.IshRef `
								-PubVersion $ishObjectPub.version_version_value `
								-PubLng $ishObjectPub.fishpubsourcelanguages_version_value `
								-IshObject $ishObjectTopic `
								-Type $annotationType `
								-Text $annotationText `
								-Status $annotationStatus `
								-Category $annotationCategory `
								-Address $annotationAddress `
								-Metadata $metadata
			
			$proposedChangeTextUpdated = $proposedChangeText + "updated"
			$annotationTextUpdated = $annotationText + "updated"
			$ishAnnotation = $ishAnnotation | Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChangeTextUpdated |
											Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
			
			$ishAnnotationUpdated = $ishAnnotation | Add-IshAnnotation -IshSession $ishsession
		}
		It "ishAnnotation RevisionId" {
			$ishAnnotationUpdated.fishrevisionid_annotation_value | Should -BeExactly $revisionId
		}
		It "ishAnnotation LogicalId" {
			$ishAnnotationUpdated.fishcontentobjlogicalid_annotation_value | Should -BeExactly $ishObjectTopic.IshRef
		}
		It "ishAnnotation Version" {
			$ishAnnotationUpdated.fishcontentobjversion_annotation_value | Should -BeExactly $ishObjectTopic.version_version_value
		}
		It "ishAnnotation Lng" {
			$ishAnnotationUpdated.fishcontentobjlanguage_annotation_value | Should -BeExactly $ishObjectTopic.doclanguage
		}	
		It "ishAnnotation PublogicalId" {
			$ishAnnotation.fishpublogicalid_annotation_value | Should -BeExactly $ishObjectPub.IshRef
		}
		It "ishAnnotation PubVersion" {
			$ishAnnotation.fishpubversion_annotation_value | Should -BeExactly $ishObjectPub.version_version_value
		}
        It "ishAnnotation PubLng" {
			$ishAnnotation.fishpublanguage_annotation_value | Should -BeExactly $ishObjectPub.fishpubsourcelanguages_version_value
		}	
		It "ishAnnotation Status" {
			$ishAnnotationUpdated.fishannotationstatus_annotation_value | Should -BeExactly $annotationStatus
		}
		It "ishAnnotation Address" {
			$ishAnnotationUpdated.fishannotationaddress_annotation_value | Should -BeExactly $annotationAddress
		}
		It "ishAnnotation Text" {
			$ishAnnotationUpdated.fishannotationtext_annotation_value | Should -BeExactly $annotationTextUpdated
		}
		It "ishAnnotation Category" {
			$ishAnnotationUpdated.fishannotationcategory_annotation_value | Should -BeExactly $annotationCategory
		}
		It "ishAnnotation Type" {
			$ishAnnotationUpdated.fishannotationtype_annotation_value | Should -BeExactly $annotationType
		}
		It "ishAnnotation ProposedChngText" {
			$ishAnnotationUpdated.fishannotproposedchngtxt_annotation_value | Should -BeExactly $proposedChangeTextUpdated
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	$publicationOutputs = Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
                          Where-Object -Property IshFolderType -EQ -Value "ISHPublication" | 
                          Get-IshFolderContent -IshSession $ishSession
    try { $publicationOutputs | Get-IshAnnotation -IshSession $ishSession | Remove-IshAnnotation -IshSession $ishSession } catch { }
	try { $publicationOutputs | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Where-Object -Property IshFolderType -EQ -Value "ISHMasterDoc" |
		  Get-IshFolderContent -IshSession $ishSession |
		  Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Get-IshFolderContent -IshSession $ishSession |
		  Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	
}
