# ADR-005: Terrain DEM Pipeline (Source, Encoding, Data Layout)

Status: Accepted (M1)

## Context

The game map is built from real elevation data (concept doc 12.1; ADR-002). The predecessor project proved the pipeline shape (tiles → mosaic → warp → 2ⁿ+1 heightmap) but left it hardcoded to its own absolute paths, committed ~585 MB of intermediate DEM products directly into git without LFS, and had an importer that ignored its own metadata file and read PNGs through Unity's lossy 8-bit red channel. The old project directory will be deleted; its valuable data must live in this repo first.

## Decision

- **Source:** Copernicus DEM GLO-30 (30 m, public AWS S3) as the working source. Upgrade option: ÍslandsDEM v1.0 (10 m, CC BY 4.0) — the pipeline's `--tiles-dir` accepts any raster tile directory, so upgrading is a reprocess, not a rewrite.
- **CRS / grid:** EPSG:3057 (ISN93 / Lambert 1993), fixed metric resolution (default 50 m), heightmap side length 2ⁿ+1 (default 2049).
- **Vertical encoding (binding for all importers):** `raw = clip(height_m × 10 + 10000, 0, 65535)`; sea level = 10000; `0` = source-nodata only. Ocean areas arrive from GLO-30 as true 0.0 m elevation and therefore encode to 10000 — terrain at 0 m until a water plane renders it (M4/M8). Canonical import source is the little-endian UInt16 RAW file — never the PNG via Unity texture import (8-bit lossy path).
- **Tooling:** `tools/terrain/fetch_tiles.py` (download, gitignored cache) and `tools/terrain/process_dem.py` (parameterized region/size/resolution, coverage check that fails loudly instead of shifting the map, metadata JSON output). Python + rasterio/numpy; no GDAL CLI, no QGIS.
- **Data layout:**
  - `terrain-cache/` — gitignored, holds source tiles (411 MB for Iceland) and pipeline temp files; fully regenerable via the tools.
  - `Assets/StreamingAssets/Terrain/` — repo-tracked (via git-LFS: `*.raw` added to `.gitattributes`) processed heightmap + metadata JSON for the current playable region.
  - Intermediate mosaic/warp products are never committed; the predecessor's are discarded (regenerable).
- **Region:** the playable region is a game-design choice, parameterized per run and recorded in the metadata JSON. Current choice: `southwest` (center Þingvellir/Faxaflói area, ~102×102 km), matching the concept doc's region proposal.

## Consequences

- The repo carries one ~8 MB RAW + ~4.6 MB PNG per region instead of hundreds of MB of intermediates.
- Reprocessing for a different region or resolution is one script run (minutes).
- M4's HeightmapImporter must implement: RAW UInt16-LE reading, scale/offset application from the metadata JSON, 2ⁿ+1 resolution handling, EPSG:3057-bounds→terrain-size mapping.
- Binary heightmaps depend on git-LFS being available wherever the repo is cloned; a future remote must be LFS-capable (tracked risk in arc42 §11).
