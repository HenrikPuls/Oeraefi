using NUnit.Framework;
using UnityEngine;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// The importer core's TerrainData setup at a small scale: heights
    /// round-trip through Unity's heightmap storage (which quantizes to
    /// UInt16). Never touches the real 2049 grid and never creates asset
    /// files.
    /// </summary>
    public class TerrainDataTests
    {
        private TerrainData _terrainData;

        [TearDown]
        public void TearDown()
        {
            if (_terrainData != null) Object.DestroyImmediate(_terrainData);
        }

        [Test]
        public void TerrainData_SmallResolution_SetHeightsReadback()
        {
            const int resolution = 65; // 2^6 + 1
            var heights = new float[resolution, resolution];
            for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                    heights[y, x] = 0.25f + 0.5f * (x / (float)(resolution - 1));

            _terrainData = new TerrainData();
            _terrainData.heightmapResolution = resolution;
            _terrainData.size = new Vector3(3200f, 200f, 3200f);
            _terrainData.SetHeights(0, 0, heights);

            Assert.That(_terrainData.heightmapResolution, Is.EqualTo(resolution));
            Assert.That(_terrainData.size.x, Is.EqualTo(3200f).Within(0.01f));
            Assert.That(_terrainData.size.y, Is.EqualTo(200f).Within(0.01f));

            float[,] readback = _terrainData.GetHeights(0, 0, resolution, resolution);
            // Unity stores heights as UInt16: 1/65535 is the quantization step.
            for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                    Assert.That(readback[y, x],
                        Is.EqualTo(heights[y, x]).Within(1f / 65535f),
                        $"height at ({x}, {y}) round-trips");
        }
    }
}
