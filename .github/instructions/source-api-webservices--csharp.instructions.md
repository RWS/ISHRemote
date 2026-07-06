---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/**/*.cs"
description: "How to choose the right Tridion Docs Web Services API call (API 2.5 SOAP vs API 3.0 OpenAPI) when implementing a cmdlet: the three-tier knowledge stack, how to navigate live documentation, reading the compatibility table correctly, and writing correct version guards."
---

# ISHRemote Web Services API Selection and Documentation

Every ISHRemote cmdlet calls one or more server-side API methods. There are three tiers of
knowledge for those calls, in strict priority order. **Higher tier always wins on contradictions.**

## 1. Primary source: `Service References/*/Reference.cs` (generated SOAP proxies)

The folders under `Source/ISHRemote/Trisoft.ISHRemote/Service References/` each contain a
`Reference.cs` generated from the WCF WSDL. These are the **contractual truth** for every
API 2.5 SOAP call: C# method names, parameter names, parameter types, return types, and SOAP
action URIs. `IshSession` exposes them as `IshSession.Application25`, `.Folder25`,
`.DocumentObj25`, `.BackgroundTask25`, `.User25`, etc.

**Always read `Reference.cs` before consulting documentation.** If anything in a documentation
page contradicts what the proxy says, the proxy is correct — docs can lag or be for a
different version. The `.xsd` files in the same folder describe the WCF message envelope
(request/response element shapes), but they type every XML-payload parameter as plain
`xs:string` — they do **not** define the XML content inside those strings. See §3 for how
to discover that content.

What `Reference.cs` does NOT tell you (use §3 for these):
- What XML structure belongs inside `String` parameters — every payload `String` is typed as
  `xs:string` in the `.xsd` files too; the actual XML shape lives in the dedicated XML structure
  topics linked from the doc page description column (see §3)
- Business rules and preconditions ("Requirements are:", "Note that:")
- Algorithm pseudocode ("The algorithm could do:")
- Why a method is deprecated and what its replacement actually does differently

## 2. Secondary source: `OpenApiISH30.json` + generated client (API 3.0)

`Source/ISHRemote/Trisoft.ISHRemote.OpenApiISH30/OpenApiISH30.json` (≈1 MB) is the contractual
truth for API 3.0 REST calls via `IshSession.OpenApiISH30Client`. It includes typed request/
response schemas, parameter descriptions, and required fields in a way the SOAP WSDL does not.

## 3. Tertiary source: live docs.rws.com

Use only for what the proxies and JSON spec do not provide. Always fetch live — never rely on a
cached or copied version of the documentation in this repo.

### Finding the latest documentation (search-first pattern — use every time)

Publication IDs change per Tridion Docs release. **Never hardcode a publication ID.**
Use this two-step pattern to always land on the latest version:

**Step 1 — Find the latest publication ID:**
```
https://docs.rws.com/en-US/search?query=%22Web+Services+API+compatibility+across+releases%22&page=1
```
The results list multiple Tridion Docs versions. Pick the one with the highest version number.
Its URL reveals the publication ID: `https://docs.rws.com/en-US/{pubId}/331933/...`

Known publication IDs (for cross-reference only — always verify via search):
- Tridion Docs 15.0 → `992527`
- Tridion Docs 15.1 → `1151795`
- Tridion Docs 15.2 → `1165616`
- Tridion Docs 15.3 → `1312231` ← current latest as of 2026

**Step 2 — Find the topic ID for a specific method:**
The topic ID is **stable across all publications** — it never changes for a given page. Note that Genius/DXD urls are only permalink (so using the logical card id for a topic, instead of the version-aware language cardid) since Tridion Docs 15.0.0 where Publish plugin `ISHDITADELIVERYPREPAREOVERALLPACKAGE` got the `<parameter name="ContentObjectIdLevel">logical</parameter>` option.
Search by method name to retrieve it:
```
https://docs.rws.com/en-US/search?query=%22{ServiceName}+2.5+{MethodName}%22&page=1
```
Examples:
- `%22Application+2.5+GetVersion%22` → topic `68528`
- `%22Baseline+2.5+ExpandReport%22` → topic `68586`
- Compatibility table → topic `331933` (always)

**Construct the final URL:**
```
https://docs.rws.com/en-US/{pubId}/{topicId}/tridion-docs-main-documentation/{slug}
```
The slug is human-readable but secondary — the `pubId`+`topicId` pair is sufficient.

### What API 2.5 method pages provide beyond the proxy

Each `{ServiceName} 2.5 {MethodName}` page documents:
- **Parameter directions** (`In`, `Out`, `InOut`) — essential for identifying which `string`
  parameters carry an XML payload you must construct (`In`) vs ones you must parse
  (`Out`/`InOut`). The direction column in the parameter table is the only place this is stated.
- **The name of the XML structure** for payload strings (e.g., `psBaselineReport | String | In |
  A Baseline Report XML structure`). That structure name is a **hyperlink** leading to a
  dedicated XML structure topic in the docs. Each such topic has two sections: `## XML structure`
  (the formal XSD schema with typed attributes and enumerations) and `## Example` (a real XML
  snippet). See "Finding the XML content inside string parameters" below for how to reach those
  pages when the hyperlink is not directly followable.
- **"Requirements are:"** — access rights and preconditions the server enforces
- **"Note that:"** — edge cases, fallback language logic, multi-value join rules
- **"The algorithm could do:"** — the canonical sequence of calls for a complex operation
- A "Recommendations" section linking to the successor method and related calls

### Finding the XML content inside `string` parameters

The SOAP API is deliberately untyped at the payload level. A parameter like
`string xmlBaselineReport` in `Reference.cs` and `<xs:element name="xmlBaselineReport"
type="xs:string" />` in the `.xsd` both confirm the parameter exists but say nothing about
what XML goes inside. The authoritative source is a set of dedicated XML structure pages in
the docs. The actual discovery path has four steps:

1. **Follow the hyperlink from the method's parameter description column (primary source).**
   On every `{ServiceName} 2.5 {MethodName}` doc page the description cell for a `String`
   payload parameter is a clickable hyperlink to a dedicated XML structure topic. That topic has
   two sections: `## XML structure` (formal XSD with typed attributes and enumerations) and
   `## Example` (a real XML snippet). When following a link is not possible directly (e.g., when
   using a tool that strips inline hyperlinks from table cells), apply the search-first pattern
   using the structure name from the description:
   ```
   https://docs.rws.com/en-US/search?query=%22{StructureName}%22&page=1
   ```
   For example, `psBaselineReport | String | In | A Baseline Report XML structure` →
   search `%22Baseline+Report+XML+structure%22`. Pick the result whose title matches exactly
   (e.g., "Baseline Report Information") and is from the highest-version publication.
   These topic IDs are stable across publications — the same topic ID works for any pubId.

2. **Cross-check against existing parsers in this codebase.** Grep `Objects/` and `Cmdlets/` for
   `SelectNodes`/`SelectSingleNode`/`XmlElement`. Model classes frequently embed commented XML
   examples and are reliable cross-checks when the live doc may be outdated.
   `IshBaselineItem.cs` documents the `xmlBaseline` format returned by `GetBaseline`,
   which is the flat list of pinned versions you pass back to `ExpandReport`:
   ```xml
   <baseline ref="GUID-D1C23864-304D-408D-86C0-52C5B58343BD">
     <objects>
       <object ref="GUID.007DFDAD.CEFD.40F3.A75E.2C081228DC89" versionnumber="2"
               author="Admin" source="save:LatestAvailable"
               created="10/12/2008 15:05:09" modified="10/12/2008 15:05:09"/>
     </objects>
   </baseline>
   ```

3. **Distinguish related-but-different structures.** One service can expose multiple named XML
   structures sharing the same word root. `Baseline25` has at least two: `xmlBaseline` (flat
   pinned-version list from `GetBaseline`) and `xmlBaselineReport` (expanded dependency + status
   report from `GetReport` / `ExpandReport` / `CompleteReportByMode`). The `In`/`Out` direction
   column tells you which parameter shape each method parameter carries — confirm this before
   building a parser.

4. **Read simpler sibling methods.** `Baseline25.Update` takes `xmlChanges` (a diff list) and
   `GetBaseline` returns `xmlBaseline` (the full version list). Both are simpler than the report
   format and their XPath patterns in existing cmdlets give reliable element/attribute names to
   extrapolate from.

These business rules survive the API 2.5 → API 3.0 migration. When implementing an API 3.0
equivalent, read the API 2.5 doc page to ensure the same preconditions and edge cases are handled.

## 4. Compatibility table — reading it correctly

Canonical URL (use search-first to confirm latest pubId):
```
https://docs.rws.com/en-US/1312231/331933/tridion-docs-main-documentation/web-services-api-compatibility-across-releases
```

The five version columns left-to-right represent: **14.x**, **15.0**, **15.1**, **15.2**, **15.3** at the time of writing.

| Symbol | Meaning |
|---|---|
| `S` | Supported — call this method |
| `D` | Deprecated — callable, but a better option exists (named in the rightmost column) |
| `I` | Internal — not public; do not call |
| `-` | Not available (not yet added, or removed) |

**Critical: `D` does NOT mean "use the API30 equivalent."**
The rightmost column names the concrete replacement. It may be another API25 method:
- `API25.Baseline.RetrieveMetadata` D → `API25.Baseline.RetrieveMetadata2`
- `API25.BackgroundTask.CreateBackgroundTaskWithStartAfter` D → `API30.CreateBackgroundTask`

Only move to API 3.0 if the replacement column explicitly names `API30.*` AND
`IshSession.Protocol == OpenApiWithOpenIdConnect`.

**Determining minimum server version:**
Scan the row left-to-right for the first `S`. That column is the minimum version. If the first
`S` appears in the 15.1 column, the method requires Tridion Docs ≥ 15.1 → add a
`PlatformNotSupportedException` guard (see §5).

## 5. Decision flow: which API call to use

For each server operation when implementing or modifying a cmdlet:

1. Read `Reference.cs` for the API 2.5 SOAP signature.
2. Look up the method in the compatibility table.
3. Does the rightmost column name `API30.*` AND is `IshSession.Protocol == OpenApiWithOpenIdConnect`
   AND does the server have the required version?
   - **Yes** → use `IshSession.OpenApiISH30Client` with the typed NSwag method.
   - **No** → use `IshSession.*25` SOAP proxy. Consult the API 2.5 method doc page for XML
     structure and business rules. If the row shows `D`, use the named API25 replacement instead
     of the deprecated call.
4. Use `switch (IshSession.Protocol)` — never a bare `if` — with explicit `case` arms for each
   protocol. Fall-through from OpenAPI to SOAP must be intentional and documented.
5. Determine minimum supported version from the table → add `PlatformNotSupportedException`
   guard if any column to the left of the first `S` is `-` (see §6).

## 6. `PlatformNotSupportedException` version guard pattern

Place in `BeginProcessing()`, immediately after session resolution. Copy this exact shape from
[`AddIshBackgroundTask.cs`](../../Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/BackgroundTask/AddIshBackgroundTask.cs):

```csharp
if ((IshSession.ServerIshVersion.MajorVersion < 13) ||
    ((IshSession.ServerIshVersion.MajorVersion == 13) && (IshSession.ServerIshVersion.RevisionVersion < 2)))
{
    throw new PlatformNotSupportedException(
        $"Add-IshBackgroundTask with the current parameter set requires server-side BackgroundTask API " +
        $"which is only available starting from 13SP2/13.0.2 and up. " +
        $"ServerIshVersion[{IshSession.ServerVersion}]");
}
```

Rules:
- Use `MajorVersion` and `MinorVersion` (`ServerIshVersion` properties on `IshSession`).
- Message must include: what feature, the minimum version, and `ServerIshVersion[{IshSession.ServerVersion}]`.
- Do **not** add a guard for methods that show `S` on all ISHRemote-supported versions (≥13SP2).
  The module already enforces 13SP2 as a floor — guards below that are redundant.
- Do **not** emit a `WriteWarning` and continue — throw unconditionally. The user cannot work
  around a server-side method that does not exist.

## 7. Don't

- Don't invent XML payload content for `string` API25 parameters — read the method doc page.
- Don't assume `D` in the compatibility table means `API30.*` replacement — read the rightmost
  column on that specific row.
- Don't hardcode publication IDs in comments, instruction files, or code — always use the
  search-first pattern (§3) to find the current latest.
- Don't bypass the generated `Service References` proxy by constructing raw SOAP requests —
  always call through the typed `IshSession.*25` client.
- Don't copy business-rule logic from an old version's doc page without verifying against the
  latest version — fetch live, using the search-first pattern.
- Don't skip the `switch (IshSession.Protocol)` pattern and fall through silently from one
  protocol to another — the switch must be explicit and every arm must be intentional.
- Don't make rapid successive live docs.rws.com fetches — pace requests to avoid HTTP 429
  throttling. Determine exactly which pages are needed first, then fetch them one at a time.
  If a 429 is received, stop immediately and do not retry until the underlying need is
  reassessed; never loop through large sets of doc pages in a single task.
