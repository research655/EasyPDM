# SPEC: sparkCAD vault (owner: Daniel; agents read only)

Build exactly this (`.claude/rules/sparkcad-plan.md`). Each milestone is its own branch and PR. Decisions:
`docs/DECISIONS.md`. Upstream design reference: `TECHNICAL.md`.

## Milestones and done criteria

| ID | Milestone | Done when |
|---|---|---|
| V0 | Repo on the workstation-core template | AGENTS router, DECISIONS, SPEC, rules, STATE, BACKLOG, archives, state-files-gate CI exist; state_lint passes |
| V1 | Part numbers (V-004, V-005) | Kinds PRT, SUB, ASY, CBL, WIR with 4 digits (V-012: no MFR or CUS items); an API call reserves the next N numbers per kind for the Fusion add-in and records who reserved them; upload refuses a number the vault did not issue or that is already used; tests cover each |
| V2 | Hand-over package upload (V-003, V-006) | The API accepts the drawing tool's package (hand-over JSON + PDF + DXF + editable drawing file; STEP per D8) for one item and rev; files stored by role; made children from the JSON become `item_relations` (part number, qty); bought and customer children are kept as BOM rows on the assembly, not items (V-012); unknown made-part numbers listed, not invented |
| V3 | Maker files (V-002, V-007) | Roles `dxf`, `nc`, `gcode`, `slice` (more by config); each bound to item, rev and STEP fingerprint; "Add maker file" in the web app for an item and rev; G-code / .nc get the stamp comment line, DXF the text note; upload against an old rev refused |
| V4 | Stale and review flags (V-008) | A new STEP fingerprint marks that item's old drawing and maker files stale; every parent gets "review needed"; no automatic rev |
| V5 | Release check (V-008) | Releasing an assembly fails, with a list, until every custom child down the tree has a drawing and its needed maker files with matching fingerprints |
| V6 | Contains and where used (V-003) | API and web app show "contains" and "where used", one level and full tree, from `item_relations` |
| V7 | Drawing check-out and edit (V-006) | Check-out locks the item; the editable drawing file downloads for the drawing tool; check-in stores a new version; rev labels per V-010 |
| V8 | Approvals and PDF stamp | Approval rule per V-011; at approval the vault stamps item number, rev and release date on the PDF |

## Interfaces

- Drawing tool to vault: hand-over JSON `<name>.sparkcad.json`, draft fields (sr-drawing-pack `xq_sheet.py`):
  `kind`, `part_number`, `description`, `rev`, `render_mode`, `drawing_pdf`, `drawing_dxf`, `step`. To add (V2,
  V3): `step_sha256` (the fingerprint), `files[]` (`role`, `name`, `sha256`, `made_from` fingerprint),
  `children[]` (`part_number`, `qty`), `drawing_source` (the editable drawing file). The schema is agreed with
  sr-drawing-pack before V2 code starts.
- Fusion add-in to vault: "reserve numbers" (V1) and "upload package" (V2). Authentication: the vault's
  existing login; no secrets in either repo.
- BOM fields match sr-bom / sr-export: `part_number`, `description`, `material`.

## Open (Daniel)

- D8 STEP files in the vault: a) no STEP stored; the vault keeps only its fingerprint, and the editable drawing file
  carries the 2D view lines so it opens without a STEP (rec) · b) store the STEP with each released rev
  (3D preview and CNC shops, but a second copy of the model to keep in step)

## Tolerances and values

- None yet.
