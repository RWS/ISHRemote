````instructions
---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/**/*.Tests.ps1"
description: "Reviewer's checklist for ISHRemote Pester (*.Tests.ps1) pull requests: file anatomy, structure, data lifecycle, session handling, tenant variables, and special test types."
---

# ISHRemote Pester Test Code Review Checklist

Use this when reviewing a PR that touches `*.Tests.ps1` files. Each item is a binary pass/flag.
Authoring detail lives in
[`source-cmdlets--pester.instructions.md`](source-cmdlets--pester.instructions.md).

## 1. File anatomy

- [ ] Top-level `BeforeAll` sets `$cmdletName` to the exact public cmdlet name (e.g.
  `"Add-IshUserGroup"`) and dot-sources `ISHRemote.PesterSetup.ps1`, if the cmdlet requires `New-IshSession` creation, via the exact relative path:
  ```powershell
  . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
  ```
- [ ] Top-level `BeforeAll` also emits a `Write-Host` banner:
  ```powershell
  Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
  ```
- [ ] Top-level `AfterAll` exists, emits a `Write-Host` cleanup banner, and deletes **all** data
  created by this file — see §4.
- [ ] The file is named `XxxIshYyy.Tests.ps1` and lives next to its `XxxIshYyy.cs` (or next to
  its `.ps1` for script tests).

## 2. Describe / Context / It structure

- [ ] Each `Describe` block is named after the **public cmdlet** (e.g. `"Add-IshUserGroup"`) and
  carries one of the CRUD tags: `-Tags "Create"`, `"Read"`, `"Update"`, or `"Delete"`.
- [ ] Each `Context` is named after the **parameter set** being exercised (e.g.
  `"Add-IshUserGroup ParameterGroup"`, `"Add-IshUserGroup IshObjectGroup"`).
- [ ] The first `It` in every `Context` is an invalid-session guard:
  ```powershell
  It "Parameter IshSession invalid" {
      { Verb-IshNoun -IshSession "INVALIDISHSESSION" ... } | Should -Throw
  }
  ```
- [ ] Type-assertion `It` blocks use `Should -BeExactly` against `.GetType().Name` (not
  `GetType().FullName`) to confirm the correct `Ish*` type is returned:
  ```powershell
  $result.GetType().Name | Should -BeExactly "IshUserGroup"
  ```

## 3. Test data naming and uniqueness

- [ ] Every object created in a test uses a name composed of `$cmdletName` + a millisecond
  timestamp to guarantee uniqueness:
  ```powershell
  $name = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " SuffixLabel")
  ```
- [ ] Folder-based tests create their working folder **named after `$cmdletName`** under the
  shared root:
  ```powershell
  Add-IshFolder -ParentFolderId $folderIdTestRootOriginal -FolderName $cmdletName ...
  ```
- [ ] When many objects are created rapidly inside a loop, `Start-Sleep -Milliseconds 1000` is
  used between creates to avoid the `CARD_NAME_I1` duplicate-key constraint (second-resolution
  uniqueness). This is only applied when integration tests fail.

## 4. Self-contained data lifecycle

- [ ] All data needed for the test is **created inside `BeforeAll`** (per `Describe`/`Context`)
  or inline in the `It`; no test relies on objects a different file created.
- [ ] The top-level `AfterAll` deletes **everything this file created**, using filter-based patterns
  scoped to `$cmdletName`:
  ```powershell
  # Folders:
  Remove-IshFolder -FolderPath (Join-Path $folderTestRootPath $cmdletName) -Recurse
  # Users / user groups:
  Find-IshUser -MetadataFilter (Set-IshMetadataFilterField -Name USERNAME -FilterOperator like -Value "$cmdletName%") | Remove-IshUser
  ```
- [ ] Every delete in `AfterAll` is wrapped in `try { ... } catch { }` so cleanup failure never
  marks the suite run as failed.

## 5. Session handling

- [ ] Tests use `$ishSession` from the shared `ISHRemote.PesterSetup.ps1` setup; they do **not**
  call `New-IshSession` themselves.
- [ ] Each `Context` exercises **both** the explicit `-IshSession $ishSession` form **and** the
  implicit (session-state-resolved) form where relevant.
- [ ] `ISHRemote.PesterSetup.Debug.ps1` is **not** committed — it is git-ignored and contains live
  credentials.

## 6. Tenant variables — no hard-coded LOV values

- [ ] Language codes, status values, LOV element names, and similar tenant-specific constants are
  referenced via the **variables declared in `ISHRemote.PesterSetup.ps1`** (e.g. `$ishLng`,
  `$ishLngLabel`, `$ishLngTarget1`, `$ishStatusDraft`, `$ishStatusReleased`, `$ishLovId`).
- [ ] If a test needs a new piece of tenant data not yet in `ISHRemote.PesterSetup.ps1`, a
  **defaulted variable** is added to that shared file (so CI and other tenants can override it)
  rather than hard-coding a value in the test.

## 7. Special test types

- [ ] **Server-less tests** (e.g. `TestAnonymization.Tests.ps1`) do **not** dot-source
  `ISHRemote.PesterSetup.ps1` — they must be runnable without a live server.
- [ ] **PS 7+ only tests** guard with `-Skip:($PSVersionTable.PSVersion.Major -lt 7)` on the
  `It` or `Context` block.
- [ ] **Script / MCP tests** that mock collaborators use `Mock -ModuleName ISHRemote <FunctionName>
  { ... }` scoped to the module, not a global mock.

## 8. Anonymization

- [ ] No real server URLs, hostnames, IP addresses, or credentials appear in assertions, expected
  strings, or fixture data — use `https://example.com/ISHWS/` as the canonical placeholder.
- [ ] Test output printed via `Write-Host` or embedded in `Should` failure messages does not
  contain customer-identifiable data.
````
