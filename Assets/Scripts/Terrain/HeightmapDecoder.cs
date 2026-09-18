using System;

namespace Oerfi.Terrain
{
    /// <summary>
    /// Decodes the DEM pipeline's canonical heightmap format (ADR-005):
    /// UInt16 little-endian RAW with encoding
    /// <c>raw = clip(height_m × scale_factor + offset)</c> — sea level sits
    /// at <c>offset</c> (10000), raw 0 is the source nodata sentinel (GLO-30
    /// ocean arrives as true 0.0 m and encodes to the offset value).
    /// Pure byte[]-in logic; file I/O belongs to the editor importer/tests.
    ///
    /// Migration note: the predecessor's importer only normalized 0–65535 →
    /// 0..1 and never applied scale/offset — importing its shipped data
    /// produced a wrongly scaled terrain. This decoder is metadata-driven.
    /// </summary>
    public static class HeightmapDecoder
    {
        public const int BYTES_PER_SAMPLE = 2;

        /// <summary>
        /// Decodes RAW samples to metric heights. Row order is flipped
        /// vertically (DEM row 0 = north, Unity heightmap row 0 = south).
        /// Samples at/below the sea level raw value — which covers the
        /// nodata sentinel — decode to 0 m.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When the buffer is smaller than width × height × 2 bytes.
        /// </exception>
        public static float[,] DecodeHeightsMeters(
            byte[] rawSamples, int width, int height, TerrainMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            int expected = width * height * BYTES_PER_SAMPLE;
            if (rawSamples == null || rawSamples.Length < expected)
                throw new ArgumentException(
                    $"RAW buffer too small ({rawSamples?.Length ?? 0} bytes, " +
                    $"expected {expected} for {width}x{height} 16-bit).",
                    nameof(rawSamples));

            var heights = new float[height, width];
            for (int y = 0; y < height; y++)
            {
                int sourceY = height - 1 - y; // DEM row 0 = north → Unity row 0 = south
                for (int x = 0; x < width; x++)
                {
                    int i = (sourceY * width + x) * BYTES_PER_SAMPLE;
                    ushort sample = (ushort)(rawSamples[i] | (rawSamples[i + 1] << 8));
                    heights[y, x] = SampleToMeters(sample, metadata);
                }
            }
            return heights;
        }

        /// <summary>
        /// Same decode, normalized for <c>TerrainData.SetHeights</c>:
        /// meters divided by the metadata's max height, clamped to [0, 1].
        /// </summary>
        public static float[,] DecodeHeightsNormalized(
            byte[] rawSamples, int width, int height, TerrainMetadata metadata)
        {
            float maxHeight = metadata.MaxHeightMeters;
            if (maxHeight <= 0f)
                throw new ArgumentException(
                    "Metadata height range maximum must be positive.", nameof(metadata));

            float[,] meters = DecodeHeightsMeters(rawSamples, width, height, metadata);

            int height2 = meters.GetLength(0);
            int width2 = meters.GetLength(1);
            var normalized = new float[height2, width2];
            for (int y = 0; y < height2; y++)
                for (int x = 0; x < width2; x++)
                    normalized[y, x] = Math.Clamp(meters[y, x] / maxHeight, 0f, 1f);
            return normalized;
        }

        /// <summary>
        /// Snaps a sample-grid side length to a Unity heightmap resolution
        /// (2^n + 1): 2049 stays 2049, 512 becomes 513, 513 stays 513.
        /// </summary>
        public static int SnapToHeightmapResolution(int sampleSize)
        {
            if (sampleSize < 2) return 2;
            return UnityEngine.Mathf.NextPowerOfTwo(sampleSize - 1) + 1;
        }

        /// <summary>
        /// Sanity metrics over a decoded region (min/max height in meters,
        /// fraction of zero-height cells). Used to cross-check the decode
        /// against the metadata (max ~ height range max, zero fraction ~
        /// sea fraction).
        /// </summary>
        public static (float minMeters, float maxMeters, float zeroFraction) DecodeSummary(
            byte[] rawSamples, int width, int height, TerrainMetadata metadata)
        {
            float[,] meters = DecodeHeightsMeters(rawSamples, width, height, metadata);

            float min = float.MaxValue;
            float max = float.MinValue;
            int zeroCells = 0;
            int total = width * height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = meters[y, x];
                    if (value < min) min = value;
                    if (value > max) max = value;
                    if (value <= 0f) zeroCells++;
                }
            }
            return (min, max, (float)zeroCells / total);
        }

        private static float SampleToMeters(ushort sample, TerrainMetadata metadata)
        {
            if (sample <= metadata.sea_level_raw) return 0f; // sea + nodata sentinel
            return (sample - metadata.offset) / (float)metadata.scale_factor;
        }
    }
}
