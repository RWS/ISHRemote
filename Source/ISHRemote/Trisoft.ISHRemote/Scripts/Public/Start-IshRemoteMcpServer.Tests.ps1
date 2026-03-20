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

Describe "Start-IshRemoteMcpServer" -Tags "Read" {
    Context "Start-IshRemoteMcpServer with ActivateWhileLoop=false" {
        BeforeEach {
            Mock -ModuleName ISHRemote Write-IshRemoteLog { }
        }
        It "Validates CmdletsToRegister parameter is mandatory" {
            { Start-IshRemoteMcpServer -ActivateWhileLoop $false } | Should -Throw
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
            Should -Invoke -ModuleName ISHRemote Register-IshRemoteMcpTool -ParameterFilter { 
                $FunctionNameFullLoad -contains 'Get-Help' -and $FunctionNameFullLoad -contains 'New-IshSession'
            }
        }
        It "Validates CmdletsToRegisterFullLoad only holds explicitly set, but not implicit Get-Help and New-IshSession" {
            Mock -ModuleName ISHRemote Register-IshRemoteMcpTool { return "{}" }
            $cmdlets = @('Get-IshFolder')
            $cmdletsFullLoad = @('Get-IshDocumentObj', 'Set-IshDocumentObj')
            Start-IshRemoteMcpServer -CmdletsToRegister $cmdlets -CmdletsToRegisterFullLoad $cmdletsFullLoad -ActivateWhileLoop $false
            Should -Invoke -ModuleName ISHRemote Register-IshRemoteMcpTool -ParameterFilter { 
                $FunctionNameFullLoad -contains 'Get-IshDocumentObj' -and $FunctionNameFullLoad -contains 'Set-IshDocumentObj' -and -not ($FunctionNameFullLoad -contains 'Get-Help') -and -not ($FunctionNameFullLoad -contains 'New-IshSession')
            }
        }
        It "Starts server with single cmdlet in CmdletsToRegister" {
            { Start-IshRemoteMcpServer -CmdletsToRegister @('Get-IshFolder') -ActivateWhileLoop $false } | Should -Not -Throw
        }
    }
}

AfterAll {
    Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}