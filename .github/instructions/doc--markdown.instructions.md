---
applyTo: "Doc/**/*.md"
description: "Structure and intent of the ISHRemote Doc/ markdown: human-readable ReleaseNotes (clean, concise, anonymized — they become blog posts) and the ThePlan / TheExecution thinking-out-loud documents that make roadmap reasoning and GitHub issue clustering visible on the public repository."
---

# ISHRemote `Doc/` Markdown Conventions

`Doc/` holds the **human-readable**, public-facing narrative of the project — not API help (that is
generated from the C# triple-slash comments). Everything here is read by customers, partners and
search engines, and the ReleaseNotes in particular get republished as **blog posts**. Write for that
audience: clear prose, concise runnable examples, and **fully anonymized** content.

There are four document families, each one file per minor release line, all sharing the file-name
pattern `<Type>-ISHRemote-<Major>.<Minor>.md`:
- `ReleaseNotes-ISHRemote-*.md` — what shipped, with samples and screenshots (→ blog posts).
- `ThePlan-ISHRemote-*.md` — forward-looking thinking: the problem, the plan, milestones/stories.
- `TheExecution-ISHRemote-*.md` — a working log of how the plan is actually being built.
- `Installation-ISHRemote-*.md` — practical install/upgrade/uninstall guide per PowerShell host.

When adding a new release line, **copy the newest file of the same family and adapt it** — don't
invent a new shape. Keep the section order and headings consistent with the predecessor so readers
(and the blog pipeline) get a predictable document.

## 0. Golden rule — anonymize everything (no secrets, no real URLs)
These pages are public and become blog posts, so **never** paste anything customer- or
tenant-specific. Use the established placeholders already used throughout `Doc/`:
- **Server URLs:** `https://example.com/ISHWS/` or `https://ish.example.com/ISHWS/`. Never a real
  customer/host/load-balancer URL. The only "real" URLs allowed are public ones: github.com/rws,
  docs.rws.com, learn.microsoft.com, and similar reference links.
- **Credentials:** generic demo values only — `-PSCredential Admin`, `-IshUserName admin
  -IshPassword admin`.
- **Client Credentials:** mask them — `-ClientId "c82..." -ClientSecret "ziK...=="`. Never a full
  client id or secret, token, or JWT.
- **Machine / server names** (e.g. in the QA performance table): mask to a stub like `LEUDEVDDE...`
  (optionally with a server build suffix `...@15.3.0b2303`). Never a full internal hostname.
- **File-system paths:** neutral examples like `C:\temp\` or `C:\Temp\API30Docs\`.
If you are tempted to show real data to make a point, stop and substitute a placeholder.

## 1. ReleaseNotes — the blog-post document
Audience: end users upgrading. Tone: friendly, concrete, example-driven. Canonical skeleton (see
[ReleaseNotes-ISHRemote-8.3.md](../../Doc/ReleaseNotes-ISHRemote-8.3.md) and
[ReleaseNotes-ISHRemote-8.0.md](../../Doc/ReleaseNotes-ISHRemote-8.0.md)):
- `# Release Notes of ISHRemote vX.Y` then a line pointing to the high-level
  [GitHub release](https://github.com/rws/ISHRemote/releases) notes ("below the most detailed
  release notes we have :)"), and the **STAR the repo** call-to-action.
- `## General` — state that it inherits the prior `v0.1 … vPrev` branch and the compatibility stance
  ("we expect it all to work still :)"); end with "describes the delta compared to fielded release
  ISHRemote vPrev."
- `### Remember` — the standard two bullets linking to the C# source `master` tree (call out the
  `Connection` protocols across `net48`/`net6.0`/`net10.0`) and to the per-cmdlet Pester tests, using
  the recurring examples `AddIshDocumentObj.Tests.ps1` and `TestIshValidXml.Tests.ps1`.
- **Feature sections** — `## Sample - <Title>` or `## Introducing <Feature>` (with `###`
  sub-sections). Each is a short intro paragraph explaining *why*, then a single concise fenced
  ```powershell block, then usually an **animated GIF** screenshot (see §4). Keep one idea per
  sample; show the cmdlet, assume a `$ishSession` already exists.
- **Trailing standard sections** (keep these headings, write `n/a` when empty):
  `## Implementation Details` (bullets with GitHub issue refs like `#210` and `Thanks @handle`),
  `## Breaking Changes - Cmdlets`, `## Breaking Changes - Code`, `## Breaking Changes - Platform`,
  `## Known Issues`, `## Dependencies` (the aggregated before→after table, see below), and
  `## Quality Assurance` (Pester version + a performance comparison table with **anonymized** server
  names) as the final section.

### The `## Dependencies` section — aggregated before→after per library
ISHRemote pins its NuGet/runtime dependencies inline in the `.csproj` files, and bumps mostly arrive
as Dependabot PRs (see [.github/dependabot.yml](../../.github/dependabot.yml)). The ReleaseNotes carry
**one aggregated table** of every dependency that changed since the previous fielded release — added,
upgraded or removed — placed **at the back, just before `## Quality Assurance`** (the test section).
Maintain it cumulatively across the minor line: each dependency PR that lands updates or adds a row
rather than starting a fresh list, so the table mirrors the before→after data from the Dependabot PRs.
- One row per library, **before → after**. Use `n/a (new)` or `n/a (removed)` when a library is
  introduced or dropped — the user explicitly wants newly introduced libraries listed too.
- **Lead with the most security-relevant** change (mirror the PR title — see
  [copilot-instructions.md](../copilot-instructions.md)); list the rest alphabetically.
- Note the affected target framework when a bump is conditional (the project multi-targets
  `net48;net6.0;net10.0`, e.g. `Duende.IdentityModel.OidcClient` differs per TFM).

```markdown
## Dependencies

| Library | Before | After | Note |
|---------|--------|-------|------|
| System.Security.Cryptography.Xml | 4.7.0 | 4.7.1 | security advisory (NU1903) — lead with the vulnerable one |
| Duende.IdentityModel.OidcClient (net10.0) | n/a (new) | 7.1.0 | replaces IdentityModel.OidcClient (#220) |
```

## 2. ThePlan — make the thinking visible (before)
Audience: stakeholders and contributors; purpose is to **put reasoning out in the open and cluster
it around GitHub issues** so people can react and vote. Canonical skeleton (see
[ThePlan-ISHRemote-8.3.md](../../Doc/ThePlan-ISHRemote-8.3.md) and
[ThePlan-ISHRemote-7.0.md](../../Doc/ThePlan-ISHRemote-7.0.md)):
- `# The Plan of ISHRemote vX.Y` then the standard disclaimer paragraph: *"This plan brings together
  input from several stakeholders … not set-in-stone and will evolve … please let us know what you
  think!"*
- `# TLDR (Too Long; Didn't Read)...` — a few sentences capturing the whole intent.
- `## The problem...` → `## The plan...` — frame the why before the what.
- Then the work, as either `## Milestone - <name>` or `## Story - <name>` (and/or an
  `## The algorithm...` walk-through). It is fine and expected that **code blocks here are aspirational**
  — they may reference cmdlets/parameters that don't exist yet; that's the proposal.
- Track intent inline with **task checklists** (`- [ ]` / `- [x]`) and **status suffixes/tags** on
  headings (e.g. `...DONE`).
- Close with `## Suggestions...` inviting a 👍 vote on the relevant GitHub issue.
- Reference GitHub issues by `#NNN` and cluster related reasons together rather than scattering them.

## 3. TheExecution — the working log (during/after)
Audience: future-self and the curious; purpose is to **trace how a decision was reached**. It reads
as a stream-of-consciousness build log (the author notes it's done in free time). Canonical skeleton
(see [TheExecution-ISHRemote-8.0.md](../../Doc/TheExecution-ISHRemote-8.0.md)):
- `# The Execution of the plan of ISHRemote vX.Y` then the standard intro: *"This page will try to
  track work in progress … help trace how I got where I am … Inspired by [ThePlan-…]"* — link back
  to the matching `ThePlan` (and prior `TheExecution`).
- Organize by `# Problem: <…>` / `## Analysis` blocks and proposal sections carrying **decision tags**
  in the heading: `[APPROVED]`, `[DENIED]`, `[PROBABLY]`, `[BACKLOG]`, or `IDEA:` / `IDEA: … [BACKLOG]`.
- Cite the **research** that informed a choice — external links to learn.microsoft.com, RFCs,
  StackOverflow, AWS docs, etc. — this is where the "why" lives.
- It's acceptable to leave dangling thoughts, dead-ends, and superseded ideas; that's the point of a
  visible execution trace. Cross-link to `ThePlan` and ReleaseNotes when an item lands.

## 4. Shared conventions (all `Doc/` files)
- **Images** live in [Doc/Images/](../../Doc/Images) and are referenced relatively as
  `./Images/ISHRemote-<version>--<DescriptiveName>.gif`. Prefer **animated GIFs** for cmdlet demos;
  give alt text a descriptive name plus a size hint (e.g. `... 1024x512`). Screenshots must also be
  anonymized (no real URLs/usernames on screen).
- **Product & tech naming** — be consistent: "Tridion Docs"; dual version forms like `14SP4/14.0.4`,
  `15/15.0.0`, `15.1/15.1.0`; component acronyms `ISHWS`, `ISHAM`, `ISHID`, `ISHSTS`, `ISHCM`,
  `ISHCS`, `OWCF`; protocols spelled exactly `WcfSoapWithWsTrust`, `WcfSoapWithOpenIdConnect`,
  `OpenApiWithOpenIdConnect`.
- **GitHub references** — issues/PRs as `#NNN`; thank contributors as `Thanks @handle`; link the repo
  under the `rws`/`RWS` org.
- **Code fences** — use ```powershell for cmdlet examples (the common case); examples should be
  copy-pasteable after the reader has their own `$ishSession`. Keep them short.
- **Markdown hygiene** — follow the repo [.editorconfig](../../.editorconfig): CRLF line endings and a
  final newline. Headings in Title Case; one `#` H1 per file.

## 5. Don't
- Don't include real URLs, hostnames, usernames, passwords, client ids/secrets, tokens, or
  customer data — anywhere, including screenshots and QA tables (see §0).
- Don't put API/cmdlet reference docs here — that help is generated from the C# triple-slash
  comments, not hand-written in `Doc/`.
- Don't restructure an existing release line's document into a new shape; mirror the previous file in
  the same family.
- Don't treat `ThePlan`/`TheExecution` code as a shipped contract — it is intentionally provisional.
