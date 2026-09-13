#!/usr/bin/env python3
"""
Island DEM Pipeline — mosaikiert 44 Copernicus DEM GLO-30 Tiles zu
einem Unity-fertigen 16-Bit Heightmap (2ⁿ+1 Auflösung, 50 m Grid).

Benötigt: GDAL (osgeo) — in QGIS-Umgebung oder `conda install -c conda-forge gdal` verfügbar.

Output:
- iceland_heightmap_50m_2049x2049.png  (16-Bit PNG, für HeightmapImporter)
- iceland_heightmap_50m_2049x2049.raw  (RAW UInt16, Alternative)
- iceland_metadata.json                (Bounds, Scale, Offset für Importer)
"""

import os
import json
import numpy as np
from osgeo import gdal, osr

# Pfade anpassen falls nötig
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

# 2. VRT-Mosaik bauen
vrt_path = os.path.join(OUTPUT_DIR, "iceland_mosaic.vrt")
print("Baue VRT-Mosaik...")
vrt = gdal.BuildVRT(vrt_path, tif_files, options=gdal.BuildVRTOptions(resampleAlg='cubic'))
vrt = None
print(f"VRT: {vrt_path}")

# 3. VRT prüfen
ds = gdal.Open(vrt_path)
print(f"Mosaik-Größe: {ds.RasterXSize} x {ds.RasterYSize} px")
print(f"Projektion: {ds.GetProjection()}")
gt = ds.GetGeoTransform()
print(f"GeoTransform: {gt}")
ds = None

# 4. Reprojizieren auf ISN93 / Lambert 1993 (EPSG:3057) + 50 m Grid
# Island Bounds in EPSG:3057: ca. X: -150000..600000, Y: 300000..850000
# Wir nutzen gdalwarp für Reprojektion + Resampling
warped_path = os.path.join(OUTPUT_DIR, "iceland_warped_50m.tif")
print("Reprojiziere auf EPSG:3057 @ 50 m...")
warp_opts = gdal.WarpOptions(
    dstSRS='EPSG:3057',
    xRes=50, yRes=50,
    resampleAlg='cubic',
    dstNodata=-32768,
    outputType=gdal.GDT_Float32,
    format='GTiff',
    creationOptions=['COMPRESS=DEFLATE', 'TILED=YES', 'BIGTIFF=YES']
)
warped = gdal.Warp(warped_path, vrt_path, options=warp_opts)
warped = None
print(f"Gewarpt: {warped_path}")

# 5. Auf 2ⁿ+1 croppen (nächstes 2ⁿ+1 >= Ausdehnung)
# 2049 = 2¹¹+1 deckt Island gut ab (~300 km / 50 m = 6000 px -> nächstes 2ⁿ+1 = 8193, 
# aber für Gameplay reicht 2049 (102 km) — wir croppen auf Zentral-Island)
target_size = 2049  # 2^11 + 1 = 2049, entspricht ~102 km Kantenlänge
# Wir nehmen den zentralen Bereich Islands
ds = gdal.Open(warped_path)
gt = ds.GetGeoTransform()
x_center = gt[0] + ds.RasterXSize * gt[1] / 2
y_center = gt[3] + ds.RasterYSize * gt[5] / 2
half_extent = target_size * 50 / 2  # 50 m/Pixel
# Ausdehnung in EPSG:3057 Koordinaten
ulx = x_center - half_extent
uly = y_center + half_extent
lrx = x_center + half_extent
lry = y_center - half_extent

cropped_path = os.path.join(OUTPUT_DIR, "iceland_cropped_2049.tif")
print(f"Croppe auf {target_size}x{target_size} (Zentral-Island)...")
translate_opts = gdal.TranslateOptions(
    projWin=[ulx, uly, lrx, lry],
    width=target_size,
    height=target_size,
    resampleAlg='cubic',
    outputType=gdal.GDT_Float32
)
cropped = gdal.Translate(cropped_path, warped_path, options=translate_opts)
cropped = None
print(f"Gecroppt: {cropped_path}")

# 6. In 16-Bit UInt16 umwandeln (Skalierung: Meter -> 0-65535)
# Meeresspiegel = 0 m -> Offset 10000 (damit negative Werte möglich sind)
# Skalierung: 1 m = 10 Einheiten -> 6553.5 m Max-Höhe
ds = gdal.Open(cropped_path)
band = ds.GetRasterBand(1)
arr = band.ReadAsArray()
print(f"Roh-Höhen: min={arr.min():.1f}, max={arr.max():.1f}, mean={arr.mean():.1f}")

# NoData behandeln
nodata = band.GetNoDataValue()
if nodata is not None:
    arr = np.where(arr == nodata, 0, arr)  # Meer = 0

# Skalierung: Offset 10000 (für -1000 m bis +5553 m), Faktor 10
OFFSET = 10000
SCALE = 10
arr_uint16 = np.clip((arr * SCALE + OFFSET).astype(np.uint16), 0, 65535)
print(f"UInt16: min={arr_uint16.min()}, max={arr_uint16.max()}")

# 7. Als PNG speichern (16-Bit)
png_path = os.path.join(OUTPUT_DIR, "iceland_heightmap_50m_2049x2049.png")
driver = gdal.GetDriverByName('PNG')
out_ds = driver.Create(png_path, target_size, target_size, 1, gdal.GDT_UInt16)
out_ds.SetGeoTransform(ds.GetGeoTransform())
out_ds.SetProjection(ds.GetProjection())
out_band = out_ds.GetRasterBand(1)
out_band.WriteArray(arr_uint16)
out_band.SetNoDataValue(0)
out_ds.FlushCache()
out_ds = None
print(f"PNG gespeichert: {png_path}")

# 8. Als RAW speichern (Alternative)
raw_path = os.path.join(OUTPUT_DIR, "iceland_heightmap_50m_2049x2049.raw")
arr_uint16.tofile(raw_path)
print(f"RAW gespeichert: {raw_path}")

# 9. Metadaten für HeightmapImporter
meta = {
    "source": "Copernicus DEM GLO-30 Public (44 Tiles, N63-66 W013-025)",
    "target_crs": "EPSG:3057 (ISN93 / Lambert 1993)",
    "resolution_m": 50,
    "size": [target_size, target_size],
    "extent_m": [target_size * 50, target_size * 50],
    "center_3057": [float(x_center), float(y_center)],
    "bounds_3057": [float(ulx), float(lry), float(lrx), float(uly)],
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