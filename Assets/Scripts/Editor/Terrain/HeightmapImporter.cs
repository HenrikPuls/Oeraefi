using System;
using System.IO;
using Oerfi.Terrain;
using UnityEditor;
using UnityEngine;

namespace Oerfi.Editor.Terrain
{
    /// <summary>
    /// Shared import core: reads the canonical RAW heightmap + metadata JSON
    /// (ADR-005), decodes metric heights via
    /// <see cref="HeightmapDecoder"/>, and writes the region's TerrainData
    /// asset. Used by the editor window and the batchmode entries.
    ///
    /// Asset policy: the TerrainData asset is DERIVED data (fully
    /// reproducible from the RAW + metadata) — a re-import updates the
    /// existing asset in place (GUID-stable, references survive), never
    /// delete+create.
    /// </summary>
    internal static class HeightmapImportCore
    {
        /// <summary>
        /// Imports <paramref name="rawPath"/> + <paramref name="metadataPath"/>
        /// into the TerrainData asset at <paramref name="terrainDataAssetPath"/>.
        /// Throws on any failure (so batchmode exits nonzero) and logs a
        /// summary of the decoded data on success.
        /// </summary>
        public static UnityEngine.TerrainData Import(
            string rawPath, string metadataPath, string terrainDataAssetPath)
        {
            if (!File.Exists(rawPath))
                throw new FileNotFoundException($"Heightmap RAW not found: {rawPath}");
            if (!File.Exists(metadataPath))
                throw new FileNotFoundException($"Heightmap metadata not found: {metadataPath}");

            TerrainMetadata metadata = TerrainMetadata.FromJson(File.ReadAllText(metadataPath));
            if (metadata == null || metadata.Width == 0 || metadata.Height == 0)
                throw new InvalidDataException(
                    $"Metadata does not describe a grid: {metadataPath}");

            byte[] raw = File.ReadAllBytes(rawPath);
            int width = metadata.Width;
            int height = metadata.Height;
            int expected = width * height * HeightmapDecoder.BYTES_PER_SAMPLE;
            if (raw.Length != expected)
                throw new InvalidDataException(
                    $"RAW size mismatch: {raw.Length} bytes, metadata says " +
                    $"{width}x{height} = {expected}.");

            float[,] normalized = HeightmapDecoder.DecodeHeightsNormalized(
                raw, width, height, metadata);

            int resolution = HeightmapDecoder.SnapToHeightmapResolution(width);
            var terrainData = AssetDatabase.LoadAssetAtPath<UnityEngine.TerrainData>(
                terrainDataAssetPath);
            bool existed = terrainData != null;
            if (terrainData == null)
            {
                EnsureFolder(Path.GetDirectoryName(terrainDataAssetPath)?.Replace('\\', '/'));
                terrainData = new UnityEngine.TerrainData();
                AssetDatabase.CreateAsset(terrainData, terrainDataAssetPath);
            }

            // Unity heightmap resolutions are 2^n + 1; the pipeline's 2^n+1
            // grids pass through unchanged.
            terrainData.heightmapResolution = resolution;
            terrainData.size = new Vector3(
                metadata.ExtentMeters, metadata.MaxHeightMeters, metadata.ExtentMeters);
            terrainData.SetHeights(0, 0, normalized);
            EditorUtility.SetDirty(terrainData);

            (float minMeters, float maxMeters, float zeroFraction) =
                HeightmapDecoder.DecodeSummary(raw, width, height, metadata);
            Debug.Log(
                $"[HeightmapImporter] {metadata.region}: {width}x{height} -> " +
                $"{terrainDataAssetPath} ({(existed ? "updated" : "created")}), " +
                $"terrain {metadata.ExtentMeters:F0}x{metadata.ExtentMeters:F0} m, " +
                $"height 0..{metadata.MaxHeightMeters:F1} m, decoded range " +
                $"{minMeters:F1}..{maxMeters:F1} m, zero fraction {zeroFraction:F4} " +
                $"(metadata sea fraction {metadata.sea_fraction:F4}).");
            return terrainData;
        }

        internal static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }

    /// <summary>
    /// Manual importer window (editor-side convenience): pick a RAW + its
    /// metadata, import the TerrainData asset, generate the region's
    /// resource layer, and create a temporary scene preview for eyeballing.
    /// </summary>
    public class HeightmapImporter : EditorWindow
    {
        private DefaultAsset _rawFile;
        private string _metadataPath = "";
        private string _terrainDataPath = "Assets/Terrain/SouthwestTerrainData.asset";

        [MenuItem("Oerfi/Terrain/Heightmap Importer")]
        public static void ShowWindow() =>
            GetWindow<HeightmapImporter>("Heightmap Importer");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Heightmap (RAW UInt16-LE)", EditorStyles.boldLabel);
            _rawFile = (DefaultAsset)EditorGUILayout.ObjectField(
                "RAW file", _rawFile, typeof(DefaultAsset), false);
            _metadataPath = EditorGUILayout.TextField("Metadata JSON", _metadataPath);
            _terrainDataPath = EditorGUILayout.TextField(
                "TerrainData asset", _terrainDataPath);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Import TerrainData Asset")) ImportFromWindow();
                if (GUILayout.Button("Generate Southwest Resource Layer"))
                    GenerateResourceLayerFromWindow();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Create Scene Preview"))
                CreateScenePreview();
        }

        private string RawPath() =>
            _rawFile != null ? AssetDatabase.GetAssetPath(_rawFile) : "";

        private void ImportFromWindow()
        {
            try
            {
                HeightmapImportCore.Import(RawPath(), _metadataPath, _terrainDataPath);
                AssetDatabase.SaveAssets();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[HeightmapImporter] {exception.Message}");
            }
        }

        private void GenerateResourceLayerFromWindow()
        {
            try
            {
                TerrainResourceLayerGenerator.GenerateLayer(
                    RawPath(), _metadataPath, TerrainResourceLayerGenerator.SOUTHWEST_LAYER_PATH);
                AssetDatabase.SaveAssets();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[HeightmapImporter] {exception.Message}");
            }
        }

        private void CreateScenePreview()
        {
            var terrainData = AssetDatabase.LoadAssetAtPath<UnityEngine.TerrainData>(
                _terrainDataPath);
            if (terrainData == null)
            {
                Debug.LogError(
                    $"[HeightmapImporter] no TerrainData asset at {_terrainDataPath} " +
                    "(import first).");
                return;
            }

            // Temporary preview for manual inspection only — never committed
            // (real scene generation is M5, texturing is M8).
            GameObject preview = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
            preview.name = "TerrainPreview";
            Undo.RegisterCreatedObjectUndo(preview, "Terrain Preview");

            // Frame the scene view on the preview — a 100-km terrain placed
            // next to the template camera is invisible unless framed.
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Vector3 size = terrainData.size;
                var bounds = new Bounds(preview.transform.position + size * 0.5f, size);
                sceneView.pivot = bounds.center;
                sceneView.size = bounds.size.magnitude * 0.6f;
                sceneView.rotation = Quaternion.Euler(40f, 30f, 0f);
                sceneView.Repaint();
            }
            Selection.activeGameObject = preview;

            Debug.Log(
                "[HeightmapImporter] scene preview created (not saved) — scene view " +
                "framed on the terrain. Note: untextured by design until M8.");
        }
    }

    /// <summary>
    /// Deterministic batchmode entries for the shipped southwest region.
    /// Run via: Unity.exe -batchmode -projectPath . -executeMethod
    /// Oerfi.Editor.Terrain.HeightmapImporter.ImportSouthwest -quit -logFile …
    /// (-quit IS correct here, unlike -runTests).
    /// </summary>
    public static class HeightmapBatchmode
    {
        public const string SOUTHWEST_RAW_PATH =
            "Assets/StreamingAssets/Terrain/southwest_heightmap_50m_2049x2049.raw";
        public const string SOUTHWEST_METADATA_PATH =
            "Assets/StreamingAssets/Terrain/southwest_metadata.json";
        public const string SOUTHWEST_TERRAIN_DATA_PATH =
            "Assets/Terrain/SouthwestTerrainData.asset";

        [MenuItem("Oerfi/Terrain/Import Southwest Heightmap")]
        public static void ImportSouthwest()
        {
            HeightmapImportCore.Import(
                SOUTHWEST_RAW_PATH, SOUTHWEST_METADATA_PATH, SOUTHWEST_TERRAIN_DATA_PATH);
            AssetDatabase.SaveAssets();
        }
    }
}
