# Current Status

Updated at the end of each session (or before a significant pause). Keep short — running note, not a log.

## State (2026-09-18, M3 session)

- **M0–M2 committed** (through `8acf168`): verification gate, DEM pipeline + southwest heightmap, predecessor history archive, Core (calendar) + Economy (goods/pricing/agent/two-tier).
- **M3 implemented (pending gate-final + commit approval):** Characters migrated (`AttributeId`/`Attributes` named-field struct, `SkillId`, `Origin`/`OriginProfile` with Unity-serializable named fields replacing the predecessor's silently-lossy `int[][]`, `SkillGrowthConfig` SO, `SkillSystem` with defensive `GetAll` copy, `CheckResolver` 2d10 + documented inclusive `IRandomSource` bounds, `Character`/`CharacterFactory`) and Household migrated (`ConcealedScalar` with a single value field — fixes the predecessor's unrest field-shadowing bug, `MoralValue`, `VitalityValue`, `UnrestSystem`, `ClothingVitalityLink`, `ThrallSupplyNeed` with 364-day clothing rate and season-as-parameter, `HouseholdGoodsStock`, `ManumissionService`). Origin/skill-growth assets generated (`CharacterAssetGenerator`, 4 origins + config, verified against concept 11.6 by table test). Tests: 62 EditMode (was 23) + 2 PlayMode green, 0 warnings, format clean.

## Intentionally deferred / open threads

- **Predecessor dir `E:\projects\game\oerfi` may be deleted now** (all valuable data transferred in M1).
- No git remote configured — push impossible; remote must be LFS-capable.
- Optional package diet still undecided (`ai.assistant`, `visualscripting`, …).
- Two-tier simulation parameters (batch factor, distance threshold) are code constants for now — SO-ification deferred to M13.
- Calendar/attribute/skill/stage display names are German game content — UI surfacing comes with M5.
- 2W10 checks have no difficulty modifiers or criticals (concept 11.4 defines none) — revisit at M14 (combat).
- Unrest accumulation rates (which systems raise/lower unrest per tick) land with M4 AI (predecessor: −0.1/tick passive recovery).
- `HouseholdGoodsStock` stays quantity-based, uncoupled from Economy `Good` (as predecessor; revisit M13).
- Attribute↔skill coupling happens at call sites (`Resolve(attribute, skill)`); no mapping table modeled.
- Batchmode runs add `SENTIS_ANALYTICS_ENABLED` to ProjectSettings scripting defines (package noise) — reverted before each commit; recurs on every editor/batchmode session.

## Next step

Present M3 summary → explicit approval → 4 commits, then **M4 (AI + Terrain: `TaskAssignment`, `AutonomousWorker`, `PriorityOverride`, `HeightmapImporter`, `TerrainResourceLayer`, `ForestRegenerationTick`)** in a fresh plan-mode session.
