using System;
using UnityEngine;

namespace Oerfi.Terrain
{
    /// <summary>
    /// Metadata sidecar of the DEM heightmap pipeline (ADR-005,
    /// docs/pipeline/terrain-import.md), parsed from
    /// <c>&lt;region&gt;_metadata.json</c> via JsonUtility.
    ///
    /// Naming deviation, intentional: the public fields below are bound 1:1
    /// to the pipeline's JSON keys (snake_case) — renaming them would break
    /// deserialization. Consumers use the PascalCase properties. Unknown
    /// JSON keys are ignored by JsonUtility (forward-compatible).
    /// </summary>
    [Serializable]
    public class TerrainMetadata
    {
        public string region;
        public int[] size;            // [width, height] in samples
        public float resolution_m;    // meters per sample
        public float[] extent_m;      // [x, y] in meters (square regions)
        public float[] bounds_3057;   // xmin/ymin/xmax/ymax, EPSG:3057
        public int scale_factor;      // raw = height_m * scale_factor + offset
        public int offset;            // sea level lands at this raw value
        public int nodata_value;      // raw nodata sentinel (0)
        public int sea_level_raw;     // raw value of sea level
        public float[] height_range_m; // [min, max] in meters
        public float sea_fraction;    // fraction of cells at/below sea level

        public static TerrainMetadata FromJson(string json)
        {
            return JsonUtility.FromJson<TerrainMetadata>(json);
        }

        public int Width => size != null && size.Length > 0 ? size[0] : 0;

        public int Height => size != null && size.Length > 1 ? size[1] : 0;

        /// <summary>Highest occurring height in meters; the normalized-height divisor.</summary>
        public float MaxHeightMeters =>
            height_range_m != null && height_range_m.Length > 1 ? height_range_m[1] : 0f;

        /// <summary>Region extent in meters (square regions: x == y).</summary>
        public float ExtentMeters => extent_m != null && extent_m.Length > 0 ? extent_m[0] : 0f;
    }
}
