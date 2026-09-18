# Current Status

Updated at the end of each session (or before a significant pause). Keep short — running note, not a log.

## State (2026-09-18, M2 session)

- **M0 + M1 committed** (through `b943b84`): verification gate, DEM pipeline + southwest heightmap (LFS), predecessor history archive.
- **M2 implemented (pending gate + commit approval):** Core migrated (`Season`, `ITickable`, `CalendarModel` — pure C#, 364-day year, Aukanœtr 4 days, Sumarauki leap week every 6th year per user decision; no MonoBehaviour singleton anymore). Economy migrated with English API (`Good` + runtime-only factory, `PricingConfig` SO, `PricingEngine`, `FarmEconomyAgent` with priority-sorted consumption, two-tier detail levels). `GoodAssetGenerator` rebuilt to actually write values (predecessor's assets were empty shells): creates with concept-table values, verifies existing ones and warns on drift without overwriting user balancing. Tests ported + extended (calendar 10, economy 9 + asset-table guard 2). arc42 §5/§6 updated.

## Intentionally deferred / open threads

- **Predecessor dir `E:\projects\game\oerfi` may be deleted now** (all valuable data transferred in M1).
- No git remote configured — push impossible; remote must be LFS-capable.
- Optional package diet still undecided (`ai.assistant`, `visualscripting`, …).
- Two-tier simulation parameters (batch factor, distance threshold) are code constants for now — SO-ification deferred to M13.
- Calendar month identifiers are ASCII-safe in code; proper Icelandic display names must be used in UI (M3/M5).
- M3 next: Characters + Household migration (attributes/origins/skills/2d10, thrall supply, concealed values, vitality).

## Next step

Verify M2 (dotnet format, gate All), present summary, execute the 4 planned commits, then **M3 (Characters + Household migration)** in a fresh plan-mode session.
