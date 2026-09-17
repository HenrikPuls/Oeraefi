# Current Status

Updated at the end of each session (or before a significant pause). Keep short — running note, not a log.

## State (2026-09-18, M1 session — committed)

- **M0 committed** (`b55d7f1`, `29108ff`, `f56c9dd`): verification gate, assembly structure, smoke tests, docs baseline.
- **M1 committed:** scripted DEM pipeline `tools/terrain/` (fetch_tiles.py / process_dem.py, parameterized, ADR-005); legacy GDAL scripts deleted; 44 GLO-30 tiles in gitignored `terrain-cache/` (411 MB); playable region `southwest` (~102×102 km, center Þingvellir/Faxaflói) in `Assets/StreamingAssets/Terrain/` via git-LFS (RAW exact size, max 1406.8 m, 11% sea — Faxaflói + Langjökull verified); old history archived as binary-stripped bundle `docs/archive/predecessor-history.bundle` (13 commits, 163 KB, probe-clone verified). EditMode gate green (2/2, exit 0, no warnings), Unity import verified (all metas, no warnings).

## Intentionally deferred / open threads

- **Predecessor dir `E:\projects\game\oerfi` may be deleted now** — everything valuable is transferred (tiles in terrain-cache, history in bundle, region heightmap in repo).
- Uncommitted editor-session resaves (`Assets/Settings/DefaultVolumeProfile.asset`, `ProjectSettings/ProjectSettings.asset`) deliberately excluded from M1 commits — decide: separate chore commit or discard.
- No git remote configured — push impossible; remote must be LFS-capable (heightmaps now depend on LFS).
- Optional package diet still undecided (`ai.assistant`, `visualscripting`, …).
- M2 next: migrate Core + Economy modules (CalendarService, Goods, PricingEngine, FarmEconomyAgent) cleaned to Oerfi.* conventions; port EditMode tests.

## Next step

Start **M2 (Core + Economy migration)** in a fresh plan-mode session.
