# Current Status

Updated at the end of each session (or before a significant pause). Keep short — running note, not a log.

## State (2026-09-18, M4 session)

- **M0–M3 committed** (through `a89ccfd`): verification gate, DEM pipeline + southwest heightmap, predecessor history archive, Core + Economy, Characters + Household, remote `origin` = `git@github.com:HenrikPuls/Oeraefi.git` (SSH Ed25519, LFS).
- **M4 implemented (pending commit approval):** AI migrated (`TaskRole` 18 roles, `SeasonalWindow`/`TaskRoleDefinitions` — only Fowling/Trading/Farming gated to summer months 0–5, `AutonomousWorker` with unified `Tick(month, good, heavyWork)` that really feeds stock (predecessor's always-null good stub fixed), explicit skill mapping incl. the late roles (Construction/Carpenter→Woodworking, Stonemason→Stonemasonry), unrest coupling constants +0.5/−0.1/7, `PriorityOverride` without reflection) and Terrain migrated (`TerrainMetadata` JsonUtility DTO, pure `HeightmapDecoder` implementing the ADR-005 encoding — predecessor never decoded scale/offset, `TerrainResourceLayer` SO with flat arrays and user-tunable regen rate, plain-class `ForestRegenerationTick : ITickable` (predecessor's adapter never registered), `ForestHeuristic` elevation/slope initial density — user decision). Southwest TerrainData asset + 513×513 @ 200 m resource layer generated via batchmode (both idempotency-tested: update-in-place / create-once), both git-LFS-tracked. Tests: 103 EditMode (was 62) + 2 PlayMode green, 0 warnings, format clean.

## Intentionally deferred / open threads

- Terrain renders untextured until M8 (no placeholder terrain layers by design); no water plane yet (M5/M8); manual "Create Scene Preview" button in the importer window for eyeballing.
- Worker role→Good mapping and per-role heavy-work classification are caller-side until M5/M6 data tables exist.
- ForestHeuristic thresholds (200/450 m, 30°, base 0.8) and worker base yields are code constants — SO-ification M13 (two-tier-params precedent).
- Seasonal forest regeneration rates: open concept question (`OnSeasonChange` is a documented no-op).
- Springs (`SpringType`) initialize as None — initialization when a consumer exists.
- Predecessor dir `E:\projects\game\oerfi` may be deleted now (terrain backup code was deliberately NOT ported; visual-dressing ideas for M8 live in the archived history).
- Optional package diet still undecided (`ai.assistant`, `visualscripting`, …).
- Two-tier simulation parameters are code constants for now — SO-ification M13.
- Calendar/attribute/skill/stage/role display names are German game content — UI surfacing in M5.
- 2W10 checks have no difficulty modifiers or criticals (concept 11.4 defines none) — revisit M14.
- Unrest accumulation: overload +0.5/tick, passive recovery −0.1/tick (adopted from predecessor, user decision) — other causes (under-supply, harsh treatment) land with later systems.
- `HouseholdGoodsStock` stays quantity-based, uncoupled from Economy `Good` (revisit M13).
- Attribute↔skill coupling happens at call sites (`Resolve(attribute, skill)`); no mapping table modeled.
- Batchmode runs can add `SENTIS_ANALYTICS_ENABLED` to ProjectSettings scripting defines (package noise) — reverted before commits; did NOT occur in this session's runs.

## Next step

Present M4 summary → explicit approval → 4 commits (AI, terrain runtime+tests, terrain editor+assets, docs), then **M5 (playable slice: runtime bootstrap, isometric camera, calendar ticker driving ITickables, minimal sandbox HUD, scene via editor script, PlayMode smoke tests, user smoke check)** in a fresh plan-mode session.
