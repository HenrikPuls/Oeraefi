using System.Collections.Generic;
using NUnit.Framework;
using Oerfi.Economy;
using UnityEditor;
using UnityEngine;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Guards the generated good assets against concept-doc drift (table
    /// 14.6) and against a regression of the predecessor bug where the
    /// generator created assets but never wrote any values.
    /// </summary>
    public class GoodAssetTableTests
    {
        private static readonly (string id, GoodCategory category, float kugildi, bool perishable)[] CONCEPT_TABLE =
        {
            ("milchkuh", GoodCategory.Food, 1f, false),
            ("schaf-traechtig", GoodCategory.Food, 1f / 6f, false),
            ("vadmal-elle", GoodCategory.CraftGood, 1f / 36f, false),
            ("loeffel", GoodCategory.CraftGood, 0.01f, false),
            ("eimer", GoodCategory.CraftGood, 0.05f, false),
            ("axt", GoodCategory.CraftGood, 0.4f, false),
            ("bauholz-import", GoodCategory.ImportGood, 0.75f, false),
            ("hof-grundausstattung", GoodCategory.CraftGood, 2.5f, false),
        };

        [Test]
        public void GoodsAssets_MatchConceptTable()
        {
            string[] guids = AssetDatabase.FindAssets("t:Good", new[] { "Assets/Data/Goods" });
            Assert.That(
                guids.Length,
                Is.EqualTo(CONCEPT_TABLE.Length),
                $"expected {CONCEPT_TABLE.Length} good assets in Assets/Data/Goods, found {guids.Length} " +
                "- run 'Oerfi/Economy/Generate Default Goods'");

            var seenIds = new HashSet<string>();
            const float EPSILON = 0.0001f;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Good good = AssetDatabase.LoadAssetAtPath<Good>(path);
                Assert.That(good, Is.Not.Null, $"asset at {path} is not a Good");

                (string id, GoodCategory category, float kugildi, bool perishable)? entry = null;
                foreach (var candidate in CONCEPT_TABLE)
                    if (candidate.id == good.Id) { entry = candidate; break; }

                Assert.That(entry, Is.Not.Null, $"unexpected good '{good.Id}' at {path}");
                Assert.That(good.Category, Is.EqualTo(entry.Value.category), $"category drift on {good.Id}");
                Assert.That(good.KugildiValue, Is.EqualTo(entry.Value.kugildi).Within(EPSILON),
                    $"Kúgildi drift on {good.Id}");
                Assert.That(good.Perishable, Is.EqualTo(entry.Value.perishable), $"perishable drift on {good.Id}");
                seenIds.Add(good.Id);
            }

            var expectedIds = new HashSet<string>();
            foreach (var entry in CONCEPT_TABLE) expectedIds.Add(entry.id);
            Assert.That(seenIds, Is.EquivalentTo(expectedIds), "table ids and asset ids diverge");
        }

        [Test]
        public void PricingConfig_ExistsWithConceptDefaults()
        {
            PricingConfig config = AssetDatabase.LoadAssetAtPath<PricingConfig>(
                "Assets/Data/Economy/PricingConfig.asset");
            Assert.That(config, Is.Not.Null, "PricingConfig.asset missing - run the generator");
            Assert.That(config.WageRate, Is.EqualTo(0.01f).Within(0.0001f));
            Assert.That(config.IronMultiplier, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(config.WoodMultiplier, Is.EqualTo(2f).Within(0.0001f));
        }
    }
}
