BeforeAll {
	$cmdletName = "Register-IshRemoteMcpInstructions"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
}

Describe "Register-IshRemoteMcpInstructions" -Skip:($PSVersionTable.PSVersion.Major -lt 7) {
    
    Context "Function Output" {
        It "Should return valid JSON" {
            $result = Register-IshRemoteMcpInstructions
            { $result | ConvertFrom-Json } | Should -Not -Throw
        }

        It "Should return non-empty string" {
            $result = Register-IshRemoteMcpInstructions
            $result | Should -Not -BeNullOrEmpty
        }
    }

    Context "Instruction Content" {
        BeforeAll {
            $instructions = (Register-IshRemoteMcpInstructions | ConvertFrom-Json)
        }

        It "Should contain New-IshSession cmdlet reference" {
            $instructions | Should -Match "New-IShSession"
        }

        It "Should contain Get-IshTypeFieldDefinition reference" {
            $instructions | Should -Match "Get-IshTypeFieldDefinition"
        }

        It "Should contain FilterOperator reference" {
            $instructions | Should -Match "FilterOperator"
        }

        It "Should contain field types (String, Number, DateTime, LongText)" {
            $instructions | Should -Match "String"
            $instructions | Should -Match "Number"
            $instructions | Should -Match "DateTime"
            $instructions | Should -Match "LongText"
        }

        It "Should contain ISHType object references" {
            $instructions | Should -Match "IShUser"
            $instructions | Should -Match "IShFolder"
            $instructions | Should -Match "IShDocumentObj"
        }

        It "Should contain level references (logical, version, lng)" {
            $instructions | Should -Match "logical"
            $instructions | Should -Match "version"
            $instructions | Should -Match "lng"
        }

        It "Should mention PSNoteType properties" {
            $instructions | Should -Match "PSNoteType"
        }

        It "Should contain Get-Help cmdlet reference" {
            $instructions | Should -Match "Get-Help"
        }

        It "Should mention case-sensitivity" {
            $instructions | Should -Match "case-sensitive"
        }

        It "Should contain wildcard operator guidance" {
            $instructions | Should -Match "percentage"
            $instructions | Should -Match "%"
        }
    }
}


AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}