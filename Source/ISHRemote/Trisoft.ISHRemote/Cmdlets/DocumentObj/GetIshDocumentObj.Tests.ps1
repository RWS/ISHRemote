BeforeAll {
	$cmdletName = "Get-IshDocumentObj"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshDocumentObj" {
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

		$tempFilePath = (New-TemporaryFile).FullName

		# amount of test data to generate 
		$ishDocumentObjTopicsCount = 1000
		$ishDocumentObjMapsCount = 1000
		$ishDocumentObjLibsCount = 1000
		$ishDocumentObjImagesCount = 1000
		$ishDocumentObjOthersCount = 1000
	}
	Context "Get-IshDocumentObj Generate Performance Test Data within the test" {
		BeforeAll {
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
			# catching created objects in an array 
			$ishDocumentObjTopics = @()
			$ishDocumentObjMaps = @()
			$ishDocumentObjLibs = @()
			$ishDocumentObjImages = @()
			$ishDocumentObjOthers = @()
		}
		It "Generate Topics" {
			for ($i = 1; $i -le $ishDocumentObjTopicsCount; $i++)
			{
				$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Get-IshDocumentObj Topic[$($i.ToString('0000'))] $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
				$ishDocumentObjTopics += Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '2' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			}
		}
		It "Generate Maps" {
			for ($i = 1; $i -le $ishDocumentObjMapsCount; $i++)
			{
				$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Get-IshDocumentObj Map[$($i.ToString('0000'))] $timestamp" |
						      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
				$ishDocumentObjMaps += Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -Version '3' -Lng $ishLng -Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $ditaMapFileContent
			}
		}
		It "Generate Library Topics" {
			for ($i = 1; $i -le $ishDocumentObjLibsCount; $i++)
			{
				$ishLibMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Get-IshDocumentObj Lib[$($i.ToString('0000'))] $timestamp" |
							  Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
				$ishDocumentObjLibs += Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderLib -IshType ISHLibrary -Version '4' -Lng $ishLng -Metadata $ishLibMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent
			}
		}
		It "Generate Images" {
			for ($i = 1; $i -le $ishDocumentObjImagesCount; $i++)
			{
				$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Get-IshDocumentObj Image[$($i.ToString('0000'))] $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
				$ishDocumentObjImages += Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderImage -IshType ISHIllustration -Version '5' -Lng $ishLng -Resolution $ishResolution -Metadata $ishImageMetadata -Edt "EDTJPEG" -FilePath $tempFilePath
			}
		}
		It "Generate Others like EDT-TEXT" {
			for ($i = 1; $i -le $ishDocumentObjOthersCount; $i++)
			{
				$ishOtherMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Get-IshDocumentObj Other[$($i.ToString('0000'))] $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
				$ishOtherMetadata | Out-File $tempFilePath
				$ishDocumentObjOthers += Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderOther -IshType ISHTemplate -Version '6' -Lng $ishLng -Metadata $ishOtherMetadata -Edt "EDT-TEXT" -FilePath $tempFilePath
			}
		}
	}
	Context  "Get-IshDocumentObj Query 5 times per test" {
		BeforeAll {
			$ishDocumentObjTopics = Get-IshFolder -IshFolder $global:ishFolderCmdlet -Recurse -FolderTypeFilter ISHModule | Get-IshFolderContent
			$ishDocumentObjMaps = Get-IshFolder -IshFolder $global:ishFolderCmdlet -Recurse -FolderTypeFilter ISHMasterDoc | Get-IshFolderContent
			$ishDocumentObjLibs = Get-IshFolder -IshFolder $global:ishFolderCmdlet -Recurse -FolderTypeFilter ISHLibrary | Get-IshFolderContent
			$ishDocumentObjImages = Get-IshFolder -IshFolder $global:ishFolderCmdlet -Recurse -FolderTypeFilter ISHIllustration | Get-IshFolderContent
			$ishDocumentObjOthers = Get-IshFolder -IshFolder $global:ishFolderCmdlet -Recurse -FolderTypeFilter ISHTemplate | Get-IshFolderContent
			$allIShDocumentObjs = $ishDocumentObjTopics + $ishDocumentObjMaps + $ishDocumentObjLibs + $ishDocumentObjImages + $ishDocumentObjOthers
		}
		It "Get-IShDocumentObj by LogicalId for 1 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId $ishDocumentObjTopics[0].IshRef
			Get-IshDocumentObj -LogicalId $ishDocumentObjMaps[0].IshRef
			Get-IshDocumentObj -LogicalId $ishDocumentObjLibs[0].IshRef
			Get-IshDocumentObj -LogicalId $ishDocumentObjImages[0].IshRef
			Get-IshDocumentObj -LogicalId $ishDocumentObjOthers[0].IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 1 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject $ishDocumentObjTopics[1]
			Get-IshDocumentObj -IshObject $ishDocumentObjMaps[1]
			Get-IshDocumentObj -IshObject $ishDocumentObjLibs[1]
			Get-IshDocumentObj -IshObject $ishDocumentObjImages[1]
			Get-IshDocumentObj -IshObject $ishDocumentObjOthers[1]
		}
		It "Get-IShDocumentObj by LogicalId for 2 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 2).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 2).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 2).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 2).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 2).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 2 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 2)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 2)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 2)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 2)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 2)
		}
		It "Get-IShDocumentObj by LogicalId for 10 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 10).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 10).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 10).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 10).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 10).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 10 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 10)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 10)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 10)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 10)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 10)
		}
		It "Get-IShDocumentObj by LogicalId for 100 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 100).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 100).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 100).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 100).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 100).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 100 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 100)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 100)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 100)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 100)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 100)
		}
		It "Get-IShDocumentObj by LogicalId for 200 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 200).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 200).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 200).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 200).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 200).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 200 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 200)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 200)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 200)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 200)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 200)
		}
		It "Get-IShDocumentObj by LogicalId for 400 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 400).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 400).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 400).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 400).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 400).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 400 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 400)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 400)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 400)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 400)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 400)
		}
		It "Get-IShDocumentObj by LogicalId for 600 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 600).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 600).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 600).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 600).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 600).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 600 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 600)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 600)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 600)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 600)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 600)
		}
		It "Get-IShDocumentObj by LogicalId for 800 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 800).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 800).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 800).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 800).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 800).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 800 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 800)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 800)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 800)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 800)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 800)
		}
		It "Get-IShDocumentObj by LogicalId for 1000 (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($ishDocumentObjTopics | Get-Random -Count 1000).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjMaps | Get-Random -Count 1000).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjLibs | Get-Random -Count 1000).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjImages | Get-Random -Count 1000).IshRef
			Get-IshDocumentObj -LogicalId ($ishDocumentObjOthers | Get-Random -Count 1000).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 1000 (RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($ishDocumentObjTopics | Get-Random -Count 1000)
			Get-IshDocumentObj -IshObject ($ishDocumentObjMaps | Get-Random -Count 1000)
			Get-IshDocumentObj -IshObject ($ishDocumentObjLibs | Get-Random -Count 1000)
			Get-IshDocumentObj -IshObject ($ishDocumentObjImages | Get-Random -Count 1000)
			Get-IshDocumentObj -IshObject ($ishDocumentObjOthers | Get-Random -Count 1000)
		}
		It "Get-IShDocumentObj by LogicalId for 2000 [MetadataBatchSize999|Basic] (RetrieveMetadata)" {
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 2000 [MetadataBatchSize999|Basic](RetrieveMetadataByIshLngRefs)" {
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
		}
		It "Get-IShDocumentObj by LogicalId for 2000 [MetadataBatchSize1M|All] (RetrieveMetadata)" {
			$ishSession.MetadataBatchSize = 1000000
			$ishSession.DefaultRequestedMetadata="All"
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 2000).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 2000 [MetadataBatchSize1M|All](RetrieveMetadataByIshLngRefs)" {
			$ishSession.MetadataBatchSize = 1000000
			$ishSession.DefaultRequestedMetadata="All"
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 2000)
		}
		It "Get-IShDocumentObj by LogicalId for 5000 [MetadataBatchSize1M|All] (RetrieveMetadata)" {
			$ishSession.MetadataBatchSize = 1000000
			$ishSession.DefaultRequestedMetadata="All"
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 5000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 5000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 5000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 5000).IshRef
			Get-IshDocumentObj -LogicalId ($allIShDocumentObjs | Get-Random -Count 5000).IshRef
		}
		It "Get-IShDocumentObj by IshLngref for 5000 [MetadataBatchSize1M|All](RetrieveMetadataByIshLngRefs)" {
			$ishSession.MetadataBatchSize = 1000000
			$ishSession.DefaultRequestedMetadata="All"
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 5000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 5000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 5000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 5000)
			Get-IshDocumentObj -IshObject ($allIShDocumentObjs | Get-Random -Count 5000)
		}
	} 
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	try { Remove-Item $tempFilePath -Force } catch { }
}

