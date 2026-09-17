#!/usr/bin/env python3
"""Fetch Copernicus GLO-30 DEM source tiles into the local terrain cache.

Downloads the public Copernicus GLO-30 COG tiles covering a lat/lon bbox
from AWS S3 (no auth required). Files already present and plausibly sized
are skipped, so the script is safe to re-run. Tiles are cached OUTSIDE the
git working tree data paths: terrain-cache/ is gitignored (see ADR-005).

Usage (from anywhere; paths resolved relative to the repo root):
    python tools/terrain/fetch_tiles.py                        # Iceland N63-66 / W13-25
    python tools/terrain/fetch_tiles.py --bbox 63 66 13 25
    python tools/terrain/fetch_tiles.py --force                # re-download

Requires: curl on PATH.
"""

import argparse
import subprocess
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
S3_BASE = "https://copernicus-dem-30m.s3.eu-central-1.amazonaws.com"
MIN_VALID_BYTES = 1000  # smaller than this means a failed/XML-error response
CURL_TIMEOUT_S = 300
ATTEMPTS = 3


def tile_name(lat: int, lon: int) -> str:
    """Copernicus GLO-30 tile stem for an integer degree cell (lon negative = west)."""
    lat_str = f"N{lat:02d}_00" if lat >= 0 else f"S{abs(lat):02d}_00"
    lon_str = f"E{lon:03d}_00" if lon >= 0 else f"W{abs(lon):03d}_00"
    return f"Copernicus_DSM_COG_10_{lat_str}_{lon_str}_DEM"


def is_valid(dest: Path) -> bool:
    return dest.is_file() and dest.stat().st_size > MIN_VALID_BYTES


def download(tile: str, dest: Path) -> bool:
    url = f"{S3_BASE}/{tile}/{tile}.tif"
    for attempt in range(1, ATTEMPTS + 1):
        print(f"  downloading {tile} (attempt {attempt}/{ATTEMPTS})")
        try:
            result = subprocess.run(
                ["curl", "-L", "-f", "-s", "-o", str(dest), url],
                capture_output=True,
                timeout=CURL_TIMEOUT_S,
            )
        except Exception as exc:  # noqa: BLE001 - report and retry
            print(f"    error: {exc}")
            continue
        if result.returncode == 0 and is_valid(dest):
            return True
        print(f"    failed (curl exit {result.returncode})")
    dest.unlink(missing_ok=True)
    return False


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument(
        "--bbox",
        type=int,
        nargs=4,
        metavar=("LAT_MIN", "LAT_MAX", "LON_W_MIN", "LON_W_MAX"),
        default=(63, 66, 13, 25),
        help="Integer degree bbox; longitudes given as positive west degrees "
        "(default: 63 66 13 25 = Iceland N63-66 / W13-25)",
    )
    parser.add_argument(
        "--out",
        type=Path,
        default=REPO_ROOT / "terrain-cache" / "Iceland_Tiles",
        help="Tile cache directory (default: terrain-cache/Iceland_Tiles)",
    )
    parser.add_argument("--force", action="store_true", help="Re-download existing tiles")
    args = parser.parse_args()

    lat_min, lat_max, lon_w_min, lon_w_max = args.bbox
    cells = [
        (lat, lon)
        for lat in range(lat_min, lat_max + 1)
        for lon in range(-lon_w_max, -lon_w_min + 1)  # west degrees -> negative lon
    ]
    args.out.mkdir(parents=True, exist_ok=True)

    print(f"bbox N{lat_min}-N{lat_max} / W{lon_w_min}-W{lon_w_max} -> {len(cells)} tiles")
    print(f"cache dir: {args.out}")

    ok = 0
    skipped = 0
    failed = []
    for lat, lon in cells:
        stem = tile_name(lat, lon)
        dest = args.out / f"{stem}.tif"
        if is_valid(dest) and not args.force:
            print(f"  skip (cached): {stem}")
            skipped += 1
            ok += 1
            continue
        if download(stem, dest):
            ok += 1
        else:
            failed.append(stem)

    print(f"\ndone: {ok} ok ({skipped} skipped as cached), {len(failed)} failed")
    if failed:
        for stem in failed:
            print(f"  FAILED: {stem}")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
