BeforeAll {
	$cmdletName = "Register-IshRemoteMcpTool"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	function Write-IshRemoteLog {
    param(
        [Parameter(Mandatory = $true)]        
        [object]$LogEntry
    )
	}

}

Describe "Register-IshRemoteMcpTool" -Tags "Read" -Skip:($PSVersionTable.PSVersion.Major -lt 7) {
	Context "Register-IshRemoteMcpTool (beware Get-Help is empty if Get-InstalledPSResource still shows ISHRemote installed)" {
		BeforeEach{
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
        }
		It "Full Load of Get-IshFolder cmdlet" {
			$resultJson = Register-IshRemoteMcpTool -FunctionNameFullLoad @('Get-IshFolder') -FunctionNamePartialLoad @() | ConvertFrom-Json
			$expectedJson = 
@"
{"name":"Get-IshFolder","description":"The Get-IshFolder cmdlet retrieves metadata for the folders by providing one of the following input data: - FolderPath string with the separated full folder path - FolderIds array containing identifiers of the folders - BaseFolder enum value referencing the specified root folder - IshFolder[] array passed through the pipeline Query and Reference folders are not supported.\n\nThis PowerShell cmdlet has the following parameter sets to choose form where square brackets indicate optional parameters while the other parameters are mandatory:\nGet-IshFolder -FolderId <long> [-Depth <int>] [-FolderTypeFilter {ISHNone | ISHModule | ISHMasterDoc | ISHLibrary | ISHTemplate | ISHIllustration | ISHPublication | ISHReference | ISHQuery}] [-IshSession <IshSession>] [-Recurse <SwitchParameter>] [-RequestedMetadata <IshField[]>] [<CommonParameters>]\r\n\r\nGet-IshFolder -FolderPath <string> [-Depth <int>] [-FolderTypeFilter {ISHNone | ISHModule | ISHMasterDoc | ISHLibrary | ISHTemplate | ISHIllustration | ISHPublication | ISHReference | ISHQuery}] [-IshSession <IshSession>] [-Recurse <SwitchParameter>] [-RequestedMetadata <IshField[]>] [<CommonParameters>]\r\n\r\nGet-IshFolder -IshFolder <IshFolder[]> [-Depth <int>] [-FolderTypeFilter {ISHNone | ISHModule | ISHMasterDoc | ISHLibrary | ISHTemplate | ISHIllustration | ISHPublication | ISHReference | ISHQuery}] [-IshSession <IshSession>] [-Recurse <SwitchParameter>] [-RequestedMetadata <IshField[]>] [<CommonParameters>]\r\n\r\nGet-IshFolder -BaseFolder {Data | System | Favorites | EditorTemplate} [-Depth <int>] [-FolderTypeFilter {ISHNone | ISHModule | ISHMasterDoc | ISHLibrary | ISHTemplate | ISHIllustration | ISHPublication | ISHReference | ISHQuery}] [-IshSession <IshSession>] [-Recurse <SwitchParameter>] [-RequestedMetadata <IshField[]>] [<CommonParameters>]\n\nThe PowerShell cmdlet has the following examples as inspiration:\n----------  EXAMPLE 1  ----------\r\n\r\n$ishSession = New-IshSession -WsBaseUrl \"https://example.com/ISHWS/\" -PSCredential Admin\r\nGet-IshFolder -FolderPath \"\\General\\__ISHRemote\\Add-IshPublicationOutput\\Pub\"\r\n\r\nNew-IshSession will submit into SessionState, so it can be reused by this cmdlet. Returns the IshFolder object.\r\n----------  EXAMPLE 2  ----------\r\n\r\n$ishSession = New-IshSession -WsBaseUrl \"https://example.com/ISHWS/\" -PSCredential Admin\r\n(Get-IshFolder -BaseFolder Data).name\r\n\r\nNew-IshSession will submit into SessionState, so it can be reused by this cmdlet. Returns the name of the root data folder, typically called 'General'.\r\n----------  EXAMPLE 3  ----------\r\n\r\n$ishSession = New-IshSession -WsBaseUrl \"https://example.com/ISHWS/\" -PSCredential \"Admin\"\r\n$requestedMetadata = Set-IshMetadataFilterField -Name \"FNAME\" -Level \"None\"\r\n$folderId = 7598 # provide a real folder identifier\r\n$ishFolder = Get-IshFolder -FolderId $folderId -RequestedMetaData $requestedMetadata\r\n$retrievedFolderName = $ishFolder.name\r\n\r\nGet folder name using Id with explicit requested metadata\r\n----------  EXAMPLE 4  ----------\r\n\r\n$ishSession = New-IshSession -WsBaseUrl \"https://example.com/ISHWS/\" -PSCredential \"Admin\"\r\n$ishFolders = Get-IshFolder -FolderPath \"General\\Myfolder\" -FolderTypeFilter @(\"ISHModule\", \"ISHMasterDoc\", \"ISHLibrary\") -Recurse\r\n\r\nGet folders recursively with filtering on folder type\r\n----------  EXAMPLE 5  ----------\r\n\r\nNew-IshSession -WsBaseUrl \"https://example.com/ISHWS/\"\r\n$imageCount = 0\r\n$xmlCount = 0\r\nGet-IshFolder -FolderPath \"General\\Myfolder\" -FolderTypeFilter @(\"ISHIllustration\", \"ISHModule\", \"ISHMasterDoc\", \"ISHLibrary\") -Recurse | \r\nGet-IshFolderContent -VersionFilter \"\" | \r\nForEach-Object -Process { \r\n  if ($_.IshType -in @(\"ISHIllustration\")) { ++$imageCount }\r\n  if ($_.IshType -in @(\"ISHModule\", \"ISHMasterDoc\", \"ISHLibrary\")) { ++$xmlCount }\r\n}\r\nWrite-Host (\"imageCount[\"+$imageCount+\"]\")\r\nWrite-Host (\"xmlCount[\"+$xmlCount+\"]\")\r\n\r\nVarious statistics can be gathered by crawling across many API calls. This sample recursively goes over some subfolder, and retrieves all content objects in the folder and aggregates to a rough count. The ForEach-Object construct is important as it only keeps the essence, the counters, and avoids keeping all objects retrieved over the API in memory - potentially running out of client-side/PowerShell memory.\r\n----------  EXAMPLE 6  ----------\r\n\r\nNew-IshSession -WsBaseUrl \"https://example.com/ISHWS/\"\r\n$metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FISHPUBSTATUS -ValueType Element -FilterOperator In -Value VPUBSTATUSUNPUBLISHFAILED\r\n$ishObjects = Get-IshFolder -BaseFolder Data -FolderTypeFilter @(\"ISHPublication\") -Recurse | \r\n              Get-IshFolderContent -IshFolder $ishFolders -VersionFilter LATEST -LanguagesFilter ('en-US','de-DE') -MetadataFilter $metadataFilter\r\n\r\nThis Get-IshFolder iteratively loops your repository folder structure and only passes Publication folders to the next cmdlet. Then Get-IshFolderContent cmdlet only retrieves LATEST versions of Publications with a languages filter and metadata filter.","annotations":{"type":"object","destructiveHint":"false","idempotentHint":"true","readOnlyHint":"true"},"inputSchema":{"type":"object","properties":{"BaseFolder":{"type":"string","description":"The BaseFolder enumeration to get subfolders for the specified root folder\r\n\r\n\r\nPossible values: Data, System, Favorites, EditorTemplate"},"Depth":{"type":"number","description":"Perform recursive retrieval of up to Depth of the provided incoming folder(s)"},"FolderId":{"type":"number","description":"Unique folder identifier"},"FolderPath":{"type":"string","description":"Separated string with the full folder path, e.g. \"General\\Project\\Topics\""},"FolderTypeFilter":{"type":"string","description":"Recursive retrieval will loop all folder, this filter will only return folder matching the filter to the pipeline\r\n\r\n\r\nPossible values: ISHNone, ISHModule, ISHMasterDoc, ISHLibrary, ISHTemplate, ISHIllustration, ISHPublication, ISHReference, ISHQuery"},"IshFolder":{"type":"string","description":"Folders for which to retrieve the metadata. This array can be passed through the pipeline or explicitly passed via the parameter."},"IshSession":{"type":"string","description":"The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet."},"Recurse":{"type":"boolean","description":"Perform recursive retrieval of the provided incoming folder(s)"},"RequestedMetadata":{"type":"string","description":"The metadata fields to retrieve"}},"required":[]},"returns":{"type":"string","description":"Get-IshFolder"}}
"@ | ConvertFrom-Json
			#NAME
			$resultJson.name | Should -Be $expectedJson.name
			#DESCRIPTION or SYNOPSIS
			$resultJson.description -like 'The Get-IshFolder cmdlet retrieves metadata for the folders by providing one of the following input data*' | Should -Be $true
			#EXAMPLES
			$resultJson.description -like '*PowerShell cmdlet has the following examples as inspiration*' | Should -Be $true
			#SYNTAX or PARAMETER SETS
			$resultJson.description -like '*This PowerShell cmdlet has the following parameter sets to choose form where square brackets indicate optional parameters while the other parameters are mandatory*' | Should -Be $true
			#PARAMETERS
			$resultJson.inputSchema.properties.IShSession.type | Should -Be $expectedJson.inputSchema.properties.IShSession.type
			$resultJson.inputSchema.properties.IShSession.description | Should -Be $expectedJson.inputSchema.properties.IShSession.description
			#OUTPUT
			$resultJson.returns.type | Should -Be $expectedJson.returns.type
		}
		It "Partial Load of Get-IshFolder cmdlet" {
			$resultJson = Register-IshRemoteMcpTool -FunctionNameFullLoad @() -FunctionNamePartialLoad @('Get-IshFolder') | ConvertFrom-Json
			$expectedJson =
@"
{"name":"Get-IshFolder","description":"The Get-IshFolder cmdlet retrieves metadata for the folders by providing one of the following input data: - FolderPath string with the separated full folder path - FolderIds array containing identifiers of the folders - BaseFolder enum value referencing the specified root folder - IshFolder[] array passed through the pipeline Query and Reference folders are not supported.","annotations":{"type":"object","destructiveHint":"false","idempotentHint":"true","readOnlyHint":"true"},"inputSchema":{"type":"object","properties":{},"required":[]},"returns":{"type":"string","description":"Get-IshFolder"}}
"@ | ConvertFrom-Json
#NAME
			$resultJson.name | Should -Be $expectedJson.name
			#DESCRIPTION or SYNOPSIS
			$resultJson.description -like 'The Get-IshFolder cmdlet retrieves metadata for the folders by providing one of the following input data*' | Should -Be $true
			#EXAMPLES
			$resultJson.description -notlike '*PowerShell cmdlet has the following examples as inspiration*' | Should -Be $true
			#SYNTAX or PARAMETER SETS
			$resultJson.description -notlike '*This PowerShell cmdlet has the following parameter sets to choose form where square brackets indicate optional parameters while the other parameters are mandatory*' | Should -Be $true
			#PARAMETERS
			$resultJson.inputSchema.properties.ToString().Length | Should -Be 0
			#OUTPUT
			$resultJson.returns.type | Should -Be $expectedJson.returns.type
		}
		It "Full Load of all ISHRemote cmdlets (>100k characters like 288225)" {
			$resultString = Register-IshRemoteMcpTool -FunctionNameFullLoad (Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name
			$resultString.Length -gt 100000 | Should -Be $true
		}
		It "Partial Load of all ISHRemote cmdlets (<100k characters like 44603)" {
			$resultString = Register-IshRemoteMcpTool -FunctionNamePartialLoad (Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name
			$resultString.Length -gt 10000 | Should -Be $true
			$resultString.Length -lt 100000 | Should -Be $true
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}



