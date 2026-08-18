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
            $result | Should-NotBeNull
        }
    }

    Context "Instruction Content" {
        BeforeAll {
            $instructions = (Register-IshRemoteMcpInstructions | ConvertFrom-Json)
        }

        It "Should contain New-IshSession cmdlet reference" {
            $instructions | Should-MatchString "New-IShSession"
        }

        It "Should contain Get-IshTypeFieldDefinition reference" {
            $instructions | Should-MatchString "Get-IshTypeFieldDefinition"
        }

        It "Should contain FilterOperator reference" {
            $instructions | Should-MatchString "FilterOperator"
        }

        It "Should contain field types (String, Number, DateTime, LongText)" {
            $instructions | Should-MatchString "String"
            $instructions | Should-MatchString "Number"
            $instructions | Should-MatchString "DateTime"
            $instructions | Should-MatchString "LongText"
        }

        It "Should contain ISHType object references" {
            $instructions | Should-MatchString "IShUser"
            $instructions | Should-MatchString "IShFolder"
            $instructions | Should-MatchString "IShDocumentObj"
        }

        It "Should contain level references (logical, version, lng)" {
            $instructions | Should-MatchString "logical"
            $instructions | Should-MatchString "version"
            $instructions | Should-MatchString "lng"
        }

        It "Should mention PSNoteType properties" {
            $instructions | Should-MatchString "PSNoteType"
        }

        It "Should contain Get-Help cmdlet reference" {
            $instructions | Should-MatchString "Get-Help"
        }

        It "Should mention case-sensitivity" {
            $instructions | Should-MatchString "case-sensitive"
        }

        It "Should contain wildcard operator guidance" {
            $instructions | Should-MatchString "percentage"
            $instructions | Should-MatchString "%"
        }
    }
}


AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}