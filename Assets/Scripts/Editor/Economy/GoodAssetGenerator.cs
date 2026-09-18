using Oerfi.Economy;
using UnityEditor;
using UnityEngine;

namespace Oerfi.Editor.Economy
{
    /// <summary>
    /// Generates the default good catalog and pricing config from the
    /// concept-doc tables (14.6). Deterministic entry point for both the
    /// editor menu and batchmode runs (-executeMethod).
    ///
    /// Balancing safety (CLAUDE.md): assets are CREATED with table values;
    /// existing assets are only verified — value drift is logged as a
    /// warning, never overwritten (balancing changes belong to the user).
    /// </summary>
    public static class GoodAssetGenerator
    {
        private const string DATA_ROOT = "Assets/Data";
        private const string GOODS_FOLDER = DATA_ROOT + "/Goods";
        private const string ECONOMY_FOLDER = DATA_ROOT + "/Economy";
        private const string PRICING_PATH = ECONOMY_FOLDER + "/PricingConfig.asset";

        private static readonly (string id, string displayName, GoodCategory category, float kugildiValue, bool perishable, float spoilageRatePerDay)[] DEFAULT_GOODS =
        {
            ("milchkuh", "Milchkuh", GoodCategory.Food, 1f, false, 0f),
            ("schaf-traechtig", "trächtiges Schaf", GoodCategory.Food, 1f / 6f, false, 0f),
            ("vadmal-elle", "Vaðmál (Elle)", GoodCategory.CraftGood, 1f / 36f, false, 0f),
            ("loeffel", "Löffel", GoodCategory.CraftGood, 0.01f, false, 0f),
            ("eimer", "Eimer (Dauben)", GoodCategory.CraftGood, 0.05f, false, 0f),
            ("axt", "Axt/Werkzeug (Eisen)", GoodCategory.CraftGood, 0.4f, false, 0f),
            ("bauholz-import", "Import-Bauholz (Fuhre)", GoodCategory.ImportGood, 0.75f, false, 0f),
            ("hof-grundausstattung", "Hof-Grundausstattung", GoodCategory.CraftGood, 2.5f, false, 0f),
        };

        [MenuItem("Oerfi/Economy/Generate Default Goods")]
        public static void GenerateDefaults()
        {
            EnsureFolder("Assets");
            EnsureFolder(DATA_ROOT);
            EnsureFolder(GOODS_FOLDER);
            EnsureFolder(ECONOMY_FOLDER);

            int created = 0;
            int verified = 0;
            int drifted = 0;
            foreach (var definition in DEFAULT_GOODS)
            {
                string path = $"{GOODS_FOLDER}/{definition.id}.asset";
                Good good = AssetDatabase.LoadAssetAtPath<Good>(path);
                if (good == null)
                {
                    good = ScriptableObject.CreateInstance<Good>();
                    AssetDatabase.CreateAsset(good, path);
                    WriteValues(good, definition.id, definition.displayName, definition.category,
                        definition.kugildiValue, definition.perishable, definition.spoilageRatePerDay);
                    EditorUtility.SetDirty(good);
                    created++;
                    continue;
                }

                if (MatchesTable(good, definition.id, definition.displayName, definition.category,
                        definition.kugildiValue, definition.perishable, definition.spoilageRatePerDay))
                {
                    verified++;
                }
                else
                {
                    // Do NOT overwrite: existing values may be user balancing edits.
                    drifted++;
                    Debug.LogWarning(
                        $"[GoodAssetGenerator] '{definition.id}' drifted from the concept table " +
                        $"(current: kugildi={good.KugildiValue}, category={good.Category}, " +
                        $"perishable={good.Perishable}) — left untouched.");
                }
            }

            bool pricingCreated = EnsurePricingConfig();

            AssetDatabase.SaveAssets();
            Debug.Log(
                $"[GoodAssetGenerator] goods: {created} created, {verified} verified, {drifted} drifted " +
                $"(left untouched); pricing config: {(pricingCreated ? "created" : "present")}.");
        }

        private static bool EnsurePricingConfig()
        {
            PricingConfig config = AssetDatabase.LoadAssetAtPath<PricingConfig>(PRICING_PATH);
            if (config != null) return false;

            config = ScriptableObject.CreateInstance<PricingConfig>();
            AssetDatabase.CreateAsset(config, PRICING_PATH);
            EditorUtility.SetDirty(config);
            return true;
        }

        private static void WriteValues(
            Good good,
            string id,
            string displayName,
            GoodCategory category,
            float kugildiValue,
            bool perishable,
            float spoilageRatePerDay)
        {
            SerializedObject serialized = new SerializedObject(good);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_category").enumValueIndex = (int)category;
            serialized.FindProperty("_kugildiValue").floatValue = kugildiValue;
            serialized.FindProperty("_perishable").boolValue = perishable;
            serialized.FindProperty("_spoilageRatePerDay").floatValue = spoilageRatePerDay;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool MatchesTable(
            Good good,
            string id,
            string displayName,
            GoodCategory category,
            float kugildiValue,
            bool perishable,
            float spoilageRatePerDay)
        {
            const float EPSILON = 0.0001f;
            return good.Id == id
                && good.DisplayName == displayName
                && good.Category == category
                && Mathf.Abs(good.KugildiValue - kugildiValue) < EPSILON
                && good.Perishable == perishable
                && Mathf.Abs(good.SpoilageRatePerDay - spoilageRatePerDay) < EPSILON;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
