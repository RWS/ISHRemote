Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshAnnotation"
try {

Describe “Get-IshAnnotation" -Tags "Create" {
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
	$ishFolderImage = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishAnnotationCmdlet.IshFolderRef) -FolderType ISHIllustration -FolderName "Image" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

    # add image object
	$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Image $timestamp" |
				        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
	    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$tempFilePath = (New-TemporaryFile).FullName
	Add-Type -AssemblyName "System.Drawing"
	$bmp = New-Object -TypeName System.Drawing.Bitmap(100,100)
	for ($i = 0; $i -lt 100; $i++)
	{
		for ($j = 0; $j -lt 100; $j++)
		{
			$bmp.SetPixel($i, $j, 'Red')
		}
	}
	$bmp.Save($tempFilePath, [System.Drawing.Imaging.ImageFormat]::Jpeg)
	$ishObjectImage = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderImage -IshType ISHIllustration -LogicalId "MYOWNGENERATEDLOGICALIDIMAGE" -Version '1' -Lng $ishLng -Resolution $ishResolution -Metadata $ishImageMetadata -Edt "EDTJPEG" -FilePath $tempFilePath

	## Publication 1
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

	## Publication 2
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

	## Publication 3 (annotation has replies)
    #add topic
	$ishTopicMetadata3 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic 3 $timestamp" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectTopic3 = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata3 -FileContent $ditaTopicFileContent `
					  | Get-IshDocumentObj -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "ED" -Level Lng)
	#add map
	$ditaMap3 = $ditaMapWithTopicrefFileContent.Replace("<GUID-PLACEHOLDER>", $ishObjectTopic3.IshRef)
	$ishMapMetadata3 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Map $timestamp" |
				      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
	                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectMap3 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -Version '1' -Lng $ishLng -Metadata $ishMapMetadata3 -Edt "EDTXML" -FileContent $ditaMap3
	
	#add publication
    $ishPubMetadata3 = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Pub $timestamp" |
				      Set-IshMetadataField -IshSession $ishSession -Name "FISHMASTERREF" -Level Version -ValueType Element -Value $ishObjectMap3.IshRef |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
	$ishObjectPub3 = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -Version '1' -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata3
	
    #add annotations
	$revisionId = $ishObjectTopic.ed
	$annotationAddress = "[{""revisionId"":""$revisionId"",""startContainerQuery"":""/*[1]/node()[1]/node()[1]"",""startOffset"":0,""endContainerQuery"":""/*[1]/node()[1]/node()[1]"",""endOffset"":4,""type"":""TEXT_RANGE_SELECTOR""}]"
    $annotationTextCustom1 = "by #1 ISHRemote Pester on $timestamp"
    $annotationTextCustom2 = "by #2 ISHRemote Pester on $timestamp"
	$annotationCategory = "Comment"
	$annotationType = "General"
	$annotationStatus = "Unshared"
	$proposedChngText = "My proposed change text"

	# Add annotations - Publication 1
	$metadata =	Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChngText
    $ishAnnotation1P1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub1.IshRef -PubVersion $ishObjectPub1.version_version_value -PubLng $ishObjectPub1.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic1.IshRef -Version $ishObjectTopic1.version_version_value -Lng $ishObjectTopic1.doclanguage -Type $annotationType -Text $annotationTextCustom1 -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata
	$ishAnnotation2P1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub1.IshRef -PubVersion $ishObjectPub1.version_version_value -PubLng $ishObjectPub1.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic1.IshRef -Version $ishObjectTopic1.version_version_value -Lng $ishObjectTopic1.doclanguage -Type $annotationType -Text $annotationTextCustom2 -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata
    $annotationIdsP1 = @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef)
	
    # Add annotations - Publication 2
	$metadata =	Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChngText
    $ishAnnotation1P2 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub2.IshRef -PubVersion $ishObjectPub2.version_version_value -PubLng $ishObjectPub2.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic2.IshRef -Version $ishObjectTopic2.version_version_value -Lng $ishObjectTopic2.doclanguage -Type $annotationType -Text $annotationTextCustom1 -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata
	$ishAnnotation2P2 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub2.IshRef -PubVersion $ishObjectPub2.version_version_value -PubLng $ishObjectPub2.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic2.IshRef -Version $ishObjectTopic2.version_version_value -Lng $ishObjectTopic2.doclanguage -Type $annotationType -Text $annotationTextCustom2 -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata

    # Add annotations with replies - Publication 3
	$metadata =	Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value $proposedChngText
    $ishAnnotation1P3 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub3.IshRef -PubVersion $ishObjectPub3.version_version_value -PubLng $ishObjectPub3.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic3.IshRef -Version $ishObjectTopic3.version_version_value -Lng $ishObjectTopic3.doclanguage -Type $annotationType -Text $annotationTextCustom1 -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress -Metadata $metadata
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotation1P3.IshRef)</ishfield></ishfields>"
	$replyRef1 = $ishsession.Annotation25.CreateReply($ishAnnotation1P3.IshRef, $strMetadataReply)
	$replyRef2 = $ishsession.Annotation25.CreateReply($ishAnnotation1P3.IshRef, $strMetadataReply)
	$replyIdsP3 = @($replyRef1, $replyRef2)
	
    # array with all added annotations
    $annotationIdsP1P2 = @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef,$ishAnnotation1P2.IshRef, $ishAnnotation2P2.IshRef)
	$annotationIdsP1P2P3 = @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef,$ishAnnotation1P2.IshRef, $ishAnnotation2P2.IshRef, $ishAnnotation1P3.IshRef)
	
    Context "Get-IshAnnotation ParametersGroup" {
		It "Parameter AnnotationId is an empty array" {
			{Get-IshAnnotation -IshSession $ishsession -AnnotationId @()} | Should Throw
		}
		It "Parameter AnnotationId contains non-existing Ids" {
			$ishAnnotations = Get-IshAnnotation -IshSession $ishsession -AnnotationId @("GUID-NON-EXISTING1","GUID-NON-EXISTING1")
            $ishAnnotations.Count | Should Be 0
            $ishAnnotations | Should Be $null
		}
		It "Get without RequestedMetadata and MetadataFilter" {
			$ishAnnotations = Get-IshAnnotation -IshSession $ishsession -AnnotationId @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef)
			$ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
		}
		It "Get with RequstedMetadata and without MetadataFilter" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTATIONREPLIES" -Level Annotation
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession `
                                                -AnnotationId @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef) `
                                                -RequestedMetadata $requestedMetadata
            $repliesValue1 = Get-IshMetadataField -IshObject $ishAnnotations[0] -Name "FISHANNOTATIONREPLIES" -Level Annotation -ValueType Value
            $repliesValue2 = Get-IshMetadataField -IshObject $ishAnnotations[1] -Name "FISHANNOTATIONREPLIES" -Level Annotation -ValueType Value

			$ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
            $repliesValue1 | Should Be ""
            $repliesValue2 | Should Be ""
		}
		It "Get with RequstedMetadata and with MetadataFilter" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTATIONREPLIES" -Level Annotation
            $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHANNOTATIONTEXT -Level Annotation -FilterOperator Like -Value "by #1%"
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession `
                                                -AnnotationId @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef) `
                                                -RequestedMetadata $requestedMetadata `
                                                -MetadataFilter $metadataFilter
			$ishAnnotations.Count | Should BeExactly 1

            $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHANNOTATIONTEXT -Level Annotation -FilterOperator Like -Value "by%"
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession `
                                                -AnnotationId @($ishAnnotation1P1.IshRef, $ishAnnotation2P1.IshRef) `
                                                -RequestedMetadata $requestedMetadata `
                                                -MetadataFilter $metadataFilter
			$ishAnnotations.Count | Should BeExactly 2
		}
    }

    Context "Get-IshAnnotation IshObjectGroup" {
        It "Get for Map object" {
            $ishAnnotations = $ishObjectMap1 | Get-IshAnnotation -IshSession $ishsession
            $ishAnnotations.Count | Should BeExactly 0
        }
        It "Get for Image object" {
            $ishAnnotations =  $ishObjectImage | Get-IshAnnotation -IshSession $ishsession
            $ishAnnotations.Count | Should BeExactly 0
        }
        It "Get for Publication1 without RequestedMetadata" {
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshObject $ishObjectPub1
            $ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
        }
        It "Get for Publication1 with RequestedMetadata" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshObject $ishObjectPub1 -RequestedMetadata $requestedMetadata
            $ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
            $ishAnnotations[0].fishannotproposedchngtxt_annotation_value | Should BeExactly $proposedChngText
            $ishAnnotations[1].fishannotproposedchngtxt_annotation_value | Should BeExactly $proposedChngText
            Get-IshMetadataField -IshObject $ishAnnotations[0] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
            Get-IshMetadataField -IshObject $ishAnnotations[1] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
        }
         It "Get for Publication1 with RequestedMetadata, Pipeline" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = $ishObjectPub1 | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
            $ishAnnotations.Count | Should BeExactly 2
            Get-IshMetadataField -IshObject $ishAnnotations[0] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
            Get-IshMetadataField -IshObject $ishAnnotations[1] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
        }
         It "Get for Publication1,Publication2 with RequestedMetadata, Pipeline" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = @($ishObjectPub1, $ishObjectPub2) | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
            $ishAnnotations.Count | Should BeExactly 4
            $annotationIdsP1P2 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1P2 -contains $ishAnnotations[1].IshRef | Should Be $true
            $annotationIdsP1P2 -contains $ishAnnotations[2].IshRef | Should Be $true
            $annotationIdsP1P2 -contains $ishAnnotations[3].IshRef | Should Be $true
        }
        It "Get for Topic without RequestedMetadata" {
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshObject $ishObjectTopic1
            $ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
        }
        It "Get for Topic with RequestedMetadata" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshObject $ishObjectTopic1 -RequestedMetadata $requestedMetadata
            $ishAnnotations.Count | Should BeExactly 2
            Get-IshMetadataField -IshObject $ishAnnotations[0] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
            Get-IshMetadataField -IshObject $ishAnnotations[1] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
        }
        It "Get for Topic with RequestedMetadata, Pipeline" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = $ishObjectTopic1 | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
            $ishAnnotations.Count | Should BeExactly 2
            Get-IshMetadataField -IshObject $ishAnnotations[0] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
            Get-IshMetadataField -IshObject $ishAnnotations[1] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
        }
        It "Get for Topic1, Topic2 with RequestedMetadata, Pipeline" {
            $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = @($ishObjectTopic1, $ishObjectTopic2) | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
            $ishAnnotations.Count | Should BeExactly 4
            $annotationIdsP1P2 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1P2 -contains $ishAnnotations[1].IshRef | Should Be $true
            $annotationIdsP1P2 -contains $ishAnnotations[2].IshRef | Should Be $true
            $annotationIdsP1P2 -contains $ishAnnotations[3].IshRef | Should Be $true
        }
		It "Get for Publication3 where annotation has 2 replies"{
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
			$ishAnnotations = $ishObjectPub3 | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
			$ishAnnotations.Count | Should BeExactly 2
			$replyIdsP3 -contains $ishAnnotations[0].ReplyRef | Should Be $true 
			$replyIdsP3 -contains $ishAnnotations[1].ReplyRef | Should Be $true 
		}
    }
    Context "Get-IshAnnotation IshObjectGroup mixed IshObjects from Get-IshFolderContent" {
        It "Get Annotations, pipeline all objects from all folders" {
            $ishAnnotations = Get-IshFolder -IshSession $ishsession -FolderId ($global:ishAnnotationCmdlet.IshFolderRef) -Recurse |
                              Get-IshFolderContent -IshSession $ishsession |
                              Get-IshAnnotation -IshSession $ishsession
            $ishAnnotations.Count | Should BeExactly 6
			foreach($ishAnnotation in $ishAnnotations)
			{
				$annotationIdsP1P2P3 -contains $ishAnnotation.IshRef | Should Be $true
			}
         }

         It "Get-IshAnnotation IshObjectGroup, pass all mixed IshObjects via parameter" {
            $ishObjects = Get-Ishfolder -IshSession $ishsession -FolderId ($global:ishAnnotationCmdlet.IshFolderRef) -Recurse |
                          Get-IshFolderContent -IshSession $ishsession                              
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshObject $ishObjects 
            $ishAnnotations.Count | Should BeExactly 6
			foreach($ishAnnotation in $ishAnnotations)
			{
				$annotationIdsP1P2P3 -contains $ishAnnotation.IshRef | Should Be $true
			}
        }
    }
    Context "Get-IshAnnotation IshAnnotationGroup" {
        It "Get array of IshAnnotation without RequestedMetadata" {
			$ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshAnnotation @($ishAnnotation1P1, $ishAnnotation2P1)
			$ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
		}
        It "Get array of IshAnnotation with RequestedMetadata" {
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshAnnotation @($ishAnnotation1P1, $ishAnnotation2P1) -RequestedMetadata $requestedMetadata
			$ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
            Get-IshMetadataField -IshObject $ishAnnotations[0] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
            Get-IshMetadataField -IshObject $ishAnnotations[1] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
		}
        It "Get single IshAnnotation without RequestedMetadata" {
			$ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotation1P1
			$ishAnnotations.Count | Should BeExactly 1
            $ishAnnotations.IshRef | Should BeExactly $ishAnnotation1P1.IshRef
		}
        It "Get single IshAnnotation with RequestedMetadata" {
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = Get-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotation1P1 -RequestedMetadata $requestedMetadata
			$ishAnnotations.Count | Should BeExactly 1
            $ishAnnotations.IshRef | Should BeExactly $ishAnnotation1P1.IshRef
            Get-IshMetadataField -IshObject $ishAnnotations -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
		}

        It "Get with RequestedMetadata, pipeline single object" {
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = $ishAnnotation1P1 | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
			$ishAnnotations.Count | Should BeExactly 1
            $annotationIdsP1 -contains $ishAnnotations.IshRef | Should Be $true
            Get-IshMetadataField -IshObject $ishAnnotations -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
        }
        It "Get with RequestedMetadata, pipeline array" {
			$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
            $ishAnnotations = @($ishAnnotation1P1, $ishAnnotation2P1) | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
			$ishAnnotations.Count | Should BeExactly 2
            $annotationIdsP1 -contains $ishAnnotations[0].IshRef | Should Be $true
            $annotationIdsP1 -contains $ishAnnotations[1].IshRef | Should Be $true
            Get-IshMetadataField -IshObject $ishAnnotations[0] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
            Get-IshMetadataField -IshObject $ishAnnotations[1] -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation | Should BeExactly $proposedChngText
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