BeforeAll {
	$cmdletName = "Remove-IshDocumentObj"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Remove-IshDocumentObj" -Tags "Delete" {
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

		$global:ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderMap = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "Map" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderLib = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHLibrary -FolderName "Library" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderImage = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHIllustration -FolderName "Image" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		$ishFolderOther = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHTemplate -FolderName "Other" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
		
		$ishObjectMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		
		$imageFilePath = (New-TemporaryFile).FullName
		Add-Type -AssemblyName "System.Drawing"
		$bmp = New-Object -TypeName System.Drawing.Bitmap(100,100)
		for ($i = 0; $i -lt 100; $i++)
		{
			for ($j = 0; $j -lt 100; $j++)
			{
				$bmp.SetPixel($i, $j, 'Red')
			}
		}
		$bmp.Save($imageFilePath, [System.Drawing.Imaging.ImageFormat]::Jpeg)
	}

	Context "Remove-IshDocumentObj check exceptions" {
		It "LogicalId does not exist. Force=Yes" {
            {Remove-IshDocumentObj -IshSession $ishSession -LogicalId "NON-EXISTING-LOGICAL-ID" -Version "1" -Lng $ishLngLabel -Force} | Should -Throw
		}	
		It "LogicalId does not exist" {
            {Remove-IshDocumentObj -IshSession $ishSession -LogicalId "NON-EXISTING-LOGICAL-ID" -Version "1" -Lng $ishLngLabel} | Should -Throw
		}
		It "Providing both Parameters and ISHObject" {
    		$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			{Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObject -LogicalId $ishObject.IshRef -Version $ishObject.version_version_value -Lng $ishObject.doclanguage} | Should -Throw
		}
	}

	Context "Remove-IshDocumentObj remove object (ParameterGroup)" {
		It "Parameters (Topic). Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject.IshRef -Version $ishObject.version_version_value -Lng $ishObject.doclanguage -Force
			$ishObjectRetrieved = Get-IshDocumentObj -IshSession $ishSession -IshObject $ishObject	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}	
		It "Parameters (Map). Force=Yes" {
			$ishMapMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -LogicalId "MYOWNGENERATEDLOGICALIDMAP" -Version '3' -Lng $ishLng -Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $ditaMapFileContent
            Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject.IshRef -Version $ishObject.version_version_value -Lng $ishObject.doclanguage -Force
			$ishObjectRetrieved = Get-IshDocumentObj -IshSession $ishSession -IshObject $ishObject	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}
		It "Parameters (Lib). Force=Yes" {
			$ishLibMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderLib -IshType ISHLibrary -LogicalId "MYOWNGENERATEDLOGICALIDLIB" -Version '4' -Lng $ishLng -Metadata $ishLibMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject.LngRef -gt 0 | Should -Be $true
            Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject.IshRef -Version $ishObject.version_version_value -Lng $ishObject.doclanguage -Force
			$ishObjectRetrieved = Get-IshDocumentObj -IshSession $ishSession -IshObject $ishObject	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}
		
		It "Parameters (Topic) with multiple versions" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -Version $ishObject1.version_version_value -Lng $ishObject1.doclanguage
			$ishObjectsRetrieved = Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef
            $ishObjectsRetrieved.length -eq 2 | Should -Be $true
			$ishObjectsRetrieved[0].version_version_value -eq 2 | Should -Be $true
			$ishObjectsRetrieved[1].version_version_value -eq 3 | Should -Be $true
		}
		
		It "Parameters (Topic) with multiple versions. Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -Version $ishObject1.version_version_value -Lng $ishObject1.doclanguage -Force
			$ishObjectsRetrieved = Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef
            $ishObjectsRetrieved.length -eq 2 | Should -Be $true
			$ishObjectsRetrieved[0].version_version_value -eq 2 | Should -Be $true
			$ishObjectsRetrieved[1].version_version_value -eq 3 | Should -Be $true
		}
		
		It "Parameters (Topic) with multiple versions and languages" {
			$ishTopicMetadata = $ishObjectMetadata | 
										Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp" |
										Set-IshMetadataField -IshSession $ishSession -Name "FNOTRANSLATIONMGMT" -Level "Logical" -ValueType Element -Value "TRUE"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLngLabel -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLngLabel -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLngTarget2Label -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject3.IshRef -Version $ishObject3.version_version_value -Lng $ishObject3.doclanguage
			$ishObjectsRetrieved = Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef
            $ishObjectsRetrieved.length -eq 2 | Should -Be $true
			$ishObjectsRetrieved[0].doclanguage -eq $ishLngLabel | Should -Be $true
			$ishObjectsRetrieved[0].version_version_value -eq 1 | Should -Be $true
			$ishObjectsRetrieved[1].doclanguage -eq $ishLngLabel | Should -Be $true
			$ishObjectsRetrieved[1].version_version_value -eq 2 | Should -Be $true
		}
		
		It "Parameters (Topic) with multiple versions and languages. Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | 
										Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp" |
										Set-IshMetadataField -IshSession $ishSession -Name "FNOTRANSLATIONMGMT" -Level "Logical" -ValueType Element -Value "TRUE"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLngLabel -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLngLabel -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLngTarget2Label -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject3.IshRef -Version $ishObject3.version_version_value -Lng $ishObject3.doclanguage -Force
			$ishObjectsRetrieved = Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef
            $ishObjectsRetrieved.length -eq 2 | Should -Be $true
			$ishObjectsRetrieved[0].doclanguage -eq $ishLngLabel | Should -Be $true
			$ishObjectsRetrieved[0].version_version_value -eq 1 | Should -Be $true
			$ishObjectsRetrieved[1].doclanguage -eq $ishLngLabel | Should -Be $true
			$ishObjectsRetrieved[1].version_version_value -eq 2 | Should -Be $true
		}
		
		It "Parameters (Image). Force=Yes" {
			$ishImageMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderImage -IshType ISHIllustration -Version '1' -Lng $ishLng -Resolution $ishResolution -Metadata $ishImageMetadata -Edt "EDTJPEG" -FilePath $imageFilePath
			Remove-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject.IshRef -Version $ishObject.version_version_value -Lng $ishObject.doclanguage -Resolution $ishObject.fresolution -Force
			$ishObjectRetrieved = Get-IshDocumentObj -IshSession $ishSession -IshObject $ishObject	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}
	}
	
	Context "Remove-IshDocumentObj remove object (IshObjectsGroup)" {
		It "IshObject passed via pipeline (Topic). Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject | Remove-IshDocumentObj -IshSession $ishSession -Force
			$ishObjectRetrieved = Get-IshDocumentObj -IshSession $ishSession -IshObject $ishObject	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}	
		It "IshObject passed via parameter (Topic). Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
           	Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObject -Force
			$ishObjectRetrieved = Get-IshDocumentObj -IshSession $ishSession -IshObject $ishObject	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}	
		
		It "IshObject passed via pipeline (Topic) multiple versions" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject1 | Remove-IshDocumentObj -IshSession $ishSession
			$ishObjectsRetrieved = Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef
            $ishObjectsRetrieved.length -eq 2 | Should -Be $true
			$ishObjectsRetrieved[0].version_version_value -eq 2 | Should -Be $true
			$ishObjectsRetrieved[1].version_version_value -eq 3 | Should -Be $true
		}
		
		It "IshObject passed via pipeline (Topic) multiple versions. Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject1 | Remove-IshDocumentObj -IshSession $ishSession -Force
			$ishObjectsRetrieved = Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef
            $ishObjectsRetrieved.length -eq 2 | Should -Be $true
			$ishObjectsRetrieved[0].version_version_value -eq 2 | Should -Be $true
			$ishObjectsRetrieved[1].version_version_value -eq 3 | Should -Be $true
		}
		
		It "Multiple IshObjects of the same LogicalId passed via pipeline. Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef -IshType ISHModule -Version '3' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef | Remove-IshDocumentObj -IshSession $ishSession -Force
			$ishObjectsRetrieved = Get-IshDocumentObj -IshSession $ishSession -LogicalId $ishObject1.IshRef
            $ishObjectsRetrieved.length -eq 0 | Should -Be $true
		}
		
		It "Multiple IshObjects of different LogicalId passed via pipeline. Force=Yes" {
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject1 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
            $ishObject2 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			$ishObject3 = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			@($ishObject1,$ishObject2,$ishObject3) | Get-IshDocumentObj -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force
			$ishObjectsRetrieved = @($ishObject1,$ishObject2,$ishObject3) | Get-IshDocumentObj -IshSession $ishSession
            $ishObjectsRetrieved.length -eq 0 | Should -Be $true
		}
		
		It "IshObject passed via pipeline (Image). Force=Yes" {
			$ishImageMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObject = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderImage -IshType ISHIllustration -Version '1' -Lng $ishLng -Resolution $ishResolution -Metadata $ishImageMetadata -Edt "EDTJPEG" -FilePath $imageFilePath
			$ishObject | Remove-IshDocumentObj -IshSession $ishSession -Force
			$ishObjectRetrieved = Get-IshDocumentObj -IshSession $ishSession -IshObject $ishObject	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}
		
		It "Multiple IshObjects passed via pipeline (Image and Topic). Force=Yes" {
			$ishImageMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObjectImage = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderImage -IshType ISHIllustration -Version '1' -Lng $ishLng -Resolution $ishResolution -Metadata $ishImageMetadata -Edt "EDTJPEG" -FilePath $imageFilePath
			$ishTopicMetadata = $ishObjectMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
			$ishObjectTopic = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			@($ishObjectTopic, $ishObjectImage) | Remove-IshDocumentObj -IshSession $ishSession -Force
			$ishObjectRetrieved = @($ishObjectTopic, $ishObjectImage) | Get-IshDocumentObj -IshSession $ishSession	            
            $ishObjectRetrieved.length -eq 0 | Should -Be $true
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession -VersionFilter "" | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	try { Remove-Item $tempFilePath -Force } catch { }
}

