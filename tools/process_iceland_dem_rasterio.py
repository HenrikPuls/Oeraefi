#!/usr/bin/env python3
"""
Island DEM Pipeline mit rasterio — mosaikiert 44 Copernicus DEM GLO-30 Tiles zu
einem Unity-fertigen 16-Bit Heightmap (2ⁿ+1 Auflösung, 50 m Grid).

Benötigt: rasterio, numpy, affine, pyproj

Output:
- iceland_heightmap_50m_2049x2049.png  (16-Bit PNG, für HeightmapImporter)
- iceland_heightmap_50m_2049x2049.raw  (RAW UInt16, Alternative)
- iceland_metadata.json                (Bounds, Scale, Offset für Importer)
"""

import os
import json
import numpy as np
import rasterio
from rasterio.merge import merge
from rasterio.warp import reproject, Resampling, calculate_default_transform
from rasterio.crs import CRS
from rasterio.transform import Affine
from rasterio.enums import Resampling as RioResampling

# Pfade
TILES_DIR = r"E:/projects/game/oerfi/game/Assets/_Project/StreamingAssets/Heightmaps/Iceland_Tiles"
OUTPUT_DIR = r"E:/projects/game/oerfi/game/Assets/_Project/StreamingAssets/Heightmaps/Processed"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# 1. Alle Tile-Pfade sammeln
tif_files = sorted([
    os.path.join(TILES_DIR, f) 
    for f in os.listdir(TILES_DIR) 
    if f.endswith('_DEM.tif') and not f.endswith('_QL.tif') and not f.endswith('_ABS_QL.tif')
])
print(f"Gefundene Tiles: {len(tif_files)}")

# 2. Mosaik bauen mit rasterio.merge
print("Baue Mosaik...")
src_files = [rasterio.open(f) for f in tif_files]
mosaic, mosaic_transform = merge(src_files, resampling=RioResampling.cubic)
# mosaic shape: (bands, height, width) -> (1, H, W)
print(f"Mosaik Shape: {mosaic.shape}")
print(f"Mosaik Transform: {mosaic_transform}")
print(f"Mosaik CRS: {src_files[0].crs}")

# Metadaten vom ersten Tile
profile = src_files[0].profile.copy()
profile.update({
    'height': mosaic.shape[1],
    'width': mosaic.shape[2],
    'transform': mosaic_transform,
    'compress': 'deflate',
    'tiled': True,
    'dtype': 'float32',
    'nodata': -32768
})

# Mosaik als temporäres TIF speichern
mosaic_path = os.path.join(OUTPUT_DIR, "iceland_mosaic.tif")
with rasterio.open(mosaic_path, 'w', **profile) as dst:
    dst.write(mosaic)

# Close source files
for src in src_files:
    src.close()
print(f"Mosaik gespeichert: {mosaic_path}")

# 3. Reprojizieren auf ISN93 / Lambert 1993 (EPSG:3057) @ 50 m
print("Reprojiziere auf EPSG:3057 @ 50 m...")
with rasterio.open(mosaic_path) as src:
    # Ziel-CRS
    dst_crs = CRS.from_epsg(3057)
    
    # Default-Transform berechnen
    dst_transform, dst_width, dst_height = calculate_default_transform(
        src.crs, dst_crs, src.width, src.height, *src.bounds,
        resolution=50
    )
    
    print(f"Ziel-Größe: {dst_width} x {dst_height}")
    print(f"Ziel-Transform: {dst_transform}")
    
    # Destination profile
    dst_profile = src.profile.copy()
    dst_profile.update({
        'crs': dst_crs,
        'transform': dst_transform,
        'width': dst_width,
        'height': dst_height,
        'dtype': 'float32',
        'nodata': -32768,
        'compress': 'deflate',
        'tiled': True
    })
    
    warped_path = os.path.join(OUTPUT_DIR, "iceland_warped_50m.tif")
    with rasterio.open(warped_path, 'w', **dst_profile) as dst:
        for i in range(1, src.count + 1):
            reproject(
                source=rasterio.band(src, i),
                destination=rasterio.band(dst, i),
                src_transform=src.transform,
                src_crs=src.crs,
                dst_transform=dst_transform,
                dst_crs=dst_crs,
                resampling=RioResampling.cubic,
                src_nodata=src.nodata,
                dst_nodata=-32768
            )
    print(f"Gewarpt: {warped_path}")

# 4. Auf 2049x2049 croppen (zentral Island)
target_size = 2049  # 2^11 + 1

with rasterio.open(warped_path) as src:
    # Zentrum berechnen
    bounds = src.bounds
    center_x = (bounds.left + bounds.right) / 2
    center_y = (bounds.bottom + bounds.top) / 2
    half_extent = target_size * 50 / 2  # 50 m/Pixel
    
    # Neues Fenster definieren
    new_bounds = (
        center_x - half_extent,
        center_y - half_extent,
        center_x + half_extent,
        center_y + half_extent
    )
    
    # Window berechnen
    window = src.window(*new_bounds)
    print(f"Crop Window: {window}")
    
    # Neues Transform für das Fenster
    window_transform = src.window_transform(window)
    
    # Daten lesen
    data = src.read(1, window=window, out_shape=(target_size, target_size), 
                    resampling=RioResampling.cubic)
    
    print(f"Gecroppte Daten: {data.shape}, min={data.min():.1f}, max={data.max():.1f}")

# 5. In 16-Bit UInt16 umwandeln
# Meeresspiegel = 0 m -> Offset 10000 (für negative Werte)
# Skalierung: 1 m = 10 Einheiten
OFFSET = 10000
SCALE = 10

# NoData (Meer) auf 0 setzen
data = np.where(data == -32768, 0, data)
# Auch negative Werte (unter Meeresspiegel) auf 0 clampen für Meer
data = np.maximum(data, 0)

arr_uint16 = np.clip((data * SCALE + OFFSET).astype(np.uint16), 0, 65535)
print(f"UInt16: min={arr_uint16.min()}, max={arr_uint16.max()}")
print(f"Höhenbereich: {((arr_uint16.min() - OFFSET) / SCALE):.1f} bis {((arr_uint16.max() - OFFSET) / SCALE):.1f} m")

# 6. Als 16-Bit PNG speichern
png_path = os.path.join(OUTPUT_DIR, "iceland_heightmap_50m_2049x2049.png")
png_profile = {
    'driver': 'PNG',
    'height': target_size,
    'width': target_size,
    'count': 1,
    'dtype': 'uint16',
    'crs': dst_crs,
    'transform': window_transform,
    'nodata': 0
}
with rasterio.open(png_path, 'w', **png_profile) as dst:
    dst.write(arr_uint16, 1)
print(f"PNG gespeichert: {png_path}")

# 7. Als RAW speichern (little-endian)
raw_path = os.path.join(OUTPUT_DIR, "iceland_heightmap_50m_2049x2049.raw")
arr_uint16.tofile(raw_path)
print(f"RAW gespeichert: {raw_path}")

# 8. Metadaten für HeightmapImporter
meta = {
    "source": "Copernicus DEM GLO-30 Public (44 Tiles, N63-66 W013-025)",
    "target_crs": "EPSG:3057 (ISN93 / Lambert 1993)",
    "resolution_m": 50,
    "size": [target_size, target_size],
    "extent_m": [target_size * 50, target_size * 50],
    "center_3057": [float(center_x), float(center_y)],
    "bounds_3057": [float(new_bounds[0]), float(new_bounds[1]), float(new_bounds[2]), float(new_bounds[3])],
    "scale_factor": SCALE,
    "offset": OFFSET,
    "nodata_value": 0,
    "sea_level_raw": OFFSET,  # 10000 = 0 m
    "height_range_m": [float((arr_uint16.min() - OFFSET) / SCALE), float((arr_uint16.max() - OFFSET) / SCALE)],
    "format": "UInt16 PNG / RAW (little-endian)",
    "note": "In HeightmapImporter: Resolution 2049 (2^11+1), Scale=0.1, Offset=-1000"
}
meta_path = os.path.join(OUTPUT_DIR, "iceland_metadata.json")
with open(meta_path, 'w') as f:
    json.dump(meta, f, indent=2)
print(f"Metadaten: {meta_path}")

print("\n✅ Fertig! Importiere im Unity Editor:")
print(f"  1. Terrain → Heightmap Importer")
print(f"  2. PNG laden: {png_path}")
print(f"  3. Resolution: 2049 (wird erzwungen)")
print(f"  4. Scale: 0.1, Offset: -1000")
print(f"  5. Import → TerrainData erzeugt")