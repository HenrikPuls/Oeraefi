# Current Status

Updated at the end of each session (or before a significant pause). Keep short — running note, not a log.

## State (2026-09-13, M0 session)

- **M0 complete (pending user commit approval):** verification gate `scripts/run-tests.ps1` (EditMode/PlayMode batchmode), assembly structure (`Oerfi.Runtime`, `Oerfi.Editor`, `Oerfi.Tests.EditMode/PlayMode`), smoke tests green, `.editorconfig`, `docs/ROADMAP.md`, arc42 updates, ADR-001..004 backfilled.
- Old predecessor project at `E:\projects\game\oerfi` still exists — holds the migrated-from codebase (13 commits, Phase A–C), 44 GLO-30 DEM tiles, processed 2049² heightmap + metadata, and its git history.

## Intentionally deferred / open threads

- **Do not delete `E:\projects\game\oerfi` until M1 is done** (heightmap, tiles, and git-bundle archive must be transferred first).
- No git remote configured — push impossible; LFS-capability of any future remote unverified (`.gitattributes` routes binary types to LFS).
- Optional package diet not decided: `com.unity.ai.assistant`, `com.unity.ai.inference`, `com.unity.visualscripting`, storytelling/worldbuilding feature packages could be removed to speed up batchmode runs.
- `dotnet format` works with `Oerfi.slnx`/Unity-generated csproj. Formatting gate is scoped to the `Oerfi.*.csproj` assemblies; `Assembly-CSharp.csproj` (contains the intentionally-kept `TutorialInfo/` template scripts) is excluded — it would fail CHARSET checks on kept files.
- `TutorialInfo/` + `Readme.asset` intentionally kept (user decision); stray `New Scene.unity` deleted (GUID-checked unreferenced before deletion).

## Next step

Start **M1 (Data & Archive Migration)** in a fresh plan-mode session: LFS import of heightmap+metadata, tiles → `terrain-cache/`, old history → `docs/archive/*.bundle`, rework `tools/` DEM scripts to parameterized paths, English rewrite of `docs/pipeline/terrain-import.md`, ADR-005.
