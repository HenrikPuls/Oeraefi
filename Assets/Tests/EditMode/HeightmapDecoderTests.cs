using System;
using System.IO;
using NUnit.Framework;
using Oerfi.Terrain;
using UnityEngine;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Heightmap decoder tests: synthetic buffers for the byte-level rules
    /// (little-endian, vertical flip, sea/nodata clamp) plus two tests
    /// against the real shipped southwest data — the metadata schema pin
    /// and a full decode whose metrics must match the metadata. The
    /// predecessor's tests copy-pasted the decoder instead of calling it;
    /// these call the real one.
    /// </summary>
    public class HeightmapDecoderTests
    {
        private const string SOUTHWEST_RAW =
            "southwest_heightmap_50m_2049x2049.raw";
        private const string SOUTHWEST_METADATA_JSON =
            "southwest_metadata.json";
        private const float MAX_HEIGHT_M = 1406.8f;

        private static TerrainMetadata MakeMetadata()
        {
            return new TerrainMetadata
            {
                region = "test",
                size = new[] { 4, 4 },
                resolution_m = 50f,
                extent_m = new[] { 200f, 200f },
                scale_factor = 10,
                offset = 10000,
                nodata_value = 0,
                sea_level_raw = 10000,
                height_range_m = new[] { 0f, MAX_HEIGHT_M },
                sea_fraction = 0.25f
            };
        }

        private static byte[] RawFromSamples(params ushort[] samples)
        {
            var bytes = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                bytes[i * 2] = (byte)(samples[i] & 0xFF);        // low byte first
                bytes[i * 2 + 1] = (byte)(samples[i] >> 8);      // little-endian
            }
            return bytes;
        }

        [Test]
        public void Decode_LittleEndian_ConvertsToMeters()
        {
            // 0x8000 = 32768 -> (32768 - 10000) / 10 = 2276.8 m.
            byte[] raw = RawFromSamples(0x8000);
            float[,] heights = HeightmapDecoder.DecodeHeightsMeters(raw, 1, 1, MakeMetadata());

            Assert.That(heights[0, 0], Is.EqualTo(2276.8f).Within(0.0001f));
        }

        [Test]
        public void Decode_VerticalFlip_NorthRowToSouthRow()
        {
            // DEM row 0 = north (1000 m), DEM row 1 = south (500 m).
            // Unity heightmap row 0 = south: the 500 m row must land first.
            byte[] raw = RawFromSamples(20000, 15000);
            float[,] heights = HeightmapDecoder.DecodeHeightsMeters(raw, 1, 2, MakeMetadata());

            Assert.That(heights[0, 0], Is.EqualTo(500f).Within(0.0001f), "south first");
            Assert.That(heights[1, 0], Is.EqualTo(1000f).Within(0.0001f), "north second");
        }

        [Test]
        public void Decode_SeaAndNodata_ClampToZero()
        {
            // Raw 0 = nodata sentinel, 9999 = just below sea, 10000 = sea
            // level. Source row 0 is north: after the vertical flip source
            // rows land in decoded rows [1] then [0].
            byte[] raw = RawFromSamples(10000, 10001, 0, 9999);
            float[,] heights = HeightmapDecoder.DecodeHeightsMeters(raw, 2, 2, MakeMetadata());

            Assert.That(heights[0, 0], Is.EqualTo(0f), "nodata sentinel (source row 1)");
            Assert.That(heights[0, 1], Is.EqualTo(0f), "below sea level");
            Assert.That(heights[1, 0], Is.EqualTo(0f), "sea level (source row 0)");
            Assert.That(heights[1, 1], Is.EqualTo(0.1f).Within(0.0001f),
                "first meter above sea level");
        }

        [Test]
        public void Decode_Normalized_DividesByMaxHeight_Clamped()
        {
            // 1406.8 m exactly -> 1.0; 5553.5 m (beyond the range max) -> clamped 1.0.
            byte[] raw = RawFromSamples(24068, 65535);
            float[,] normalized = HeightmapDecoder.DecodeHeightsNormalized(
                raw, 2, 1, MakeMetadata());

            Assert.That(normalized[0, 0], Is.EqualTo(1f).Within(0.00001f));
            Assert.That(normalized[0, 1], Is.EqualTo(1f).Within(0.00001f), "clamped at 1");
        }

        [Test]
        public void Decode_ShortBuffer_Throws()
        {
            byte[] raw = RawFromSamples(10000); // 2 bytes, but 2x2 needs 8
            Assert.Throws<ArgumentException>(() =>
                HeightmapDecoder.DecodeHeightsMeters(raw, 2, 2, MakeMetadata()));
            Assert.Throws<ArgumentException>(() =>
                HeightmapDecoder.DecodeHeightsMeters(null, 2, 2, MakeMetadata()));
        }

        [Test]
        public void SnapResolution_Keeps2nPlus1_MapsPowersUp()
        {
            Assert.That(HeightmapDecoder.SnapToHeightmapResolution(2049), Is.EqualTo(2049));
            Assert.That(HeightmapDecoder.SnapToHeightmapResolution(513), Is.EqualTo(513));
            Assert.That(HeightmapDecoder.SnapToHeightmapResolution(512), Is.EqualTo(513));
            Assert.That(HeightmapDecoder.SnapToHeightmapResolution(33), Is.EqualTo(33));
        }

        [Test]
        public void Metadata_FromCanonicalJson_ParsesSchema()
        {
            // Schema pin: the DTO's field names must stay in sync with the
            // pipeline's JSON (JsonUtility binds by field name).
            string path = Path.Combine(Application.streamingAssetsPath, "Terrain",
                SOUTHWEST_METADATA_JSON);
            TerrainMetadata metadata = TerrainMetadata.FromJson(File.ReadAllText(path));

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.region, Is.EqualTo("southwest"));
            Assert.That(metadata.Width, Is.EqualTo(2049));
            Assert.That(metadata.Height, Is.EqualTo(2049));
            Assert.That(metadata.resolution_m, Is.EqualTo(50f));
            Assert.That(metadata.scale_factor, Is.EqualTo(10));
            Assert.That(metadata.offset, Is.EqualTo(10000));
            Assert.That(metadata.sea_level_raw, Is.EqualTo(10000));
            Assert.That(metadata.nodata_value, Is.EqualTo(0));
            Assert.That(metadata.MaxHeightMeters, Is.EqualTo(1406.8f).Within(0.0001f));
            Assert.That(metadata.ExtentMeters, Is.EqualTo(102450f).Within(0.01f));
            Assert.That(metadata.sea_fraction, Is.EqualTo(0.110459f).Within(0.0001f));
        }

        [Test]
        public void Decode_SouthwestRealData_MetricsMatchMetadata()
        {
            string metadataPath = Path.Combine(Application.streamingAssetsPath, "Terrain",
                SOUTHWEST_METADATA_JSON);
            string rawPath = Path.Combine(Application.streamingAssetsPath, "Terrain",
                SOUTHWEST_RAW);
            TerrainMetadata metadata = TerrainMetadata.FromJson(File.ReadAllText(metadataPath));
            byte[] raw = File.ReadAllBytes(rawPath);
            Assert.That(raw.Length,
                Is.EqualTo(metadata.Width * metadata.Height * HeightmapDecoder.BYTES_PER_SAMPLE),
                "shipped RAW size must match the metadata grid exactly");

            (float minMeters, float maxMeters, float zeroFraction) =
                HeightmapDecoder.DecodeSummary(raw, metadata.Width, metadata.Height, metadata);

            Assert.That(maxMeters, Is.EqualTo(MAX_HEIGHT_M).Within(0.2f),
                "highest decoded cell ~ metadata height range max (0.1 m quantization)");
            Assert.That(minMeters, Is.EqualTo(0f), "sea/nodata cells decode to zero");
            Assert.That(zeroFraction, Is.EqualTo(metadata.sea_fraction).Within(0.001f),
                "zero-height fraction ~ metadata sea fraction");
        }
    }
}
