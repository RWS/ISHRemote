---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/**/*.Tests.ps1"
description: "How ISHRemote Pester (*.Tests.ps1) acceptance tests are structured, configured, build/clean their own data, and why they cannot run in parallel."
---

# ISHRemote Pester Test Conventions

These are **Pester 5.3+ acceptance/integration tests** that run against a **live Tridion Docs
(InfoShare) tenant**. They are not unit tests — almost every `It` makes real Web Services API calls,
creates server objects, asserts, and then deletes them. Keep new tests consistent with the patterns
below or you will create flaky runs for everyone.

## Required file anatomy
Every `*.Tests.ps1` follows the same skeleton; copy it from a sibling (e.g. `AddIshUser.Tests.ps1`,
`AddIshFolder.Tests.ps1`):

```powershell
BeforeAll {
    $cmdletName = "Verb-IshNoun"            # exact cmdlet under test
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
    Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Verb-IshNoun" -Tags "Create" {   # Tag = Create | Read | Update | Delete
    BeforeAll { <# build the shared test data this Describe needs #> }
    Context "Verb-IshNoun ParameterGroup" {
        It "Parameter IshSession invalid" { { Verb-IshNoun -IshSession "INVALIDISHSESSION" } | Should -Throw }
        It "GetType().Name" { $ishObject.GetType().Name | Should -BeExactly "IshX" }
    }
}

AfterAll {
    Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
    <# delete everything this file created #>
}
```

Rules:
- The top `BeforeAll` **must** set `$cmdletName` and dot-source
  [ISHRemote.PesterSetup.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.PesterSetup.ps1)
  using the relative `\..\..\` hop (every test file is two folders below the project root).
- Name the file `XxxIshYyy.Tests.ps1` next to its `XxxIshYyy.cs`; `Describe` is the public cmdlet
  name; tag it with the CRUD verb (`Create`/`Read`/`Update`/`Delete`).
- Reuse the shared `$ishSession` created by the setup; pass `-IshSession $ishSession` explicitly and
  also cover the implicit-session and pipeline parameter sets, mirroring existing files.

## Test environment & the Debug override (tenant + list-of-values)
[ISHRemote.PesterSetup.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.PesterSetup.ps1) is
the central, committed setup. It imports the built module (`bin\debug\ISHRemote` locally,
`bin\release\ISHRemote` when `$env:GITHUB_ACTIONS -eq "true"`) and declares **shared, tenant-specific
variables** that tests depend on, e.g. `$folderTestRootPath = "\General\__ISHRemote"`, language LOV
values `$ishLng='VLANGUAGEEN'` / `$ishLngLabel='en'`, `$ishLngTarget1='VLANGUAGEES'` / `'es'`,
`$ishResolution='VRESLOW'`, status `$ishStatusDraft`/`$ishStatusReleased`, `$ishLovId='DLANGUAGE'`,
`$ishEventTypeToPurge`, etc.

It then dot-sources the git-ignored
**`ISHRemote.PesterSetup.Debug.ps1`** (if present) as the local source of truth, to **point at your
tenant and re-map those list-of-values to match that database**:

```powershell
$baseUrl            = 'https://your-tenant.example.com'
$webServicesBaseUrl = "$baseUrl/ISHWS/"   # MUST end with a trailing slash
$ishUserName = 'admin'; $ishPassword = 'admin'
$amClientId  = '...'  ; $amClientSecret = '...'
# Re-map LOV/language codes to this tenant's configuration:
$ishLngLabel='en-us'; $ishLngTarget1='VLANGUAGEESES'; $ishLngTarget1Label='es-es'
$ishLngTarget2='VLANGUAGEDEDE'; $ishLngTarget2Label='de-de'; $ishLngCombination='en-us'
$ishEventTypeToPurge='TESTBACKGROUNDTASK'
```

Why this matters: language/LOV element names and labels differ per database. A default DITA tenant
uses short codes (`VLANGUAGEEN` / `en`), while RWS demo tenants (`*.sdlproducts.com`) use
`VLANGUAGEESES` / `es-es` style codes. Tests reference the **variables**, never hard-coded language
codes, so a correct Debug override is what makes the suite green on a given tenant. When you add a
test that needs a new piece of tenant data, add a defaulted variable to `ISHRemote.PesterSetup.ps1`
(so CI/other tenants can override it) rather than hard-coding a value. **Never commit
`ISHRemote.PesterSetup.Debug.ps1`** — it holds credentials.

## Build test data per test, clean it up per test
Each file is responsible for **creating its own data and removing all of it**, leaving the tenant in
its original state:
- Build inside `BeforeAll` (per `Describe`/`Context`) or inline in the `It`. Folder tests create a
  subfolder **named after the cmdlet** under the shared root:
  `Add-IshFolder -ParentFolderId $folderIdTestRootOriginal -FolderName $cmdletName ...`.
- Clean up in the top-level `AfterAll`, scoped to this file only. Canonical patterns:
  - Folders: `Remove-IshFolder -FolderPath (Join-Path $folderTestRootPath $cmdletName) -Recurse`.
  - Users: `Find-IshUser -MetadataFilter (Set-IshMetadataFilterField -Name USERNAME -FilterOperator like -Value "$cmdletName%") | Remove-IshUser`.
  - Wrap deletes in `try { ... } catch { }` so cleanup never fails the run.
- Name created objects with `$cmdletName` + a timestamp (`Get-Date -Format "yyyyMMddHHmmssfff"`) so
  filter-based cleanup is reliable and names stay unique. When creating many objects fast, add
  `Start-Sleep -Milliseconds 1000` to dodge the second-resolution CARD unique-name constraint
  (`Cannot insert duplicate key row ... 'CARD_NAME_I1'`).

## Do NOT assume parallel execution
The tests share mutable server state, so they are effectively **serial**:
- **Hot spot = the folder named after the cmdlet.** Every file works under the same
  `\General\__ISHRemote` root and creates `\General\__ISHRemote\<Cmdlet>`. Two files for the same/
  overlapping cmdlet folder, or a `-Recurse` cleanup, will collide if run concurrently.
- **Settings tests mutate global configuration** (field setup / metadata-bound fields,
  `FISHEXTENSIONCONFIG`, output formats, LOV values, background-task/event setup). Those changes are
  server-wide, so they **interfere with content-object tests and cause race-condition flakiness** if
  run at the same time.
- Because of this, [ISHRemote.PesterSetup.Run.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.PesterSetup.Run.ps1)
  forces `*IshOutputFormat*`, `*IshLovValue*`, `*IshBackgroundTask*`, `*IshEvent*` and `*IshUserRole*`
  to run **serially**, only the rest in parallel jobs. CI runs the whole suite serially.
- Keep every test **self-contained and order-independent**: create what you need, assert, delete it,
  and don't rely on objects another file produced.
