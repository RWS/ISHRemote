#Requires -Version 5.1
<#
    TestAiInstructions.Tests.ps1

    Repo-hygiene meta-test: asserts the AI coding agent instruction files are internally
    consistent and correctly wired across the three artefact sets that form each of the 14
    canonical instruction domains:

      1. Canonical file    .github/instructions/<name>.instructions.md
      2. AI coding agent stub  .claude/rules/<name>.md
      3. AGENTS.md row     the @-ref lazy-load table in AGENTS.md

    Invariants enforced (any violation is a defect — see AGENTS.md > Maintaining these instructions):
      a. Every canonical file begins with `---` at line 1 (YAML frontmatter); no code fence.
      b. Frontmatter contains both `applyTo:` and `description:` keys, non-empty.
      c. Every canonical file has a matching stub in .claude/rules/ (same <name>, .md suffix).
      d. Every stub has a matching canonical file (no orphaned stubs).
      e. Every stub has `---` at line 1 and declares a non-empty `paths:` frontmatter key.
      f. The stub `paths:` value equals the canonical `applyTo:` value (globs kept in sync).
      g. Every canonical file appears as an @-ref in the AGENTS.md lazy-load table.
      h. Every @-ref in the AGENTS.md table resolves to an existing canonical file.
      i. The AGENTS.md lazy-load directive block is present.
      j. .github/copilot-instructions.md references AGENTS.md (pointer, not duplicate).
      k. CLAUDE.md contains the @AGENTS.md import.
      l. No canonical file or AGENTS.md refers to any specific AI harness by product name
         (use "AI coding agent" instead); file path strings are exempted.

    This file scans static text only — it needs no IshSession and no live server — so it
    deliberately does NOT dot-source ISHRemote.PesterSetup.ps1.
#>

# ---------------------------------------------------------------------------
# Script-scope helpers — dot-sourcing into script scope so they are visible
# both in BeforeDiscovery (discovery time) and in It blocks (run time).
# Pester v5 executes It scriptblocks in a child scope of the container but
# script-scope functions defined at the file level are always accessible.
# ---------------------------------------------------------------------------

function script:Get-FmValue {
    <#
    .SYNOPSIS
        Returns the value(s) of a YAML frontmatter key from a markdown file.
        Handles scalar (key: "value") and list (key:\n  - "value") forms.
    #>
    param([string] $FilePath, [string] $Key)
    $lines = Get-Content -LiteralPath $FilePath -ErrorAction Stop
    if ($lines.Count -eq 0 -or $lines[0] -ne '---') { return $null }
    $close = 1
    while ($close -lt $lines.Count -and $lines[$close] -ne '---') { $close++ }
    $inKey  = $false
    $values = [System.Collections.Generic.List[string]]::new()
    foreach ($line in $lines[1..($close - 1)]) {
        if ($line -match "^$Key\s*:\s*(.*)$") {
            $s = $Matches[1].Trim().Trim('"')
            if ($s -ne '') { $values.Add($s) }
            $inKey = $true; continue
        }
        if ($inKey -and $line -match '^\s+-\s+"?(.+?)"?\s*$') { $values.Add($Matches[1]); continue }
        if ($inKey -and $line -match '^\S') { break }
    }
    if ($values.Count -eq 0) { return $null }
    , $values.ToArray()
}

function script:Get-Stem {
    <#
    .SYNOPSIS
        "source-cmdlets--csharp.instructions.md" -> "source-cmdlets--csharp"
    #>
    param([string] $FileName)
    $FileName -replace '\.instructions\.md$', ''
}

function script:Get-HarnessViolation {
    <#
    .SYNOPSIS
        Returns "L<n>: <match>" for lines naming a specific AI harness product.
        Exempts: file paths (contain / or \) and qualifier usage ("X-compatible").
    #>
    param([string[]] $Lines)
    $names   = @('GitHub Copilot', 'Copilot Chat', 'Claude Code', 'OpenCode',
                 'Cursor', 'Codeium', 'Tabnine')
    $pattern = ($names | ForEach-Object { [regex]::Escape($_) }) -join '|'
    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]
        foreach ($m in [regex]::Matches($line, $pattern)) {
            $startIdx = [Math]::Max(0, $m.Index - 10)
            $len      = [Math]::Min($line.Length - $startIdx, $m.Length + 20)
            $ctx      = $line.Substring($startIdx, $len)
            if ($ctx -match '[/\\]') { continue }
            if ($line.Substring($m.Index) -match '^[\w\s]+-compatible') { continue }
            "L$($i+1): $($m.Value)"
        }
    }
}

# ---------------------------------------------------------------------------
# BeforeDiscovery — populates variables that drive foreach/TestCases.
# Runs at collection time, before BeforeAll.
# ---------------------------------------------------------------------------

BeforeDiscovery {
    $repoRootD  = (Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '..\..\..\..\..') ).Path
    $instrDirD  = Join-Path $repoRootD '.github\instructions'
    $rulesDirD  = Join-Path $repoRootD '.claude\rules'
    $agentsMdD  = Join-Path $repoRootD 'AGENTS.md'

    $script:canonicalCases = @(
        Get-ChildItem -LiteralPath $instrDirD -Filter '*.instructions.md' -File |
            Sort-Object Name |
            ForEach-Object { @{ CfName = $_.Name; CfPath = $_.FullName } }
    )

    $script:stubCases = @(
        Get-ChildItem -LiteralPath $rulesDirD -Filter '*.md' -File |
            Sort-Object Name |
            ForEach-Object { @{ SfName = $_.Name; SfPath = $_.FullName } }
    )

    $script:pairedCases = @(
        foreach ($cf in $script:canonicalCases) {
            $stem = Get-Stem $cf.CfName
            $sp   = Join-Path $rulesDirD "$stem.md"
            if (Test-Path -LiteralPath $sp) {
                @{ CfName=$cf.CfName; CfPath=$cf.CfPath; SfName="$stem.md"; SfPath=$sp }
            }
        }
    )

    $agentsMdRaw = Get-Content -LiteralPath $agentsMdD -Raw
    $script:agentsRefCases = @(
        [regex]::Matches($agentsMdRaw,
            '@(\.github/instructions/[\w.-]+\.instructions\.md)') |
            ForEach-Object { @{ Ref = $_.Groups[1].Value; RepoRoot = $repoRootD } }
    )
}

# ---------------------------------------------------------------------------
Describe 'AI instruction files are internally consistent and correctly wired' {

    BeforeAll {
        $script:repoRoot  = (Resolve-Path -LiteralPath (
            Join-Path $PSScriptRoot '..\..\..\..\..') ).Path
        $script:instrDir  = Join-Path $script:repoRoot '.github\instructions'
        $script:rulesDir  = Join-Path $script:repoRoot '.claude\rules'
        $script:agentsMd  = Join-Path $script:repoRoot 'AGENTS.md'
        $script:claudeMd  = Join-Path $script:repoRoot 'CLAUDE.md'
        $script:copilotMd = Join-Path $script:repoRoot '.github\copilot-instructions.md'
    }

    # -------------------------------------------------------------------------
    Context 'Canonical file structure (.github/instructions/*.instructions.md)' {

        It 'instructions directory exists and contains files' {
            Test-Path -LiteralPath $script:instrDir | Should -BeTrue
            @(Get-ChildItem -LiteralPath $script:instrDir -Filter '*.instructions.md' -File).Count |
                Should -BeGreaterThan 0
        }

        It "[<CfName>] line 1 is '---' (YAML frontmatter, no code fence)" `
           -TestCases $script:canonicalCases {
            (Get-Content -LiteralPath $CfPath -TotalCount 1) | Should -BeExactly '---' -Because (
                "line 1 must open YAML frontmatter; a code fence makes applyTo: inert. " +
                "Remove any opening ``` or ```` fence from '$CfName'.")
        }

        It "[<CfName>] frontmatter has non-empty 'applyTo:'" `
           -TestCases $script:canonicalCases {
            Get-FmValue -FilePath $CfPath -Key 'applyTo' |
                Should -Not -BeNullOrEmpty -Because (
                    "'$CfName' needs applyTo: for AI coding agent auto-injection.")
        }

        It "[<CfName>] frontmatter has non-empty 'description:'" `
           -TestCases $script:canonicalCases {
            Get-FmValue -FilePath $CfPath -Key 'description' |
                Should -Not -BeNullOrEmpty -Because (
                    "'$CfName' needs description: for the agent to surface the file.")
        }
    }

    # -------------------------------------------------------------------------
    Context 'AI coding agent stubs (.claude/rules/*.md)' {

        It 'rules directory exists' {
            Test-Path -LiteralPath $script:rulesDir | Should -BeTrue -Because (
                ".claude/rules/ must exist for Claude Code-compatible path-scoped injection.")
        }

        It "[<CfName>] has a matching stub in .claude/rules/" `
           -TestCases $script:canonicalCases {
            $stem = Get-Stem $CfName
            Test-Path -LiteralPath (Join-Path $script:rulesDir "$stem.md") | Should -BeTrue -Because (
                "'$CfName' requires a stub '$stem.md'. " +
                "Add .claude/rules/$stem.md with paths: matching applyTo:.")
        }

        It "[stub <SfName>] has a matching canonical file" `
           -TestCases $script:stubCases {
            $canonName = "$($SfName -replace '\.md$','').instructions.md"
            Test-Path -LiteralPath (Join-Path $script:instrDir $canonName) | Should -BeTrue -Because (
                "stub '$SfName' has no matching canonical '$canonName'. " +
                "Add the canonical file or remove the orphaned stub.")
        }

        It "[stub <SfName>] line 1 is '---' (YAML frontmatter)" `
           -TestCases $script:stubCases {
            (Get-Content -LiteralPath $SfPath -TotalCount 1) | Should -BeExactly '---' -Because (
                "stub '$SfName' must open with YAML frontmatter at line 1.")
        }

        It "[stub <SfName>] frontmatter has non-empty 'paths:'" `
           -TestCases $script:stubCases {
            Get-FmValue -FilePath $SfPath -Key 'paths' |
                Should -Not -BeNullOrEmpty -Because (
                    "stub '$SfName' needs paths: for Claude Code-compatible path-scoped injection.")
        }
    }

    # -------------------------------------------------------------------------
    Context 'Glob synchronisation (applyTo: in canonical == paths: in stub)' {

        It "[<CfName>] applyTo: matches paths: in stub" `
           -TestCases $script:pairedCases {
            $applyTo = @(Get-FmValue -FilePath $CfPath -Key 'applyTo' | Sort-Object)
            $paths   = @(Get-FmValue -FilePath $SfPath -Key 'paths'   | Sort-Object)
            $applyTo | Should -BeExactly $paths -Because (
                "applyTo: in '$CfName' and paths: in '$SfName' must express the same glob. " +
                "Update the out-of-sync value — see AGENTS.md > Maintaining these instructions.")
        }
    }

    # -------------------------------------------------------------------------
    Context 'AGENTS.md lazy-load table coverage' {

        It 'AGENTS.md exists at repository root' {
            Test-Path -LiteralPath $script:agentsMd | Should -BeTrue
        }

        It 'AGENTS.md contains the lazy-load directive block' {
            (Get-Content -LiteralPath $script:agentsMd -Raw) |
                Should -Match 'Scoped instructions' -Because (
                    "AGENTS.md must contain '## Scoped instructions' for OpenCode-compatible lazy loading.")
        }

        It "[<CfName>] appears as @-ref in AGENTS.md" `
           -TestCases $script:canonicalCases {
            $ref = ".github/instructions/$CfName"
            (Get-Content -LiteralPath $script:agentsMd -Raw) |
                Should -Match ([regex]::Escape("@$ref")) -Because (
                    "'$CfName' must have a row in the AGENTS.md @-ref table. " +
                    "Add: | <path description> | @$ref |")
        }

        It "[@<Ref>] resolves to an existing canonical file" `
           -TestCases $script:agentsRefCases {
            $p = Join-Path $RepoRoot ($Ref -replace '/', '\')
            Test-Path -LiteralPath $p | Should -BeTrue -Because (
                "AGENTS.md references @$Ref but the file does not exist. " +
                "Add the canonical file or remove the stale @-ref.")
        }
    }

    # -------------------------------------------------------------------------
    Context 'Pointer files integrity' {

        It 'CLAUDE.md exists and contains @AGENTS.md import' {
            Test-Path -LiteralPath $script:claudeMd | Should -BeTrue
            (Get-Content -LiteralPath $script:claudeMd -Raw) | Should -Match '@AGENTS\.md' -Because (
                "CLAUDE.md must import AGENTS.md for Claude Code-compatible harnesses.")
        }

        It '.github/copilot-instructions.md exists and references AGENTS.md' {
            Test-Path -LiteralPath $script:copilotMd | Should -BeTrue
            (Get-Content -LiteralPath $script:copilotMd -Raw) | Should -Match 'AGENTS\.md' -Because (
                ".github/copilot-instructions.md must reference AGENTS.md (pointer, not duplicate).")
        }

        It '.github/copilot-instructions.md is a short pointer (under 25 lines)' {
            (Get-Content -LiteralPath $script:copilotMd).Count | Should -BeLessOrEqual 25 -Because (
                ".github/copilot-instructions.md must be a thin pointer. " +
                "Full content belongs in AGENTS.md. Keep under 25 lines.")
        }
    }

    # -------------------------------------------------------------------------
    Context 'Harness-neutral prose (no AI product names in instructions)' {

        It "[<CfName>] does not name a specific AI harness product" `
           -TestCases $script:canonicalCases {
            $violations = Get-HarnessViolation -Lines (Get-Content -LiteralPath $CfPath)
            $violations | Should -BeNullOrEmpty -Because (
                "Use 'AI coding agent' instead of a product name. " +
                "Violations in '$CfName': $($violations -join '; ')")
        }

        It 'AGENTS.md does not name a specific AI harness product' {
            $violations = Get-HarnessViolation -Lines (Get-Content -LiteralPath $script:agentsMd)
            $violations | Should -BeNullOrEmpty -Because (
                "AGENTS.md must use 'AI coding agent'. Violations: $($violations -join '; ')")
        }
    }
}
