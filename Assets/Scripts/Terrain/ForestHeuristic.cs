using System;

namespace Oerfi.Terrain
{
    /// <summary>
    /// Derives initial forest density per cell from elevation and slope
    /// (settlement-period Iceland: dense lowland birch woods, tree line
    /// ~400–450 m, barren highlands; concept doc 3 — "signifikanter
    /// Birkenwald-/Buschbestand, Niederwald"). Runs in the editor
    /// generator when a region's resource layer is first created.
    ///
    /// The thresholds below are balancing-adjacent parameters kept as code
    /// constants for now (two-tier-params precedent; SO-ification is M13).
    /// </summary>
    public static class ForestHeuristic
    {
        /// <summary>Full base density at/below this elevation (m).</summary>
        public const float FULL_DENSITY_ELEVATION_M = 200f;
        /// <summary>Zero forest at/above this elevation (m); linear fade between the two.</summary>
        public const float TREE_LINE_ELEVATION_M = 450f;
        /// <summary>No forest on slopes at/above this steepness (degrees).</summary>
        public const float MAX_FOREST_SLOPE_DEGREES = 30f;
        /// <summary>Target density of flat lowland forest (fraction of the maximum).</summary>
        public const float BASE_FOREST_DENSITY = 0.8f;

        /// <summary>
        /// density = elevationFactor × slopeFactor, both in [0, 1]. Sea and
        /// nodata cells (elevation ≤ 0) are always 0.
        /// </summary>
        public static float ComputeDensity(float elevationMeters, float slopeDegrees)
        {
            if (elevationMeters <= 0f) return 0f; // sea / nodata

            float elevationFactor;
            if (elevationMeters <= FULL_DENSITY_ELEVATION_M)
                elevationFactor = 1f;
            else if (elevationMeters >= TREE_LINE_ELEVATION_M)
                elevationFactor = 0f;
            else
                elevationFactor = 1f
                    - (elevationMeters - FULL_DENSITY_ELEVATION_M)
                        / (TREE_LINE_ELEVATION_M - FULL_DENSITY_ELEVATION_M);

            float slopeFactor = UnityEngine.Mathf.Clamp01(
                1f - slopeDegrees / MAX_FOREST_SLOPE_DEGREES);

            return BASE_FOREST_DENSITY * elevationFactor * slopeFactor;
        }

        /// <summary>
        /// Per-cell density for a metric height grid. Slope per cell from
        /// central differences of the neighbours (one-sided at edges),
        /// spacing = <paramref name="cellSizeMeters"/>.
        /// </summary>
        public static float[,] ComputeDensityGrid(float[,] heightsMeters, float cellSizeMeters)
        {
            if (heightsMeters == null) throw new ArgumentNullException(nameof(heightsMeters));
            if (cellSizeMeters <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cellSizeMeters));

            int height = heightsMeters.GetLength(0);
            int width = heightsMeters.GetLength(1);
            var densities = new float[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float slope = SlopeDegreesAt(heightsMeters, x, y, cellSizeMeters);
                    densities[y, x] = ComputeDensity(heightsMeters[y, x], slope);
                }
            }
            return densities;
        }

        /// <summary>
        /// Nearest-sample decimation: target[i, j] = source[i × stride,
        /// j × stride] (stride 1 = identity). 2049 with stride 4 → 513.
        /// </summary>
        public static float[,] Downsample(float[,] sourceHeights, int stride)
        {
            if (sourceHeights == null) throw new ArgumentNullException(nameof(sourceHeights));
            if (stride < 1) throw new ArgumentOutOfRangeException(nameof(stride));

            int sourceHeight = sourceHeights.GetLength(0);
            int sourceWidth = sourceHeights.GetLength(1);
            int targetHeight = (sourceHeight - 1) / stride + 1;
            int targetWidth = (sourceWidth - 1) / stride + 1;

            var target = new float[targetHeight, targetWidth];
            for (int y = 0; y < targetHeight; y++)
                for (int x = 0; x < targetWidth; x++)
                    target[y, x] = sourceHeights[y * stride, x * stride];
            return target;
        }

        /// <summary>Slope in degrees via gradient magnitude (rise over run).</summary>
        private static float SlopeDegreesAt(float[,] heights, int x, int y, float cellSizeMeters)
        {
            int height = heights.GetLength(0);
            int width = heights.GetLength(1);

            // Central difference spans two cells; one-sided edge difference one.
            float dx;
            float divisorX;
            if (x > 0 && x < width - 1)
            {
                dx = heights[y, x + 1] - heights[y, x - 1];
                divisorX = 2f;
            }
            else
            {
                dx = x == 0 ? heights[y, 1] - heights[y, 0] : heights[y, width - 1] - heights[y, width - 2];
                divisorX = 1f;
            }

            float dy;
            float divisorY;
            if (y > 0 && y < height - 1)
            {
                dy = heights[y + 1, x] - heights[y - 1, x];
                divisorY = 2f;
            }
            else
            {
                dy = y == 0 ? heights[1, x] - heights[0, x] : heights[height - 1, x] - heights[height - 2, x];
                divisorY = 1f;
            }

            float gradientX = dx / (divisorX * cellSizeMeters);
            float gradientY = dy / (divisorY * cellSizeMeters);
            float gradient = UnityEngine.Mathf.Sqrt(gradientX * gradientX + gradientY * gradientY);
            return UnityEngine.Mathf.Atan(gradient) * UnityEngine.Mathf.Rad2Deg;
        }
    }
}
