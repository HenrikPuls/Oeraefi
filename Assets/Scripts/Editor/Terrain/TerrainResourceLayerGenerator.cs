using System;
using System.IO;
using Oerfi.Terrain;
using UnityEditor;
using UnityEngine;

namespace Oerfi.Editor.Terrain
{
    /// <summary>
    /// Generates a region's initial <see cref="TerrainResourceLayer"/> asset
    /// from the RAW heightmap: decode metric heights → downsample to the
    /// coarse resource grid → per-cell forest density via
    /// <see cref="ForestHeuristic"/> (elevation + slope). Springs stay None
    /// and erosion 0 (no consumer exists yet).
    ///
    /// Asset policy (CLAUDE.md balancing safety): the layer carries
    /// user-tunable balancing data (regeneration rate, densities after
    /// play) — it is CREATED once; an existing asset is never overwritten
    /// (delete the asset to regenerate).
    ///
    /// Deterministic entry point for both the editor menu and batchmode
    /// runs (-executeMethod).
    /// </summary>
    public static class TerrainResourceLayerGenerator
    {
        /// <summary>Resource grid cell size in meters (coarser than the DEM grid).</summary>
        public const float RESOURCE_CELL_SIZE_METERS = 200f;

        public const string SOUTHWEST_LAYER_PATH =
            "Assets/Data/Terrain/SouthwestTerrainResourceLayer.asset";

        private const string SOUTHWEST_RAW_PATH =
            "Assets/StreamingAssets/Terrain/southwest_heightmap_50m_2049x2049.raw";
        private const string SOUTHWEST_METADATA_PATH =
            "Assets/StreamingAssets/Terrain/southwest_metadata.json";

        [MenuItem("Oerfi/Terrain/Generate Southwest Resource Layer")]
        public static void GenerateSouthwest()
        {
            GenerateLayer(
                SOUTHWEST_RAW_PATH, SOUTHWEST_METADATA_PATH, SOUTHWEST_LAYER_PATH);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Core shared by the menu entry, batchmode, and the importer window.
        /// </summary>
        public static void GenerateLayer(
            string rawPath, string metadataPath, string layerAssetPath)
        {
            TerrainResourceLayer existing =
                AssetDatabase.LoadAssetAtPath<TerrainResourceLayer>(layerAssetPath);
            if (existing != null)
            {
                Debug.LogWarning(
                    "[TerrainResourceLayerGenerator] " +
                    $"{layerAssetPath} already exists — left untouched (user " +
                    "balancing); delete the asset to regenerate.");
                return;
            }

            if (!File.Exists(rawPath))
                throw new FileNotFoundException($"Heightmap RAW not found: {rawPath}");
            if (!File.Exists(metadataPath))
                throw new FileNotFoundException($"Heightmap metadata not found: {metadataPath}");

            TerrainMetadata metadata = TerrainMetadata.FromJson(File.ReadAllText(metadataPath));
            byte[] raw = File.ReadAllBytes(rawPath);

            // Coarse metric grid for the heuristic (nearest-sample stride).
            int stride = Math.Max(
                1, Mathf.RoundToInt(RESOURCE_CELL_SIZE_METERS / metadata.resolution_m));
            float[,] coarseHeights = ForestHeuristic.Downsample(
                HeightmapDecoder.DecodeHeightsMeters(
                    raw, metadata.Width, metadata.Height, metadata),
                stride);
            float coarseSpacing = metadata.resolution_m * stride;

            float[,] densities = ForestHeuristic.ComputeDensityGrid(
                coarseHeights, coarseSpacing);

            int height = coarseHeights.GetLength(0);
            int width = coarseHeights.GetLength(1);

            HeightmapImportCore.EnsureFolder(
                Path.GetDirectoryName(layerAssetPath)?.Replace('\\', '/'));
            var layer = ScriptableObject.CreateInstance<TerrainResourceLayer>();
            AssetDatabase.CreateAsset(layer, layerAssetPath);
            layer.Initialize(width, height, coarseSpacing);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    layer.SetForestDensity(x, y, densities[y, x]);
            EditorUtility.SetDirty(layer);

            // Summary: mean density + sea share (both over land-relevant cells).
            double densitySum = 0;
            int seaCells = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    densitySum += densities[y, x];
                    if (coarseHeights[y, x] <= 0f) seaCells++;
                }
            int cells = width * height;
            Debug.Log(
                $"[TerrainResourceLayerGenerator] {metadata.region}: {width}x{height} " +
                $"cells @ {coarseSpacing:F0} m -> {layerAssetPath} (created); " +
                $"mean density {(float)(densitySum / cells):F3}, " +
                $"sea cells {seaCells}/{cells}. Note: the coarse grid spans " +
                $"{(width - 1) * coarseSpacing:F0} of {metadata.ExtentMeters:F0} m " +
                "extent (edge sliver ignored).");
        }
    }
}
