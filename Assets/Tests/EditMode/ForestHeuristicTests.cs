using NUnit.Framework;
using Oerfi.Terrain;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Forest initial-density heuristic tests (user decision: heightmap
    /// heuristic). All constants are pinned so generator output stays
    /// explainable.
    /// </summary>
    public class ForestHeuristicTests
    {
        [Test]
        public void SeaCell_ZeroDensity()
        {
            Assert.That(ForestHeuristic.ComputeDensity(0f, 0f), Is.EqualTo(0f));
            Assert.That(ForestHeuristic.ComputeDensity(-1000f, 0f), Is.EqualTo(0f),
                "nodata/sea depths stay zero");
        }

        [Test]
        public void LowlandFlat_FullBaseDensity()
        {
            Assert.That(ForestHeuristic.ComputeDensity(100f, 0f),
                Is.EqualTo(ForestHeuristic.BASE_FOREST_DENSITY).Within(0.0001f));
        }

        [Test]
        public void AboveTreeLine_Zero()
        {
            Assert.That(ForestHeuristic.ComputeDensity(
                ForestHeuristic.TREE_LINE_ELEVATION_M, 0f), Is.EqualTo(0f));
            Assert.That(ForestHeuristic.ComputeDensity(500f, 0f), Is.EqualTo(0f),
                "highland barren");
        }

        [Test]
        public void SteepSlope_ReducesToZeroAtLimit()
        {
            Assert.That(ForestHeuristic.ComputeDensity(100f,
                ForestHeuristic.MAX_FOREST_SLOPE_DEGREES), Is.EqualTo(0f),
                "no forest at the slope limit");
            Assert.That(ForestHeuristic.ComputeDensity(100f, 15f),
                Is.EqualTo(0.4f).Within(0.0001f), "half the limit = half the slope factor");
        }

        [Test]
        public void ElevationFade_LinearBetweenFullAndTreeLine()
        {
            // 325 m is the midpoint of 200..450: factor 0.5 × base 0.8 = 0.4.
            float midpoint = (ForestHeuristic.FULL_DENSITY_ELEVATION_M
                + ForestHeuristic.TREE_LINE_ELEVATION_M) / 2f;
            Assert.That(ForestHeuristic.ComputeDensity(midpoint, 0f),
                Is.EqualTo(0.4f).Within(0.0001f));
        }

        [Test]
        public void Downsample_StridePicksEveryNth()
        {
            var source = new float[4, 4];
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    source[y, x] = y * 10 + x;

            float[,] stride1 = ForestHeuristic.Downsample(source, 1);
            Assert.That(stride1.GetLength(0), Is.EqualTo(4));
            Assert.That(stride1.GetLength(1), Is.EqualTo(4));
            Assert.That(stride1[2, 3], Is.EqualTo(source[2, 3]), "stride 1 = identity");

            float[,] stride2 = ForestHeuristic.Downsample(source, 2);
            Assert.That(stride2.GetLength(0), Is.EqualTo(2));
            Assert.That(stride2.GetLength(1), Is.EqualTo(2));
            Assert.That(stride2[0, 0], Is.EqualTo(source[0, 0]));
            Assert.That(stride2[0, 1], Is.EqualTo(source[0, 2]));
            Assert.That(stride2[1, 0], Is.EqualTo(source[2, 0]));
            Assert.That(stride2[1, 1], Is.EqualTo(source[2, 2]));

            // 2049-style sizes: (2049 - 1) / 4 + 1 = 513.
            Assert.That((2049 - 1) / 4 + 1, Is.EqualTo(513));
        }

        [Test]
        public void ComputeDensityGrid_SyntheticHeightmap()
        {
            // 7x5 grid at 200 m spacing (the generator's real spacing):
            // sea plain, near-flat coastal lowland, then a cliff rising
            // above the tree line.
            var heights = new float[7, 5];
            for (int x = 0; x < 5; x++)
            {
                heights[0, x] = 0f;    // rows 0-2: sea
                heights[1, x] = 0f;
                heights[2, x] = 0f;
                heights[3, x] = 30f;   // rows 3-5: coastal lowland, nearly flat
                heights[4, x] = 30f;
                heights[5, x] = 30f;
                heights[6, x] = 500f;  // row 6: cliff top above the tree line
            }

            float[,] densities = ForestHeuristic.ComputeDensityGrid(heights, 200f);

            Assert.That(densities[0, 2], Is.EqualTo(0f), "sea cell");
            Assert.That(densities[3, 2], Is.GreaterThan(0.5f),
                "coastal lowland densely forested");
            Assert.That(densities[4, 2], Is.GreaterThan(0.5f));
            Assert.That(densities[5, 2], Is.LessThan(densities[4, 2]),
                "cliff edge is steep - reduced density");
            Assert.That(densities[6, 2], Is.EqualTo(0f), "cliff top above the tree line");
        }
    }
}
