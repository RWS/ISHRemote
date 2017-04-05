Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshBaseline"
try {

Describe "Get-IshBaseline" -Tags "Read" {
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
	$readAccessTestRootOriginal = (Get-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element -IshField $ishFolderTestRootOriginal.IshField).Split($ishSession.Seperator)

	$ishFolderCmdlet = Add-IshFolder -IShSession $ishSession -ParentFolderId $folderIdTestRootOriginal -FolderType ISHPublication -FolderName $cmdletName -OwnedBy $ownedByTestRootOriginal -ReadAccess $readAccessTestRootOriginal
	
						 
	Context "Get-IshBaseline ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshBaseline -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Get-IshBaseline returns IshBaseline object" {
		# Creating a publicationoutput, which implicitly creates a fresh baseline
		$metadata = Set-IshMetadataField -IShSession $ishSession -Name "FTITLE" -Level Logical -Value "$cmdletName Pub" |
		Set-IshMetadataField -IShSession $ishSession -Name "FISHPUBSOURCELANGUAGES" -Level Version -ValueType Element -Value $ishLng |
		Set-IshMetadataField -IShSession $ishSession -Name "FISHREQUIREDRESOLUTIONS" -Level Version -ValueType Element -Value $ishResolution |
		Set-IshMetadataField -IShSession $ishSession -Name "FISHPUBSTATUS" -Level Lng -ValueType Element -Value "VPUBSTATUSTOBEPUBLISHED"
		$ishPublicationOutput = Add-IshPublicationOutput -IshSession $ishSession -FolderId $ishFolderCmdlet.IshFolderRef -LanguageCombination $ishLngCombination -OutputFormat $ishOutputFormatDitaXml -Metadata $metadata
		$baselineId =  $ishPublicationOutput | 
		               Get-IshPublicationOutput -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHBASELINE" -Level Version -ValueType Element) |
					   Get-IshMetadataField -IshSession $ishSession -Name "FISHBASELINE" -Level Version -ValueType Element
					   
		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME" |
							 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHBASELINEACTIVE" |
							 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -ValueType Element |
							 Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHLABELRELEASED" -ValueType Element
		$metadataFilter = Set-IshMetadataFilterField -IShSession $ishSession -Name "NAME" -Level None -FilterOperator Like -Value "%"
		$ishObject = Get-IshBaseline -IShSession $ishSession -Id $baselineId -RequestedMetadata $requestedMetadata -MetadataFilter $metadataFilter
		It "GetType()" {
			$ishObject.GetType().Name | Should BeExactly "IshObject"
		}
		It "$ishObject.IshRef" {
			$ishObject.IshRef -ge 0 | Should Be $true
		}
		It "$ishObject.IshType" {
			$ishObject.IshType | Should Not BeNullOrEmpty
		}
		It "$ishObject.IshField" {
			$ishObject.IshField | Should Not BeNullOrEmpty
		}
	}

	<#
	Context "Get-IshBaseline IshObjectsGroup" {
		# TODO [Could] When Add-IshBaseline is available, extend pipeline test by creating baselines here
		It "Parameter IshObject invalid" {
			{ Get-IshBaseline -IShSession $ishSession -IshObject "INVALIDBASELINE" } | Should Throw
		}
		It "Parameter IshObject Single" {
			$ishObjects = Add-IshBaseline -IshSession $ishSession -IshObject $ishObjectA
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple" {
			$ishObjects = Add-IshBaseline -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$ishObjects = $ishObjectD | Add-IshBaseline -IshSession $ishSession
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishObjects = @($ishObjectE,$ishObjectF) | Add-IshBaseline -IshSession $ishSession
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
	}
	#>
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
