Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Find-IshAnnotation"
try {

Describe “Find-IshAnnotation" -Tags "Create" {
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

	## Publication 1 (2 annotations)
    #add topic
	$ishTopicMetadata1 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						 Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						 Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectTopic1 = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata1 -FileContent $ditaTopicFileContent `
					  | Get-IshDocumentObj -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "ED" -Level Lng)
	#add map
	$ditaMap1 = $ditaMapWithTopicrefFileContent.Replace("<GUID-PLACEHOLDER>", $ishObjectTopic1.IshRef)
	$ishMapMetadata1 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Map $timestamp" |
				       Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
	                   Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectMap1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -Version '1' -Lng $ishLng -Metadata $ishMapMetadata1 -Edt "EDTXML" -FileContent $ditaMap1
	
	#add publication
    $ishPubMetadata1 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Pub $timestamp" |
				      Set-IshMetadataField -IshSession $ishSession -Name "FISHMASTERREF" -Level Version -ValueType Element -Value $ishObjectMap1.IshRef |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
	$ishObjectPub1 = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -Version '1' -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata1

	## Publication 2 (1 annotation with 2 replies)
    #add topic
	$ishTopicMetadata2 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic 2 $timestamp" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectTopic2 = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata2 -FileContent $ditaTopicFileContent `
					  | Get-IshDocumentObj -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "ED" -Level Lng)
	#add map
	$ditaMap2 = $ditaMapWithTopicrefFileContent.Replace("<GUID-PLACEHOLDER>", $ishObjectTopic2.IshRef)
	$ishMapMetadata2 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Map $timestamp" |
				      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
	                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectMap2 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -Version '1' -Lng $ishLng -Metadata $ishMapMetadata2 -Edt "EDTXML" -FileContent $ditaMap2
	
	#add publication
    $ishPubMetadata2 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Pub $timestamp" |
				      Set-IshMetadataField -IshSession $ishSession -Name "FISHMASTERREF" -Level Version -ValueType Element -Value $ishObjectMap2.IshRef |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
	$ishObjectPub2 = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -Version '1' -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata2
	
    #add annotations
	$revisionId = $ishObjectTopic.ed
	$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
	$annotationCategory = "Comment"
	$annotationType = "General"
	$annotationStatus = "Unshared"
	$proposedChngText = "My proposed change text"

	# Add annotations - Publication 1
    $annotationTextCustom1 = "by #1 ISHRemote Pester on $timestamp"
    $annotationTextCustom2 = "by #2 ISHRemote Pester on $timestamp"
	$metadata =	Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChngText
    $ishAnnotation1P1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub1.IshRef -PubVersion $ishObjectPub1.version_version_value -PubLng $ishObjectPub1.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic1.IshRef -Version $ishObjectTopic1.version_version_value -Lng $ishObjectTopic1.doclanguage -Type $annotationType -Text $annotationTextCustom1 -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata
	$ishAnnotation2P1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub1.IshRef -PubVersion $ishObjectPub1.version_version_value -PubLng $ishObjectPub1.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic1.IshRef -Version $ishObjectTopic1.version_version_value -Lng $ishObjectTopic1.doclanguage -Type $annotationType -Text $annotationTextCustom2 -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata
    $annotationIdsP1 = @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef)

    # Add annotations with replies - Publication 2
	$metadata =	Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChngText
    $annotationText = "by ISHRemote Pester on $timestamp"
	$ishAnnotation1P2 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub2.IshRef -PubVersion $ishObjectPub2.version_version_value -PubLng $ishObjectPub2.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic2.IshRef -Version $ishObjectTopic2.version_version_value -Lng $ishObjectTopic2.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotation1P2.IshRef)</ishfield></ishfields>"
	$replyRef1 = $ishsession.Annotation25.CreateReply($ishAnnotation1P2.IshRef, $strMetadataReply)
	$replyRef2 = $ishsession.Annotation25.CreateReply($ishAnnotation1P2.IshRef, $strMetadataReply)
	$replyIdsP2 = @($replyRef1, $replyRef2)
	
    Context "Find-IshAnnotation" {
		It "Find all annotations from Publication1 with RequestedMetadata" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTATIONREPLIES" -Level Annotation
            $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHPUBLOGICALID -Level Annotation -FilterOperator Equal -Value $ishObjectPub1.IshRef
            $ishAnnotations = Find-IshAnnotation -IshSession $ishsession `
                                                -RequestedMetadata $requestedMetadata `
                                                -MetadataFilter $metadataFilter
			$ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
		}
		It "Find all annotations from Publication1 without RequestedMetadata" {
            $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHPUBLOGICALID -Level Annotation -FilterOperator Equal -Value $ishObjectPub1.IshRef
            $ishAnnotations = Find-IshAnnotation -IshSession $ishsession `
                                                 -MetadataFilter $metadataFilter
			$ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
		}

		It "Find only 1 annotation from Publication1" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTATIONREPLIES" -Level Annotation
            $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHPUBLOGICALID -Level Annotation -FilterOperator Equal -Value $ishObjectPub1.IshRef |
                              Set-IshMetadataFilterField -IshSession $ishSession -Name FISHANNOTATIONTEXT -Level Annotation -FilterOperator Like -Value "by #1%"
            $ishAnnotations = Find-IshAnnotation -IshSession $ishsession `
                                                -RequestedMetadata $requestedMetadata `
                                                -MetadataFilter $metadataFilter
			$ishAnnotations.Count | Should BeExactly 1
            $ishAnnotation1P1.IshRef | Should BeExactly $ishAnnotations.IshRef
		}

        It "Find annotations and replies from Publication2 without RequestedMetadata" {
            $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHPUBLOGICALID -Level Annotation -FilterOperator Equal -Value $ishObjectPub2.IshRef
            $ishAnnotations = Find-IshAnnotation -IshSession $ishsession `
                                                 -MetadataFilter $metadataFilter
			$ishAnnotations.Count | Should BeExactly 2
            $replyIdsP2 -contains $ishAnnotations[0].ReplyRef | Should Be $true
            $replyIdsP2 -contains $ishAnnotations[1].ReplyRef | Should Be $true
		}
    }
}

} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
    try { $publicationOutputs = Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Where-Object -Property IshFolderType -EQ -Value "ISHPublication" | Get-IshFolderContent -IshSession $ishSession
          $publicationOutputs | Get-IshAnnotation -IshSession $ishSession | Remove-IshAnnotation -IshSession $ishSession
          $publicationOutputs | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
    try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Where-Object -Property IshFolderType -EQ -Value "ISHMasterDoc" |
		  Get-IshFolderContent -IshSession $ishSession |
		  Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Get-IshFolderContent -IshSession $ishSession |
		  Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}