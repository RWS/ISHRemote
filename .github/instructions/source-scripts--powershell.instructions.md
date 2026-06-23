---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/Scripts/**/*.*"
description: "Structure and intent of the ISHRemote Scripts layer: PowerShell advanced functions that ship inside the binary module. Public/ functions are part of the public library surface (approved Verb-IshNoun naming, PSScriptAnalyzer-clean, explicitly exported, StrictMode-safe) and each should gain a *.Tests.ps1 following the Pester instructions; Private/ holds internal Aux helpers."
---

# ISHRemote `Scripts/` Conventions

`Scripts/` holds the **PowerShell advanced functions** that ship *inside* the otherwise-binary
ISHRemote module. They sit next to the C# cmdlets and are loaded by
[ISHRemote.psm1](../../Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.psm1), so to a user a function
here is indistinguishable from a compiled cmdlet. There are two tiers:
- **`Scripts/Public/`** — user-facing features that become part of the **public library surface**
  (the MCP server family, `Expand-ISHParameter`, `Write-IshRemoteLog`).
- **`Scripts/Private/`** — internal **`*IshAux*` helpers** that support the public ones
  (`Get-IshAuxSessionState`, `New-IshAuxCompletionResult`, `Register-IshAuxParameterCompleter`).

## 0. Public scripts ARE library surface — treat them like cmdlets
A `Public/` function is shipped and callable, so it carries the **same obligations as a C# cmdlet**
(see [source-cmdlets--csharp.instructions.md](./source-cmdlets--csharp.instructions.md)):
- **Approved `Verb-Noun` naming.** Use a verb from `Get-Verb` and the `Ish` noun convention. The
  established families are `Verb-IshRemote*` for the MCP server (`Start-IshRemoteMcpServer`,
  `Register-IshRemoteMcpTool`, `Register-IshRemoteMcpInstructions`, `Register-IshRemoteMcpResource`,
  `Invoke-IshRemoteMcpHandleRequest`), plus `Write-IshRemoteLog` and the older
  `Expand-ISHParameter`. Match a sibling rather than inventing a new shape.
- **It is a compatibility contract.** Renaming a public function or its parameters is a breaking
  change — same bar as renaming a cmdlet parameter; **ask the implementer** first.
- **`Private/` helpers signal internal intent** by the `IshAux` infix; keep new internal helpers
  there with that naming and don't promote them to a user-facing name casually.

## 1. No Apache license header here
Unlike the C# tree, the `.ps1` files in `Scripts/` do **not** carry the Apache header — match the
neighbours. (Attribution `# Hat tip to …` comments for borrowed techniques are welcome, as in
[Expand-ISHParameter.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/Scripts/Public/Expand-ISHParameter.ps1).)

## 2. File shape
- **One function per file, file name = function name** (`Start-IshRemoteMcpServer.ps1` →
  `function Start-IshRemoteMcpServer`). The exception is a deliberately *side-effecting* script that
  runs at dot-source time — [Expand-ISHParameter.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/Scripts/Public/Expand-ISHParameter.ps1)
  patches `TabExpansion2` and registers argument completers rather than defining one exported
  function.
- **Comment-based help** (`.SYNOPSIS` / `.DESCRIPTION` / `.PARAMETER` / `.EXAMPLE`) drives `Get-Help`,
  and for MCP tools it is also **parsed into the tool/JSON schema** — so keep it accurate and
  complete. Examples must be **anonymized** like the ReleaseNotes (`https://example.com/ISHWS/`, no
  secrets).
- Use `[CmdletBinding()]` and typed `param(...)` with `[Parameter(Mandatory)]` where appropriate;
  prefer singular parameter names, mirroring the cmdlet conventions.

## 3. How they load and export (build gotchas)
- [ISHRemote.psm1](../../Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.psm1) **dot-sources every
  `Scripts/Private/*.ps1` then `Scripts/Public/*.ps1`, excluding `*.Tests.ps1`**, and then runs
  `Set-StrictMode -Version Latest`. → **Your script must be StrictMode-Latest clean** (no
  use of unassigned variables, no implicit `$null` member access, etc.).
- **Exported functions are an explicit allow-list**, not folder-derived. The module manifest is
  generated in
  [Trisoft.ISHRemote.csproj](../../Source/ISHRemote/Trisoft.ISHRemote/Trisoft.ISHRemote.csproj) via
  `New-ModuleManifest -FunctionsToExport @(...)`. **When you add a new function that users should
  call, add its name to that `-FunctionsToExport` list** — otherwise it loads but isn't visible.
  (Note `Expand-ISHParameter` is intentionally *not* exported: it does its work as a side effect at
  import time.)
- Packaging copies `Scripts/**/*.*` into the published module **but excludes `*.Tests.ps1`** (both
  casings) — so tests live beside the code in source yet never ship.

## 4. Linting — keep `Scripts/**` PSScriptAnalyzer-clean
CI runs `Invoke-ScriptAnalyzer -Path Source/ISHRemote/Trisoft.ISHRemote/Scripts -Recurse`. It reports
but does **not** fail the build on findings — **still, keep it clean.** The most relevant rules here:
use approved verbs (`PSUseApprovedVerbs`), declare `[CmdletBinding()]`, avoid unapproved aliases, and
don't leave unused variables. A clean analyzer run is the bar for a PR.

## 5. Every function should get a `*.Tests.ps1`
Each function should **eventually have a Pester counterpart** next to it
(`Start-IshRemoteMcpServer.Tests.ps1`, `Register-IshRemoteMcpTool.Tests.ps1`, …) that **follows the
rules in [source-cmdlets--pester.instructions.md](./source-cmdlets--pester.instructions.md)**:
`BeforeAll` sets `$cmdletName` and dot-sources `ISHRemote.PesterSetup.ps1` via the relative
`\..\..\` hop, `Describe`/`Context` structure, self-cleaning data, etc.
- Some script tests differ from the live-server acceptance suite: the MCP tests are **PowerShell 7+
  only** (`-Skip:($PSVersionTable.PSVersion.Major -lt 7)`) and may **`Mock`** collaborators (e.g.
  `Write-IshRemoteLog`) so that pure-JSON assertions run without a reachable CMS. That is fine — they
  still follow the same file/`BeforeAll` structure.
- Keep expected/asserted strings **anonymized** (the existing fixtures use
  `https://example.com/ISHWS/`).

## 6. Don't
- Don't rename/remove a `Public/` function or its parameters without implementer approval (breaks
  scripts, help, and the MCP tool schema) — see §0.
- Don't forget to add a new exported function to `-FunctionsToExport` in the `.csproj` (§3).
- Don't write code that trips `Set-StrictMode -Version Latest` or PSScriptAnalyzer (§3, §4).
- Don't add an Apache header to `.ps1` files here (§1), and don't let `*.Tests.ps1` ship — the build
  already excludes them.
- Don't put real customer URLs/secrets in help, examples, or test fixtures — anonymize (§2, §5).
