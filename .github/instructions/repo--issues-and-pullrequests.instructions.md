---
applyTo: ".github/**"
description: "Naming conventions for ISHRemote GitHub issues and pull requests: issue types, labels, branch naming, title shape, verb vocabulary, before-and-after style for version bumps, commit prefixes, and how to format Dependabot grouped-PR bodies and the matching ReleaseNotes Dependencies section."
---

# ISHRemote Issue and Pull Request Conventions

Issues and PRs share one title style — an issue title typically becomes the PR title.

## Issue types

Issue type is set via the `type:` field in the ISSUE_TEMPLATE YAML — never via labels. The three
allowed values are `feature`, `bug`, and `task`, matching the default types configured at the
organization level.

## Branch naming

Branch names follow `<type>/<issue-number>-<kebab-slug>` where the slug is a lowercased
kebab-case abbreviation of the issue title — long enough to be recognizable, not necessarily
the full title verbatim.

| Issue type | Branch prefix | Example |
|---|---|---|
| Feature or Task | `feature/` | `feature/240-maintain-project-ai` |
| Bug | `bug/` | `bug/238-get-ishdocumentobjdata-does-not-trim-long-ftitle` |

## Title shape

```
<Verb> <cmdlet | family | component> [with/to <specific parameter or capability>] [for/to <risk or purpose>]
```

Derive new titles from the existing tracker rather than inventing a style.

## Verb vocabulary (features and tasks)

`Add`, `Augment`, `Enhance`, `Extend`, `Enable`, `Replace`, `Refresh`, `Update`, `Rewrite`,
`Improve`, `Maintain`. Name the cmdlet or family (`*-IshTranslationJob`), then the specific
parameter/capability, then the purpose.

Examples from the tracker:
- *"Add cmdlet Remove-IshEvent"* (#120)
- *"Augment Get-IshDocumentObj with protocol OpenApiWithOpenIdConnect implementation"* (#229)
- *"Extend Set-IshBaselineItem to update a baseline using the incoming IshDocumentObj versions"* (#222)

## Bug titles

Describe the observable symptom and the condition:
- *"Get-IshDocumentObjData does not trim long FTITLE on PS 5.1 and when no trailing slash for FolderPath is provided"* (#238)

## Before→after style for version bumps

Spell out both values explicitly:
- *"Update ISHRemoteMcpServer MCP Protocol Version from '0.3.0' to '2024-11-05'"* (#228)
- *"Replace package references of IdentityModel.OidcClient to Duende.IdentityModel.OidcClient"* (#220)

## Commit prefixes (from `.github/dependabot.yml`)

- `deps` — NuGet package bumps
- `ci` — GitHub Actions version bumps

## Dependabot grouped PR titles and bodies

Lead with the **single most significant (most security-relevant / most vulnerable) library** and its
`from X to Y` bump, then list the **before→after for every changed or added library** in the grouped
PR body. Mirror that same aggregated before→after list into the ReleaseNotes `## Dependencies`
section (see `.github/instructions/doc--markdown.instructions.md`).

## ReleaseNotes cross-reference

Every merged PR that changes a public surface, fixes a bug, or bumps a dependency gets a bullet in
the matching `Doc/ReleaseNotes*.md` entry. Format follows the instructions in
`.github/instructions/doc--markdown.instructions.md`. The `## Dependencies` table uses the
aggregated before→after format from Dependabot PR bodies.
