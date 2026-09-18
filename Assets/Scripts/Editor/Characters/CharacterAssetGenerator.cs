using Oerfi.Characters;
using UnityEditor;
using UnityEngine;

namespace Oerfi.Editor.Characters
{
    /// <summary>
    /// Generates the default origin profiles and the skill growth config
    /// from the concept-doc tables (11.6). Deterministic entry point for
    /// both the editor menu and batchmode runs (-executeMethod).
    ///
    /// Balancing safety (CLAUDE.md): assets are CREATED with table values;
    /// existing assets are only verified — value drift is logged as a
    /// warning, never overwritten (balancing changes belong to the user).
    /// </summary>
    public static class CharacterAssetGenerator
    {
        private const string DATA_ROOT = "Assets/Data";
        private const string CHARACTERS_FOLDER = DATA_ROOT + "/Characters";
        private const string ORIGINS_FOLDER = CHARACTERS_FOLDER + "/Origins";
        private const string SKILL_GROWTH_PATH = CHARACTERS_FOLDER + "/SkillGrowthConfig.asset";

        // Concept doc 11.6. Attribute order: Strength, Dexterity, Toughness,
        // Perception, Wits, Willpower. Skill order: Farming, AnimalHusbandry,
        // Smithing, Weaving, Woodworking, Stonemasonry, Leatherworking,
        // Coopering, Fishing, Fowling, SealHunting, CoastalSailing,
        // Helmsmanship, Trading, Combat, ThingOratory.
        private static readonly (Origin origin, string displayName, int[] attributeDeltas, int[] skillStarts)[]
            DEFAULT_ORIGINS =
            {
                (Origin.NorwegianFarmer, "Norwegischer Bauer (Norðmaðr)",
                    new[] { 1, 0, 1, 0, 0, 0 },
                    new[] { 4, 4, 1, 2, 3, 1, 2, 2, 2, 1, 1, 3, 1, 3, 1, 2 }),
                (Origin.HebrideanGael, "Hebridisch-gälischer Siedler (Vestmaðr)",
                    new[] { 0, 1, 0, 1, 0, 0 },
                    new[] { 2, 2, 1, 3, 1, 4, 1, 1, 3, 3, 2, 2, 1, 1, 2, 1 }),
                (Origin.FaroeseShetland, "Färöisch/Shetland-Herkunft",
                    new[] { 0, 0, 1, 1, 0, 0 },
                    new[] { 1, 2, 0, 1, 1, 2, 2, 1, 4, 4, 3, 4, 2, 1, 1, 1 }),
                (Origin.Freedman, "Freigelassener/einfacher Siedler",
                    new[] { 0, 0, 0, 0, 0, 1 },
                    new[] { 2, 2, 1, 2, 2, 2, 2, 2, 2, 1, 1, 1, 0, 1, 1, 0 }),
            };

        [MenuItem("Oerfi/Characters/Generate Default Origins")]
        public static void GenerateDefaults()
        {
            EnsureFolder("Assets");
            EnsureFolder(DATA_ROOT);
            EnsureFolder(CHARACTERS_FOLDER);
            EnsureFolder(ORIGINS_FOLDER);

            int created = 0;
            int verified = 0;
            int drifted = 0;
            foreach (var definition in DEFAULT_ORIGINS)
            {
                string path = $"{ORIGINS_FOLDER}/{definition.origin}.asset";
                OriginProfile profile = AssetDatabase.LoadAssetAtPath<OriginProfile>(path);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<OriginProfile>();
                    AssetDatabase.CreateAsset(profile, path);
                    WriteValues(profile, definition.origin, definition.displayName,
                        definition.attributeDeltas, definition.skillStarts);
                    EditorUtility.SetDirty(profile);
                    created++;
                    continue;
                }

                if (MatchesTable(profile, definition.origin, definition.displayName,
                        definition.attributeDeltas, definition.skillStarts))
                {
                    verified++;
                }
                else
                {
                    // Do NOT overwrite: existing values may be user balancing edits.
                    drifted++;
                    Debug.LogWarning(
                        $"[CharacterAssetGenerator] origin '{definition.origin}' drifted from the " +
                        $"concept table — left untouched.");
                }
            }

            bool growthCreated = EnsureSkillGrowthConfig();

            AssetDatabase.SaveAssets();
            Debug.Log(
                $"[CharacterAssetGenerator] origins: {created} created, {verified} verified, " +
                $"{drifted} drifted (left untouched); skill growth config: " +
                $"{(growthCreated ? "created" : "present")}.");
        }

        private static bool EnsureSkillGrowthConfig()
        {
            SkillGrowthConfig config = AssetDatabase.LoadAssetAtPath<SkillGrowthConfig>(SKILL_GROWTH_PATH);
            if (config != null) return false;

            config = ScriptableObject.CreateInstance<SkillGrowthConfig>();
            AssetDatabase.CreateAsset(config, SKILL_GROWTH_PATH);
            EditorUtility.SetDirty(config);
            return true;
        }

        private static void WriteValues(
            OriginProfile profile,
            Origin origin,
            string displayName,
            int[] attributeDeltas,
            int[] skillStarts)
        {
            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("_origin").enumValueIndex = (int)origin;
            serialized.FindProperty("_displayName").stringValue = displayName;
            ApplyAttributeDeltas(serialized, attributeDeltas);
            ApplySkillStarts(serialized, skillStarts);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyAttributeDeltas(SerializedObject serialized, int[] deltas)
        {
            var fieldNames = new[]
            {
                "_strengthDelta", "_dexterityDelta", "_toughnessDelta",
                "_perceptionDelta", "_witsDelta", "_willpowerDelta"
            };
            for (int i = 0; i < fieldNames.Length; i++)
                serialized.FindProperty(fieldNames[i]).intValue = deltas[i];
        }

        private static void ApplySkillStarts(SerializedObject serialized, int[] starts)
        {
            var fieldNames = new[]
            {
                "_farmingStart", "_animalHusbandryStart", "_smithingStart", "_weavingStart",
                "_woodworkingStart", "_stonemasonryStart", "_leatherworkingStart", "_cooperingStart",
                "_fishingStart", "_fowlingStart", "_sealHuntingStart", "_coastalSailingStart",
                "_helmsmanshipStart", "_tradingStart", "_combatStart", "_thingOratoryStart"
            };
            for (int i = 0; i < fieldNames.Length; i++)
                serialized.FindProperty(fieldNames[i]).intValue = starts[i];
        }

        private static bool MatchesTable(
            OriginProfile profile,
            Origin origin,
            string displayName,
            int[] attributeDeltas,
            int[] skillStarts)
        {
            if (profile.Origin != origin) return false;
            if (profile.DisplayName != displayName) return false;

            var attributeIds = new[]
            {
                AttributeId.Strength, AttributeId.Dexterity, AttributeId.Toughness,
                AttributeId.Perception, AttributeId.Wits, AttributeId.Willpower
            };
            for (int i = 0; i < attributeIds.Length; i++)
                if (profile.GetAttributeDelta(attributeIds[i]) != attributeDeltas[i]) return false;

            var skillIds = new[]
            {
                SkillId.Farming, SkillId.AnimalHusbandry, SkillId.Smithing, SkillId.Weaving,
                SkillId.Woodworking, SkillId.Stonemasonry, SkillId.Leatherworking, SkillId.Coopering,
                SkillId.Fishing, SkillId.Fowling, SkillId.SealHunting, SkillId.CoastalSailing,
                SkillId.Helmsmanship, SkillId.Trading, SkillId.Combat, SkillId.ThingOratory
            };
            for (int i = 0; i < skillIds.Length; i++)
                if (profile.GetSkillStart(skillIds[i]) != skillStarts[i]) return false;

            return true;
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
