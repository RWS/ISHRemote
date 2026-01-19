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

Describe "Register-IshRemoteMcpTool" -Tags "Read" {
	Context "Register-IshRemoteMcpTool" {
		BeforeEach{
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
        }
		It "Get-IshFolder" {
			$resultJson = Register-IshRemoteMcpTool -FunctionName Get-IshFolder | ConvertFrom-Json
			$expectedJson = '{"name":"Get-IshFolder","description":"Get-IshFolder -FolderId <long> [-IshSession <IshSession>] [-RequestedMetadata <IshField[]>] [-Recurse] [-Depth <int>] [-FolderTypeFilter <Enumerations+IshFolderType[]>] [<CommonParameters>]\r\n\r\nGet-IshFolder -FolderPath <string> [-IshSession <IshSession>] [-RequestedMetadata <IshField[]>] [-Recurse] [-Depth <int>] [-FolderTypeFilter <Enumerations+IshFolderType[]>] [<CommonParameters>]\r\n\r\nGet-IshFolder -IshFolder <IshFolder[]> [-IshSession <IshSession>] [-RequestedMetadata <IshField[]>] [-Recurse] [-Depth <int>] [-FolderTypeFilter <Enumerations+IshFolderType[]>] [<CommonParameters>]\r\n\r\nGet-IshFolder -BaseFolder <Enumerations+BaseFolder> [-IshSession <IshSession>] [-RequestedMetadata <IshField[]>] [-Recurse] [-Depth <int>] [-FolderTypeFilter <Enumerations+IshFolderType[]>] [<CommonParameters>]\n\nThis PowerShell cmdlet has the following parameter sets to choose form where square brackets indicate optional parameters while the other parameters are mandatory:\nsyntaxItem\r\n----------\r\n{@{name=Get-IshFolder; CommonParameters=True; parameter=System.Object[]}, @{name=Get-IshFolder; CommonParameters=True; parameter=System.Object[]}, @{n…\n\nThe PowerShell cmdlet has the following examples as inspiration:\n","annotations":{"type":"object","destructiveHint":"false","idempotentHint":"true","readOnlyHint":"true"},"inputSchema":{"type":"object","properties":{"BaseFolder":{"type":"string","description":"No description available for this parameter."},"Depth":{"type":"number","description":"No description available for this parameter."},"FolderId":{"type":"number","description":"No description available for this parameter."},"FolderPath":{"type":"string","description":"No description available for this parameter."},"FolderTypeFilter":{"type":"string","description":"No description available for this parameter."},"IshFolder":{"type":"string","description":"No description available for this parameter."},"IshSession":{"type":"string","description":"No description available for this parameter."},"Recurse":{"type":"boolean","description":"No description available for this parameter."},"RequestedMetadata":{"type":"string","description":"No description available for this parameter."}},"required":[]},"returns":{"type":"string","description":"Get-IshFolder"}}' | ConvertFrom-Json
			$resultJson.name | Should -Be $expectedJson.name
			$resultJson.returns.type | Should -Be $expectedJson.returns.type
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}

