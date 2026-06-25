#Requires -Version 5.1
<#
    TestAnonymization.Tests.ps1

    Repo-hygiene meta-test: asserts the public, blog-like Doc/ and Samples/ material uses
    example.com placeholders instead of real server URLs, internal hostnames, private IPs or
    tokens (see .github/instructions/doc--markdown.instructions.md and
    source-samples--mixed.instructions.md).

    This test scans static files only - it needs no IshSession and no live server - so it
    deliberately does NOT dot-source ISHRemote.PesterSetup.ps1. The few acknowledged historical,
    non-routable references from the "thinking out loud" notes are baked into $knownException
    inside Get-IshContentLeak; extend that list only for intentional history.
#>

Describe 'Public Doc and Samples stay anonymized' {

    BeforeAll {
        # Returns one 'L<line>: <match>' entry per leak found in the given lines of text.
        function Get-IshContentLeak {
            [CmdletBinding()]
            param(
                [Parameter(Mandatory)]
                [AllowEmptyString()]
                [AllowEmptyCollection()]
                [string[]] $Line
            )
            Set-StrictMode -Version Latest

            # Acknowledged historical, non-routable references already committed to the notes.
            $knownException = @(
                'medevddemeyer10.global.sdl.corp'
                'mecdev14qa01.global.sdl.corp'
                'mecdev12qa01.global.sdl.corp'
                '192.168.1.160'
            )
            # Hostnames that are obviously placeholders, never a leak.
            $placeholderHost = @(
                'localhost', '127.0.0.1', 'hostname', 'host',
                'server', 'servername', 'yourhost', 'yourserver'
            )
            # High-signal rules; each optional Filter returns $true only for a real finding.
            $rule = @(
                @{
                    Pattern = 'https?://(?<host>[\w.<>-]+)/(?:ISHWS|ISHCM|ISHSTS|ISHAM|InfoShare)\w*/'
                    Filter  = {
                        param([System.Text.RegularExpressions.Match] $Match)
                        $candidateHost = $Match.Groups['host'].Value.ToLowerInvariant()
                        if ($candidateHost -match '[<>]') { return $false }
                        if ($candidateHost -eq 'example.com' -or $candidateHost.EndsWith('.example.com')) { return $false }
                        return -not ($placeholderHost -contains $candidateHost)
                    }
                }
                @{
                    Pattern = '\b[\w-]+\.(?:global\.)?sdl\.corp\b'
                    Filter  = $null
                }
                @{
                    Pattern = '\b\d{1,3}(?:\.\d{1,3}){3}\b'
                    Filter  = {
                        param([System.Text.RegularExpressions.Match] $Match)
                        $octet = $Match.Value -split '\.'
                        foreach ($part in $octet) { if ([int] $part -gt 255) { return $false } }
                        ([int] $octet[0] -eq 10) -or
                        ([int] $octet[0] -eq 192 -and [int] $octet[1] -eq 168) -or
                        ([int] $octet[0] -eq 172 -and [int] $octet[1] -ge 16 -and [int] $octet[1] -le 31)
                    }
                }
                @{
                    Pattern = '\beyJ[\w-]{10,}\.eyJ[\w-]{10,}\.[\w-]+'
                    Filter  = $null
                }
            )

            $leak = [System.Collections.Generic.List[string]]::new()
            for ($index = 0; $index -lt $Line.Count; $index++) {
                foreach ($currentRule in $rule) {
                    foreach ($match in [regex]::Matches($Line[$index], $currentRule.Pattern)) {
                        if ($null -ne $currentRule.Filter -and -not (& $currentRule.Filter $match)) { continue }
                        $value = $match.Value
                        if ($knownException | Where-Object { $value.Contains($_) }) { continue }
                        $leak.Add(('L{0}: {1}' -f ($index + 1), $value))
                    }
                }
            }
            , $leak.ToArray()
        }

        # Scans every text file under Doc/ and Samples/, returning '<relative path>  L<line>: <match>'.
        function Get-IshAnonymizationFinding {
            [CmdletBinding()]
            param()
            Set-StrictMode -Version Latest

            # This file lives at Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/_TestEnvironment/,
            # so the repository root is five folders up.
            $repoRoot = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath '..\..\..\..\..')).Path
            $textExtension = '.md', '.markdown', '.txt', '.ps1', '.psm1', '.psd1', '.xml', '.json',
                             '.yml', '.yaml', '.html', '.htm', '.xhtml', '.dita', '.ditamap', '.csv', '.config'
            $scanRoot = @(
                (Join-Path -Path $repoRoot -ChildPath 'Doc')
                (Join-Path -Path $repoRoot -ChildPath 'Source\ISHRemote\Trisoft.ISHRemote\Samples')
            )

            $finding = [System.Collections.Generic.List[string]]::new()
            foreach ($root in $scanRoot) {
                if (-not (Test-Path -LiteralPath $root)) { continue }
                $file = Get-ChildItem -LiteralPath $root -Recurse -File |
                    Where-Object { $textExtension -contains $_.Extension.ToLowerInvariant() }
                foreach ($currentFile in $file) {
                    $relPath = ($currentFile.FullName.Substring($repoRoot.Length).TrimStart('\', '/') -replace '\\', '/')
                    foreach ($leak in (Get-IshContentLeak -Line @(Get-Content -LiteralPath $currentFile.FullName))) {
                        $finding.Add("$relPath  $leak")
                    }
                }
            }
            , $finding.ToArray()
        }
    }

    It 'rules flag a non-anonymized sample (guard-for-the-guard)' {
        Get-IshContentLeak -Line 'connect to https://realhost.acme.local/ISHWS/ now' | Should -Not -BeNullOrEmpty
        Get-IshContentLeak -Line 'internal box at 10.20.30.40' | Should -Not -BeNullOrEmpty
    }

    It 'rules ignore the agreed example.com placeholders' {
        Get-IshContentLeak -Line 'New-IshSession -WsBaseUrl https://ish.example.com/ISHWS/' | Should -BeNullOrEmpty
        Get-IshContentLeak -Line 'Generate "https://hostname/InfoShareWS/" "username" "password"' | Should -BeNullOrEmpty
    }

    It 'Doc and Samples contain no real URL, internal hostname, private IP or token' {
        $finding = Get-IshAnonymizationFinding
        $finding | Should -BeNullOrEmpty -Because 'public material must use example.com placeholders (see .github/instructions/doc--markdown.instructions.md)'
    }
}
