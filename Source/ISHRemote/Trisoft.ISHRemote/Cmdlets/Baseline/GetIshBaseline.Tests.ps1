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
		It "Parameter IshSession explicit" {
			$ishObject = Get-IshBaseline -IShSession $ishSession -Id $baselineId -RequestedMetadata $requestedMetadata -MetadataFilter $metadataFilter
			$ishObject.GetType().Name | Should BeExactly "IshBaseline"
			$ishObject.IshRef -ge 0 | Should Be $true
			$ishObject.IshType | Should Not BeNullOrEmpty
			$ishObject.IshField | Should Not BeNullOrEmpty
		}
		It "Parameter IshSession/RequestedMetadata implicit" {
			$ishObject = Get-IshBaseline -Id $baselineId -MetadataFilter $metadataFilter
			$ishObject.GetType().Name | Should BeExactly "IshBaseline"
			$ishObject.IshRef -ge 0 | Should Be $true
			$ishObject.IshType | Should Not BeNullOrEmpty
			$ishObject.IshField | Should Not BeNullOrEmpty
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			$ishObject.fishdocumentrelease.Length -ge 1 | Should Be $true 
			$ishObject.fishdocumentrelease_none_element.StartsWith('GUID') | Should Be $true 
		}
	}

	Context "Get-IshBaseline IshObjectsGroup" {
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
		$ishObjectA = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
		$ishObjectB = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
		$ishObjectC = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
		$ishObjectD = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		It "Parameter IshObject invalid" {
			{ Get-IshBaseline -IShSession $ishSession -IshObject "INVALIDBASELINE" } | Should Throw
		}
		It "Parameter IshObject Single" {
			(Get-IshBaseline -IshSession $ishSession -IshObject $ishObjectA).IshRef.Length -ge 0 | Should Be $true
		}
		It "Parameter IshObject Multiple" {
			(Get-IshBaseline -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)).Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$ishObjects = $ishObjectD | Get-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishSession.MetadataBatchSize = 2
			$ishObjects = @($ishObjectA,$ishObjectB,$ishObjectC,$ishObjectD) | Get-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 4
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshPublicationOutput -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}
