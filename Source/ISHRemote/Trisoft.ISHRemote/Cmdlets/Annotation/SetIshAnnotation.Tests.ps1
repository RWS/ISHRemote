Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Set-IshAnnotation"
try {

Describe “Set-IshAnnotation" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
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
	
    #add annotations
	$revisionId = $ishObjectTopic.ed
	$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
	$annotationText = "by ISHRemote Pester on $timestamp"
	$annotationCategory = "Comment"
	$annotationType = "General"
	$annotationStatus = "Unshared"
    $annotationStatusShared = "Shared"
	
	# Add 2 annotations - ParametersGroup
	$ishAnnotationPG1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationPGWithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationPGWithReplies.IshRef)</ishfield></ishfields>"
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationPGWithReplies.IshRef, $strMetadataReply)
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationPGWithReplies.IshRef, $strMetadataReply)
	
	# Add 2 annotations - IshAnnotationGroup
	$ishAnnotationIAG1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGWithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationIAGWithReplies.IshRef)</ishfield></ishfields>"
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGWithReplies.IshRef, $strMetadataReply)
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGWithReplies.IshRef, $strMetadataReply)
	
	Context "Set-IshAnnotation ParametersGroup" {
		It "Parameter AnnotationId is empty" {
			{Set-IshAnnotation -IshSession $ishsession -AnnotationId "" -Metadata (Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "update should fail")} | Should Throw
		}
		It "Parameter AnnotationId non-existing Id" {
			{Set-IshAnnotation -IshSession $ishsession -AnnotationId "GUID-NON-EXISTING" -Metadata (Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "update should fail")} | Should Throw
		}
		It "Set without RequiredCurrentMetadata" {
            $annotationTextUpdated = $ishAnnotationPG1.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPG1.IshRef -Metadata $metadataUpdate
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationPG1.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}
		It "Set with Metadata and RequiredCurrentMetadata (exception)" {
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "Update should fail"
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value "Shared" -ValueType Value -Level Annotation
            {Set-IshAnnotation -IshSession $ishsession `
                              -AnnotationId $ishAnnotationPG1.IshRef `
                              -Metadata $metadataUpdate `
                              -RequiredCurrentMetadata $requiredCurrentMetadata} | Should Throw
		}
		It "Set with Metadata and RequiredCurrentMetadata" {
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value $annotationStatus -ValueType Value -Level Annotation
            $annotationTextUpdated = $ishAnnotationPG1.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession `
                                               -AnnotationId $ishAnnotationPG1.IshRef `
                                               -Metadata $metadataUpdate `
                                               -RequiredCurrentMetadata $requiredCurrentMetadata
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationPG1.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}

		It "Annotation with replies, Set without RequiredCurrentMetadata" {
            $annotationTextUpdated = $ishAnnotationPGWithReplies.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPGWithReplies.IshRef -Metadata $metadataUpdate
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationPGWithReplies.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}
		It "Annotation with replies, Set with RequiredCurrentMetadata (exception)" {
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "Update should fail"
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value "Shared" -ValueType Value -Level Annotation
            {Set-IshAnnotation -IshSession $ishsession `
                              -AnnotationId $ishAnnotationPGWithReplies.IshRef `
                              -Metadata $metadataUpdate `
                              -RequiredCurrentMetadata $requiredCurrentMetadata} | Should Throw
		}
		It "Annotation with replies, Set with RequiredCurrentMetadata" {
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value $annotationStatus -ValueType Value -Level Annotation
            $annotationTextUpdated = $ishAnnotationPGWithReplies.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession `
                                               -AnnotationId $ishAnnotationPGWithReplies.IshRef `
                                               -Metadata $metadataUpdate `
                                               -RequiredCurrentMetadata $requiredCurrentMetadata
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationPGWithReplies.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}
    }

	Context "Set-IshAnnotation IshAnnotationGroup, passing via parameter" {
		It "Set without RequiredCurrentMetadata" {
            $annotationTextUpdated = $ishAnnotationIAG1.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotationIAG1 -Metadata $metadataUpdate
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationIAG1.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}
		It "Set with RequiredCurrentMetadata (exception)" {
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "Update should fail"
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value "Shared" -ValueType Value -Level Annotation
            {Set-IshAnnotation -IshSession $ishsession `
                              -IshAnnotation $ishAnnotationIAG1 `
                              -Metadata $metadataUpdate `
                              -RequiredCurrentMetadata $requiredCurrentMetadata} | Should Throw
		}
		It "Set with RequiredCurrentMetadata" {
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value $annotationStatus -ValueType Value -Level Annotation
            $annotationTextUpdated = $ishAnnotationIAG1.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession `
                                               -IshAnnotation $ishAnnotationIAG1 `
                                               -Metadata $metadataUpdate `
                                               -RequiredCurrentMetadata $requiredCurrentMetadata
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationIAG1.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}

		It "Annotation with replies, Set without RequiredCurrentMetadata" {
            $annotationTextUpdated = $ishAnnotationIAGWithReplies.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotationIAGWithReplies -Metadata $metadataUpdate
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationIAGWithReplies.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}
		It "Annotation with replies, Set with RequiredCurrentMetadata (exception)" {
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "Update should fail"
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value "Shared" -ValueType Value -Level Annotation
            {Set-IshAnnotation -IshSession $ishsession `
                               -IshAnnotation $ishAnnotationIAGWithReplies `
                               -Metadata $metadataUpdate `
                               -RequiredCurrentMetadata $requiredCurrentMetadata} | Should Throw
		}
		It "Annotation with replies, Set with RequiredCurrentMetadata" {
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value $annotationStatus -ValueType Value -Level Annotation
            $annotationTextUpdated = $ishAnnotationIAGWithReplies.fishannotationtext_annotation_value + "updated"
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value $annotationTextUpdated -ValueType Value
            $ishAnnotation = Set-IshAnnotation -IshSession $ishsession `
                                               -IshAnnotation $ishAnnotationIAGWithReplies `
                                               -Metadata $metadataUpdate `
                                               -RequiredCurrentMetadata $requiredCurrentMetadata
            $ishAnnotation.IshRef | Should BeExactly $ishAnnotationIAGWithReplies.IshRef
 			$ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
		}
    }

	Context "Set-IshAnnotation IshAnnotationGroup, passing via pipeline" {
		It "Annotations of a publication, Set without RequiredCurrentMetadata" {
            $ishAnnotationsPub = $ishObjectPub | Get-IshAnnotation -IshSession $ishSession
            foreach($ishAnnotation in $ishAnnotationsPub)
            {
                $ishAnnotation.fishannotationstatus_annotation_value | Should BeExactly $annotationStatus
            }
            $ishAnnotationsSet = $ishAnnotationsPub | Set-IshAnnotation -IshSession $ishsession `
                                                                        -Metadata (Set-IshMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value $annotationStatusShared)
            foreach($ishAnnotation in $ishAnnotationsSet)
            {
                $ishAnnotation.fishannotationstatus_annotation_value | Should BeExactly $annotationStatusShared
            }
		}

		It "Array of annotations, Set with RequiredCurrentMetadata" {
            $annotationTextUpdated = "New annotation text"
            $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Value $annotationStatusShared -ValueType Value -Level Annotation
            $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value  $annotationTextUpdated
            $ishAnnotationsSet = @($ishAnnotationIAG1, $ishAnnotationIAGWithReplies) | Set-IshAnnotation -IshSession $ishsession `
                                                                                                         -Metadata $metadataUpdate `
                                                                                                         -RequiredCurrentMetadata $requiredCurrentMetadata
            foreach($ishAnnotation in $ishAnnotationsSet)
            {
                $ishAnnotation.fishannotationtext_annotation_value | Should BeExactly $annotationTextUpdated
            }
		}
    }
}

} finally {
	Write-Host "Cleaning Test Data and Variables"
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