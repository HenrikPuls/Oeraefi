# Terrain Pipeline: DEM → Unity Heightmap

Scripted pipeline that turns public Copernicus GLO-30 elevation tiles into the heightmap consumed by the Unity `HeightmapImporter` (built in milestone M4). No QGIS or manual GIS steps required. See **ADR-005** for the decision record and credit lines.

## Data flow

```
Copernicus GLO-30 COG tiles (AWS S3, public)
  → tools/terrain/fetch_tiles.py        (download into gitignored terrain-cache/)
  → tools/terrain/process_dem.py        (mosaic → warp EPSG:3057 → crop 2^n+1 → UInt16 encode)
  → Assets/StreamingAssets/Terrain/     (*.raw canonical, *.png inspection, *_metadata.json)
  → (M4) HeightmapImporter reads RAW + metadata JSON → Unity Terrain
```

Both scripts resolve paths relative to the repo root; run them from anywhere.

## 1. Fetch source tiles

```bash
python tools/terrain/fetch_tiles.py                      # Iceland N63-66 / W13-25 (default)
python tools/terrain/fetch_tiles.py --bbox 63 66 13 25   # explicit integer-degree bbox
```

Tiles land in `terrain-cache/Iceland_Tiles/` (gitignored — 411 MB for full Iceland, regenerable). Existing valid tiles are skipped; `--force` re-downloads.

## 2. Process into a heightmap

```bash
python tools/terrain/process_dem.py --region southwest            # named region table
python tools/terrain/process_dem.py --lat 64.256 --lon -21.13     # custom center
python tools/terrain/process_dem.py --region central --size 1025  # smaller map
```

Steps performed: tile preselection by intersection → coverage check (fails loudly if the window exceeds cached tiles; no silent map shifting) → mosaic → warp to EPSG:3057 (ISN93 / Lambert 1993) at `--resolution` (default 50 m) → crop of `--size` (default 2049 = 2¹¹+1) pixels square around the region center → UInt16 encoding.

Outputs in `Assets/StreamingAssets/Terrain/`:

| File | Purpose |
|---|---|
| `<region>_heightmap_<res>m_<size>x<size>.raw` | **Canonical** import source: UInt16 little-endian, exactly `size² × 2` bytes |
| `<region>_heightmap_<res>m_<size>x<size>.png` (+ `.aux.xml`) | 16-bit grayscale copy for human/GIS inspection |
| `<region>_metadata.json` | Region, CRS, bounds (EPSG:3057), scale/offset, height range, sea fraction, tile list — consumed by the M4 importer |

## 3. Vertical encoding convention (binding for importers)

```
raw_value = clip(height_m * 10 + 10000, 0, 65535)
height_m  = (raw_value - 10000) / 10
10000     = sea level
0         = source nodata only (GLO-30 ocean arrives as true 0.0 m → encodes to 10000)
```

The PNG is encoded identically; the 16-bit precision is preserved in both. Note the predecessor project's importer read PNGs through Unity's 8-bit red channel — that lossy path must not be used; import from RAW (M4).

## 4. Upgrade path: ÍslandsDEM (10 m)

The current source is Copernicus GLO-30 (30 m). The concept doc (section 12.1) prefers **ÍslandsDEM v1.0** (10 m, partially 2 m; Landmælingar Íslands + PGC, CC BY 4.0, <https://dem.lmi.is/mapview/>). Because `process_dem.py` takes any raster tile directory, the upgrade is: fetch ÍslandsDEM GeoTIFFs into a cache folder, point `--tiles-dir` at it, reprocess. Required credit either way — game credits must carry:

> Map data: Landmælingar Íslands (ÍslandsDEM v1.0), CC BY 4.0

or for the current source:

> Copernicus DEM — GLO-30: contains modified Copernicus data
