# Terrain-Import-Pipeline: DEM → Unity (manuelle QGIS-Vorbereitung)

Diese Anleitung beschreibt den **manuellen Schritt außerhalb von Unity**:
Vorbereitung der Höhendaten in QGIS und Export als Heightmap, die der
`HeightmapImporter` (Unity Editor-Tool) anschließend einliest.

## Datenquelle

- **ÍslandsDEM v1.0** — Höhenmodell, 10 m Auflösung (stellenweise 2 m)
- Herausgeber: Landmælingar Íslands + PGC, Lizenz **CC-BY-4.0**
- Download/Ansicht: <https://dem.lmi.is/mapview/> bzw.
  <https://www.lmi.is/is/thaettir-um-land/fyrirtaekid/gagnasofn/islandsdem>

Lizenzhinweis: CC-BY-4.0 verlangt Namensnennung — im Spiel-Credits
"Kartendaten: Landmælingar Íslands (ÍslandsDEM v1.0), CC BY 4.0" aufnehmen.

## Schritt 1: Region zuschneiden

1. QGIS öffnen (getestet mit QGIS 3.x LTR), ÍslandsDEM als Raster-Layer laden.
2. Zielregion festlegen. Empfohlene Eingrenzung (dichte Landnahme-Saga-
   Geografie statt ganz Island), z. B. Südwesten:
   - Faxaflói/Reykjavík, Þingvellir, optional bis Hjörleifshöfði/
     Vestmannaeyjar
3. Zuschnitt über *Raster → Extraktion → Raster nach Rechteck zuschneiden*
   (oder Temporärer Ausschnitt-Layer + *Exportieren → Aktuelle Ausdehnung*).

**Größenrichtwert:** Zielauflösung im Spiel ist ein 50-m-Raster
(`TerrainResourceLayer`, Grid-Auflösung konfigurierbar). Eine Region von
50 × 50 km ergibt bei 50 m Auflösung 1000 × 1000 Zellen — eine sinnvolle
Obergrenze für Unity-Terrain (Unity-Terrain erlaubt max. 4097 × 4097
Heightmap-Auflösung, ist aber ab ~2000² pro Tile handhabbar).

## Schritt 2: Reprojizieren/Downsamplen

1. **Reprojektion:** *Raster → Projektionen → Reprojizieren*
   - Quell-KS: ÍslandsDEM liefert ISN93/Lambert 1993 (EPSG:3057) oder
     ähnliches — im Layer nachsehen.
   - Ziel-KS: **EPSG:3057 (ISN93 / Lambert 1993)** beibehalten ist am
     einfachsten (Metrisch, quasi-ebener für Island-Größenordnungen).
     Ein Web-Mercator-Ziel (EPSG:3857) ist **ungeeignet** (Höhen-/Flächen-
     verzerrung).
2. **Downsamplen:** *Raster → Konversion → Übersetzen (Puffer speichern)*
   (GDAL-Translate) mit `-tr 50 50` für 50-m-Auflösung und
   `-r average` als Resampling-Methode.

## Schritt 3: Export als 16-Bit-Graustufen-Heightmap

1. *Raster → Konversion → Übersetzen*:
   - Ausgabetyp: **UInt16**
   - Rescaling: Höhenwerte auf 16-Bit-Bereich mappen. Beispiel für eine
     Region mit min 0 m / max 1500 m:
     `gdal_translate -ot UInt16 -scale 0 1500 0 65535 input.tif heightmap.png`
     (Min/Max vorher über *Eigenschaften → Histogramm/Information* ermitteln
     und **notieren** — die Werte werden in Unity wieder zurücksiskaliert.)
2. Format: **PNG (16-Bit-Graustufen)** oder **RAW**.
   - PNG: einfacher Handling, max. 65535 Stufen.
   - RAW: mit 8-Bit-Header-Byte-Größe gemäß Unity-Konvention aufpassen —
     für Windows-Byteordnung (Little Endian) exportieren.
3. Notierte Metadaten neben die Datei als Textdatei legen, z. B.
   `heightmap_meta.txt`:

   ```
   minHoehe_m: 0
   maxHoehe_m: 1500
   aufloesung_m: 50
   breite_px: 1000
   hoehe_px: 1000
   CRS: EPSG:3057
   ```

## Schritt 4: Import in Unity

1. Heightmap (PNG/RAW) + Metadatendatei in `game/Assets/_Project/Terrain/`
   (oder beliebig unter `Assets/`) ablegen.
2. Unity Editor: Menü **Terrain → Heightmap Importer** öffnen
   (`Project.Terrain.Editor.HeightmapImporter`).
3. Parameter setzen:
   - Heightmap-Datei
   - Terrain-Größe in Metern (X/Z, z. B. 50000 × 50000 für 50 km) und
     Höhenskala (Y, aus `maxHoehe_m`)
   - Auflösung
4. **Import** klicken → erzeugt/überschreibt ein Unity-`Terrain`-Objekt
   mit den Höhendaten.

## Testdaten (ohne echtes DEM)

Für automatische Tests wird **kein** echtes Island-DEM benötigt (Lizenz-
Beschaffung bleibt manueller Nutzer-Schritt). Der Test generiert eine
prozedurale 512 × 512-Heightmap und prüft die Terrain-Dimensionen —
siehe `Project.Tests.EditMode.HeightmapImporterTests`.

## Zusammenfassung der Pipeline

```
ÍslandsDEM (GeoTIFF, EPSG:3057)
  → QGIS: Region zuschneiden
  → QGIS: -tr 50 50 -r average (downsamplen)
  → gdal_translate: -ot UInt16 -scale <min> <max> 0 65535
  → heightmap.png (16-Bit Graustufen) + heightmap_meta.txt
  → Unity: HeightmapImporter (Editor-Fenster) → Terrain-Objekt
```
