Write-Host("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path(Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshPublicationOutput"
try {

Describe “Add-IshPublicationOutput" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
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

	$global:ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	
    #$ishFolderTopic = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHModule -FolderName "Topic" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
    #$ishFolderLib = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHLibrary -FolderName "Library" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	#$ishFolderImage = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHIllustration -FolderName "Image" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	#$ishFolderOther = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHTemplate -FolderName "Other" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
    $ishFolderMap = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHMasterDoc -FolderName "Map" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishMapMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Map $timestamp" |
				      Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -ValueType Element -Value $ishUserAuthor |
	                  Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level Lng -ValueType Element -Value $ishStatusDraft
	$ishObjectMap = Add-IshDocumentObj -IshSession $ishSession -IshFolder $ishFolderMap -IshType ISHMasterDoc -LogicalId "MYOWNGENERATEDLOGICALIDMAP" -Version '3' -Lng $ishLng -Metadata $ishMapMetadata -Edt "EDTXML" -FileContent $ditaMapFileContent

    $ishFolderPub = Add-IshFolder -IshSession $ishSession -ParentFolderId($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHPublication -FolderName "Pub" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
    $ishPubMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "All Parameters Pub $timestamp" |
				      Set-IshMetadataField -IshSession $ishSession -Name "FISHMASTERREF" -Level Version -ValueType Element -Value $ishObjectMap.IshRef |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
                      Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
	$ishObjectPub = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -LogicalId "MYOWNGENERATEDLOGICALIDPUB" -Version '1' -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata

	$tempFilePath = (New-TemporaryFile).FullName

    Context "Add-IshPublicationOutput returns IshObject object" {
		
        It "GetType().Name" {
			$ishObjectPub.GetType().Name | Should BeExactly "IshPublicationOutput"
		}
		It "ishObjectPub.IshField" {
			$ishObjectPub.IshField | Should Not BeNullOrEmpty
		}
		It "ishObjectPub.IshRef" {
			$ishObjectPub.IshRef | Should Not BeNullOrEmpty
		}
		It "ishObjectPub.IshType" {
			$ishObjectPub.IshType | Should Not BeNullOrEmpty
		}
		# Double check following 3 ReferenceType enum usage 
		It "ishObjectPub.ObjectRef" {
			$ishObjectPub.ObjectRef | Should Not BeNullOrEmpty
		}
		It "ishObjectPub.VersionRef" {
			$ishObjectPub.VersionRef | Should Not BeNullOrEmpty
		}
		It "ishObjectPub.LngRef" {
			$ishObjectPub.LngRef | Should Not BeNullOrEmpty
		}
		It "ishObjectPub ConvertTo-Json" {
			(ConvertTo-Json $ishObjectPub).Length -gt 2 | Should Be $true
		}
		It "Option IshSession.DefaultRequestedMetadata" {
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			#logical
			$ishObjectPub.ftitle_logical_value.Length -ge 1 | Should Be $true 
			#version
			$ishObjectPub.version_version_value.Length -ge 1 | Should Be $true 
			#language
			$ishObjectPub.fishpubstatus.Length -ge 1 | Should Be $true 
			$ishObjectPub.fishpubstatus_lng_element.StartsWith('VPUBSTATUS') | Should Be $true 
			$ishObjectPub.fishpublngcombination.Length -ge 1 | Should Be $true  # Field names like DOC-LANGUAGE get stripped of the hyphen, otherwise you get $ishObject.'doc-language' and now you get the more readable $ishObject.doclanguage
			#$ishObjectPub.fishpublngcombination_lng_element.StartsWith('VLANGUAGE') | Should Be $true # Note that fishpublngcombination is a string like 'en+fr+nl' so doesn't have element name
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
    try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Where-Object -Property IshFolderType -EQ -Value "ISHPublication" | Get-IshFolderContent -IshSession $ishSession | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	try { Remove-Item $tempFilePath -Force } catch { }
}
