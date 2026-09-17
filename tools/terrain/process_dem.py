#!/usr/bin/env python3
"""Process cached Copernicus GLO-30 tiles into a Unity-ready heightmap.

Pipeline: select tiles intersecting the requested region -> mosaic -> warp
to EPSG:3057 (ISN93 / Lambert 1993) at a fixed metric resolution -> crop a
(2^n + 1) square around the region center -> encode as UInt16 with
raw = clip(height_m * SCALE + OFFSET) where SCALE=10, OFFSET=10000 (sea
level = 10000, sea/nodata = 0) -> write RAW (UInt16 little-endian, canonical
import source), 16-bit PNG (inspection/GIS), and a metadata JSON consumed
by the Unity HeightmapImporter (see ADR-005 and docs/pipeline/terrain-import.md).

Usage (paths resolved relative to the repo root):
    python tools/terrain/process_dem.py --region southwest
    python tools/terrain/process_dem.py --lat 64.256 --lon -21.13 --size 2049

Requires: rasterio, numpy (no pyproj needed; CRS math goes through GDAL).
"""

import argparse
import json
import sys
from pathlib import Path

import numpy as np
import rasterio
import rasterio.warp
import rasterio.windows
from rasterio.enums import Resampling
from rasterio.merge import merge
from rasterio.warp import Resampling as WarpResampling
from rasterio.warp import calculate_default_transform, transform

REPO_ROOT = Path(__file__).resolve().parents[2]

# Vertical encoding (ADR-005): raw = clip(height_m * 10 + 10000, 0, 65535)
SCALE = 10
OFFSET = 10000
NODATA_SRC = -32768.0
TILE_BBOX_MARGIN_DEG = 0.25  # safety margin around the window when preselecting tiles

# Named regions (center latitude, center longitude). Reference entries only —
# the actual playable region is a game-design decision recorded in the metadata.
NAMED_REGIONS = {
    "southwest": (64.256, -21.13),  # Þingvellir / Faxaflói area (concept doc region proposal)
    "central": (64.9, -18.6),  # island center / highlands (predecessor project's map)
}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument(
        "--region",
        choices=sorted(NAMED_REGIONS),
        default=None,
        help="Named region from the built-in table (default: southwest if no --lat/--lon)",
    )
    parser.add_argument("--lat", type=float, help="Center latitude (overrides --region)")
    parser.add_argument("--lon", type=float, help="Center longitude, negative = west")
    parser.add_argument(
        "--tiles-dir",
        type=Path,
        default=REPO_ROOT / "terrain-cache" / "Iceland_Tiles",
        help="Directory with Copernicus *_DEM.tif tiles",
    )
    parser.add_argument(
        "--out",
        type=Path,
        default=REPO_ROOT / "Assets" / "StreamingAssets" / "Terrain",
        help="Output directory (default: Assets/StreamingAssets/Terrain)",
    )
    parser.add_argument("--size", type=int, default=2049, help="Heightmap pixels per side (2^n + 1)")
    parser.add_argument("--resolution", type=int, default=50, help="Grid resolution in meters")
    args = parser.parse_args()

    if args.lat is not None and args.lon is not None:
        args.center = (args.lat, args.lon)
        args.region_name = "custom"
    else:
        region = args.region or "southwest"
        args.center = NAMED_REGIONS[region]
        args.region_name = region
    if (args.lat is None) != (args.lon is None):
        parser.error("--lat and --lon must be given together")
    if (args.size - 1) & (args.size - 2) != 0:
        parser.error("--size must be 2^n + 1 (e.g. 513, 1025, 2049)")
    return args


def collect_tiles(tiles_dir: Path) -> list[Path]:
    tiles = sorted(
        p
        for p in tiles_dir.glob("*_DEM.tif")
        if not p.name.endswith("_QL.tif") and not p.name.endswith("_ABS_QL.tif")
    )
    if not tiles:
        raise SystemExit(f"no *_DEM.tif tiles found in {tiles_dir} - run fetch_tiles.py first")
    return tiles


def select_tiles(tiles: list[Path], window_4326: tuple[float, float, float, float]) -> list[Path]:
    """Keep tiles whose bounds intersect the window bbox (minx, miny, maxx, maxy)."""
    minx, miny, maxx, maxy = window_4326
    selected = []
    for path in tiles:
        with rasterio.open(path) as src:
            b = src.bounds
        if b.right < minx or b.left > maxx or b.top < miny or b.bottom > maxy:
            continue
        selected.append(path)
    if not selected:
        raise SystemExit("no source tiles intersect the requested window")
    return selected


def main() -> int:
    args = parse_args()
    center_lat, center_lon = args.center
    res = args.resolution
    size = args.size
    half_extent = size * res / 2.0

    # 1. Region center -> EPSG:3057 (ISN93) via GDAL.
    center_x, center_y = transform("EPSG:4326", "EPSG:3057", [center_lon], [center_lat])
    center_x, center_y = center_x[0], center_y[0]

    # 2. Window bounds in 3057, and the window bbox in 4326 (strict + padded for
    #    tile preselection). All coverage comparisons happen in 4326, the CRS the
    #    tile bounds are expressed in.
    win_minx, win_maxx = center_x - half_extent, center_x + half_extent
    win_miny, win_maxy = center_y - half_extent, center_y + half_extent
    window_3057 = (win_minx, win_miny, win_maxx, win_maxy)
    corners_x = [win_minx, win_maxx, win_minx, win_maxx]
    corners_y = [win_miny, win_maxy, win_maxy, win_miny]
    corner_lons, corner_lats = transform("EPSG:3057", "EPSG:4326", corners_x, corners_y)
    window_4326_strict = (min(corner_lons), min(corner_lats), max(corner_lons), max(corner_lats))
    window_4326 = tuple(
        v - TILE_BBOX_MARGIN_DEG if i < 2 else v + TILE_BBOX_MARGIN_DEG
        for i, v in enumerate(window_4326_strict)
    )

    # 3. Tile preselection + coverage check (fail loudly instead of shifting the map).
    all_tiles = collect_tiles(args.tiles_dir)
    tiles = select_tiles(all_tiles, window_4326)
    print(f"tiles: {len(tiles)}/{len(all_tiles)} intersect the window")
    bounds_list = []
    for path in tiles:
        with rasterio.open(path) as src:
            bounds_list.append(src.bounds)
    coverage = (
        min(b.left for b in bounds_list),
        min(b.bottom for b in bounds_list),
        max(b.right for b in bounds_list),
        max(b.top for b in bounds_list),
    )
    if (
        window_4326_strict[0] < coverage[0]
        or window_4326_strict[1] < coverage[1]
        or window_4326_strict[2] > coverage[2]
        or window_4326_strict[3] > coverage[3]
    ):
        raise SystemExit(
            f"window (4326 bbox) {window_4326_strict} exceeds tile coverage "
            f"{coverage} - fetch more tiles first (fetch_tiles.py)"
        )

    # 4. Mosaic the selected tiles.
    print("mosaicking...")
    sources = [rasterio.open(p) for p in tiles]
    mosaic, mosaic_transform = merge(sources, resampling=Resampling.cubic)
    mosaic_crs = sources[0].crs
    mosaic_profile = sources[0].profile.copy()
    for src in sources:
        src.close()
    print(f"mosaic: {mosaic.shape[2]}x{mosaic.shape[1]} crs={mosaic_crs}")

    # 5. Write the mosaic to a temp GTiff (this rasterio build ships no memory
    #    module), then reproject directly into a numpy destination array.
    print(f"warping to EPSG:3057 @ {res} m...")
    tmp_dir = REPO_ROOT / "terrain-cache" / "tmp"
    tmp_dir.mkdir(parents=True, exist_ok=True)
    mosaic_path = tmp_dir / f"{args.region_name}_mosaic.tif"
    profile = mosaic_profile.copy()
    profile.update(
        height=mosaic.shape[1],
        width=mosaic.shape[2],
        transform=mosaic_transform,
        crs=mosaic_crs,
        dtype="float32",
        nodata=NODATA_SRC,
        compress="deflate",
    )
    with rasterio.open(mosaic_path, "w", **profile) as dataset:
        dataset.write(mosaic.astype("float32"))

    with rasterio.open(mosaic_path) as dataset:
        dst_transform, dst_width, dst_height = calculate_default_transform(
            dataset.crs,
            "EPSG:3057",
            dataset.width,
            dataset.height,
            *dataset.bounds,
            resolution=res,
        )
        warped = np.full((dst_height, dst_width), NODATA_SRC, dtype="float32")
        rasterio.warp.reproject(
            source=rasterio.band(dataset, 1),
            destination=warped,
            src_transform=dataset.transform,
            src_crs=dataset.crs,
            src_nodata=NODATA_SRC,
            dst_transform=dst_transform,
            dst_crs="EPSG:3057",
            dst_nodata=NODATA_SRC,
            resampling=WarpResampling.cubic,
        )
    mosaic_path.unlink()

    print(f"warped: {dst_width}x{dst_height}")

    # 6. Crop the fixed-size window around the region center (no silent clamping).
    col = (center_x - dst_transform.c) / res
    row = (dst_transform.f - center_y) / res
    col0 = int(round(col - size / 2))
    row0 = int(round(row - size / 2))
    if col0 < 0 or row0 < 0 or col0 + size > dst_width or row0 + size > dst_height:
        raise SystemExit(
            f"window does not fit warped raster "
            f"(row0={row0}, col0={col0}, need {size}x{size}, have {dst_height}x{dst_width})"
        )
    win = rasterio.windows.Window(col0, row0, size, size)
    window_transform = rasterio.windows.transform(win, dst_transform)
    data = warped[row0 : row0 + size, col0 : col0 + size]

    # 7. Encode: meters -> UInt16 (SCALE/OFFSET), sea/nodata -> 0.
    meters = np.where(data == NODATA_SRC, 0.0, data)
    meters = np.maximum(meters, 0.0)
    encoded = np.clip(meters * SCALE + OFFSET, 0, 65535).astype("uint16")

    # 8. Outputs.
    args.out.mkdir(parents=True, exist_ok=True)
    stem = f"{args.region_name}_heightmap_{res}m_{size}x{size}"
    raw_path = args.out / f"{stem}.raw"
    png_path = args.out / f"{stem}.png"
    meta_path = args.out / f"{args.region_name}_metadata.json"

    encoded.astype("<u2").tofile(raw_path)  # explicit little-endian (ADR-005)

    png_profile = {
        "driver": "PNG",
        "height": size,
        "width": size,
        "count": 1,
        "dtype": "uint16",
        "crs": "EPSG:3057",
        "transform": window_transform,
    }
    with rasterio.open(png_path, "w", **png_profile) as dst_png:
        dst_png.write(encoded, 1)

    actual_center_x = window_transform.c + size * res / 2
    actual_center_y = window_transform.f - size * res / 2
    heights = (encoded.astype(np.float64) - OFFSET) / SCALE
    # Ocean arrives from GLO-30 as true 0.0 m elevation (encodes to 10000);
    # count it as sea. encoded == 0 would only flag source nodata.
    sea_fraction = float(np.count_nonzero(heights <= 0.0) / heights.size)
    meta = {
        "region": args.region_name,
        "requested_center_latlon": [center_lat, center_lon],
        "source": "Copernicus DEM GLO-30 (see ADR-005 for credit lines)",
        "source_tiles": [p.name for p in tiles],
        "target_crs": "EPSG:3057 (ISN93 / Lambert 1993)",
        "resolution_m": res,
        "size": [size, size],
        "extent_m": [size * res, size * res],
        "center_3057": [float(actual_center_x), float(actual_center_y)],
        "bounds_3057": [
            float(window_transform.c),
            float(window_transform.f - size * res),
            float(window_transform.c + size * res),
            float(window_transform.f),
        ],
        "scale_factor": SCALE,
        "offset": OFFSET,
        "nodata_value": 0,
        "sea_level_raw": OFFSET,
        "height_range_m": [float(heights.min()), float(heights.max())],
        "sea_fraction": sea_fraction,
        "format": "UInt16 little-endian RAW (canonical) / 16-bit PNG (inspection)",
        "encoding": "raw = clip(height_m * 10 + 10000, 0, 65535); 10000 = sea level; "
        "ocean areas arrive as 0.0 m elevation and encode to 10000",
    }
    meta_path.write_text(json.dumps(meta, indent=2) + "\n", encoding="utf-8")

    # 9. Report.
    print("\n=== result ===")
    print(f"raw:      {raw_path}  ({raw_path.stat().st_size} bytes, expected {size * size * 2})")
    print(f"png:      {png_path}  ({png_path.stat().st_size} bytes)")
    print(f"metadata: {meta_path}")
    print(
        f"elevation: min {heights.min():.1f} m, max {heights.max():.1f} m, "
        f"mean {heights.mean():.1f} m, sea {sea_fraction * 100:.1f}%"
    )
    print(f"bounds 3057: {meta['bounds_3057']}")
    if raw_path.stat().st_size != size * size * 2:
        print("ERROR: raw file size mismatch")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
