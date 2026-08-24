---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/**/*.cs"
description: "Structure and conventions for ISHRemote C# cmdlet files: header, naming, parameters, parameter sets, triple-slash help, the BeginProcessing/ProcessRecord/EndProcessing lifecycle with batching, and the tuned exception handling."
---

# ISHRemote C# Cmdlet Conventions

Every cmdlet is one `XxxIshYyy.cs` file under `Cmdlets/<Domain>/`. Consistency across files is the
whole point — these classes drive `Get-Help` and the day-to-day pipeline experience. Before writing
a new cmdlet, read 2–3 siblings in the same domain **and** the same verb in other domains, then
match them. When a design choice is ambiguous (a new parameter, a new parameter set, batching vs
per-record), **ask the implementer what they want** rather than guessing.

> For the reviewer's binary pass/flag checklist see
> [`source-codereview-csharp.instructions.md`](source-codereview-csharp.instructions.md).

> **Protocol direction (where to invest).** Most cmdlets call the SOAP `*25` proxies, which the
> product now considers **deprecated**. For new or rewired cmdlets, prefer the **OpenAPI**
> (`OpenApiWithOpenIdConnect`) path where server parity exists (15.3.0+), falling back to
> `WcfSoapWithOpenIdConnect`; keep `WcfSoapWithWsTrust` working for older InfoShare (≤14.x) but don't
> build new features solely on it. See the repo-wide `.github/copilot-instructions.md` "Legacy &
> where to invest less". When unsure which path a change should take, **ask the implementer.**

> **Which API call to use, how to read the compatibility table, and how to navigate live docs** —
> see [`source-api-webservices--csharp.instructions.md`](source-api-webservices--csharp.instructions.md)
> (auto-injected when editing `Cmdlets/**/*.cs`).

## 1. License header (mandatory, verbatim)
Every `.cs` starts with the Apache 2.0 header exactly as in neighbouring files — copy it, don't
re-type or "modernize" the year/entity text (it is historical and tooling checks for it, see
[Add-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Add-SDLOpenSourceHeader.ps1) /
[Test-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Test-SDLOpenSourceHeader.ps1)):

```csharp
/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* ... Apache License, Version 2.0 ...
*/
```

## 2. File / class / type naming
- File name = `Verb` + `IshNoun` with no hyphen: `Add-IshUser` → `AddIshUser.cs`; the **class name
  must equal the file name**, and `[Cmdlet(VerbsCommon.Add, "IshUser", ...)]` must spell the same.
- Use the correct .NET verb constant (`VerbsCommon.Get/Add/Set/Remove/New`, `VerbsLifecycle.Stop`,
  etc.) — never a raw string verb.
- The class is `sealed` and inherits the **per-domain base class** (e.g. `UserCmdlet`,
  `BaselineCmdlet`), which supplies `public Enumerations.ISHType[] ISHType { get; }`. Put a new
  cmdlet in the matching domain folder so it picks up that base.
- `[OutputType(typeof(IshX))]` — always `typeof(...)`, never `nameof(...)` (XmlDoc2CmdletDoc fails on
  `nameof`). Write cmdlets set `SupportsShouldProcess = true`; read cmdlets set it `false`.

## 3. Parameters — naming & shape consistency is critical
Parameter names are part of the public contract; pick the name an existing cmdlet already uses for
the same concept and reuse it **exactly**, both within the domain and across domains.
- **Singular names**, even for arrays: `IshObject`, `IshFolder`, `FilePath`, `Id`, `MetadataFilter`,
  `RequestedMetadata`, `Metadata`, `Name`. Never `IshObjects`/`FilePaths`.
- **Standard types & validators (match these):**
  - `IshSession IshSession` — `Mandatory = false`, repeated once per parameter set,
    `[ValidateNotNullOrEmpty]`. Resolved in `BeginProcessing` (see §5), so users rarely pass it.
  - Pipeline objects: `IshObject[] IshObject` — `Mandatory = true, ValueFromPipeline = true`,
    `[AllowEmptyCollection]` (so an empty array is a documented no-op, not an error).
  - Identifier lists: `string[] Id`, `[ValidateNotNullOrEmpty]`.
  - Field arrays: `IshField[] MetadataFilter` / `IshField[] RequestedMetadata` / `IshField[]
    Metadata`, `[ValidateNotNull]`.
  - Scalars like `string Name` are `[ValidateNotNullOrEmpty]`.
- **Pipeline rules:** the array form bound `ValueFromPipeline = true` is what enables
  `... | Verb-IshNoun`. Prefer `ValueFromPipeline` on the object array; use
  `ValueFromPipelineByPropertyName = false` elsewhere unless a property bind is genuinely intended.
- Any property used as a `[Parameter]` **must have a getter** (setter-only breaks help generation).
  A computed default can be a private get-only property (see `UserGroup` in `AddIshBaseline.cs`).

## 4. Parameter sets — names carry meaning, keep them stable
Parameter set names appear in `Get-Help` syntax and disambiguation errors, so they must be
consistent and purposeful. The canonical pair, used by most Add/Set/Remove/Get cmdlets (both
**singular**, keep them that way):
- `"ParameterGroup"` — build the target from discrete parameters (`-Name`, `-Id`, `-Metadata`,
  filters…).
- `"IshObjectGroup"` — accept `IshObject[]` from the pipeline.

Domain-specific sets follow the same singular `<Concept>Group` shape (`MyMetadataGroup`,
`IshFolderGroup`, `FolderIdGroup`, `BaseFolderGroup`). Each set must be a coherent,
mutually-exclusive way to call the cmdlet — don't add a set that overlaps an existing one. Always
reuse these exact names. Set `[Cmdlet(... DefaultParameterSetName = "...")]` when more than one set
exists and one is the natural default.

## 5. Lifecycle: BeginProcessing / ProcessRecord / EndProcessing
PowerShell calls **`BeginProcessing` once**, **`ProcessRecord` once per pipeline item**, and
**`EndProcessing` once** after the loop. Use that deliberately for throughput:
- `BeginProcessing` (once): resolve the session and nothing user-visible. Copy this block verbatim
  from a sibling (e.g. `AddIshUser.cs`):
  ```csharp
  if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
  if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
  if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
  ```
- **Throughput is a primary goal.** The InfoShare API (SOAP and OpenAPI) exposes group/batch
  operations, so it is usually far better to **accumulate pipeline items in `ProcessRecord`** into a
  private `_retrieved...` field and **do the grouped server call(s) in `EndProcessing`**, rather than
  one round-trip per item. Established cmdlets that batch this way: `GetIshFolder.cs`,
  `MoveIshDocumentObj.cs`, `SetIshMetadataField.cs`, `AddIshBackgroundTask.cs`.
- Respect the server limits: split work with
  `DivideListInBatches<T>(list, IshSession.MetadataBatchSize)` (objects/cards via
  `IshSession.BatchSize`, metadata via `IshSession.MetadataBatchSize`, default 999) and loop the
  batches. Never send one unbounded call.
- Report progress with `WriteParentProgress("...", current, total)` (and the child progress record
  for sub-loops). Progress is informational — it must **not** change batching or throughput.
- Simple create/per-item cmdlets may instead do the work directly in `ProcessRecord` (see
  `AddIshBaseline.cs`); choose per-record vs accumulate-then-flush based on whether a batch API
  exists. If unsure which the implementer wants, ask.
- Wrap server writes in `if (ShouldProcess(target)) { ... }` for `SupportsShouldProcess = true`
  cmdlets, and after writing, **retrieve and return typed `Ish*` objects** via
  `WriteObject(IshSession, ISHType, ..., true)` shaped by `DefaultRequestedMetadata`.

## 6. Triple-slash help (drives Get-Help — required)
- **Cmdlet class:** `<para type="synopsis">` + `<para type="description">` **and at least one**
  `<example>` containing `<code>` and an explanatory `<para>`. Keep examples **short and concise**,
  one idea each.
- **In examples, assume an existing `$ishSession`; do NOT include `New-IshSession`** (it is noise and
  leaks URLs/credentials). Show the cmdlet itself, e.g. `Add-IshBaseline -Name "My baseline"` or a
  one-line pipeline.
- **Every `[Parameter]` property** needs its own `<para type="description">...</para>`. Describe what
  it does and any default; reuse the wording siblings use for the same parameter so help reads
  consistently.

## 7. Exception handling — tuned over years, preserve the order
Close `ProcessRecord`/`EndProcessing` with this exact catch ladder (copy from a sibling). The order
and the per-type `ErrorCategory` matter; each ends in `ThrowTerminatingError`:
```csharp
catch (TrisoftAutomationException e) { ThrowTerminatingError(new ErrorRecord(e, base.GetType().Name, ErrorCategory.InvalidOperation, null)); }
catch (OpenApiISH30.OpenApiISH30Exception<OpenApiISH30.InfoShareProblemDetails> e) {if (e.Result != null) { WriteWarning($"Status[{e.Result.Status}] Title[{e.Result.Title}] EventName[{e.Result.EventName}] Detail[{e.Result.Detail}]"); foreach (var error in e.Result.Errors) { WriteWarning($"ErrorEventName[{error.EventName}] ErrorDetail[{error.Detail}]"); } } ThrowTerminatingError(new ErrorRecord(e, base.GetType().Name, ErrorCategory.InvalidOperation, null)); }
catch (AggregateException e)         { var f = e.Flatten(); WriteWarning(f.ToString()); ThrowTerminatingError(new ErrorRecord(f, base.GetType().Name, ErrorCategory.NotSpecified, null)); }
catch (TimeoutException e)           { WriteVerbose("TimeoutException Message[" + e.Message + "] StackTrace[" + e.StackTrace + "]"); ThrowTerminatingError(new ErrorRecord(e, base.GetType().Name, ErrorCategory.OperationTimeout, null)); }
catch (CommunicationException e)     { WriteVerbose("CommunicationException Message[" + e.Message + "] StackTrace[" + e.StackTrace + "]"); ThrowTerminatingError(new ErrorRecord(e, base.GetType().Name, ErrorCategory.OperationStopped, null)); }
catch (Exception e)                  { if (e.InnerException != null) { WriteWarning(e.InnerException.ToString()); } ThrowTerminatingError(new ErrorRecord(e, base.GetType().Name, ErrorCategory.NotSpecified, null)); }
```
The `OpenApiISH30Exception` catch is only required when the cmdlet makes OpenAPI calls; SOAP-only
cmdlets may omit it. Don't reorder, collapse, or silently swallow these. If you believe the
handling can genuinely be improved, **challenge it explicitly with the implementer** before changing it.

## 8. Diagnostic logging density
A `-Debug` or `-Verbose` transcript must contain enough context to reconstruct what happened and
diagnose a ticket **without** requiring reproduction on a live server.

### 8.1 Which method goes where
`TrisoftCmdlet` overrides all three write methods to prepend the class name; `WriteVerbose` also
forwards into `WriteDebug`, so every Verbose message appears in both streams:

| What to log | Method | Visible when |
|---|---|---|
| Session origin, phase markers, per-item loop counters, payload sizes | `WriteDebug(...)` | `-Debug` |
| Empty no-ops, post-call result counts | `WriteVerbose(...)` | `-Verbose` (and `-Debug`) |
| Stack traces for `TimeoutException` / `CommunicationException` | `WriteVerbose(...)` | `-Verbose` (and `-Debug`) |
| Full `AggregateException`, OpenAPI error detail, `InnerException` | `WriteWarning(...)` | always |
| Long-running loops with a known item count | `WriteParentProgress(...)` | always (progress bar) |

**Secrets and PII must never appear in any stream.** Passwords, client secrets, bearer tokens, and
personal data must not be logged. If you need to confirm a value was set, log its length or a
masked form instead:
```csharp
WriteDebug($"ClientSecret.Length[{ClientSecret.Length}] ClientSecret[{new string('*', ClientSecret.Length)}]");
```

`WriteProgress` is UI sugar — it is **not** a substitute for Verbose/Debug log lines.

### 8.2 Mandatory log points (copy from siblings)
**`BeginProcessing` — session confirmation** (one line, after session is resolved):
```csharp
WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");
```

**Phase markers** before each major logical block:
```csharp
WriteDebug("Validating");
// ... then ...
WriteDebug("Adding");       // or "Updating", "Deleting", "Retrieving"
```

**Empty-collection early exit** (both update and retrieve skipped):
```csharp
WriteVerbose("IshObject is empty, so nothing to update");
WriteVerbose("IshObject is empty, so nothing to retrieve");
```

**Per-item loop** — key identifier(s) and `{++current}/{total}` counter:
```csharp
WriteDebug($"Id[{ishObject.IshRef}] Metadata.length[{ishObject.IshFields.ToXml().Length}] {++current}/{IshObject.Length}");
```

**Pre-call parameter summary** (for the `ParameterGroup` path, before the server call):
```csharp
WriteDebug($"Id[{Id}] metadata.length[{metadata.ToXml().Length}]");
WriteDebug($"Finding ActivityFilter[{activityFilter}] MetadataFilter.length[{mf.ToXml().Length}] RequestedMetadata.length[{rm.ToXml().Length}]");
```

**Post-operation result count** (just before `WriteObject`):
```csharp
WriteVerbose("returned object count[" + returnedObjects.Count + "]");
```

**Exception catch logging** (already part of §7, repeated here for completeness):
```csharp
catch (AggregateException ae)      { var f = ae.Flatten(); WriteWarning(f.ToString()); ... }
catch (TimeoutException te)        { WriteVerbose("TimeoutException Message[" + te.Message + "] StackTrace[" + te.StackTrace + "]"); ... }
catch (CommunicationException ce)  { WriteVerbose("CommunicationException Message[" + ce.Message + "] StackTrace[" + ce.StackTrace + "]"); ... }
catch (Exception e)                { if (e.InnerException != null) { WriteWarning(e.InnerException.ToString()); } ... }
```
For OpenAPI error responses, surface every structured field before terminating:
```csharp
catch (OpenApiISH30Exception<InfoShareProblemDetails> ex)
{
    if (ex.Result != null)
    {
        WriteWarning($"Status[{ex.Result.Status}] Title[{ex.Result.Title}] EventName[{ex.Result.EventName}] Detail[{ex.Result.Detail}]");
        foreach (var error in ex.Result.Errors) { WriteWarning($"ErrorEventName[{error.EventName}] ErrorDetail[{error.Detail}]"); }
    }
    ThrowTerminatingError(...);
}
```

### 8.3 Threading constraint — never call Write* from a non-pipeline thread
All PowerShell stream methods (`WriteDebug`, `WriteVerbose`, `WriteWarning`, `WriteProgress`,
`WriteObject`, `ThrowTerminatingError`) **must be called on the PowerShell pipeline thread** — the
thread PS invoked the cmdlet on. Calling them from any other thread throws
`InvalidOperationException` at runtime and is very hard to trace.

- All OpenAPI async calls use `.GetAwaiter().GetResult()` — this blocks synchronously **on the
  pipeline thread** and is the correct pattern for cmdlets. Do not introduce `await` or
  `Task.Run()`.
- If an async helper must return diagnostic text, capture it in a local variable and log **after**
  `.GetAwaiter().GetResult()` returns — never from inside a `.ContinueWith(...)` or
  `Task.Run(...)` lambda.
- The `ILogger` injected into the Connection layer routes through `TrisoftCmdletLogger` which also
  calls `WriteDebug`/`WriteVerbose` — the same pipeline-thread rule applies there.

## 9. Building `ishfields` XML — field names and the live-server dependency

API 2.5 `String` parameters like `xmlRequestedMetadata`, `xmlMetadata`, and `xmlMetadataFilter`
are all `<ishfields>` XML strings. **Never hand-build `<ishfields>` XML.** Use the established
helper chain (see `TrisoftCmdlet.cs` and `IshTypeFieldSetup.cs`):
- `IshSession.IshTypeFieldSetup.ToIshMetadataFields(...)` — for write metadata (`Add*`/`Set*`/`Move*`)
- `IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(...)` — for read metadata (`Get*`/`Find*`)
- `IshSession.IshTypeFieldSetup.ToIshMetadataFilterFields(...)` — for filter metadata (`Find*`)

These helpers validate field names, levels, and data types against `IshSession.IshTypeFieldDefinition`
and honour `IshSession.StrictMetadataPreference`.

**Where field names, levels, and data types come from (in order of reliability):**

1. **Sibling cmdlets in the same domain** — copy `DefaultRequestedMetadata` arrays verbatim.
   Fields already used in a sibling that compiles and passes CI are correct by definition.
2. **Existing model XML comments** — classes like `IshBaselineItem.cs` embed real API response
   XML as `/// <remarks>` showing field names in context. Grep `Objects/` for
   `SelectNodes\|ishfields\|ishfield`.
3. **Ask the implementer** to run `Get-IshTypeFieldDefinition` on their live system:
   ```powershell
   Get-IshTypeFieldDefinition -IshSession $ishSession |
       Where-Object { $_.ISHType -eq 'ISHModule' } |
       Format-List ISHType, Level, Name, DataType, IsMultiValue
   ```

