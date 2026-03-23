BeforeAll {
    $cmdletName = "Invoke-IshRemoteMcpHandleRequest"
    Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
    	
	function Write-IshRemoteLog {
    param(
        [Parameter(Mandatory = $true)]        
        [object]$LogEntry
    )
	}

}

Describe "Invoke-IshRemoteMcpHandleRequest" -Skip:($PSVersionTable.PSVersion.Major -lt 7){
    Context "Method Initialize" {
        BeforeAll {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
            $request = [PSCustomObject]@{
                id = 1
                method = "initialize"
                params = @{}
            }
            $toolsJson = '[]'
            $resourcesJson = '[]'
            $instructionsJson = '"test instructions"'
            $result = Invoke-IshRemoteMcpHandleRequest -Request $request -ToolsListJson $toolsJson -ResourcesListJson $resourcesJson -InstructionsJson $instructionsJson
        }
        It "Should return valid JSON" {
            { $result | ConvertFrom-Json } | Should -Not -Throw
        }
        It "Should return jsonrpc 2.0" {
            $json = $result | ConvertFrom-Json
            $json.jsonrpc | Should -Be "2.0"
        }
        It "Should return matching request id" {
            $json = $result | ConvertFrom-Json
            $json.id | Should -Be 1
        }
        It "Should contain protocolVersion" {
            $json = $result | ConvertFrom-Json
            $json.result.protocolVersion | Should -Be "2024-11-05"
        }
        It "Should contain capabilities" {
            $json = $result | ConvertFrom-Json
            $json.result.capabilities | Should -Not -BeNullOrEmpty
        }
        It "Should contain instructions" {
            $json = $result | ConvertFrom-Json
            $json.result.instructions | Should -Be "test instructions"
        }
        It "Should contain serverInfo" {
            $json = $result | ConvertFrom-Json
            $json.result.serverInfo.name | Should -Not -BeNullOrEmpty
            $json.result.serverInfo.version | Should -Not -BeNullOrEmpty
        }
    }

    Context "Method Ping" {
        BeforeAll {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
            $request = [PSCustomObject]@{
                id = 2
                method = "ping"
                params = @{}
            }
            $result = Invoke-IshRemoteMcpHandleRequest -Request $request -ToolsListJson '[]' -ResourcesListJson '[]' -InstructionsJson '""'
        }
        It "Should return valid JSON" {
            { $result | ConvertFrom-Json } | Should -Not -Throw
        }
        It "Should return jsonrpc 2.0" {
            $json = $result | ConvertFrom-Json
            $json.jsonrpc | Should -Be "2.0"
        }
        It "Should return matching request id" {
            $json = $result | ConvertFrom-Json
            $json.id | Should -Be 2
        }
        It "Should return empty result" {
            $json = $result | ConvertFrom-Json
            $json.result | Should -BeNullOrEmpty
        }
    }

    Context "Method Tools/List" {
        BeforeAll {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
            $request = [PSCustomObject]@{
                id = 3
                method = "tools/list"
                params = @{}
            }
            $toolsJson = '[{"name":"Get-IshFolder","description":"Test tool"}]'
            $result = Invoke-IshRemoteMcpHandleRequest -Request $request -ToolsListJson $toolsJson -ResourcesListJson '[]' -InstructionsJson '""'
        }
        It "Should return valid JSON" {
            { $result | ConvertFrom-Json } | Should -Not -Throw
        }
        It "Should return jsonrpc 2.0" {
            $json = $result | ConvertFrom-Json
            $json.jsonrpc | Should -Be "2.0"
        }
        It "Should return matching request id" {
            $json = $result | ConvertFrom-Json
            $json.id | Should -Be 3
        }
        It "Should contain tools list" {
            $json = $result | ConvertFrom-Json
            $json.result.tools | Should -Not -BeNullOrEmpty
        }
    }

    Context "Method Tools/Call" {
        BeforeAll {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
            $request = [PSCustomObject]@{
                id = 4
                method = "tools/call"
                params = @{
                    name = "Get-Date"
                    arguments = @{
                        Format = "yyyy-MM-dd"
                    }
                }
            }
            $result = Invoke-IshRemoteMcpHandleRequest -Request $request -ToolsListJson '[]' -ResourcesListJson '[]' -InstructionsJson '""'
        }
        It "Should return valid JSON" {
            { $result | ConvertFrom-Json } | Should -Not -Throw
        }
        It "Should return jsonrpc 2.0" {
            $json = $result | ConvertFrom-Json
            $json.jsonrpc | Should -Be "2.0"
        }
        It "Should return matching request id" {
            $json = $result | ConvertFrom-Json
            $json.id | Should -Be 4
        }
        It "Should contain result content" {
            $json = $result | ConvertFrom-Json
            $json.result.content | Should -Not -BeNullOrEmpty
        }
        It "Should have content type text" {
            $json = $result | ConvertFrom-Json
            $json.result.content[0].type | Should -Be "text"
        }
        It "Should have isError false" {
            $json = $result | ConvertFrom-Json
            $json.result.isError | Should -Be $false
        }
    }

    Context "Method Tools/Call  with Error" {
        BeforeAll {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
            $request = [PSCustomObject]@{
                id = 5
                method = "tools/call"
                params = @{
                    name = "Non-ExistentCommand"
                    arguments = @{}
                }
            }
            $result = Invoke-IshRemoteMcpHandleRequest -Request $request -ToolsListJson '[]' -ResourcesListJson '[]' -InstructionsJson '""'
        }
        It "Should return valid JSON" {
            { $result | ConvertFrom-Json } | Should -Not -Throw
        }
    }

    Context "Unknown Method" {
        BeforeAll {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
            $request = [PSCustomObject]@{
                id = 6
                method = "unknown/method"
                params = @{}
            }
            $result = Invoke-IshRemoteMcpHandleRequest -Request $request -ToolsListJson '[]' -ResourcesListJson '[]' -InstructionsJson '""'
        }
        It "Should return valid JSON" {
            { $result | ConvertFrom-Json } | Should -Not -Throw
        }
        It "Should return error response" {
            $json = $result | ConvertFrom-Json
            $json.error | Should -Not -BeNullOrEmpty
        }
        It "Should return error code -32601" {
            $json = $result | ConvertFrom-Json
            $json.error.code | Should -Be -32601
        }
        It "Should return 'Method not found' message" {
            $json = $result | ConvertFrom-Json
            $json.error.message | Should -Be "Method not found"
        }
    }
}

AfterAll {
    Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}