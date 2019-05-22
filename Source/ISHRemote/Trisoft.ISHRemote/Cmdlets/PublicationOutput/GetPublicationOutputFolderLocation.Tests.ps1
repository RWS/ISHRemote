Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshPublicationOutputFolderLocation"
try {

Describe “Get-IshPublicationOutputFolderLocation" -Tags "Read" {
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

	$global:ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType $folderTypeTestRootOriginal -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	$ishFolderPub = Add-IshFolder -IshSession $ishSession -ParentFolderId ($global:ishFolderCmdlet.IshFolderRef) -FolderType ISHPublication -FolderName "Publication" -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal

	$ishPublicationOutputMetadata = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -Value "Pub $timestamp" |
	                    Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
	                    Set-IshMetadataField -IshSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution
	$ishObjectPubA = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPublicationOutputMetadata
	$ishObjectPubB = Add-IshPublicationOutput -IshSession $ishSession -IshFolder $ishFolderPub -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $ishPublicationOutputMetadata

	Context "Get-IshPublicationOutputFolderLocation ParameterGroup" {
		It "Parameter IshSession/LogicalId invalid" {
			{ Get-IshPublicationOutputFolderLocation -IShSession "INVALIDISHSESSION" -LogicalId "INVALIDLOGICALID" } | Should Throw
		}
		$folderPath = Get-IshPublicationOutputFolderLocation -IshSession $ishSession -LogicalId $ishObjectPubA.IshRef
		It "GetType().Name" {
			$folderPath.GetType().Name | Should BeExactly "String"
		}
		It "Parameter LogicalId Single" {
			$folderPath | Should BeExactly (Join-Path (Join-Path $folderTestRootPath $cmdletName) "Publication")
		}
		It "Leading IshSession.FolderPathSeparator" {
			$folderPath[0] | Should Be $ishSession.FolderPathSeparator
		}
	}

	Context "Get-IshPublicationOutputFolderLocation IshObjectGroup" {
		It "GetType().Name" {
			$folderPathArray = Get-IshPublicationOutputFolderLocation -IshSession $ishSession -IshObject @($ishObjectPubA,$ishObjectPubB)
			$folderPathArray.GetType().Name | Should BeExactly "Object[]"
		}
		It "Parameter IshObject Single with implicit IshSession" {
			$folderPathArray = Get-IshPublicationOutputFolderLocation -IshObject $ishObjectPubA
			$folderPathArray.Count | Should Be 1
		}
		It "Parameter IshObject Multiple with implicit IshSession" {
			$folderPathArray = Get-IshPublicationOutputFolderLocation -IshObject @($ishObjectPubA,$ishObjectPubB)
			$folderPathArray.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$folderPathArray = $ishObjectPubA | Get-IshPublicationOutputFolderLocation -IshSession $ishSession
			$folderPathArray.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$folderPathArray = @($ishObjectPubA,$ishObjectPubB) | Get-IshPublicationOutputFolderLocation -IshSession $ishSession
			$folderPathArray.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
