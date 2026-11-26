# Decisions (append only)

Format: ID, Status, Context, Decision, Rejected, Supersedes, Reopen if. Entries V-001 to V-008 carry Daniel's
answers given in the sr-drawing-pack coordinator chat (2026-11-25; that repo's ADR-056 and later hold the
drawing-tool side). Where an older sparkCAD plan (internal-apps-platform `docs/spark_cad_design/PLAN.md`)
disagrees, these entries win.

## V-001: This fork is the sparkCAD vault, on the workstation-core template (2026-11-25, Daniel, D4a)
- Status: Accepted
- Context: The vault work for the drawing tool lives here; agents must build the agreed plan, not a fallback.
- Decision: Agents work in this repo on branches and PRs under the workstation-core layout (AGENTS.md router,
  DECISIONS, SPEC, STATE). `docs/SPEC.md` is the plan; deviations need Daniel's answer recorded here first.
- Rejected: only Daniel's sparkCAD sessions touch the fork.
- Supersedes: none.
- Reopen if: none.

## V-002: Every made item has a drawing; maker files are controlled like drawings (2026-11-25, Daniel)
- Status: Accepted
- Context: Spark Robotic cuts DXF on CNC laser, plasma and router tables, runs .nc files, and prints from
  sliced G-code.
- Decision: Every item we make has a drawing by default (reference, and to check the part). "Maker files" are
  any files used to make a part: DXF, .nc, G-code, sliced files. Not every item needs every file; the user
  picks the outputs per item. Only drawings are editable. Maker files made by other software are paired with
  their item and rev and go through the same approval as drawings, because they make parts.
- Rejected: maker files outside the vault; every item forced to carry every file type.
- Supersedes: none.
- Reopen if: none.

## V-003: Assemblies link their parts: contains and where used (2026-11-25, Daniel)
- Status: Accepted
- Context: Users need every part inside an assembly, and every assembly a part is used in.
- Decision: The drawing tool's hand-over JSON carries the BOM items. Every custom part and sub-assembly in an
  assembly, and the custom parts inside those sub-assemblies, is its own vault item with its own drawing and
  maker files. Only "contains" is stored (`item_relations`); "where used" is the same relation read
  backwards. Both queries work one level deep or as the full tree.
- Rejected: storing both directions.
- Supersedes: none.
- Reopen if: none.

## V-004: Part numbers come from the vault counter and are written into Fusion first (2026-11-25, Daniel, D1a)
- Status: Accepted
- Context: The drawing's title block and the BOM must carry the vault's number from the start; Fusion is where
  the BOM is made.
- Decision: A Fusion add-in button ("Get part numbers") asks the vault for the next numbers, by kind, for every
  custom component with a blank part number, and writes them into the Fusion components. The drawing and BOM
  read them from Fusion. On upload the vault accepts only numbers it issued and refuses duplicates. With the
  vault unreachable no number is issued; the drawing stays DRAFT and cannot be uploaded.
- Rejected: engineers type numbers by a rule (duplicates found late); the vault numbers at upload (drawing
  would not match).
- Supersedes: open conflict M of the sparkCAD plan (numbering).
- Reopen if: none.

## V-005: Part number format (2026-11-25, Daniel, C8a)
- Status: Accepted
- Context: One standard across everything made in-house, plus bought and customer parts.
- Decision: `PRT-####` part, `SUB-####` sub-assembly, `ASY-####` assembly, `CBL-####` cable, `WIR-####` wire,
  `MFR-####` purchased, `CUS-####` customer part; the outside number (maker or customer) is kept in a searchable
  field. One counter per kind is acceptable only if the vault never reuses a number.
- Rejected: one plain counter for everything.
- Supersedes: EasyPDM's prefix-plus-count default for Spark items.
- Reopen if: none.

## V-006: The editable drawing file is stored with every drawing (2026-11-25, Daniel, C7a)
- Status: Accepted
- Context: A PDF cannot be edited; Daniel's main concern is reopening a drawing later.
- Decision: Each drawing upload carries the drawing tool's editable drawing file beside its PDF and DXF. Editing
  a vault drawing: check out (lock), open in the drawing tool's editor with Fusion on the model, edit, check in
  (new version). PDF and DXF are exports of that file.
- Rejected: regenerating the drawing from scratch each time.
- Supersedes: none.
- Reopen if: none.

## V-007: Maker files are paired two ways and bound by fingerprint plus stamp (2026-11-25, Daniel, C9a, C4a)
- Status: Accepted
- Context: Maker files must always match their item's rev and change together with it.
- Decision: A maker file enters in the drawing tool's "Save to vault" window or later with "Add maker file" on an
  item and rev in the vault; same rules both ways. Each drawing and maker file records the fingerprint (sha256)
  of the STEP it was made from. The vault stamps G-code and .nc files with a comment line (item number, rev,
  fingerprint) and DXF files with a text note, so a loose file can be traced. A file uploaded against an old
  rev is refused.
- Rejected: fingerprint in the JSON only; vault-only pairing.
- Supersedes: none.
- Reopen if: a machine controller rejects the stamp comment line.

## V-008: A new child rev flags the parent; release is blocked until children are complete (2026-11-25, Daniel, C5a, C6a)
- Status: Accepted
- Context: Assemblies must not release with missing or stale child documents.
- Decision: When a child gets a new rev or a new STEP fingerprint, its old drawing and maker files are marked
  stale and every parent is flagged "review needed"; parents are never re-revved automatically. An assembly
  cannot be released until every custom child, all the way down, has its drawing and needed maker files with
  matching fingerprints.
- Rejected: parent follows the latest child; warn-only release check.
- Supersedes: none.
- Reopen if: none.

## V-009: Revs follow the model (2026-11-25, from sr-drawing-pack ADR-011)
- Status: Proposed (Daniel to confirm, D5)
- Context: The drawing tool's rev rules (letters skip I, L, O, Q; AA after Z; drawing-only fixes add a decimal,
  C to C.01) differ from EasyPDM's plain A, B, C.
- Decision: pending D5.
- Rejected: none yet.
- Supersedes: none.
- Reopen if: Daniel answers D5.

## V-010: Rev format follows the drawing tool (2026-11-25, Daniel, D5a)
- Status: Accepted
- Context: Model and drawing revs must match; EasyPDM labels revs plain A, B, C.
- Decision: Rev letters skip I, L, O and Q and continue AA, AB after Z. A drawing-only change that does not affect
  fit, form or function adds a decimal (C to C.01, C.02); a model change bumps the letter and resets the decimal
  (sr-drawing-pack ADR-011). The vault labels revs this way.
- Rejected: EasyPDM's plain A, B, C.
- Supersedes: V-009 (proposed).
- Reopen if: none.

## V-011: Who may approve (2026-11-25, Daniel, D6a)
- Status: Accepted
- Context: Branch `spark/admin-only-release` lets only admins release; Daniel's rule is broader.
- Decision: Anyone who did not make the change may approve. System admins and app owners may approve their own.
- Rejected: admins only (`spark/admin-only-release`).
- Supersedes: open conflict L of the sparkCAD plan.
- Reopen if: none.

## V-012: Only controlled files live in the vault (2026-11-25, Daniel, D7)
- Status: Accepted
- Context: Bought (off-the-shelf) and customer parts are not made here, so their files cannot be controlled. The
  live Fusion models stay in the Fusion hub as "living" or "prototype" files.
- Decision: Vault items exist only for parts, sub-assemblies, assemblies, cables and wires we make. Bought and
  customer parts get no vault item and no drawing; they appear only as BOM rows of the assemblies that use them,
  with their maker or customer number. Fusion models are not stored in the vault.
- Rejected: vault items for bought or customer parts.
- Supersedes: the MFR and CUS kinds of V-005; open item P of the sparkCAD plan.
- Reopen if: none.

## V-013: The generated STEP is stored with the drawing, for viewing (2026-11-25, Daniel, D8)
- Status: Accepted
- Context: "Make drawing" exports a STEP from Fusion to draw the views from.
- Decision: That STEP is uploaded with the drawing and stored for viewing only (3D preview, reference). It is a
  snapshot, never the model of record: the live model stays in the Fusion hub (V-012), and every new export
  replaces the stored STEP of that rev's package. Its sha256 is the fingerprint of V-007.
- Rejected: no STEP in the vault.
- Supersedes: none.
- Reopen if: none.
