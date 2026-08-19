#pester:no-parallel #Mock -ModuleName uses cross-module mocking which installs hooks into the ISHRemote module's session state. In a parallel runspace, each worker loads its own ISHRemote module instance, but the mock infrastructure that Mock -ModuleName relies on requires the parent session's module context. This causes the worker runspace to hang waiting for something that never resolves.
BeforeAll {
    $cmdletName = "Start-IshRemoteMcpServer"
    Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
    
    function Write-IshRemoteLog {
        param(
            [Parameter(Mandatory = $true)]        
            [object]$LogEntry
        )
    }
}

Describe "Start-IshRemoteMcpServer" -Tags "Read" -Skip:($PSVersionTable.PSVersion.Major -lt 7) {
    Context "Start-IshRemoteMcpServer with ActivateWhileLoop=false" {
        BeforeEach {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
        }
        It "Validates CmdletsToRegister parameter is mandatory" {
            { Start-IshRemoteMcpServer -ActivateWhileLoop $false -CmdletsToRegister @() } | Should-Throw
        }
        It "Starts server with CmdletsToRegister parameter" {
            $cmdlets = @('Get-IshFolder', 'Set-IshFolder')
            { Start-IshRemoteMcpServer -CmdletsToRegister $cmdlets -ActivateWhileLoop $false } | Should -Not -Throw
        }
        It "Starts server with both CmdletsToRegister and CmdletsToRegisterFullLoad parameters" {
            $cmdlets = @('Get-IshFolder', 'Set-IshFolder')
            $cmdletsFullLoad = @('Get-IshDocumentObj', 'Set-IshDocumentObj')
            { Start-IshRemoteMcpServer -CmdletsToRegister $cmdlets -CmdletsToRegisterFullLoad $cmdletsFullLoad -ActivateWhileLoop $false } | Should -Not -Throw
        }
        It "Validates CmdletsToRegisterFullLoad defaults to Get-Help and New-IshSession" {
            Mock -ModuleName ISHRemote Register-IshRemoteMcpTool { return "{}" }
            $cmdlets = @('Get-IshFolder')
            Start-IshRemoteMcpServer -CmdletsToRegister $cmdlets -ActivateWhileLoop $false
            Should-Invoke -ModuleName ISHRemote Register-IshRemoteMcpTool -ParameterFilter { 
                $FunctionNameFullLoad -contains 'Get-Help' -and $FunctionNameFullLoad -contains 'New-IshSession'
            }
        }
        It "Validates CmdletsToRegisterFullLoad only holds explicitly set, but not implicit Get-Help and New-IshSession" {
            Mock -ModuleName ISHRemote Register-IshRemoteMcpTool { return "{}" }
            $cmdlets = @('Get-IshFolder')
            $cmdletsFullLoad = @('Get-IshDocumentObj', 'Set-IshDocumentObj')
            Start-IshRemoteMcpServer -CmdletsToRegister $cmdlets -CmdletsToRegisterFullLoad $cmdletsFullLoad -ActivateWhileLoop $false
            Should-Invoke -ModuleName ISHRemote Register-IshRemoteMcpTool -ParameterFilter { 
                $FunctionNameFullLoad -contains 'Get-IshDocumentObj' -and $FunctionNameFullLoad -contains 'Set-IshDocumentObj' -and -not ($FunctionNameFullLoad -contains 'Get-Help') -and -not ($FunctionNameFullLoad -contains 'New-IshSession')
            }
        }
        It "Starts server with single cmdlet in CmdletsToRegister" {
            { Start-IshRemoteMcpServer -CmdletsToRegister @('Get-IshFolder') -ActivateWhileLoop $false } | Should -Not -Throw
        }
        It "Sets Console InputEncoding and OutputEncoding to UTF-8 without BOM" {
            # On Windows the default is SBCSCodePageEncoding; the server must switch to UTF-8 so
            # MCP clients (which always send UTF-8 JSON) are read correctly. Regression test for
            # the fix in GitHub issue #243.
            $cmdlets = @('Get-IshFolder')
            Start-IshRemoteMcpServer -CmdletsToRegister $cmdlets -ActivateWhileLoop $false
            [Console]::InputEncoding.WebName  | Should-Be 'utf-8'
            [Console]::OutputEncoding.WebName | Should-Be 'utf-8'
        }
        It "Sets Console Out AutoFlush to true on inner StreamWriter" {
            # Console.SetOut re-wraps in SyncTextWriter; verify the inner StreamWriter has AutoFlush=true
            # so each JSON-RPC response is flushed immediately to the pipe. Regression test for
            # the fix in GitHub issue #243.
            $cmdlets = @('Get-IshFolder')
            Start-IshRemoteMcpServer -CmdletsToRegister $cmdlets -ActivateWhileLoop $false
            $field = [Console]::Out.GetType().GetField('_out', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
            $innerWriter = $field.GetValue([Console]::Out)
            $innerWriter.AutoFlush | Should-Be $true
        }
    }
}

AfterAll {
    Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}