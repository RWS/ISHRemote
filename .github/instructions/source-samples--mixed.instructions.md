---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/Samples/**/*"
description: "Structure and intent of the ISHRemote Samples folder: historical, inspirational Sample.*.ps1 automation scripts and self-contained data/debugging sets (e.g. Data-GeneralizeDitaXml) that double as fixtures for Pester tests — all kept fully anonymized (no customer URLs or secrets), like the ReleaseNotes."
---

# ISHRemote `Samples/` Conventions

> **Status: lightly used these days.** `Samples/` is reference material, not shipped product code.
> It is used less now — mostly because few cmdlets need large file-system test data — but it remains
> the right home for **inspirational usage scripts** and **self-contained data sets** when they are
> needed. Don't expect every sample to use the newest cmdlet syntax; many are intentionally
> historical.

## 0. What this folder is
Two kinds of content live here, with different purposes:
1. **`Sample.*.ps1` automation scripts** — end-to-end examples showing *how the ISHRemote library can
   be used* for real business automation (provisioning, legacy correction, publishing). Somewhat
   legacy, but still inspirational.
2. **Data subfolders** (e.g.
   [Data-GeneralizeDitaXml/](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Data-GeneralizeDitaXml),
   [Data-DocumentObj/](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Data-DocumentObj),
   [Publish-PublicationOutputs/](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Publish-PublicationOutputs))
   — **self-contained example + debugging sets**: catalogs, DTDs, sample DITA/XML and a driver
   script, complete enough to run and hand-explain.

## 1. Golden rule — anonymize everything (no customer URLs, no secrets)
Exactly like the `Doc/` ReleaseNotes, **everything in `Samples/` is public and must be anonymized.**
Use the placeholders already established across the folder:
- **URLs:** `https://example.com/ISHWS/` or `https://example.com/InfoShareWS/` — never a real
  customer/host URL.
- **Credentials:** empty or generic placeholders (`$trisoftUserName = ''`, `username` / `password`).
  Never a real username, password, client id/secret, or token.
- **Domains / emails / users:** `exampledomain`, `@example.com`, `user1`…`user5`.
- **GUIDs, file names, paths:** keep them obviously fake (`GUID-EB2E82F1-…`, `C:\PublishedDocs`).
If you add or edit a sample, scrub it before committing — treat a leaked URL/secret as a bug.

## 2. `Sample.*.ps1` script conventions
Follow the shape of the existing scripts
([Sample.Automate.ActiveDirectory.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Sample.Automate.ActiveDirectory.ps1),
[Sample.Automate.LegacyCorrection.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Sample.Automate.LegacyCorrection.ps1),
[Sample.Automate.ProvisionUsers.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Sample.Automate.ProvisionUsers.ps1)):
- **Naming:** `Sample.<Theme>.<Topic>.ps1` (e.g. `Sample.Automate.ProvisionUsers.ps1`). A sample that
  needs its own data gets a sibling **subfolder** holding the script + its files (see
  `Publish-PublicationOutputs/`).
- **Self-documenting top:** either a comment-based help block (`.SYNOPSIS` / `.DESCRIPTION` /
  `.PARAMETER`) or a clear `#` header explaining the scenario and any minimum ISHRemote version.
- **Recognisable preamble:** `Import-Module ISHRemote -DisableNameChecking`, then the
  `$DebugPreference/$VerbosePreference/$WarningPreference/$ProgressPreference` block, then anonymized
  variables, then a `try { New-IshSession … } …` body.
- These are **illustrative, not tested** — they may use older parameter names/positional
  `New-IshSession` calls. When *adding new* samples prefer current cmdlet conventions and an existing
  `$ishSession`; when touching an old one, keep it working rather than fully modernising it (and if a
  rewrite seems warranted, ask the implementer).
- **No Apache license header** is used on these scripts (unlike the C# tree) — match the neighbours.

## 3. Self-contained data subfolders (and why they aren't generated)
A folder like
[Data-GeneralizeDitaXml/](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Data-GeneralizeDitaXml)
is deliberately a **complete, on-disk set**:
[generalization-catalog-mapping.xml](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Data-GeneralizeDitaXml/generalization-catalog-mapping.xml),
`SpecializedDTDs/`, `GeneralizedDTDs/`, and `InputFiles/` (bookmap, reference, learning, subject
scheme). It serves two roles at once: a **runnable sample** *and* a **manual debugging set** for
developing/exercising the Generalize cmdlet (`New-IshDitaGeneralizedXml`).
- **Why checked in rather than generated:** in theory the XML catalogs, DTDs and example files could
  be emitted by the test harness, but that would make them far harder to *use, inspect and explain*
  later. So historically these stay as **self-contained files** you can open and reason about.
- **Keep them self-contained and coherent:** the catalog mapping, DTDs and input files must stay
  internally consistent (the mapping resolves specialized → generalized DTDs by public id / system id
  / root element). Carry forward the explanatory comments already in the mapping file.

## 4. Some sample data is ALSO live Pester fixtures — don't break it
Parts of `Samples/` are referenced directly by the Pester suite via **relative paths**, so they are
load-bearing test fixtures, not just decoration:
- [NewIshDitaGeneralizedXml.Tests.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/NewIshDitaGeneralizedXml.Tests.ps1)
  and
  [TestIshValidXml.Tests.ps1](../../Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)
  reach into `..\..\Samples\Data-GeneralizeDitaXml\SpecializedDTDs\…`,
  `…\GeneralizedDTDs\…` and `…\generalization-catalog-mapping.xml`.
- The
  [Remember--UsedByPesterTests.txt](../../Source/ISHRemote/Trisoft.ISHRemote/Samples/Data-GeneralizeDitaXml/Remember--UsedByPesterTests.txt)
  marker exists precisely to flag this coupling. **Before renaming, moving, or deleting anything**
  under such a folder, grep the `*.Tests.ps1` files for the path and update both sides together — or
  leave it alone and ask the implementer.

## 5. Don't
- Don't commit real customer URLs, hostnames, usernames, passwords, client ids/secrets, tokens, or
  any tenant data — anonymize like the ReleaseNotes (see §1).
- Don't rename/move/delete files under a `Data-*` set without checking the Pester tests that
  reference them by relative path (see §4).
- Don't "improve" a `Data-GeneralizeDitaXml` set by generating it at test time — its value is being
  self-contained, readable and explainable (see §3).
- Don't treat samples as product code (no Apache header, not packaged, may be intentionally legacy);
  keep them illustrative and runnable.
