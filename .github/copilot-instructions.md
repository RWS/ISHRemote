# ISHRemote — Copilot Cloud Agent Instructions

Trust these instructions first. Only search the codebase if something here is incomplete or proven wrong.

## What this repository is
ISHRemote is a **binary PowerShell module** for business automation on top of RWS Tridion Docs
Content Manager (InfoShare CMS). It is a thin client over the CMS "Web Services API" (WCF SOAP +
OpenAPI protected by OpenID Connect) and is published to the PowerShell Gallery. Most cmdlets are C# classes;
a few advanced functions are PowerShell scripts. Cmdlets follow `Verb-IshNoun` naming.

- **Languages/runtimes:** C# multi-targeted `net48;net6.0;net10.0`; PowerShell 5.1 (Windows
  PowerShell / .NET Framework 4.8) and PowerShell 7+ (CoreCLR, .NET 6/10).
- **Size:** Medium. 3 C# projects, ~150+ cmdlet `.cs` files, ~58 `*.Tests.ps1` Pester files.
- **Build is Windows-only.** Targeting `net48` plus MSBuild targets that invoke `powershell.exe`
  (Windows PowerShell 5.1) to generate the module manifest mean a full build requires Windows +
  Visual Studio 2022 / MSBuild. CI runs on `windows-latest`.

## Project layout (where to make changes)
- `Source/ISHRemote/ISHRemote.sln` — the solution (3 projects).
- `Source/ISHRemote/Directory.Build.props` — **version numbers, .NET analyzers, and
  `TreatWarningsAsErrors=true` for `Release`**. Read this before touching versions or warnings.
- `Source/ISHRemote/Trisoft.ISHRemote/` — the main module project (`Trisoft.ISHRemote.csproj`):
  - `Cmdlets/<Domain>/` — C# cmdlets grouped by domain (User, Folder, DocumentObj, Session,
    Settings, etc.). Each cmdlet is `XxxIshYyy.cs` with a matching `XxxIshYyy.Tests.ps1` and a
    per-domain base class (e.g. `UserCmdlet.cs`). **Add a new cmdlet here + its `.Tests.ps1`.**
  - `Objects/`, `Connection/`, `HelperClasses/`, `ExtensionMethods/`, `Interfaces/`, `Exceptions/`.
  - `Scripts/Public/` and `Scripts/Private/` — PowerShell advanced functions (MCP server,
    parameter completion). **These are what PSScriptAnalyzer lints.**
  - `ISHRemote.psm1` — root module; loads the right `net48`/`net6.0`/`net10.0` DLL by PS edition.
  - `ISHRemote.Format.ps1xml` — output formatting.
  - `ISHRemote.PesterSetup.ps1` — central test setup, dot-sourced by every `*.Tests.ps1`.
  - `ISHRemote.PesterSetup.Debug.ps1` — git-ignored local test credentials/overrides.
- `Source/ISHRemote/Trisoft.ISHRemote.OpenApiAM10/` and `.OpenApiISH30/` — `netstandard2.0`
  NSwag-generated OpenAPI clients. Generated code; do not hand-edit the `*.json`-derived clients.
- `.editorconfig` — CRLF, final newline, indent 2 spaces (C# and `.ps1` use 4).

## How to build (validated against CI — always do these in order)
1. **Always restore the whole solution first** (fixes the transient multi-target `ProjectReference`
   errors `CS0234`/`CS0518` described in README; restoring the `.sln` is enough):
   ```
   dotnet restore Source/ISHRemote/ISHRemote.sln
   ```
2. **Build:**
   ```
   dotnet build --no-restore --no-incremental --configuration release Source/ISHRemote/ISHRemote.sln
   ```
   - Use `Debug` instead of `Release` for local iteration; the Pester tests load `bin\debug\ISHRemote`
     when not in CI and `bin\release\ISHRemote` when `$env:GITHUB_ACTIONS -eq "true"`.
   - **Release fails on any compiler/analyzer/XML-doc warning** (`TreatWarningsAsErrors=true` in
     Directory.Build.props). Keep the tree warning-clean. `NoWarn=1591` already suppresses
     "missing XML comment".
   - If the first build ever fails with `CS0234`/`CS0518` (namespace/predefined type not found),
     just **restore and build again** — it resolves once project dependencies are restored.
   - The `net48` target runs extra MSBuild targets (XmlDoc2CmdletDoc help generation, module
     manifest, packaging into `bin\<Config>\ISHRemote\` with `net48`/`net6.0`/`net10.0` subfolders).
     These require **both** `pwsh.exe` (PowerShell 7) and `powershell.exe` (Windows PowerShell 5.1).

## How to lint (validated against CI)
```powershell
Set-PSRepository PSGallery -InstallationPolicy Trusted
Install-Module PSScriptAnalyzer -ErrorAction Stop
Invoke-ScriptAnalyzer -Path Source/ISHRemote/Trisoft.ISHRemote/Scripts -Recurse
```
CI reports analyzer warnings/errors but does **not** fail the build on them. Still, keep
`Scripts/**` clean.

## How to test (Pester) — important constraints
- **The Pester suite is acceptance/integration tests that require a LIVE Tridion Docs server.** For
  local runs put the endpoint/credentials in `ISHRemote.PesterSetup.Debug.ps1` (git-ignored, the
  source of truth locally). The `ISH_BASE_URL`, `ISH_USER_NAME`, `ISH_PASSWORD`, `ISH_CLIENT_ID`,
  `ISH_CLIENT_SECRET` env vars are for CI (GitHub secrets) — don't override local debug settings
  with them for normal local runs. Without a reachable server, **most tests cannot pass** — do not
  assume a green run locally.
- **Build before testing.** Tests import the compiled module from `bin\debug\ISHRemote` (local) or
  `bin\release\ISHRemote` (CI, when `$env:GITHUB_ACTIONS -eq "true"`). `TestPrerequisite.Tests.ps1`
  verifies the packaged module (`.psd1`, `Trisoft.ISHRemote.dll-Help.xml`, and
  `net48`/`net6.0`/`net10.0` folders) exists.
- Use **Pester 5.3.0+** (not the Windows-bundled 3.4.0): `Install-Module -Name Pester -Force
  -SkipPublisherCheck`.
- **Run order (fast → broad), from repo root:**
  1. Prerequisite check first:
     `Invoke-Pester -Path Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/_TestEnvironment/TestPrerequisite.Tests.ps1 -Output Detailed`
  2. Targeted while iterating — a single file or one domain folder:
     `Invoke-Pester -Path Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/<Domain>/ -Output Detailed`
  3. Full CI-pattern suite only when needed (note the excluded slow test):
     `Invoke-Pester -Path @('Source/ISHRemote/Trisoft.ISHRemote/Scripts/Public/','Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/') -ExcludePath @('*GetIshDocumentObj.Tests.ps1') -Output Detailed`
- **Match CI per shell:** under `pwsh` run both `Scripts/Public` and `Cmdlets`; under Windows
  PowerShell 5.1 (`powershell`) run `Cmdlets` only. Both exclude `*GetIshDocumentObj.Tests.ps1`.

## CI pipeline (.github/workflows/continuous-integration.yml, runs on windows-latest)
Triggers on push/PR to `master` touching `Source/**`, `*.TXT`, `*.MD`. Order:
1. Update PowerShell to latest stable → `actions/checkout` → setup .NET 6.0.x.
2. `dotnet restore Source/ISHRemote/ISHRemote.sln`
3. `dotnet build --no-restore --no-incremental --configuration release Source/ISHRemote/ISHRemote.sln`
4. Install + run PSScriptAnalyzer on `Scripts`.
5. Pester (PowerShell 7.x and Windows PowerShell 5.1) against the live server secrets.
6. Optional publish to PSGallery only when the commit message contains
   `[PublishToPSGalleryAsPreview]` or `[PublishToPSGalleryAsRelease]`.

**To replicate CI confidence before opening a PR: restore → Release build (must be warning-clean) →
PSScriptAnalyzer on Scripts.** That is what the agent can realistically validate without a CMS.

## Runtime architecture (how it fits together)
- `IshSession` (`Objects/Public/IshSession.cs`) is the central runtime object: it loads
  `connectionconfiguration.xml`, selects a protocol (`WcfSoapWithWsTrust`,
  `WcfSoapWithOpenIdConnect`, `OpenApiWithOpenIdConnect`), manages the token/connection lifecycle,
  and exposes both SOAP proxies (the `*25` clients, e.g. `IshSession.User25`) and OpenAPI clients
  (`OpenApiISH30Client`, `OpenApiAM10Client`). SOAP is still initialized even under OpenAPI because
  many cmdlets still depend on it.
- `ISHRemote.psm1` loads the runtime-matching DLL: PowerShell Desktop → `net48`; PowerShell
  7.2–7.5 → `net6.0`; PowerShell 7.6+ → `net10.0`.
- The `net48` build packages everything into `bin/<Config>/ISHRemote/` (framework subfolders +
  `Scripts/` + generated `ISHRemote.psd1` and `Trisoft.ISHRemote.dll-Help.xml`); that folder is the
  published module and exactly what the tests import.
- MCP server/tool registration lives in `Scripts/Public` (`Start-IshRemoteMcpServer`,
  `Register-IshRemoteMcpTool`, `Invoke-IshRemoteMcpHandleRequest`) and is wired for local use via
  `.vscode/mcp.json`.

## Legacy & where to invest less (protocol direction)
ISHRemote must keep working across a wide range of InfoShare versions, but **not all protocols get
equal future investment.** When adding or rewiring functionality, bias new work toward the OpenAPI
path and treat the older paths as maintain-only.
- **`WcfSoapWithWsTrust` (WS-Trust / WS-Federation) — legacy, maintain-only.** It works and is the
  primary protocol on **older InfoShare (e.g. 13.0.2, 14.0.4)**, so keep it functioning, but it is
  **not extended** — bug-fix only. The product **removes it in 16.0.0**.
- **SOAP proxies are deprecated by the product.** Most cmdlets are still SOAP-based; that is history,
  not direction. `WcfSoapWithOpenIdConnect` reaches **many customers across 15.x** and **remains in
  16.0.0**, so it is a pragmatic target — but it is **deprecated**.
- **`OpenApiWithOpenIdConnect` (REST) is the future — invest here.** Surface arrived in 15.0.0,
  was enriched in 15.1.0/15.2.0, and reaches **functional parity with `WcfSoapWithOpenIdConnect` at
  15.3.0**. When an OpenAPI implementation is missing, `IshSession` falls back to
  `WcfSoapWithOpenIdConnect`, so new/rewired cmdlets should prefer OpenAPI where server parity exists
  and fall back deliberately. When unsure which path a change should take, **ask the implementer.**

### Rarely-touched, low-investment files (don't treat as current)
- `Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.SignAndPublish.ps1` (and the git-ignored
  `…SignAndPublish.Debug.ps1`) — manual sign-and-publish helper; almost never run (CI publishes via
  the `[PublishToPSGalleryAsPreview]`/`[PublishToPSGalleryAsRelease]` commit-message trigger). Don't
  wire it into normal flows.
- `Source/Tools/` (`Add-SDLOpenSourceHeader.ps1` / `Test-SDLOpenSourceHeader.ps1`) — license-header
  tooling, run rarely / ad-hoc.
- `BACKLOG.MD` — the **pre-GitHub** backlog; historical, superseded by GitHub Issues. Don't treat it
  as the current roadmap.

## C# cmdlet conventions & patterns (avoid breaking the build / help generation)
- Every cmdlet needs triple-slash `///` XML doc comments — they drive `Get-Help`. Include
  `<para type="synopsis">`, `<para type="description">`, and at least one `<example>`.
- **XmlDoc2CmdletDoc gotchas (cause `net48` build failures):** any property used as a `[Parameter]`
  must have a **getter** (not setter-only); `[OutputType(...)]` must use `typeof(IshX)`, not
  `nameof(IshX)`.
- Parameter names are **singular** (`IshObject`, `FilePath`), not plural.
- Implement `-WhatIf`/`-Confirm` (`SupportsShouldProcess = true`) for write operations
  (`Add/Set/Move/Remove/Publish/Stop`).
- Preserve backward compatibility; if you must rename, add a `Set-Alias` and keep help.
- Add a matching `*.Tests.ps1` next to any new cmdlet.
- **Session resolution in `BeginProcessing`:** use `-IshSession` if set, else SessionState variable
  `ISHRemoteSessionStateIshSession`, else `global:ISHRemoteSessionStateIshSession`, else throw —
  copy this from an existing cmdlet such as `AddIshUser.cs`.
- **Metadata:** convert via `IshSession.IshTypeFieldSetup.ToIshMetadataFields` /
  `ToIshRequestedMetadataFields`; do not hand-build metadata XML.
- **Return shape:** after a write, retrieve and return typed `Ish*` objects (not raw service
  responses), shaped by `DefaultRequestedMetadata` / `PipelineObjectPreference`.
- **Exception handling:** keep the standard catch order `TrisoftAutomationException` →
  `AggregateException` → `TimeoutException` → `CommunicationException` → `Exception`, each calling
  `ThrowTerminatingError(new ErrorRecord(...))`.

## Don't
- Don't add a `global.json` (none exists; the build relies on the SDKs present on the runner).
- Don't hand-edit generated OpenAPI client code or commit `ISHRemote.PesterSetup.Debug.ps1`,
  `launchSettings.json`, or `*.psd1` (git-ignored / generated).
- Don't expect `dotnet test` — there are no MSTest/xUnit projects; testing is Pester only.
