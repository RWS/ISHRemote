Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Set-IshDocumentObj"


#
# Script-file scope auxiliary function
# Gets BackgroundTasks InputData to parse out the language cardids passed to the BackgroundTask event
#
function script:CreateSquareImageBySideSize([int]$size)
{
	Add-Type -AssemblyName "System.Drawing"
	$bmp = New-Object -TypeName System.Drawing.Bitmap($size, $size)
	for ($i = 0; $i -lt $size; $i++)
	{
		for ($j = 0; $j -lt $size; $j++)
		{
			$bmp.SetPixel($i, $j, 'Red')
		}
	}
	
	return $bmp
}

try {
$tempFolder = [System.IO.Path]::GetTempPath()
Describe “Set-IshDocumentObj" -Tags "Create" {
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

	$global:ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType ISHNone -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderMap = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "Map" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderLib = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHLibrary -FolderName "Library" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderImage = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHIllustration -FolderName "Image" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderOther = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHTemplate -FolderName "Other" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

	# Create files with two images: 100*100 and 200*200
	$tempFilePathImage100x100 = (New-TemporaryFile).FullName
	$bmp = CreateSquareImageBySideSize -size 100
	$bmp.Save($tempFilePathImage100x100, [System.Drawing.Imaging.ImageFormat]::Jpeg)
	$tempFilePathImage200x200 = (New-TemporaryFile).FullName
	$bmp = CreateSquareImageBySideSize -size 200
	$bmp.Save($tempFilePathImage200x200, [System.Drawing.Imaging.ImageFormat]::Jpeg)
	
	Context "Set-IshDocumentObj returns IshObject object (Topic)" {
		
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
												-FolderId $ishFolderTopic.IshFolderRef `
												-IshType ISHModule `
												-Lng $ishLng `
												-Metadata $ishTopicMetadata `
												-FileContent $ditaTopicFileContent
		$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value "Updated topic title $timestamp"
		$ishObject = Set-IshDocumentObj -IshSession $ishSession `
										-LogicalId $ishObjectToUpdate.IshRef `
										-Version $ishObjectToUpdate.version_version_value `
										-Lng $ishObjectToUpdate.doclanguage `
										-Metadata $ishMetadataFieldsSet
										
		It "GetType().Name" {
			$ishObject.GetType().Name | Should BeExactly "IshDocumentObj"
		}
		It "ishObject.IshData" {
			{ $ishObject.IshData } | Should Not Throw
		}
		It "ishObject.IshField" {
			$ishObject.IshField | Should Not BeNullOrEmpty
		}
		It "ishObject.IshRef" {
			$ishObject.IshRef | Should Not BeNullOrEmpty
		}
		It "ishObject.IshType" {
			$ishObject.IshType | Should Not BeNullOrEmpty
		}
		It "ishObject.ObjectRef" {
			$ishObject.ObjectRef | Should Not BeNullOrEmpty
		}
		It "ishObject.VersionRef" {
			$ishObject.VersionRef | Should Not BeNullOrEmpty
		}
		It "ishObject.LngRef" {
			$ishObject.LngRef | Should Not BeNullOrEmpty
		}
		It "ishObject ConvertTo-Json" {
			(ConvertTo-Json $ishObject).Length -gt 2 | Should Be $true
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			#logical
			$ishObject.ftitle_logical_value.Length -ge 1 | Should Be $true 
			#version
			$ishObject.version_version_value.Length -ge 1 | Should Be $true 
			#language
			$ishObject.fstatus.Length -ge 1 | Should Be $true 
			$ishObject.fstatus_lng_element.StartsWith('VSTATUS') | Should Be $true 
			$ishObject.doclanguage.Length -ge 1 | Should Be $true  # Field names like DOC-LANGUAGE get stripped of the hyphen, otherwise you get $ishObject.'doc-language' and now you get the more readable $ishObject.doclanguage
			$ishObject.doclanguage_lng_element.StartsWith('VLANGUAGE') | Should Be $true 
		}
	}

	Context "Set-IshDocumentObj ParameterGroupMetadataOnly" {
		It "Topic - metadata update" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
							    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent

			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Metadata $ishMetadataFieldsSet
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
		}
		
		It "Topic - metadata update with RequiredCurrentMetadata accepted" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
							    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent

			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Metadata $ishMetadataFieldsSet `
											-RequiredCurrentMetadata $ishRequiredCurrentMetadata
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
		}
		
		It "Topic - metadata update with RequiredCurrentMetadata rejected" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
							    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent

			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusReleased
			{Set-IshDocumentObj -IshSession $ishSession `
								-LogicalId $ishObjectToUpdate.IshRef `
								-Version $ishObjectToUpdate.version_version_value `
								-Lng $ishObjectToUpdate.doclanguage `
								-Metadata $ishMetadataFieldsSet `
								-RequiredCurrentMetadata $ishRequiredCurrentMetadata} | 
			Should Throw '[-106011] The supplied expected metadata value "Released" does not match the current database value "Draft" so we rolled back your operation. To make the operation work you should make sure your value matches the latest database value. [f:158 fe:FSTATUS ft:LOV] [106011;InvalidCurrentMetadata]'
		}
		
		It "Image - metadata update without Resolution" {
			$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Image $timestamp" |
							    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderImage `
													-IshType ISHIllustration `
													-Version '3' `
													-Lng $ishLng `
													-Resolution $ishResolution `
													-Metadata $ishImageMetadata `
													-Edt "EDTJPEG" `
													-FilePath $tempFilePathImage100x100

			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Metadata $ishMetadataFieldsSet
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
		}
		
		It "Image - metadata update with Resolution" {
			$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Image $timestamp" |
							    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderImage `
													-IshType ISHIllustration `
													-Version '3' `
													-Lng $ishLng `
													-Resolution $ishResolution `
													-Metadata $ishImageMetadata `
													-Edt "EDTJPEG" `
													-FilePath $tempFilePathImage100x100

			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Resolution $ishResolution `
											-Metadata $ishMetadataFieldsSet
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
		}
		
			It "Image - metadata update with non-matching Resolution" {
			$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Image $timestamp" |
							    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
							    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderImage `
													-IshType ISHIllustration `
													-Version '3' `
													-Lng $ishLng `
													-Resolution $ishResolution `
													-Metadata $ishImageMetadata `
													-Edt "EDTJPEG" `
													-FilePath $tempFilePathImage100x100

			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			{Set-IshDocumentObj -IshSession $ishSession `
								-LogicalId $ishObjectToUpdate.IshRef `
								-Version $ishObjectToUpdate.version_version_value `
								-Lng $ishObjectToUpdate.doclanguage `
								-Resolution "VRESHIGH" `
								-Metadata $ishMetadataFieldsSet} | Should Throw "[-102] The object $($ishObjectToUpdate.IshRef)=3=en=High does not exist. [co:""$($ishObjectToUpdate.IshRef)=3=en=High""] [102;ObjectNotFound]"
		}
	}

	Context “Set-IshDocumentObj ParameterGroupFileContent" {
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Set All Parameters Topic $timestamp" |
					        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
		    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
		$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Version '1' -Lng $ishLng -Metadata $ishTopicMetadata -Edt "EDTXML" -FileContent $ditaTopicFileContent

		It "Parameter EDT explicitly EDTJPEG" {
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value "Updated title"
			{ Set-IshDocumentObj -IshSession $ishSession `
								 -LogicalId $ishObjectToUpdate.IshRef `
								 -Version $ishObjectToUpdate.version_version_value `
								 -Lng $ishObjectToUpdate.doclanguage `
								 -Metadata $ishMetadataFieldsSet `
								 -Edt "EDTJPEG" `
								 -FileContent "INVALIDFILECONTENT"} | Should Throw "FileContent parameter is only supported with Edt='EDTXML'"
		}

		It "Parameter FileContent has invalid xml content" {
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value "Updated title"
			{ Set-IshDocumentObj -IshSession $ishSession `
								 -LogicalId $ishObjectToUpdate.IshRef `
								 -Version $ishObjectToUpdate.version_version_value `
								 -Lng $ishObjectToUpdate.doclanguage `
								 -Metadata $ishMetadataFieldsSet `
								 -FileContent "INVALIDFILECONTENT"} | Should Throw "Data at the root level is invalid. Line 1, position 1."
		}

		It "Provide both FileContent and FilePath" {
			$tempFilePath = (New-TemporaryFile).FullName
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value "Updated title"
			{ Set-IshDocumentObj -IshSession $ishSession `
								 -LogicalId $ishObjectToUpdate.IshRef `
								 -Version $ishObjectToUpdate.version_version_value `
								 -Lng $ishObjectToUpdate.doclanguage `
								 -Metadata $ishMetadataFieldsSet `
								 -FileContent $ditaTopicFileContent `
								 -FilePath $tempFilePath} | Should Throw "Parameter set cannot be resolved using the specified named parameters."
		}
		
		It "Topic - update blob only" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Set All Parameters Topic $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderTopic `
													-IshType ISHModule `
													-Version '1' `
													-Lng $ishLng `
													-Metadata $ishTopicMetadata `
													-Edt "EDTXML" `
													-FileContent $ditaTopicFileContent
			$fileInfo = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectToUpdateContent = Get-Content -Path $fileInfo		
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$updatedContent = $ishObjectToUpdateContent.Replace("(optional)", "(optional-updated)")
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-FileContent $updatedContent
			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectContent = Get-Content -Path $fileInfo		
			
			$ishObjectContent -eq $updatedContent | Should Be $true
		}

		It "Topic - update blob and metadata" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Set All Parameters Topic $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderTopic `
													-IshType ISHModule `
													-Version '1' `
													-Lng $ishLng `
													-Metadata $ishTopicMetadata `
													-Edt "EDTXML" `
													-FileContent $ditaTopicFileContent
			$fileInfo = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectToUpdateContent = Get-Content -Path $fileInfo		
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$updatedContent = $ishObjectToUpdateContent.Replace("(optional)", "(optional-updated)")
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
                                            -Metadata $ishMetadataFieldsSet `
											-Lng $ishObjectToUpdate.doclanguage `
											-FileContent $updatedContent
			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectContent = Get-Content -Path $fileInfo		
			
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
			$ishObjectContent -eq $updatedContent | Should Be $true
		}
		
		It "Topic - update (RequiredCurrentMetadata accepted)" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic update with RequiredCurrentMetadata accepted $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderTopic `
													-IshType ISHModule `
													-Version '1' `
													-Lng $ishLng `
													-Metadata $ishTopicMetadata `
													-Edt "EDTXML" `
													-FileContent $ditaTopicFileContent
			$fileInfo = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectToUpdateContent = Get-Content -Path $fileInfo		
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$updatedContent = $ishObjectToUpdateContent.Replace("(optional)", "(optional-updated)")
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Metadata $ishMetadataFieldsSet `
											-RequiredCurrentMetadata $ishRequiredCurrentMetadata `
											-FileContent $updatedContent
			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectContent = Get-Content -Path $fileInfo		
			
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
			$ishObjectContent -eq $updatedContent | Should Be $true
		}	
		
		It "Topic - update (RequiredCurrentMetadata rejected)" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic update with RequiredCurrentMetadata rejected $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderTopic `
													-IshType ISHModule `
													-Version '1' `
													-Lng $ishLng `
													-Metadata $ishTopicMetadata `
													-Edt "EDTXML" `
													-FileContent $ditaTopicFileContent
			$fileInfo = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectToUpdateContent = Get-Content -Path $fileInfo		
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$updatedContent = $ishObjectToUpdateContent.Replace("(optional)", "(optional-updated)")
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusReleased
			{Set-IshDocumentObj -IshSession $ishSession `
								-LogicalId $ishObjectToUpdate.IshRef `
								-Version $ishObjectToUpdate.version_version_value `
								-Lng $ishObjectToUpdate.doclanguage `
								-Metadata $ishMetadataFieldsSet `
								-RequiredCurrentMetadata $ishRequiredCurrentMetadata `
								-FileContent $updatedContent} | 
			Should Throw '[-106011] The supplied expected metadata value "Released" does not match the current database value "Draft" so we rolled back your operation. To make the operation work you should make sure your value matches the latest database value. [f:158 fe:FSTATUS ft:LOV] [106011;InvalidCurrentMetadata]'
		}
		
		It "Map - update blob and metadata" {
			$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Map update $timestamp" |
						      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderMap `
													-IshType ISHMasterDoc `
													-Version '3' `
													-Lng $ishLng `
													-Metadata $ishMapMetadata `
													-Edt "EDTXML" `
													-FileContent $ditaMapFileContent
			$fileInfo = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectToUpdateContent = Get-Content -Path $fileInfo		
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$updatedContent = $ishObjectToUpdateContent.Replace("your map", "my map")
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
                                            -Metadata $ishMetadataFieldsSet `
											-Lng $ishObjectToUpdate.doclanguage `
											-FileContent $updatedContent
			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectContent = Get-Content -Path $fileInfo		
			
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
			$ishObjectContent -eq $updatedContent | Should Be $true

		}
		It "Lib - update blob and metadata" {
			$ishLibMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Lib update $timestamp" |
						      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderLib `
													-IshType ISHLibrary `
													-Version '4' `
													-Lng $ishLng `
													-Metadata $ishLibMetadata `
													-Edt "EDTXML" `
													-FileContent $ditaTopicFileContent
			$fileInfo = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectToUpdateContent = Get-Content -Path $fileInfo		
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$updatedContent = $ishObjectToUpdateContent.Replace("(optional)", "(optional-updated)")
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
                                            -Metadata $ishMetadataFieldsSet `
											-Lng $ishObjectToUpdate.doclanguage `
											-FileContent $updatedContent
			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectContent = Get-Content -Path $fileInfo		
			
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
			$ishObjectContent -eq $updatedContent | Should Be $true
		}
	}
	
	Context “Set-IshDocumentObj ParameterGroupFilePath" {
		
		It "Image - update blob only (providing EDT)" {
			$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Mandatory parameters Image $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderImage `
													-IshType ISHIllustration `
													-Version '3' `
													-Lng $ishLng `
													-Resolution $ishResolution `
													-Metadata $ishImageMetadata `
													-Edt "EDTJPEG" `
													-FilePath $tempFilePathImage100x100
			
			$fileInfoToUpdate = Get-Item -Path $tempFilePathImage100x100
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Edt "EDTJPEG" `
											-FilePath $tempFilePathImage200x200

			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			$fileInfoToUpdate.Length -lt $fileInfo.Length | Should Be $true
		}

		It "Image - update metadata and blob (providing EDT)" {
			$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Mandatory parameters Image $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderImage `
													-IshType ISHIllustration `
													-Version '3' `
													-Lng $ishLng `
													-Resolution $ishResolution `
													-Metadata $ishImageMetadata `
													-Edt "EDTJPEG" `
													-FilePath $tempFilePathImage100x100
			
			$fileInfoToUpdate = Get-Item -Path $tempFilePathImage100x100
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"								
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Metadata $ishMetadataFieldsSet `
											-Edt "EDTJPEG" `
											-FilePath $tempFilePathImage200x200

			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
			$fileInfoToUpdate.Length -lt $fileInfo.Length | Should Be $true
		}
		
		It "Image - update metadata and blob providing EDT, RequiredCurrentMetadata accepted" {
			$ishImageMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Mandatory parameters Image with  RequiredCurrentMetadata $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderImage `
													-IshType ISHIllustration `
													-Version '3' `
													-Lng $ishLng `
													-Resolution $ishResolution `
													-Metadata $ishImageMetadata `
													-Edt "EDTJPEG" `
													-FilePath $tempFilePathImage100x100
			
			$fileInfoToUpdate = Get-Item -Path $tempFilePathImage100x100
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"								
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle

			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Metadata $ishMetadataFieldsSet `
											-Edt "EDTJPEG" `
											-RequiredCurrentMetadata $ishRequiredCurrentMetadata `
											-FilePath $tempFilePathImage200x200

			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
			$fileInfoToUpdate.Length -lt $fileInfo.Length | Should Be $true
		}
		
		It "Topic - update metadata and blob, RequiredCurrentMetadata rejected" {
			$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic update with RequiredCurrentMetadata rejected $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderTopic `
													-IshType ISHModule `
													-Version '1' `
													-Lng $ishLng `
													-Metadata $ishTopicMetadata `
													-Edt "EDTXML" `
													-FileContent $ditaTopicFileContent
			$fileInfo = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			[string]$ishObjectToUpdateContent = Get-Content -Path $fileInfo		
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"
			$updatedContent = $ishObjectToUpdateContent.Replace("(optional)", "(optional-updated)")
			$tempFilePathUpdated = (New-TemporaryFile).FullName
			$updatedContent | Out-File -FilePath $tempFilePathUpdated -Force
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusReleased
			{Set-IshDocumentObj -IshSession $ishSession `
								-LogicalId $ishObjectToUpdate.IshRef `
								-Version $ishObjectToUpdate.version_version_value `
								-Lng $ishObjectToUpdate.doclanguage `
								-Metadata $ishMetadataFieldsSet `
								-RequiredCurrentMetadata $ishRequiredCurrentMetadata `
								-FilePath $tempFilePathUpdated} | 
			Should Throw '[-106011] The supplied expected metadata value "Released" does not match the current database value "Draft" so we rolled back your operation. To make the operation work you should make sure your value matches the latest database value. [f:158 fe:FSTATUS ft:LOV] [106011;InvalidCurrentMetadata]'
		}	
		
		It "Update Other like EDT-TEXT" {
			$ishOtherMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Update Other like EDT-TEXT $timestamp" |
						        Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
			    			    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$tempFilePath = (New-TemporaryFile).FullName
			("Update other " * 100) | Out-File $tempFilePath
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderOther -IshType ISHTemplate -Version '6' -Lng $ishLng -Metadata $ishOtherMetadata -Edt "EDT-TEXT" -FilePath $tempFilePath
			$fileInfoToUpdate = $ishObjectToUpdate | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"								
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			("Update other " * 200) | Out-File $tempFilePath -Force
			$ishObject = Set-IshDocumentObj -IshSession $ishSession `
											-LogicalId $ishObjectToUpdate.IshRef `
											-Version $ishObjectToUpdate.version_version_value `
											-Lng $ishObjectToUpdate.doclanguage `
											-Metadata $ishMetadataFieldsSet `
											-Edt "EDT-TEXT" `
											-FilePath $tempFilePath
			$fileInfo = $ishObject | Get-IshDocumentObjData -IshSession $ishSession -FolderPath (Join-Path $tempFolder $cmdletName)
			
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
			$fileInfoToUpdate.Length -lt $fileInfo.Length | Should Be $true
		}
	}

	Context “Set-IshDocumentObj IshObjectsGroup" {
		$ishTopicMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Topic $timestamp" |
						    Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
						    Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft

		It "Parameter/pipe IshObject is empty" {
			{Set-IshDocumentObj -IshSession $ishSession -IshObject @()} | Should Not Throw
			{@() | Set-IshDocumentObj -IshSession $ishSession} | Should Not Throw
		}
		
		It "Parameter IshObject invalid" {
			{ Set-IshDocumentObj -IshSession $ishSession -IshObject "INVALIDISHOBJECT" } | Should Throw "Cannot bind parameter 'IshObject'. Cannot convert the ""INVALIDISHOBJECT"" value of type ""System.String"" to type ""Trisoft.ISHRemote.Objects.Public.IshObject""."
		}

		It "Provide as parameter/pipe deleted object" {
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			Remove-IshDocumentObj -IshSession $ishSession -IshObject $ishObjectToUpdate -Force
			{$ishObjectSet = Set-IshDocumentObj -IshSession $ishSession -IshObject $ishObjectToUpdate} | Should Throw "[-102] The language object $($ishObjectToUpdate.LngRef) does not exist. [c:""$($ishObjectToUpdate.LngRef)""] [102;ObjectNotFound]"
			{$ishObjectSet = $ishObjectToUpdate | Set-IshDocumentObj -IshSession $ishSession } | Should Throw "[-102] The language object $($ishObjectToUpdate.LngRef) does not exist. [c:""$($ishObjectToUpdate.LngRef)""] [102;ObjectNotFound]"
		}
		
		It "Set metadata, provide multiple objects to IshObject" {
			$ishObjectA = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent 
			$ishObjectB = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			
			$updatedTitle = $ishObjectA.ftitle_logical_value + "...updated"								
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			
			$ishObjectArray = Set-IshDocumentObj -IshSession $ishSession -IshObject @($ishObjectA, $ishObjectB) -Metadata $ishMetadataFieldsSet
			
			[xml]$xmlRevisionInfoA = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectA.LngRef), "")
			[xml]$xmlRevisionInfoB = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectB.LngRef), "")

			$ishObjectArray.Count | Should Be 2
			$xmlRevisionInfoA.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 1
			$xmlRevisionInfoB.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 1
			
			$ishObjectArray[0].ftitle_logical_value | Should Be $updatedTitle
			$ishObjectArray[1].ftitle_logical_value | Should Be $updatedTitle
		}
		
		It "Resubmit blob, provide multiple objects to IshObject" {
			$ishObjectA = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent | Get-IshDocumentObjData
			$ishObjectB = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent | Get-IshDocumentObjData
			
			$ishObjectArray = Set-IshDocumentObj -IshSession $ishSession -IshObject @($ishObjectA, $ishObjectB)
			[xml]$xmlRevisionInfoA = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectA.LngRef), "")
			[xml]$xmlRevisionInfoB = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectB.LngRef), "")

			$ishObjectArray.Count | Should Be 2
			$xmlRevisionInfoA.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
			$xmlRevisionInfoB.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
		}
		
		It "Resubmit blob, provide multiple objects via pipeline" {
			$ishObjectA = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectB = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectAB = @($ishObjectA, $ishObjectB) | Get-IshDocumentObj -IshSession $ishSession -IncludeData
			
			$ishObjectArray = $ishObjectAB | Set-IshDocumentObj -IshSession $ishSession
			[xml]$xmlRevisionInfoA = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectA.LngRef), "")
			[xml]$xmlRevisionInfoB = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectB.LngRef), "")

			$ishObjectArray.Count | Should Be 2
			$xmlRevisionInfoA.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
			$xmlRevisionInfoB.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
		}

		It "Resubmit blob, provide multiple objects via pipeline with RequiredCurrentMetadata accepted" {
			$ishObjectA = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectB = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectAB = @($ishObjectA, $ishObjectB) | Get-IshDocumentObj -IshSession $ishSession -IncludeData
			
			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
			$ishObjectArray = $ishObjectAB | Set-IshDocumentObj -IshSession $ishSession -RequiredCurrentMetadata $ishRequiredCurrentMetadata
			[xml]$xmlRevisionInfoA = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectA.LngRef), "")
			[xml]$xmlRevisionInfoB = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectB.LngRef), "")

			$ishObjectArray.Count | Should Be 2
			$xmlRevisionInfoA.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
			$xmlRevisionInfoB.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
		}
		
		It "Resubmit blob, provide multiple objects via pipeline with RequiredCurrentMetadata rejected" {
			$ishObjectA = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectB = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectAB = @($ishObjectA, $ishObjectB) | Get-IshDocumentObj -IshSession $ishSession -IncludeData
			
			$ishRequiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusReleased
			{$ishObjectAB | Set-IshDocumentObj -IshSession $ishSession -RequiredCurrentMetadata $ishRequiredCurrentMetadata} | 
						Should Throw '[-106011] The supplied expected metadata value "Released" does not match the current database value "Draft" so we rolled back your operation. To make the operation work you should make sure your value matches the latest database value. [f:158 fe:FSTATUS ft:LOV] [106011;InvalidCurrentMetadata]'
		}
		
		It "Resubmit blob and set Metadata, provide multiple objects via pipeline" {
			$ishObjectA = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectB = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderTopic -IshType ISHModule -Lng $ishLng -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
			$ishObjectAB = @($ishObjectA, $ishObjectB) | Get-IshDocumentObj -IshSession $ishSession -IncludeData
		
			$updatedTitle = $ishObjectA.ftitle_logical_value + "...updated"								
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObjectArray = $ishObjectAB | Set-IshDocumentObj -IshSession $ishSession -Metadata $ishMetadataFieldsSet
			[xml]$xmlRevisionInfoA = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectA.LngRef), "")
			[xml]$xmlRevisionInfoB = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectB.LngRef), "")

			$ishObjectArray.Count | Should Be 2
			$xmlRevisionInfoA.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
			$xmlRevisionInfoB.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
			$ishObjectArray[0].ftitle_logical_value | Should Be $updatedTitle
			$ishObjectArray[1].ftitle_logical_value | Should Be $updatedTitle
		}

		It "Resubmit blob and set Metadata, provide single object via pipeline" {
			$ishObjectToUpdate = Add-IshDocumentObj -IshSession $ishSession `
													-IshFolder $ishFolderTopic `
													-IshType ISHModule `
													-Lng $ishLng `
													-Metadata $ishTopicMetadata `
													-FileContent $ditaTopicFileContent |
								Get-IshDocumentObj -IshSession $ishSession -IncludeData		
		
			$updatedTitle = $ishObjectToUpdate.ftitle_logical_value + "...updated"								
			$ishMetadataFieldsSet = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value $updatedTitle
			$ishObject = $ishObjectToUpdate | Set-IshDocumentObj -IshSession $ishSession -Metadata $ishMetadataFieldsSet
			[xml]$xmlRevisionInfo = $ishSession.DocumentObj25.RetrieveRevisionInfoByIshLngRefs(@($ishObjectToUpdate.LngRef), "")

			$xmlRevisionInfo.SelectNodes("//ishobjects/ishobject/ishchanges/ishchange").Count | Should Be 2
			$ishObject.ftitle_logical_value | Should Be $updatedTitle
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	try { Remove-Item $tempFilePath -Force } catch { }
}
