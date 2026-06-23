---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/**/*.cs"
description: "Structure and intent of the ISHRemote ExtensionMethods layer: an experimental, additive bridge that augments the SOAP-inspired public Objects (the integration contract used by PS1 scripts and Pester tests) so they can interpret the richer, more strongly-typed JSON OpenAPI object model while staying compliant with client-side ISHRemote enumerations."
---

# ISHRemote `ExtensionMethods/` Conventions

> **Status: experimental and intentionally incomplete.** This folder is a young bridging layer. It
> has `TODO`s, open `// [Question]` notes, and even a fully commented-out file. That is expected —
> when something here is ambiguous or missing, **ask the implementer** rather than inventing a
> contract. These instructions will be refined as the layer matures.

## 0. Why this folder exists (the big picture)
ISHRemote's **public object model lives in `Objects/`** (e.g. `Objects/Public/IshObject.cs`,
`IshField.cs`, and the `IshFields` collection). Those POCO-like classes are **part of the public
contract**: custom `.ps1` integration scripts and the Pester tests consume them directly, so their
shape must stay stable. That model is **SOAP-inspired** — it mirrors the `<ishobjects>`/`<ishfield>`
hierarchy of the WCF SOAP 2.5 API, so values are largely **stringly-typed** (a field value is text,
multi-value is one string split on `IshSession.Separator`).

The later **OpenAPI (ISH30 / AM10) model is richer**: it is JSON-based and **strongly typed** —
distinct `SetStringFieldValue` / `SetLovFieldValue` / `SetCardFieldValue` / `SetDateTimeFieldValue` /
`SetNumberFieldValue` (plus `Multi…` variants), typed `Level`, `FieldGroup`, `StatusFilter`, etc.

`ExtensionMethods/` is the **adapter** between those two worlds. Its job is twofold:
1. **Interpret the typed JSON/OpenAPI shapes** and project them onto (or from) the existing public
   `Ish*` objects, so the rest of ISHRemote keeps speaking the stable SOAP-style model.
2. **Stay compliant with client-side ISHRemote enumerations** (`Objects/Enumerations.cs`) by mapping
   each OpenAPI enum to/from its ISHRemote counterpart in exactly one place.

## 1. License header (mandatory, verbatim)
Every `.cs` starts with the Apache 2.0 header exactly as in the neighbouring files (historical
`Copyright (c) 2014 ... SDL Group` text — copy it, don't "modernize" it; tooling checks it, see
[Add-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Add-SDLOpenSourceHeader.ps1) /
[Test-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Test-SDLOpenSourceHeader.ps1)).

## 2. Shape of an extension file
- Namespace is always `Trisoft.ISHRemote.ExtensionMethods`.
- One **`internal static` class** per file holding only **`internal static` extension methods** (the
  first parameter is `this <Type>`). Nothing here is `public` — this is an internal helper layer, not
  part of the public contract (the public contract is `Objects/`).
- **File naming = the subject type + `Extensions`**, named after whichever side the method *extends*:
  - `OpenApiISH30…Extensions.cs` when extending an OpenAPI type
    ([OpenApiISH30EnumerationsExtensions.cs](../../Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/OpenApiISH30EnumerationsExtensions.cs),
    [OpenApiISH30FieldValueExtensions.cs](../../Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/OpenApiISH30FieldValueExtensions.cs)).
  - `Ish…Extensions.cs` when extending an ISHRemote `Objects/` type
    ([IshFieldsExtensions.cs](../../Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/IshFieldsExtensions.cs)).
  - (The class inside may be `…ExtensionMethods` or `…Extensions` — both exist today; prefer matching
    the file name for new files.)
- Disambiguate the two object worlds explicitly in code: qualify with `OpenApiISH30.IshField` vs
  `Objects.Public.IshField`, `OpenApiISH30.Level` vs `Enumerations.Level`, etc. Both define
  same-named types, so never rely on a bare `using` to pick the right one.

## 3. Method naming = direction of conversion (keep it predictable)
Every method is a converter and its name states the **target**:
- **ISHRemote → OpenAPI:** `ToOpenApiISH30<Thing>()` — e.g.
  `ToOpenApiISH30Level`, `ToOpenApiISH30FieldGroup`, `ToOpenApiISH30StatusFilter`,
  `ToOpenApiISH30SetFieldValues`, `ToOpenApiISH30FilterFieldValues`, `ToOpenApiISH30RequestedFields`.
- **OpenAPI → ISHRemote:** `ToISH<Thing>()` — e.g. `ToISHFieldLevel`.
Reuse the existing verb/shape for a new mapping instead of coining a new pattern.

## 4. The enumeration-compliance rule (single source of truth)
Each OpenAPI ↔ ISHRemote enum mapping is a `switch` that lives **once** in
[OpenApiISH30EnumerationsExtensions.cs](../../Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/OpenApiISH30EnumerationsExtensions.cs)
(or alongside the field logic for `Level` in `IshFieldsExtensions`). When you add an enum value:
- Handle **every** case; for a `…ToISH…` mapping that hits a genuinely unmappable OpenAPI value
  (`Object`, `Compute`), **throw `ArgumentException`** with the `"… [{value}] was unexpected."`
  wording already used — don't silently default.
- For `…ToOpenApiISH30…` mappings a `default:` fallback (e.g. to `…Descriptive`/`…None`) is the
  established style; keep unsupported cases marked with a `// TODO [Could] API30 enumerations` note
  rather than guessing a wrong target.
- Never duplicate a mapping in a second file — call the existing extension.

## 5. Bridging field values correctly (the hard part)
Converting `IshFields` (stringly-typed) to typed OpenAPI `SetFieldValue`s is **driven by the field
setup, not by guessing**. Follow the pattern in
[IshFieldsExtensions.cs](../../Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/IshFieldsExtensions.cs):
- Look up the field's `IshTypeFieldDefinition` (via `ishSession.IshTypeFieldDefinition`, matched on
  `Name` + `Level`); skip fields that have no definition.
- Branch on `ishTypeFieldDefinition.DataType` (`DateTime` / `ISHLov` / `ISHType` / `String` /
  `LongText` / `ISHMetadataBinding` / `Number`) **and** on `IsMultiValue` to pick the right
  `Set…FieldValue` vs `SetMulti…FieldValue`.
- For multi-value, split the string on `ishSession.Separator` (the same separator the SOAP model
  uses) before projecting each part.
- For `ISHType` (card reference) fields, map `ReferenceType` to the correct `Set…` object
  (`SetUser`, `SetUserGroup`, `SetFolder`, `SetElectronicDocumentType`, `SetDocumentObject`, …) — see
  `GetSetBaseObjectFromReferenceType`.
- The reverse direction (OpenAPI `FieldValue` → `IshFields`) re-emits the SOAP-style `Value`/`Id`(/
  `Element`) note-fields and joins multi-values back with `IshSession.Separator` (see how
  `ToISHFieldLevel` is consumed in [Objects/IshFields.cs](../../Source/ISHRemote/Trisoft.ISHRemote/Objects/IshFields.cs)).

## 6. Don't break the public `Objects/` contract
- This layer is **additive**: it augments/translates the public `Ish*` objects. Do **not** change the
  shape, naming, or semantics of anything in `Objects/Public/` to make a conversion easier — that
  model is consumed by external scripts and the Pester suite. If a conversion seems to *need* a public
  change, stop and ask the implementer.
- Keep the SOAP-style invariants the public model relies on (string values, `IshSession.Separator`
  for multi-value, `Value`/`Id`/`Element` note-field triplets).
- Don't hand-edit the generated OpenAPI clients (`Trisoft.ISHRemote.OpenApiISH30` /
  `…OpenApiAM10`); adapt around them here.

## 7. Working with the experimental nature
- `TODO`, `// [Question]`, and large commented-out blocks (e.g. the `#115` attempt in
  [OpenApiISH30FieldValueExtensions.cs](../../Source/ISHRemote/Trisoft.ISHRemote/ExtensionMethods/OpenApiISH30FieldValueExtensions.cs))
  are deliberate breadcrumbs — leave them unless you are the one completing that path, and prefer
  extending them over deleting context.
- Consumers today are mostly the **side-by-side OpenAPI code paths** (e.g.
  [AddIshFolder.cs](../../Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/Folder/AddIshFolder.cs)); a method
  may exist before it has a live caller. That is fine for this layer — but call it out so the
  implementer can decide whether to wire it in.
- Keep the build warning-clean (Release treats warnings as errors). New mappings need accurate
  triple-slash summaries where the siblings have them.
