BeforeAll {
    $cmdletName = "Remove-IshPublicationOutput"
    Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

    Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Remove-IshPublicationOutput" -Tags "Delete" {
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
        $ishFolderPub = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHPublication -FolderName "Publication" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

        $ishPublicationOutputMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Pub $cmdletName $timestamp" |
                            Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
                            Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
        $ishObjectPubA = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPublicationOutputMetadata
    }

    Context "Remove-IshPublicationOutput check exceptions" {
        It "-LogicalId does not exist. Force=Yes" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId "NON-EXISTING-LOGICAL-ID" -Version "1" -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Force} | Should -Throw
        }    
        It "-LogicalId does not exist" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId "NON-EXISTING-LOGICAL-ID" -Version "1" -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml} | Should -Throw
        }
        It "-Version does not exist. Force=Yes" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId $ishObjectPubA.IshRef -Version "999" -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Force} | Should -Throw
        }    
        It "-Version does not exist" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId $ishObjectPubA.IshRef -Version "999" -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml} | Should -Throw
        }
        It "-LanguageCombination does not exist. Force=Yes" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId $ishObjectPubA.IshRef -Version "1" -LanguageCombination "NON-EXISTING-LANGUAGE-COMBINATION" -OutputFormat $ishOutputFormatDitaXml -Force} | Should -Throw
        }    
        It "-LanguageCombination does not exist" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId $ishObjectPubA.IshRef -Version "1" -LanguageCombination "NON-EXISTING-LANGUAGE-COMBINATION" -OutputFormat $ishOutputFormatDitaXml} | Should -Throw
        }
        It "-OutputFormat does not exist. Force=Yes" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId $ishObjectPubA.IshRef -Version "999" -LanguageCombination $ishLngCombination -OutputFormat "NON-EXISTING-OUTPUT-FORMAT" -Force} | Should -Throw
        }    
        It "-OutputFormat does not exist" {
            {Remove-IshPublicationOutput -IshSession $ishSession -LogicalId $ishObjectPubA.IshRef -Version "999" -LanguageCombination $ishLngCombination -OutputFormat "NON-EXISTING-OUTPUT-FORMAT"} | Should -Throw
        }
    }

    Context "Remove-IshPublicationOutput remove PublicationOutput" {
        It "Logical level. Force=Yes" {
            $ishPubMetadata = $ishPublicationOutputMetadata | Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "$($____Pester.CurrentTest.Name) $timestamp"
            $ishPub = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPubMetadata
            Remove-IshPublicationOutput -IshSession $ishSession -LogicalId $ishPub.IshRef -Version $ishPub.version_version_value -LanguageCombination $ishPub.fishpublngcombination -OutputFormat $ishPub.fishoutputformatref_lng_element -Force
            $requestedMetadataRetrieve = Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FTITLE' -Level Logical
            $publicationOutput = Get-IshPublicationOutput -IshSession $ishSession -LogicalId $ishPub.IshRef -RequestedMetadata $requestedMetadataRetrieve
            $publicationOutput.length -eq 0 | Should -Be $true
        }
    }
}

AfterAll {
    Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
    $folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
    try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { Write-Host "An error occurred: $_" }
    try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { Write-Host "An error occurred: $_" }
}

