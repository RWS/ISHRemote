Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Search-IshDocumentObj"
try {
	
	$tempFolder = [System.IO.Path]::GetTempPath()
Describe "Search-IshDocumentObjData" -Tags "Read" {
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

	Context "Search-IshDocumentObj requires server-side 'Trisoft InfoShare SolrLucene' service running" {
		It "Is 'Trisoft InfoShare SolrLucene' running?" {
			{ Search-IshDocumentObj -SimpleQuery "Testing if 'Trisoft InfoShare SolrLucene' is up-and-running" } | Should Not Throw
		}
	}

	Context "Search-IshDocumentObj SimpleQueryGroup" {
		It "Parameter IshSession invalid" {
			{ Search-IshDocumentObj -IShSession "INVALIDISHSESSION" -SimpleQuery "INVALIDQUERY IS NOT INVALID IT IS JUST A QUERY" } | Should Throw
		}
		It "Parameter MaxHitsToReturn invalid" {
			{ Search-IshDocumentObj -MaxHitsToReturn "INVALIDMAXHITSTORETURN" -SimpleQuery "INVALIDQUERY IS NOT INVALID IT IS JUST A QUERY" } | Should Throw
			{ Search-IshDocumentObj -MaxHitsToReturn -3 -SimpleQuery "INVALIDQUERY IS NOT INVALID IT IS JUST A QUERY" } | Should Throw
		}
		It "Parameter MaxHitsToReturn" {
			$ishObject = Search-IshDocumentObj -MaxHitsToReturn 3 -SimpleQuery "*"
			$ishObject.Count -eq 3 | Should Be $true 
		}
		It "Parameter RequestedMetadata" {
			$requestedMetadata = Set-IshRequestedMetadataField -Level Lng -Name FISHSTATUSTYPE
			$ishObject = Search-IshDocumentObj -IShSession $ishSession -MaxHitsToReturn 2 -SimpleQuery "*" -RequestedMetadata $requestedMetadata
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			#logical
			$ishObject[0].ftitle_logical_value.Length -ge 1 | Should Be $true 
			#version
			$ishObject[0].version_version_value.Length -ge 1 | Should Be $true 
			#language
			$ishObject[0].fstatus.Length -ge 1 | Should Be $true 
			$ishObject[0].fstatus_lng_element.StartsWith('VSTATUS') | Should Be $true 
			$ishObject[0].doclanguage.Length -ge 1 | Should Be $true  # Field names like DOC-LANGUAGE get stripped of the hyphen, otherwise you get $ishObject.'doc-language' and now you get the more readable $ishObject.doclanguage
			$ishObject[0].doclanguage_lng_element.StartsWith('VLANGUAGE') | Should Be $true
			$ishObject[0].fishstatustype -ge 0  | Should Be $true  # Basic does not return FISHSTATUSTYPE but extra RequestedMetadata parameter does
		}
		It "Parameter Count" {
			$count = Search-IshDocumentObj -SimpleQuery "*" -Count
			$count -ge 0 | Should Be $true 
		}
		It "Search-IshDocumentObj returns IshObject objects" {
			$ishObject = Search-IshDocumentObj -IShSession $ishSession -MaxHitsToReturn 2 -SimpleQuery "*"
			$ishObject[0].IshField | Should Not BeNullOrEmpty
			$ishObject[0].IshRef | Should Not BeNullOrEmpty
			$ishObject[0].IshType | Should Not BeNullOrEmpty
			$ishObject[0].ObjectRef | Should Not BeNullOrEmpty
			$ishObject[0].VersionRef | Should Not BeNullOrEmpty
			$ishObject[0].LngRef | Should Not BeNullOrEmpty
			$ishObject[0].fishstatustype -ge 0  | Should Be $false  # Basic does not return FISHSTATUSTYPE but extra RequestedMetadata parameter does
		}
	}

	Context "Search-IshDocumentObj XmlQueryQueryGroup" {
		$xmlQueryLatestVersion = @"
		<ishquery>
		  <and><ishfield name='ISHANYWHERE' level='none' ishoperator='contains'>*</ishfield></and>
		  <ishsort>
			<ishsortfield name='ISHSCORE' level='none' ishorder='d'/>
			<ishsortfield name='FTITLE' level='logical' ishorder='d'/>
		  </ishsort>
		  <ishobjectfilters>
			<ishversionfilter>LatestVersion</ishversionfilter>
			<ishtypefilter>ISHModule</ishtypefilter>
			<ishtypefilter>ISHMasterDoc</ishtypefilter>
			<ishtypefilter>ISHLibrary</ishtypefilter>
			<ishtypefilter>ISHTemplate</ishtypefilter>
			<ishtypefilter>ISHIllustration</ishtypefilter>
			<ishlanguagefilter>#!#ISHRemoteParameter:UserLanguage#!#</ishlanguagefilter>
		  </ishobjectfilters>
		</ishquery>
"@  # has to be on the margin, far-left
		$xmlQueryLatestVersion = $xmlQueryLatestVersion -replace "#!#ISHRemoteParameter:UserLanguage#!#", $ishSession.UserLanguage
		$xmlQueryAllVersions = @"
		<ishquery>
		  <and><ishfield name='ISHANYWHERE' level='none' ishoperator='contains'>*</ishfield></and>
		  <ishsort>
			<ishsortfield name='ISHSCORE' level='none' ishorder='d'/>
			<ishsortfield name='FTITLE' level='logical' ishorder='d'/>
		  </ishsort>
		  <ishobjectfilters>
			<ishversionfilter>AllVersions</ishversionfilter>
			<ishtypefilter>ISHModule</ishtypefilter>
			<ishtypefilter>ISHMasterDoc</ishtypefilter>
			<ishtypefilter>ISHLibrary</ishtypefilter>
			<ishtypefilter>ISHTemplate</ishtypefilter>
			<ishtypefilter>ISHIllustration</ishtypefilter>
			<ishlanguagefilter>#!#ISHRemoteParameter:UserLanguage#!#</ishlanguagefilter>
		  </ishobjectfilters>
		</ishquery>
"@  # has to be on the margin, far-left
		$xmlQueryAllVersions = $xmlQueryAllVersions -replace "#!#ISHRemoteParameter:UserLanguage#!#", $ishSession.UserLanguage
		It "Parameter IshSession invalid" {
			{ Search-IshDocumentObj -IShSession "INVALIDISHSESSION" -XmlQuery "INVALIDQUERY" } | Should Throw
		}
		It "Parameter XmlQuery invalid" {
			{ Search-IshDocumentObj -IShSession $ishSession -XmlQuery "INVALIDQUERY" } | Should Throw
		}
		It "Parameter MaxHitsToReturn invalid" {
			{ Search-IshDocumentObj -MaxHitsToReturn "INVALIDMAXHITSTORETURN" -XmlQuery $xmlQueryLatestVersion } | Should Throw
			{ Search-IshDocumentObj -MaxHitsToReturn -4 -XmlQuery $xmlQueryLatestVersion } | Should Throw
		}
		It "Parameter MaxHitsToReturn" {
			$ishObject = Search-IshDocumentObj -MaxHitsToReturn 4 -XmlQuery $xmlQueryLatestVersion
			$ishObject.Count -eq 4 | Should Be $true 
		}
		It "Parameter RequestedMetadata" {
			$requestedMetadata = Set-IshRequestedMetadataField -Level Lng -Name FISHSTATUSTYPE
			$ishObject = Search-IshDocumentObj -IShSession $ishSession -MaxHitsToReturn 2 -XmlQuery $xmlQueryLatestVersion -RequestedMetadata $requestedMetadata
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			#logical
			$ishObject[0].ftitle_logical_value.Length -ge 1 | Should Be $true 
			#version
			$ishObject[0].version_version_value.Length -ge 1 | Should Be $true 
			#language
			$ishObject[0].fstatus.Length -ge 1 | Should Be $true 
			$ishObject[0].fstatus_lng_element.StartsWith('VSTATUS') | Should Be $true 
			$ishObject[0].doclanguage.Length -ge 1 | Should Be $true  # Field names like DOC-LANGUAGE get stripped of the hyphen, otherwise you get $ishObject.'doc-language' and now you get the more readable $ishObject.doclanguage
			$ishObject[0].doclanguage_lng_element.StartsWith('VLANGUAGE') | Should Be $true
			$ishObject[0].fishstatustype -ge 0  | Should Be $true  # Basic does not return FISHSTATUSTYPE but extra RequestedMetadata parameter does
		}
		It "Parameter Count" {
			$count = Search-IshDocumentObj -XmlQuery $xmlQueryAllVersions -Count
			$count -ge 0 | Should Be $true 
		}
		It "Search-IshDocumentObj returns IshObject objects" {
			$ishObject = Search-IshDocumentObj -IShSession $ishSession -MaxHitsToReturn 2 -XmlQuery $xmlQueryLatestVersion
			$ishObject[0].IshField | Should Not BeNullOrEmpty
			$ishObject[0].IshRef | Should Not BeNullOrEmpty
			$ishObject[0].IshType | Should Not BeNullOrEmpty
			$ishObject[0].ObjectRef | Should Not BeNullOrEmpty
			$ishObject[0].VersionRef | Should Not BeNullOrEmpty
			$ishObject[0].LngRef | Should Not BeNullOrEmpty
			$ishObject[0].fishstatustype -ge 0  | Should Be $false  # Basic does not return FISHSTATUSTYPE but extra RequestedMetadata parameter does
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$folderCmdletRootPath = (Join-Path $folderTestRootPath $cmdletName)
	try { Get-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse | Get-IshFolderContent -IshSession $ishSession | Remove-IshDocumentObj -IshSession $ishSession -Force } catch { }
	try { Remove-IshFolder -IshSession $ishSession -FolderPath $folderCmdletRootPath -Recurse } catch { }
}
