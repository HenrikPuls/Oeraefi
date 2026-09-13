# CLAUDE.md — Project Instructions

## Project Overview

Historical economic sandbox game set in Iceland during the Settlement Period (Landnám, c. 874–930 CE). Unity (C#), isometric look via 3D terrain mesh + orthographic camera. Multi-generational dynasty play, NPC-driven economy simulation, combat is secondary. Full game design is documented separately — see `docs/concept/`.

This file governs *how Claude works on this codebase*. It does not restate game design decisions; those live in the concept document and should not be duplicated or paraphrased here.

## Documentation Structure

| Location | Purpose | Language |
|---|---|---|
| `docs/concept/` | Game design document — WHAT and WHY | German (existing) |
| `docs/architecture/arc42.md` | Software architecture — HOW | English |
| `docs/architecture/decisions/` | Architecture Decision Records (ADRs) | English |
| `docs/status/CURRENT.md` | Short running note on in-progress work, open threads, and known WIP state across sessions | English |
| `docs/ROADMAP.md` | Implementation milestones, session sizing, and the standing per-milestone workflow | English |

Keep the two documentation types (concept vs. arc42) strictly separate: game-design rationale belongs in the concept doc, technical/structural rationale belongs in arc42. Don't let one drift into duplicating the other.

`docs/status/CURRENT.md` exists because agentic sessions don't carry memory between chats. At the start of a session, read it. At the end of a session (or before a significant pause mid-task), update it: what's in progress, what's intentionally left half-done, what the next step is. Keep it short — a running note, not a log.

## Architecture Documentation (arc42) — Mandatory

Update `docs/architecture/arc42.md` as part of the same change whenever a change is architecturally relevant: new systems/modules, changed data flow, new external dependencies, changed module boundaries, changed persistence format, changed threading/concurrency model. The doc update is not a follow-up task — it ships with the code change, before the change is considered done.

For a genuinely new architectural decision (not just an implementation detail), add an ADR under `docs/architecture/decisions/ADR-XXX-title.md` (short: context, decision, consequences). Don't create an ADR for routine implementation choices that don't constrain future work.

## Git Workflow — Trunk-Based, Approval Required

- Single main branch, direct commits — no feature branches by default.
- **Never commit, push, or merge autonomously.** Workflow for every change:
  1. Implement the change (code + tests + arc42 update if applicable)
  2. Run the local verification steps (see Testing) yourself — do not report a change as ready without having actually executed them in this session
  3. Present a summary/diff to the user
  4. Wait for explicit approval ("yes", "commit", "looks good", or similar)
  5. Only then `git add` / `git commit` / `git push`
- Commit messages: [Conventional Commits](https://www.conventionalcommits.org/) format (`feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`).
- If a change turns out to be larger or riskier than expected mid-implementation, pause and flag it rather than proceeding to commit on the original approval.

## Testing — Mandatory (Local Verification Gate — No CI Yet)

There is currently no CI. Until CI exists, the local batchmode run below **is** the verification gate — the only thing standing between a broken change and a commit. Do not skip it, and do not report "tests pass" from reasoning about the code instead of actually running it.

- Framework: Unity Test Framework (NUnit-based). EditMode tests for pure logic (economy calculations, skill growth, price derivation), PlayMode tests for integration/runtime behavior.
- Before proposing any commit, run (verified working command, Unity 6000.6.0f1 / Test Framework 1.8.0):

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/run-tests.ps1 -Mode EditMode
```

  and, when PlayMode-relevant code changed (or `-Mode All` for both in one run):

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/run-tests.ps1 -Mode PlayMode
```

  The script wraps Unity batchmode, checks the project isn't locked by an open Editor, fails on nonzero Unity exit code, parses the results XML in `TestResults/`, and fails on any failed test. It deliberately does **not** pass `-quit` — with `-runTests` the Editor process terminates itself; combining both is a known cause of flaky runs. The Unity Editor must be closed while the gate runs (or pass `-Force` if `Temp/UnityLockfile` is a stale leftover from a crashed run).

- Formatting check (same pre-commit step): `dotnet format Oerfi.Runtime.csproj --verify-no-changes` plus the same for `Oerfi.Tests.EditMode.csproj` / `Oerfi.Tests.PlayMode.csproj` / future `Oerfi.*.csproj` assemblies. Scoped to the Oerfi assemblies on purpose — `Assembly-CSharp.csproj` contains the intentionally-kept URP template scripts (`TutorialInfo/`) which are excluded from the formatting gate.
- Compilation: Unity has no simple standalone "compile only" CLI step. Compilation is verified as a side effect of the test run above (tests won't execute if the project doesn't compile). Don't claim "compiles without warnings" independent of this run — check the log output (`TestResults/unity-*.log`) for warnings explicitly, since a failed compile and a clean compile with warnings both need different follow-up.
- New functionality ships with tests in the same change, not as a follow-up.
- When touching existing code that lacks coverage, add tests for the parts you touch opportunistically — don't let coverage regress, but don't block unrelated work on retrofitting full coverage either.
- **Future step (not now):** once the project has multiple contributors or a remote trigger point, consider `game-ci/unity-test-runner` (GitHub Actions) for automated CI. This is an infrastructure decision — flag it as a proposal with an ADR rather than introducing it autonomously.

## Coding Conventions

- Language: English for code, comments, commit messages, and all technical documentation (arc42, ADRs). The concept document stays in German.
- C# naming: `PascalCase` for classes/methods/properties, `camelCase` with `_` prefix for private fields, `UPPER_SNAKE_CASE` for constants.
- Prefer **ScriptableObjects** for balancing/data-driven configuration (skills, origins, price tables, calendar) over hardcoded values — this maps directly to the data tables already defined in the concept doc (skill starting values, Kúgildi price table) and keeps balancing changes out of code review.
- Avoid magic numbers — reference named constants or data assets instead.
- Formatting: follow `.editorconfig` at repo root. Run `dotnet format` (or the IDE-equivalent) as part of the same pre-commit check as the test run, not as a separate manual step someone might skip.

## Unity Asset Handling — Mandatory

Unity's biggest agentic-coding risk isn't the C# code, it's asset metadata and serialized scene/prefab files. These break silently — no compile error, no failing test — and only surface later when someone opens the project in the Editor.

- **Never create, move, rename, or delete an asset file without its matching `.meta` file moving/updating with it.** A `.meta` file left behind or orphaned breaks GUID references (lost prefab/material/script bindings).
- **Never hand-edit `.unity` (scene) or `.prefab` files directly** unless explicitly instructed to. These are only safely editable assuming "Force Text" serialization is active in Editor settings, and even then, structural edits by hand risk corrupting references that only show up on next Editor load. If a task seems to require touching one, flag it and describe the intended change instead of writing the YAML directly.
- Do not assume asset serialization mode — verify `ProjectSettings/EditorSettings.asset` shows text serialization before treating any scene/prefab as diffable/editable at all.

## Persisted Data / Save Compatibility — Mandatory Escalation Trigger

This is a multi-generational dynasty game — save-game longevity across playthroughs matters, and it's an easy thing to break invisibly (e.g. renaming or retyping a field on a serialized class silently invalidates old saves, or worse, deserializes into garbage without erroring).

- Any change to a class, struct, or ScriptableObject that is serialized to a save file requires explicit flagging **before** implementation — call out that a persisted data structure is affected.
- Such a change ships with either a migration path for existing saves, or an explicit, user-approved statement that old saves are intentionally being invalidated. This is not something to decide unilaterally mid-implementation.
- Treat this the same way as the ADR trigger: architecturally relevant → document it; save-format relevant → flag and confirm before proceeding.

## Suggested Directory Structure

```
Assets/
  Scripts/
    Economy/
    Characters/
    Buildings/
    Weather/
    AI/          (task assignment, autonomous worker behavior)
  Data/          (ScriptableObjects: skills, origins, prices, calendar)
  Tests/
    EditMode/
    PlayMode/
docs/
  concept/       (game design doc, German)
  architecture/  (arc42, English)
    decisions/   (ADRs)
  status/        (CURRENT.md — running cross-session note)
```

## Definition of Done

A change is done when:
- Code compiles without warnings (verified via the batchmode test run's log output, not by inspection alone)
- Tests are written/updated and passing, verified by an actually-executed local batchmode run (see Testing)
- Formatting matches `.editorconfig` (verified, not assumed)
- Any asset file additions/moves/deletions have their `.meta` files accounted for
- Any change to persisted/serialized data structures has been flagged and a migration/invalidation decision confirmed
- `docs/architecture/arc42.md` is updated if the change is architecturally relevant
- `docs/status/CURRENT.md` is updated if the change leaves work in progress or changes the current WIP state
- A summary has been presented to the user and explicit approval received
- The commit has been made (only after approval) using Conventional Commits format

## Scope Containment for Autonomous Work

- Stay within the module(s) named or clearly implied by the task. Refactors or cleanups outside that scope are proposed, not performed, even if they seem clearly beneficial.
- Never change ScriptableObject data values (balancing numbers, price tables, skill curves) without an explicit instruction to do so — those are game-design decisions, not code decisions, even though they live in code-adjacent asset files.
- New dependencies or packages are flagged for approval before being added, never added silently as part of a larger task.

## Explicitly Out of Scope for Autonomous Action

- No autonomous `git commit` / `push` / `merge`
- No autonomous addition of new dependencies/packages without flagging them first
- No autonomous deletion of files or tests
- No silent architecture changes without an arc42 update
- No hand-editing of `.unity`/`.prefab` files without explicit instruction
- No changes to persisted/serialized data structures without flagging save-compatibility impact first
- No introduction of CI/build infrastructure without a prior proposal/ADR