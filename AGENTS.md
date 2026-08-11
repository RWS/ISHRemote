# ISHRemote — AI Coding Agent Instructions

Trust these instructions first. Only search the codebase if something here is incomplete or proven wrong.

## What this repository is

ISHRemote is a **binary PowerShell module** for business automation on top of RWS Tridion Docs
Content Manager (InfoShare CMS). It is a thin client over the CMS "Web Services API" (WCF SOAP +
OpenAPI protected by OpenID Connect) and is published to the PowerShell Gallery. Most cmdlets are
C# classes; a few advanced functions are PowerShell scripts. Cmdlets follow `Verb-IshNoun` naming.

- **Languages/runtimes:** C# multi-targeted `net48;net6.0;net10.0`; PowerShell 5.1 (Windows
  PowerShell / .NET Framework 4.8) and PowerShell 7+ (CoreCLR, .NET 6/10).
- **Build is Windows-only.** Targeting `net48` plus MSBuild targets that invoke `powershell.exe`
  (Windows PowerShell 5.1). CI runs on `windows-latest`.

## Project layout (non-obvious parts only)

- `Source/ISHRemote/Directory.Build.props` — **version numbers, .NET analyzers, and
  `TreatWarningsAsErrors=true` for `Release`**. Read before touching versions or warnings.
- `Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.PesterSetup.ps1` — dot-sourced by every
  `*.Tests.ps1`; provides `$ishSession` and all shared tenant variables.
- `Source/ISHRemote/Trisoft.ISHRemote.OpenApiAM10/` and `.OpenApiISH30/` — NSwag-generated OpenAPI
  clients. **Do not hand-edit** the `*.json`-derived clients.

## How to build (validated against CI — always in this order)

1. **Restore first** (prevents transient `CS0234`/`CS0518` multi-target errors — if they appear,
   restore and build again; they resolve once dependencies are restored):
   ```
   dotnet restore Source/ISHRemote/ISHRemote.sln
   ```
2. **Build:**
   ```
   dotnet build --no-restore --no-incremental --configuration release Source/ISHRemote/ISHRemote.sln
   ```
   - Use `Debug` for local iteration; tests load `bin\debug\ISHRemote` locally and `bin\release\ISHRemote` in CI.
   - **Release fails on any warning** (`TreatWarningsAsErrors=true`). Keep the tree warning-clean.
   - The `net48` target requires **both** `pwsh.exe` (PowerShell 7) and `powershell.exe` (Windows PowerShell 5.1).

## How to lint

```powershell
Set-PSRepository PSGallery -InstallationPolicy Trusted
Install-Module PSScriptAnalyzer -ErrorAction Stop
Invoke-ScriptAnalyzer -Path Source/ISHRemote/Trisoft.ISHRemote/Scripts -Recurse
```

CI reports analyzer findings but does not fail the build on them. Still, keep `Scripts/**` clean.

## How to test (Pester) — critical constraints

- **The Pester suite requires a LIVE Tridion Docs server.** Put credentials in
  `ISHRemote.PesterSetup.Debug.ps1` (git-ignored). The `ISH_*` env vars are for CI secrets.
  Without a reachable server, most tests cannot pass.
- **Build before testing.** Tests import the compiled module from `bin\debug\ISHRemote` (local) or
  `bin\release\ISHRemote` (CI).
- Use **Pester 5.3.0+**: `Install-Module -Name Pester -Force -SkipPublisherCheck`.
- **Run order (fast → broad), from repo root:**
  1. `Invoke-Pester -Path Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/_TestEnvironment/TestPrerequisite.Tests.ps1 -Output Detailed`
  2. `Invoke-Pester -Path Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/<Domain>/ -Output Detailed`
  3. `Invoke-Pester -Path @('Source/ISHRemote/Trisoft.ISHRemote/Scripts/Public/','Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/') -ExcludePath @('*GetIshDocumentObj.Tests.ps1') -Output Detailed`
- Match CI shell: `pwsh` runs Scripts/Public + Cmdlets; Windows PowerShell 5.1 runs Cmdlets only.

## Pre-PR validation (what you can verify without a live server)

```
dotnet restore Source/ISHRemote/ISHRemote.sln
dotnet build --no-restore --no-incremental --configuration release Source/ISHRemote/ISHRemote.sln
Invoke-ScriptAnalyzer -Path Source/ISHRemote/Trisoft.ISHRemote/Scripts -Recurse
```

## Runtime architecture

- `IshSession` (`Objects/Public/IshSession.cs`) selects a protocol (`WcfSoapWithWsTrust`,
  `WcfSoapWithOpenIdConnect`, `OpenApiWithOpenIdConnect`) and exposes both SOAP proxies and OpenAPI
  clients. Which call to use is documented in
  `.github/instructions/source-api-webservices--csharp.instructions.md`.
- `ISHRemote.psm1` loads the runtime-matching DLL: PowerShell Desktop → `net48`; PS 7.2–7.5 →
  `net6.0`; PS 7.6+ → `net10.0`.
- Tridion Docs domain knowledge (entity hierarchy, field types, filter operators, MCP agent rules)
  is single-sourced in `Doc/McpInstructions-ISHRemote.md`; read it when working on MCP, cmdlet, or
  object code that involves CMS entities or field definitions.

## Protocol direction — where to invest

- **`WcfSoapWithWsTrust` — legacy, maintain-only.** Bug-fix only; removed in product 16.0.0.
- **`WcfSoapWithOpenIdConnect` — deprecated.** Reaches many 15.x customers, remains in 16.0.0;
  pragmatic target but not extended.
- **`OpenApiWithOpenIdConnect` (REST) — the future.** Invest here. Parity with SOAP at 15.3.0.
  New/rewired cmdlets prefer OpenAPI where server parity exists; fall back deliberately to SOAP.
  When unsure, **ask the implementer.**

## Rarely-touched files — do not treat as current

- `ISHRemote.SignAndPublish.ps1` — manual sign/publish helper; CI publishes via commit-message trigger.
- `Source/Tools/` — license-header tooling, run ad-hoc.
- `BACKLOG.MD` — **pre-GitHub** backlog; historical, superseded by GitHub Issues.

## Two build-breakers repeated here (surface across all tasks)

- `[OutputType(typeof(IshX))]` — always `typeof`, **never** `nameof` (`XmlDoc2CmdletDoc` crashes on
  `nameof` at `net48` build time).
- Any `[Parameter]` property **must have a getter**; setter-only silently breaks help generation.

## Don't

- Don't add a `global.json` (none exists; build relies on SDKs present on the runner).
- Don't hand-edit generated OpenAPI clients or commit `ISHRemote.PesterSetup.Debug.ps1`,
  `launchSettings.json`, or `*.psd1` (git-ignored / generated).
- Don't use `dotnet test` — testing is Pester only; there are no MSTest/xUnit projects.

## Scoped instructions — load on demand

CRITICAL: The files below are mandatory instructions, not background reading. When your task touches
a matching path, use your Read tool to load that file **before** editing. Do NOT preemptively load
all of them — use lazy loading based on actual need. Once loaded, treat the content as overriding
your defaults.

| When you touch | Read this file |
|---|---|
| `Doc/**/*.md` | @.github/instructions/doc--markdown.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/**/*.cs` (API choice) | @.github/instructions/source-api-webservices--csharp.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/**/*.cs` (authoring) | @.github/instructions/source-cmdlets--csharp.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/**/*.Tests.ps1` (authoring) | @.github/instructions/source-cmdlets--pester.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/**/*.cs` (reviewing) | @.github/instructions/source-codereview-csharp.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/**/*.Tests.ps1` (reviewing) | @.github/instructions/source-codereview-pester.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/Scripts/**/*.ps1` (reviewing) | @.github/instructions/source-codereview-powershell.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/Connection/**/*.cs` | @.github/instructions/source-connection--csharp.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/**/*.cs` | @.github/instructions/source-extensionmethods--csharp.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/Objects/**/*.cs` | @.github/instructions/source-objects--csharp.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/Samples/**/*` | @.github/instructions/source-samples--mixed.instructions.md |
| `Source/ISHRemote/Trisoft.ISHRemote/Scripts/**/*.*` (authoring) | @.github/instructions/source-scripts--powershell.instructions.md |
| `.github/workflows/**` | @.github/instructions/ci--workflows.instructions.md |
| `.github/**` (issues, PRs, titles) | @.github/instructions/repo--issues-and-pullrequests.instructions.md |

## Maintaining these instructions

This repository has **14 canonical instruction domains**, each represented by exactly three
coordinated artefacts. **`TestAiInstructions.Tests.ps1` enforces all invariants below — run it
after any change.**

| Artefact | Location | Purpose |
|---|---|---|
| Canonical file | `.github/instructions/<name>.instructions.md` | Single source of content |
| AI coding agent stub | `.claude/rules/<name>.md` | Path-scoped injection for Claude Code-compatible harnesses |
| `AGENTS.md` row | The `@`-ref table above | Lazy-load trigger for OpenCode-compatible harnesses |

**Rules — any violation is a defect:**
1. The 14 domain names are fixed. Adding one requires **three** coordinated edits across all three
   artefacts. Removing one requires the same.
2. Canonical filename `<name>.instructions.md` maps 1:1 to stub filename `<name>.md`.
3. The `applyTo:` glob in the canonical file and the `paths:` glob in the stub must express the
   same path pattern.
4. Every canonical file must appear as an `@`-ref in the table above.
5. Canonical files must have `---` at line 1 (YAML frontmatter), never a code fence.
6. Prose must not refer to any specific AI harness by product name. Use "AI coding agent" instead.
