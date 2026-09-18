using NUnit.Framework;
using Oerfi.Terrain;
using UnityEditor;
using UnityEngine;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Terrain resource layer tests, ported from the predecessor project's
    /// HeightmapImporterTests (resource-layer parts) plus guards for the
    /// migration fixes (accessors validate instead of the dead InBounds).
    /// </summary>
    public class TerrainResourceLayerTests
    {
        private TerrainResourceLayer _layer;

        [SetUp]
        public void SetUp()
        {
            _layer = ScriptableObject.CreateInstance<TerrainResourceLayer>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_layer != null) Object.DestroyImmediate(_layer);
        }

        [Test]
        public void Initialize_GridDimensions_AndCellSize()
        {
            _layer.Initialize(8, 6, 200f);

            Assert.That(_layer.GridWidth, Is.EqualTo(8));
            Assert.That(_layer.GridHeight, Is.EqualTo(6));
            Assert.That(_layer.CellSizeMeters, Is.EqualTo(200f).Within(0.0001f));
            Assert.That(_layer.GetForestDensity(7, 5), Is.EqualTo(0f), "zeroed grid");
            Assert.That(_layer.GetSpringType(0, 0), Is.EqualTo(SpringType.None));
            Assert.That(_layer.GetErosion(0, 0), Is.EqualTo(0f));
        }

        [Test]
        public void Values_SetGet_RoundTrip_AndClamp()
        {
            _layer.Initialize(4, 4, 50f);

            _layer.SetForestDensity(1, 2, 0.7f);
            Assert.That(_layer.GetForestDensity(1, 2), Is.EqualTo(0.7f).Within(0.0001f));
            _layer.SetForestDensity(1, 2, 2f);
            Assert.That(_layer.GetForestDensity(1, 2), Is.EqualTo(1f), "clamped at max");
            _layer.SetForestDensity(1, 2, -1f);
            Assert.That(_layer.GetForestDensity(1, 2), Is.EqualTo(0f), "clamped at zero");

            _layer.SetSpringType(3, 0, SpringType.HighTemperature);
            Assert.That(_layer.GetSpringType(3, 0), Is.EqualTo(SpringType.HighTemperature));

            _layer.SetErosion(0, 3, 0.3f);
            Assert.That(_layer.GetErosion(0, 3), Is.EqualTo(0.3f).Within(0.0001f));
            _layer.SetErosion(0, 3, -1f);
            Assert.That(_layer.GetErosion(0, 3), Is.EqualTo(0f), "clamped at zero");
        }

        [Test]
        public void ForestRegeneration_RisesToMax_NeverAbove()
        {
            _layer.Initialize(4, 4, 50f);
            // Pin the serialized field name while forcing the test rate.
            var serialized = new SerializedObject(_layer);
            serialized.FindProperty("_forestRegenerationRatePerDay").floatValue =
                TerrainResourceLayer.MAX_FOREST_DENSITY / 500f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            for (int i = 0; i < 4; i++) _layer.SetForestDensity(i, i, 0.5f);

            for (int i = 0; i < 500; i++) _layer.TickForestRegeneration();
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    Assert.That(_layer.GetForestDensity(x, y),
                        Is.EqualTo(1f).Within(0.01f), $"cell ({x}, {y}) fully regrown");

            for (int i = 0; i < 100; i++) _layer.TickForestRegeneration();
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    Assert.That(_layer.GetForestDensity(x, y),
                        Is.LessThanOrEqualTo(TerrainResourceLayer.MAX_FOREST_DENSITY + 0.0001f),
                        "regeneration never exceeds the maximum");
        }

        [Test]
        public void Accessors_OutOfBounds_Throw()
        {
            // Regression: the predecessor's InBounds existed but accessors
            // silently relied on array bounds exceptions.
            _layer.Initialize(4, 4, 50f);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _layer.GetForestDensity(4, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _layer.GetForestDensity(0, 4));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _layer.SetForestDensity(-1, 0, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _layer.GetSpringType(0, -1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _layer.GetErosion(99, 99));
        }

        [Test]
        public void Accessors_BeforeInitialize_Throw()
        {
            // Regression: the predecessor NRE'd from raw accesses before init.
            Assert.Throws<System.InvalidOperationException>(
                () => _layer.GetForestDensity(0, 0));
            Assert.Throws<System.InvalidOperationException>(() => _layer.TickForestRegeneration());
        }
    }
}
