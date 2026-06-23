---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/Objects/**/*.cs"
description: "Structure and intent of the ISHRemote Objects layer: the public Ish* models are the SOAP-inspired integration contract consumed by PS1 scripts and Pester tests (backward-compatible, implementer-approval to change, tightly correlated to ISHRemote.Format.ps1xml), while the internal container objects merely group public objects off the pipeline."
---

# ISHRemote `Objects/` Conventions

`Objects/` holds ISHRemote's **data model**. It has two tiers with very different rules:
- **`Objects/Public/` (and the `public` types in `Objects/` root) = the integration CONTRACT.**
  These `Ish*` classes (and `Enumerations`, `IshBaseObject`) are what flows on the PowerShell
  pipeline and what custom `.ps1` scripts and the **Pester tests** bind to by property name. They are
  **backward-compatibility surface** — just like a cmdlet's parameter names and parameter sets.
- **`Objects/` root `internal` types = containers.** `IshObjects`, `IshFolders`, `IshLovValues`,
  `IshBackgroundTasks`, `IshBaselineItems`, `IshEvents`, `IshFeatures`, `IshSearchResults` are
  plumbing that **groups** public objects parsed from a service response. They are **not** meant to
  reach the pipeline.

## 0. The public objects are a compatibility contract (get approval to change)
A `public` type/member here is consumed by code we don't control (customer scripts) and by the test
suite. Therefore:
- **Adding** a property/type is generally safe (additive).
- **Renaming, removing, retyping, or reordering** a public member is a **breaking change** and
  requires **implementer approval** — exactly the same bar as renaming a cmdlet parameter or a
  parameter set. Don't do it casually to "tidy up".
- Be aware that `public` reaches beyond `Objects/Public/`: `Enumerations`
  ([Objects/Enumerations.cs](../../Source/ISHRemote/Trisoft.ISHRemote/Objects/Enumerations.cs)) and
  `IshBaseObject` ([Objects/ISHBaseObject.cs](../../Source/ISHRemote/Trisoft.ISHRemote/Objects/ISHBaseObject.cs))
  live in the root but are public contract too. Keep enum **names and integer values** stable.
- If a change seems to require breaking the public shape, **stop and ask the implementer** before
  touching it.

## 1. License header (mandatory, verbatim)
Every `.cs` starts with the Apache 2.0 header exactly as in the neighbouring files (historical
`Copyright (c) 2014 ... SDL Group` text — copy it, don't "modernize" it; tooling checks it, see
[Add-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Add-SDLOpenSourceHeader.ps1) /
[Test-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Test-SDLOpenSourceHeader.ps1)).

## 2. SOAP-inspired shape (why the model looks like it does)
The public model mirrors the WCF SOAP 2.5 `<ishobjects>` hierarchy, so most files carry a sample XML
snippet in a comment and a `public Ish*(XmlElement xml…)` constructor that parses it:
- `IshObject` ⇐ `<ishobject ishref=… ishtype=… ishlogicalref=…>` with nested `<ishfields>` and
  optional `<ishdata>` blob.
- `IshFolder` ⇐ `<ishfolder ishfolderref=… ishfoldertype=…>`, `IshLovValue` ⇐ `<ishlovvalue …>`, etc.
Values are **stringly-typed** and multi-value fields are a single string joined by
`IshSession.Separator`. Newer types may **also** offer an OpenAPI constructor (e.g.
`IshFolder(OpenApiISH30.Folder oFolder, string separator)`) — keep both construction paths producing
the **same** public shape (see the ExtensionMethods layer for the JSON↔model bridge).

## 3. Public vs container — and why containers stay off the pipeline
- A **container** (`internal class IshObjects`, `IshLovValues`, …) wraps a `List<IshPublicType>`
  parsed from one XML/JSON response and exposes accessors like `.Objects`, `.ObjectList`, `.Ids`.
- **PowerShell pipelines prefer an array of public objects over a single container object.** A lone
  container forces the user to reach inside it; an enumerated array of `Ish*` objects "just works"
  with `Where-Object`, `Select-Object`, `Format-Table`, `Out-GridView`. So the pattern is: cmdlet
  builds a container internally, then writes **`container.Objects` enumerated** to the pipeline —
  never the container itself.
- Keep containers `internal`; don't add pipeline-facing features to them. If you need something on
  the pipeline, it belongs on the **public** element type.

## 4. Pipeline emission & PSNoteProperty enrichment
Cmdlets don't `WriteObject` these raw; they go through the helpers in
[Cmdlets/TrisoftCmdlet.cs](../../Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/TrisoftCmdlet.cs)
(`WriteObject(IshSession, ISHType[], IshBaseObject(s), enumerateCollection)`):
- With `IshSession.PipelineObjectPreference = PSObjectNoteProperty` (default), each object is wrapped
  in a `PSObject` and **one `PSNoteProperty` per `IshField`** is added (names/formatting from
  `IshSession.NameHelper.GetPSNoteProperty`, e.g. dates normalized to sortable ISO‑8601). These
  note-properties are what surface as columns in `Select-Object *` / `Out-GridView`.
- With `PipelineObjectPreference = Off`, the bare `Ish*` objects are written (still enumerated).
- This is exactly why `IshBaseObject` exists: it exposes the `IshFields` the wrapper iterates. New
  pipeline types should derive from `IshBaseObject` (directly or via `IshObject`) so they enrich
  consistently.

## 5. Strong correlation with `ISHRemote.Format.ps1xml`
The human-readable table rendering of every public object is defined in
[ISHRemote.Format.ps1xml](../../Source/ISHRemote/Trisoft.ISHRemote/ISHRemote.Format.ps1xml), and it
is **tightly coupled** to these classes:
- Each public type has a `<View>` selected by its **full type name**
  (`Trisoft.ISHRemote.Objects.Public.IshObject`), whose `<TableControl>` lists the exact
  **`<PropertyName>`s**, their **column order**, **labels**, and **`<Width>`s**.
- Therefore a public **property name must keep matching** the `ps1xml`. If you rename/remove a
  property, add a property meant to show by default, or change what a column should display, **update
  `ISHRemote.Format.ps1xml` in the same change** (and keep widths sensible).
- This is also why **typed specializations exist**: e.g. `IshDocumentObj : IshObject` adds typed ref
  getters (`VersionRef`, `LngRef`, a `new ObjectRef`) largely "to allow *.Format.ps1xml to do magic"
  — a distinct type gives a distinct default view. `IshObjectFactory` returns the **most specific**
  type so the right view is selected.
- When you add a brand-new public pipeline type, add a matching `<View>` (copy a sibling view and
  adjust columns/widths).

## 6. Construction & specialization patterns to follow
- Provide the established constructors: an **`XmlElement`** (SOAP) one and, where relevant, an
  explicit-fields one and/or an **OpenAPI model** one — all yielding the same public shape.
- Specialize by **inheriting `IshObject`** and adding strongly-typed getters; use `new` to refine a
  base member only when the SOAP-style base would be ambiguous (as `IshDocumentObj.ObjectRef` does).
- Register new specializations in
  [IshObjectFactory.cs](../../Source/ISHRemote/Trisoft.ISHRemote/Objects/IshObjectFactory.cs) so the
  right concrete type is built from a generic `<ishobject>`.
- Keep `///` triple-slash docs with the `<para type="description">…</para>` style used by siblings
  (these flow into help and the build is warning-clean in Release).
- Keep the class diagram
  [_ObjectsDiagram.cd](../../Source/ISHRemote/Trisoft.ISHRemote/Objects/_ObjectsDiagram.cd) in sync
  when you add/rename/remove a type.

## 7. Don't
- Don't rename/remove/retype/reorder a **public** member without implementer approval — it breaks
  scripts, the Pester suite, and `ISHRemote.Format.ps1xml` (see §0, §5).
- Don't make a container `public` or write a container straight to the pipeline — emit its
  `.Objects` array enumerated instead (see §3).
- Don't change `Enumerations` value names or numbers (serialized/contract); add new members instead.
- Don't add a public default-visible property without updating the matching `Format.ps1xml` view.
- Don't break the SOAP-style invariants the model relies on (string values,
  `IshSession.Separator` for multi-value, `Value`/`Id`/`Element` note-field triplets).
