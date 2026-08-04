---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/**/*.cs"
description: "Reviewer's checklist for ISHRemote C# pull requests: build hygiene, file/type naming, session lifecycle, pipeline shape, help generation, exception handling, and backward compatibility."
---

# ISHRemote C# Code Review Checklist

Use this when reviewing a PR that touches `.cs` files. Each item is a binary pass/flag.
Authoring detail for each topic lives in the companion instruction files injected per path:
[`source-cmdlets--csharp.instructions.md`](source-cmdlets--csharp.instructions.md),
[`source-objects--csharp.instructions.md`](source-objects--csharp.instructions.md),
[`source-connection--csharp.instructions.md`](source-connection--csharp.instructions.md),
[`source-extensionmethods--csharp.instructions.md`](source-extensionmethods--csharp.instructions.md).

## 1. Build hygiene

- [ ] Release build passes with **zero warnings** — `TreatWarningsAsErrors=true` is set in
  `Directory.Build.props`; any new warning is a Release build failure.
- [ ] `[OutputType(typeof(IshX))]` uses `typeof`, **not** `nameof` — `nameof` silently skips help
  generation at `net48` build time (XmlDoc2CmdletDoc limitation).
- [ ] Every property decorated `[Parameter]` has a **getter** — a setter-only property crashes
  help generation during the `net48` MSBuild targets.
- [ ] Apache 2.0 license header is present and verbatim (same text as siblings).

## 2. File, class, and type naming

- [ ] File name = class name = `VerbIshNoun` (no hyphen): `AddIshUser.cs` → class `AddIshUser` →
  `[Cmdlet(VerbsCommon.Add, "IshUser", ...)]`. All three must match exactly.
- [ ] Verb uses the .NET constant (`VerbsCommon.Get`, `VerbsLifecycle.Stop`, etc.) — never a raw
  string literal.
- [ ] Class is `sealed` and inherits the **per-domain base class** (`UserCmdlet`, `FolderCmdlet`,
  …); a new cmdlet in a domain that has no base class needs a new base class matching siblings.
- [ ] Parameter names are **singular**: `IshObject`, `FilePath`, `Id` — never `IshObjects`,
  `FilePaths`, `Ids`.
- [ ] Parameter set names follow the singular `<Concept>Group` pattern; the canonical pair is
  `ParameterGroup` / `IshObjectGroup`. Any new set name must be discussed with the implementer.
- [ ] `DefaultParameterSetName` is set whenever more than one parameter set exists.

## 3. Session resolution

- [ ] Session is resolved in **`BeginProcessing`**, not in `ProcessRecord`, using this exact
  three-fallback chain (copied verbatim from a sibling such as `AddIshUser.cs`):
  ```csharp
  if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
  if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
  if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
  ```

## 4. Pipeline shape and throughput

- [ ] Write cmdlets (`Add/Set/Move/Remove/Publish/Stop`) have `SupportsShouldProcess = true` and
  wrap the server call in `if (ShouldProcess(target)) { … }`.
- [ ] After a write the cmdlet **retrieves and returns** typed `Ish*` objects shaped by
  `DefaultRequestedMetadata` / `PipelineObjectPreference` — raw service responses are never emitted
  to the pipeline.
- [ ] Pipeline input is **accumulated in `ProcessRecord`** into a private field and flushed as
  grouped/batch calls in `EndProcessing` where a batch API exists; per-item calls are only
  acceptable when the server exposes no batch endpoint.
- [ ] Batching uses `DivideListInBatches<T>(list, IshSession.MetadataBatchSize)` (or `BatchSize`
  for object cards) — no unbounded single server calls.
- [ ] Metadata is converted via `IshSession.IshTypeFieldSetup.ToIshMetadataFields` /
  `ToIshRequestedMetadataFields`; hand-built metadata XML is a bug.

## 5. `Get-Help` / triple-slash documentation

- [ ] The cmdlet class XML doc block contains:
  - `<para type="synopsis">` — one-line summary
  - `<para type="description">` — full description
  - At least one `<example>` with `<code>` and an explanatory `<para>`
- [ ] Examples show **only the cmdlet itself** — no `New-IshSession` call (it leaks server URLs
  and credential patterns).
- [ ] Every `[Parameter]` property has `<para type="description">` with wording consistent with
  siblings for the same parameter name.

## 6. Exception handling

- [ ] Catch ladder is in this exact order:
  `TrisoftAutomationException` → `AggregateException` → `TimeoutException` →
  `CommunicationException` → `Exception`.
- [ ] Every catch calls `ThrowTerminatingError(new ErrorRecord(e, base.GetType().Name,
  ErrorCategory.XXX, null))` with the correct `ErrorCategory` — no silent swallows, no plain
  `throw`.
- [ ] The order is not reordered, collapsed, or partially removed without explicit sign-off from
  the implementer.

## 7. Backward compatibility and protocol

- [ ] No public `Ish*` member or cmdlet parameter is **renamed** without both a `Set-Alias` kept
  in the cmdlet and an explicit breaking-change flag in the PR description.
- [ ] New functionality prefers the **OpenAPI** path (`OpenApiWithOpenIdConnect`) where server
  parity exists (15.3.0+). A SOAP-only implementation for new work needs an explicit justification
  from the implementer.

## 8. Test coverage

- [ ] Every new cmdlet has a matching `*.Tests.ps1` in the same domain folder.
- [ ] `ISHRemote.PesterSetup.Debug.ps1` is **not** committed (it is git-ignored and contains live
  credentials).
- [ ] Any new server-less test file does **not** dot-source `ISHRemote.PesterSetup.ps1` (that
  file creates an `$ishSession` and requires a live server).

## 9. Diagnostic logging density

A `-Debug` or `-Verbose` transcript must be enough to reconstruct what happened and diagnose a
ticket without requiring reproduction on a live server. Check each log point.

- [ ] `BeginProcessing` ends with the session-confirmation `WriteDebug`:
  ```csharp
  WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");
  ```
- [ ] Major phases are marked with `WriteDebug`: `"Validating"` → `"Adding"` / `"Updating"` /
  `"Deleting"` → `"Retrieving"`.
- [ ] Empty-collection early exits surface both skipped actions via `WriteVerbose`:
  `"IshObject is empty, so nothing to update"` / `"...to retrieve"`.
- [ ] Per-item loops log the key identifier(s) and `{++current}/{total}` at `WriteDebug` level.
- [ ] Pre-call parameter summaries (IDs, metadata XML length, filter lengths) are logged at
  `WriteDebug` level before the server call.
- [ ] Post-operation result counts are logged at `WriteVerbose` level just before `WriteObject`:
  `"returned object count[{n}]"`.
- [ ] `TimeoutException` and `CommunicationException` catches log
  `"... Message[{msg}] StackTrace[{stack}]"` via `WriteVerbose` before `ThrowTerminatingError`.
- [ ] `AggregateException` catch logs the flattened exception via `WriteWarning` before
  `ThrowTerminatingError`.
- [ ] Bare `Exception` catch logs `exception.InnerException.ToString()` via `WriteWarning` when
  `InnerException != null`, before `ThrowTerminatingError`.
- [ ] OpenAPI exception catch (`OpenApiISH30Exception<InfoShareProblemDetails>`) surfaces each
  structured error field via `WriteWarning` before `ThrowTerminatingError`.
- [ ] **No password, client secret, bearer token, or personal data appears in any log stream** —
  log `.Length` or a masked replacement (`new string('*', value.Length)`) to confirm presence
  without exposure.
- [ ] No `Write*` method (`WriteDebug`, `WriteVerbose`, `WriteWarning`, `WriteProgress`,
  `WriteObject`, `ThrowTerminatingError`) is called from a non-pipeline thread — all OpenAPI async
  calls use `.GetAwaiter().GetResult()` and no `Task.Run()` / `ContinueWith()` lambdas contain
  any stream writes.
