using System;
using UnityEngine;

namespace Oerfi.Terrain
{
    /// <summary>Geothermal spring classification per cell (concept doc 3.3
    /// context; initialization deferred until a consumer exists).</summary>
    public enum SpringType
    {
        None,
        LowTemperature,
        HighTemperature
    }

    /// <summary>
    /// Per-cell terrain resources as flat row-major arrays (index
    /// <c>y * GridWidth + x</c> — deliberately not Texture2D/alphamaps:
    /// memory- and serialization-friendly, aggregate per cell, no
    /// per-object management; concept doc 3/10). One asset per region.
    ///
    /// The regeneration rate is balancing data (ADR-004, user-tunable in
    /// the Inspector); densities are generated initial state that the user
    /// may tune afterwards — generators therefore never overwrite an
    /// existing layer asset.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainResourceLayer", menuName = "Oerfi/Terrain Resource Layer")]
    public class TerrainResourceLayer : ScriptableObject
    {
        public const float MAX_FOREST_DENSITY = 1f;
        public const float DEFAULT_CELL_SIZE_METERS = 50f;
        public const float DEFAULT_FOREST_REGENERATION_RATE_PER_DAY = 0.0001f;

        [SerializeField] private int _gridWidth;
        [SerializeField] private int _gridHeight;
        [SerializeField] private float _cellSizeMeters = DEFAULT_CELL_SIZE_METERS;
        [Tooltip("Forest regeneration per day (fraction of the maximum).")]
        [SerializeField]
        private float _forestRegenerationRatePerDay =
            DEFAULT_FOREST_REGENERATION_RATE_PER_DAY;
        [SerializeField] private float[] _forestDensity;
        [SerializeField] private SpringType[] _springType;
        [SerializeField] private float[] _erosion;

        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public float CellSizeMeters => _cellSizeMeters;
        public float ForestRegenerationRatePerDay => _forestRegenerationRatePerDay;

        /// <summary>Allocates zeroed grids (forest empty, springs none, erosion none).</summary>
        public void Initialize(int width, int height, float cellSizeMeters)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            _gridWidth = width;
            _gridHeight = height;
            _cellSizeMeters = cellSizeMeters;
            _forestDensity = new float[width * height];
            _springType = new SpringType[width * height];
            _erosion = new float[width * height];
        }

        public bool InBounds(int x, int y) =>
            x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight;

        public float GetForestDensity(int x, int y)
        {
            EnsureReady(x, y);
            return _forestDensity[Index(x, y)];
        }

        public void SetForestDensity(int x, int y, float value)
        {
            EnsureReady(x, y);
            _forestDensity[Index(x, y)] = Mathf.Clamp01(value);
        }

        public SpringType GetSpringType(int x, int y)
        {
            EnsureReady(x, y);
            return _springType[Index(x, y)];
        }

        public void SetSpringType(int x, int y, SpringType type)
        {
            EnsureReady(x, y);
            _springType[Index(x, y)] = type;
        }

        public float GetErosion(int x, int y)
        {
            EnsureReady(x, y);
            return _erosion[Index(x, y)];
        }

        public void SetErosion(int x, int y, float value)
        {
            EnsureReady(x, y);
            _erosion[Index(x, y)] = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Linear additive forest regeneration toward the maximum, never
        /// above it. Called once per simulated day (see
        /// <see cref="ForestRegenerationTick"/>).
        /// </summary>
        public void TickForestRegeneration()
        {
            EnsureInitialized();
            for (int i = 0; i < _forestDensity.Length; i++)
                _forestDensity[i] = Mathf.Min(
                    MAX_FOREST_DENSITY, _forestDensity[i] + _forestRegenerationRatePerDay);
        }

        private int Index(int x, int y) => y * _gridWidth + x;

        private void EnsureInitialized()
        {
            if (_forestDensity == null || _springType == null || _erosion == null)
                throw new InvalidOperationException(
                    "TerrainResourceLayer used before Initialize().");
        }

        private void EnsureReady(int x, int y)
        {
            EnsureInitialized();
            if (!InBounds(x, y))
                throw new ArgumentOutOfRangeException(
                    nameof(x), $"({x}, {y}) outside grid {_gridWidth}x{_gridHeight}.");
        }
    }
}
