Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Remove-IshAnnotation"
try {

Describe “Remove-IshAnnotation" -Tags "Create" {
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
	
	# Add annotations - ParametersGroup
	$ishAnnotationPG1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationPG2 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationPGWithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationPGWithReplies.IshRef)</ishfield></ishfields>"
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationPGWithReplies.IshRef, $strMetadataReply)
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationPGWithReplies.IshRef, $strMetadataReply)
	
	# Add annotations - IshAnnotationGroup
	$ishAnnotationIAG1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAG2 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGWithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationIAGWithReplies.IshRef)</ishfield></ishfields>"
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGWithReplies.IshRef, $strMetadataReply)
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGWithReplies.IshRef, $strMetadataReply)
	
	# annotations for mixed test (with and without replies)
	$ishAnnotationIAGMixed1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGMixed2 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGMixedWithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationIAGWithReplies.IshRef)</ishfield></ishfields>"
	$ishsession.Annotation25.CreateReply($ishAnnotationIAGMixedWithReplies.IshRef, $strMetadataReply)
	$ishsession.Annotation25.CreateReply($ishAnnotationIAGMixedWithReplies.IshRef, $strMetadataReply)
	
	# Add annotations - IshAnnotationGroup (pipeline)
	$ishAnnotationIAGpipeline1 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGpipeline2 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGpipeline3 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGpipeline4 = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationIAGpipeline1WithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationIAGpipeline1WithReplies.IshRef)</ishfield></ishfields>"
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGpipeline1WithReplies.IshRef, $strMetadataReply)
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGpipeline1WithReplies.IshRef, $strMetadataReply)
	$ishAnnotationIAGpipeline2WithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationIAGpipeline2WithReplies.IshRef)</ishfield></ishfields>"
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGpipeline2WithReplies.IshRef, $strMetadataReply)
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationIAGpipeline2WithReplies.IshRef, $strMetadataReply)
	
	# annotations for mixed test (with and without replies)
	$ishAnnotationPipelineMixed = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$ishAnnotationPipelineMixedWithReplies = Add-IshAnnotation -IshSession $ishsession -PubLogicalId $ishObjectPub.IshRef -PubVersion $ishObjectPub.version_version_value -PubLng $ishObjectPub.fishpubsourcelanguages_version_value -LogicalId $ishObjectTopic.IshRef -Version $ishObjectTopic.version_version_value -Lng $ishObjectTopic.doclanguage -Type $annotationType -Text $annotationText -Status $annotationStatus -Category $annotationCategory -Address $annotationAddress
	$strMetadataReply = "<ishfields><ishfield name='FISHANNOTATIONTEXT' level='reply'>reply to an annotation $($ishAnnotationIAGpipeline2WithReplies.IshRef)</ishfield></ishfields>"
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationPipelineMixedWithReplies.IshRef, $strMetadataReply)
	$ishAnnotationReply = $ishsession.Annotation25.CreateReply($ishAnnotationPipelineMixedWithReplies.IshRef, $strMetadataReply)
	
	Context "Remove-IshAnnotation ParametersGroup" {
		It "Parameter AnnotationId is empty" {
			{Remove-IshAnnotation -IshSession $ishsession -AnnotationId ""} | Should Throw
		}
		
		It "Parameter AnnotationId non-existing Id" {
			{Remove-IshAnnotation -IshSession $ishsession -AnnotationId "GUID-NON-EXISTING"} | Should Throw
		}
		It "Parameter AnnotationId is empty Force ON" {
			{Remove-IshAnnotation -IshSession $ishsession -AnnotationId "" -Force} | Should Throw
		}
		It "Parameter AnnotationId is empty Force OFF" {
			{Remove-IshAnnotation -IshSession $ishsession -AnnotationId ""} | Should Throw
		}
		It "Annotation with replies Force OFF" {
		{Remove-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPGWithReplies.IshRef} | Should Throw
		}
		It "Annotation with replies Force ON" {
			$ishAnnotation = Remove-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPGWithReplies.IshRef -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPGWithReplies.IshRef
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotation without replies Force OFF" {
			$ishAnnotation = Remove-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPG1.IshRef
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPG1.IshRef
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotation without replies Force ON" {
			$ishAnnotation = Remove-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPG2.IshRef -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationPG2.IshRef
            $ishAnnotation.Count | Should Be 0
		}
    }

	Context "Remove-IshAnnotation IshAnnotationGroup" {
		It "Remove-IshAnnotation Annotation with replies Force OFF" {
		{Remove-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotationIAGWithReplies } | Should Throw
		}
		It "Annotation with replies Force ON" {
			$ishAnnotation = Remove-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotationIAGWithReplies -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationIAGWithReplies.IshRef
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotation without replies Force OFF" {
			$ishAnnotation = Remove-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotationIAG1
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationIAG1.IshRef
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotation without replies Force ON" {
			$ishAnnotation = Remove-IshAnnotation -IshSession $ishsession -IshAnnotation $ishAnnotationIAG2 -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId $ishAnnotationIAG2.IshRef
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotations mixed(with and without replies), Force ON, passing as array" {
			$ishAnnotation = Remove-IshAnnotation -IshSession $ishsession -IshAnnotation @($ishAnnotationIAGMixed1, $ishAnnotationIAGMixedWithReplies, $ishAnnotationIAGMixed2) -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId @($ishAnnotationIAGMixed1.IshRef, $ishAnnotationIAGMixedWithReplies.IshRef, $ishAnnotationIAGMixed2.IshRef)
            $ishAnnotation.Count | Should Be 0
		}
    }

    Context "Remove-IshAnnotation IshAnnotationGroup pipeline" {
		It "Annotations with replies, Force OFF, passing one by one" {
			{@($ishAnnotationIAGpipeline1WithReplies, $ishAnnotationIAGpipeline2WithReplies) | Remove-IshAnnotation -IshSession $ishsession} | Should Throw
		}
		It "Annotations with replies, Force ON, passing one by one" {
			$ishAnnotation = @($ishAnnotationIAGpipeline1WithReplies, $ishAnnotationIAGpipeline2WithReplies) | Remove-IshAnnotation -IshSession $ishsession -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId @($ishAnnotationIAGpipeline1WithReplies.IshRef, $ishAnnotationIAGpipeline2WithReplies.IshRef)
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotations without replies, Force OFF, passing one by one" {
			$ishAnnotation = @($ishAnnotationIAGpipeline1, $ishAnnotationIAGpipeline2) | Remove-IshAnnotation -IshSession $ishsession
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId @($ishAnnotationIAGpipeline1.IshRef, $ishAnnotationIAGpipeline2.IshRef)
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotations without replies, Force ON, passing one by one" {
			$ishAnnotation = @($ishAnnotationIAGpipeline3, $ishAnnotationIAGpipeline4) | Remove-IshAnnotation -IshSession $ishsession -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId @($ishAnnotationIAGpipeline3.IshRef,  $ishAnnotationIAGpipeline4.IshRef)
            $ishAnnotation.Count | Should Be 0
		}
		It "Annotations mixed(with and without replies), Force ON, passing whole array" {
			$ishAnnotation = @($ishAnnotationPipelineMixed, $ishAnnotationPipelineMixedWithReplies) | Remove-IshAnnotation -IshSession $ishsession -Force
			$ishAnnotation -eq $null | Should Be $true
            $ishAnnotation = Get-IshAnnotation -IshSession $ishsession -AnnotationId @($ishAnnotationPipelineMixed.IshRef, $ishAnnotationPipelineMixedWithReplies.IshRef)
            $ishAnnotation.Count | Should Be 0
		}
    }
}

} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
    try { $publicationOutputs = Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Where-Object -Property IshFolderType -EQ -Value "ISHPublication" |
		  Get-IshFolderContent -IshSession $ishSession
          $publicationOutputs |
          Get-IshAnnotation -IshSession $ishSession |
          Remove-IshAnnotation -IshSession $ishSession -Force
          $publicationOutputs |
          Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
    try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Where-Object -Property IshFolderType -EQ -Value "ISHMasterDoc" |
		  Get-IshFolderContent -IshSession $ishSession |
		  Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse |
		  Get-IshFolderContent -IshSession $ishSession |
		  Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}