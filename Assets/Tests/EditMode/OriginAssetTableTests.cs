using System.Collections.Generic;
using NUnit.Framework;
using Oerfi.Characters;
using UnityEditor;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Guards the generated origin/skill-growth assets against concept-doc
    /// drift (table 11.6) and against a regression of the predecessor bug
    /// where origin tables were built as non-serializable jagged arrays
    /// (assets would silently lose all values on save/reload).
    /// </summary>
    public class OriginAssetTableTests
    {
        [Test]
        public void OriginProfileAssets_MatchConceptTable()
        {
            string[] guids = AssetDatabase.FindAssets("t:OriginProfile", new[] { "Assets/Data/Characters/Origins" });
            Assert.That(
                guids.Length,
                Is.EqualTo(4),
                $"expected 4 origin profile assets in Assets/Data/Characters/Origins, found {guids.Length} " +
                "- run 'Oerfi/Characters/Generate Default Origins'");

            var seenOrigins = new HashSet<Origin>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                OriginProfile profile = AssetDatabase.LoadAssetAtPath<OriginProfile>(path);
                Assert.That(profile, Is.Not.Null, $"asset at {path} is not an OriginProfile");

                int[] expectedDeltas = ConceptOriginTables.AttributeDeltasOf(profile.Origin);
                int[] expectedSkills = ConceptOriginTables.SkillStartsOf(profile.Origin);
                Assert.That(profile.DisplayName, Is.EqualTo(ConceptOriginTables.DisplayNameOf(profile.Origin)),
                    $"display name drift on {profile.Origin}");

                foreach (AttributeId attribute in System.Enum.GetValues(typeof(AttributeId)))
                    Assert.That(profile.GetAttributeDelta(attribute),
                        Is.EqualTo(expectedDeltas[(int)attribute]),
                        $"attribute delta drift on {profile.Origin}/{attribute}");

                foreach (SkillId skill in System.Enum.GetValues(typeof(SkillId)))
                    Assert.That(profile.GetSkillStart(skill),
                        Is.EqualTo(expectedSkills[(int)skill]),
                        $"skill start drift on {profile.Origin}/{skill}");

                seenOrigins.Add(profile.Origin);
            }

            var expectedOrigins = new HashSet<Origin>
            {
                Origin.NorwegianFarmer, Origin.HebrideanGael, Origin.FaroeseShetland, Origin.Freedman
            };
            Assert.That(seenOrigins, Is.EquivalentTo(expectedOrigins), "origins and assets diverge");
        }

        [Test]
        public void SkillGrowthConfig_ExistsWithConceptDefaults()
        {
            SkillGrowthConfig config = AssetDatabase.LoadAssetAtPath<SkillGrowthConfig>(
                "Assets/Data/Characters/SkillGrowthConfig.asset");
            Assert.That(config, Is.Not.Null, "SkillGrowthConfig.asset missing - run the generator");
            Assert.That(config.LearnFactor, Is.EqualTo(0.01f).Within(0.0001f));
            Assert.That(config.Damping, Is.EqualTo(2f).Within(0.0001f));
        }
    }
}
