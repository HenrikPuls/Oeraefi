# ADR-002: 3D Terrain + Orthographic Camera instead of 2D Tile Isometric

Status: Accepted (backfilled from arc42 decision table; decided pre-M0)

## Context

The map is based on real Icelandic elevation data (ÍslandsDEM / Copernicus GLO-30) with irregular fjord coastlines. Classic 2D diamond-tile isometry produces visible seams on such coastlines and fakes elevation.

## Decision

Render a true 3D terrain mesh viewed by an orthographic camera at isometric angle; blend ground types via splat-map materials instead of discrete tile sprites.

## Consequences

- Real elevation (fjords, lava fields, slopes) is structural, not painted on.
- Coastline shapes follow the DEM directly.
- Camera stays gameplay-legible (classic 30°/45° isometric look).
