# Issue 126 Decisions

Related files in this issue folder:
- `0126_Add_GetIshPublicationOutputContent_implementation.md`
- `0126_Add_GetIshPublicationOutputContent_testing.md`

## Decision Log

### DEC-0126-001
- Date: 2026-06-17
- Context: Decide artifact storage model for issue #126.
- Chosen option: Per-issue folder with segmented markdown files.
- Rationale: Separates narrative, test evidence, and decisions for long-running work while staying easy to navigate.
- Impact: Standardized three-file structure in `Issue\0126_Add_GetIshPublicationOutputContent`.

### DEC-0126-002
- Date: 2026-06-17
- Context: Decide naming convention for issue artifact folder and files.
- Chosen option: `Issue\0126_<short-slug>` plus `<folder-name>_implementation.md` pattern.
- Rationale: Aligns with existing repository pattern (`Issue\0229_...`) and improves discoverability.
- Impact: Stable folder/file naming for future agent-generated artifacts on issue #126.
