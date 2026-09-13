# ADR-001: Unity (C#) as Engine

Status: Accepted (backfilled from arc42 decision table; decided pre-M0)

## Context

Historical economic sandbox with an agent-based NPC economy, real-DEM terrain, isometric presentation, long multi-generational play sessions. Solo developer, agentic (LLM-driven) implementation workflow that needs a verifiable build/test loop and rich editor tooling.

## Decision

Unity 6 (C#) as the game engine.

## Consequences

- Test Framework (NUnit) + batchmode CLI give a scriptable local verification gate.
- Terrain tooling (heightmap import, splat layers) exists out of the box.
- Balance between editor convenience and engine overhead accepted; no custom engine path.
