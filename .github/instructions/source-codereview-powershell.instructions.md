---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/Scripts/**/*.ps1"
description: "Reviewer's checklist for ISHRemote PowerShell advanced-function pull requests (Scripts/Public and Scripts/Private): naming, file shape, comment-based help, export registration, StrictMode compliance, PSScriptAnalyzer, and backward compatibility."
---

# ISHRemote PowerShell Scripts Code Review Checklist

Use this when reviewing a PR that touches `Scripts/Public/*.ps1` or `Scripts/Private/*.ps1`.
Each item is a binary pass/flag. Authoring detail lives in
[`source-scripts--powershell.instructions.md`](source-scripts--powershell.instructions.md).

## 1. Naming and placement

- [ ] Function uses an **approved PowerShell verb** (`Get-Verb` list) — no invented or aliased
  verbs (`PSUseApprovedVerbs` rule).
- [ ] Noun follows the `IshRemote` or `Ish` convention: public user-facing functions use
  `Verb-IshRemote*` (MCP family) or `Verb-Ish*`; private helpers use the `IshAux` infix
  (e.g. `Get-IshAuxSessionState`).
- [ ] **One function per file**, file name = function name
  (`Start-IshRemoteMcpServer.ps1` → `function Start-IshRemoteMcpServer`).
- [ ] Public functions land in `Scripts/Public/`; internal helpers land in `Scripts/Private/`.
- [ ] **No Apache 2.0 license header** — `.ps1` files in `Scripts/` do not carry the header
  (unlike the C# tree); match the neighbours.

## 2. Advanced function shape

- [ ] Every function declares `[CmdletBinding()]`.
- [ ] Parameters are declared in a typed `param(...)` block with `[Parameter(Mandatory)]` where
  appropriate.
- [ ] Parameter names are **singular** (`-Name`, `-FilePath`, `-IshSession`) — never plural.
- [ ] `[ValidateNotNullOrEmpty()]` or equivalent validators are used on required string parameters.

## 3. Comment-based help

- [ ] `.SYNOPSIS` is present — one-line summary.
- [ ] `.DESCRIPTION` is present — full description.
- [ ] Every parameter has a `.PARAMETER <Name>` block that describes its purpose.
- [ ] At least one `.EXAMPLE` block is present with realistic usage.
- [ ] All URLs in examples use `https://example.com/ISHWS/` — no real server hostnames or
  credentials. See §7.

## 4. Export registration

- [ ] Any **new `Public/` function** that users should call is added to the `-FunctionsToExport`
  list in `Trisoft.ISHRemote.csproj` — without this the function loads but is invisible.
- [ ] Functions that intentionally work as side-effecting dot-source scripts (like
  `Expand-ISHParameter.ps1`) are **not** added to `-FunctionsToExport`.

## 5. StrictMode compliance

- [ ] No use of variables that may be unassigned at runtime (`Set-StrictMode -Version Latest`
  is applied by `ISHRemote.psm1` after dot-sourcing all scripts — any unassigned variable access
  throws at import time).
- [ ] No implicit `$null` member access (e.g. calling a property on a variable that could be
  `$null` without a null-guard).

## 6. PSScriptAnalyzer clean

- [ ] `Invoke-ScriptAnalyzer -Path ... -Recurse` reports **no findings** for the changed files —
  CI reports but does not fail the build on analyzer warnings; still, zero findings is the bar.
- [ ] No unapproved aliases (e.g. `%`, `?`, `select`, `where`) in shipped code.
- [ ] No unused variables left behind.

## 7. Anonymization

- [ ] No real server URLs, hostnames, IP addresses, or credentials in help text, examples, or
  test fixture strings — use `https://example.com/ISHWS/` as the canonical placeholder.
- [ ] No hard-coded local paths (e.g. `C:\TEMP\...`, `D:\GITHUB\...`) — use parameter defaults or
  `$env:TEMP` / `${env:TEMP}` where a path is needed.

## 8. Backward compatibility

- [ ] No `Public/` function name or parameter name is **renamed or removed** without an explicit
  breaking-change flag in the PR description and implementer sign-off — these are part of the
  public module surface and break existing scripts.
- [ ] If a rename is unavoidable, the old name is kept as an alias (`Set-Alias OldName NewName`)
  and documented in the release notes.
