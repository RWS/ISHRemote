---
applyTo: ".github/workflows/**"
description: "The three ISHRemote CI workflow files: their trigger scopes, job structure, concurrency model, and the minimum pre-PR validation an AI coding agent can run locally without a live server."
---

# ISHRemote CI Workflow Conventions

There are three separate workflows, each with its own trigger scope. Never collapse them into one.

## `continuous-integration.yml` (`windows-latest`, secrets available)

Triggers on push/PR to `master` touching `Source/**`, `*.TXT`, `*.MD`. Two jobs:

1. **`build`** (fast lane, no live-server secrets): UpdatePWSH → checkout → `dotnet restore
   Source/ISHRemote/ISHRemote.sln` → `dotnet build --no-restore --no-incremental --configuration
   release` → PSScriptAnalyzer on `Scripts/` → Pester anonymization check → upload compiled module
   artifact.
2. **`live`** (`needs: build`, `concurrency: run-tests-on-ishbaseurl`, queues — never cancels):
   downloads artifact → Pester PS7 (Scripts/Public + Cmdlets, excludes `*GetIshDocumentObj*` and
   `*TestAnonymization*`) → Pester PS5.1 (Cmdlets only, same excludes) → optional PSGallery publish
   when commit message contains `[PublishToPSGalleryAsPreview]` or `[PublishToPSGalleryAsRelease]`.

The `concurrency` group `run-tests-on-ishbaseurl` serialises runs against the mutable live server —
**never add `cancel-in-progress: true`**; that would corrupt server state mid-test.

## `code-quality.yml` (`ubuntu-latest`, no secrets)

Triggers on push/PR touching `Doc/**`, `Samples/**`,
`*TestAnonymization.Tests.ps1`, `*TestAiInstructions.Tests.ps1`, `.github/instructions/**`,
`.github/copilot-instructions.md`, `.claude/**`, `AGENTS.md`, `CLAUDE.md`, or the workflow file
itself. Parallel jobs (no `needs:` dependency between them):

- **`anonymization`**: installs Pester 5.3+, runs `TestAnonymization.Tests.ps1`, throws on failure.
- **`ai-instructions`**: installs Pester 5.3+, runs `TestAiInstructions.Tests.ps1`, throws on
  failure.

No build, no live server. Runs on `ubuntu-latest` to keep it fast and free of Windows-only overhead.

## `codeql.yml` (`ubuntu-latest`, no secrets)

Triggers on push/PR touching `Source/**/*.cs`, the workflow file, plus a weekly Monday schedule.
`build-mode: none` scans sources directly without a Windows build. Requires Code Scanning to be
**disabled** in repository Settings → Code Security and Analysis to avoid SARIF conflicts.

## Pre-PR validation (what an AI coding agent can verify locally)

```
dotnet restore Source/ISHRemote/ISHRemote.sln
dotnet build --no-restore --no-incremental --configuration release Source/ISHRemote/ISHRemote.sln
Invoke-ScriptAnalyzer -Path Source/ISHRemote/Trisoft.ISHRemote/Scripts -Recurse
```

That is the full `build` job. The `live` job requires live-server secrets and cannot be replicated
locally without a Tridion Docs instance.

## Editing these workflows

- Keep `permissions: contents: read` on every workflow (least privilege).
- Pin `actions/*` to a specific version tag; update via Dependabot (commit prefix `ci`).
- Do not add a `global.json` — the build relies on the SDKs present on the runner.
- Match CI shell targets: `pwsh` runs Scripts/Public + Cmdlets; `powershell` (5.1) runs Cmdlets only.
