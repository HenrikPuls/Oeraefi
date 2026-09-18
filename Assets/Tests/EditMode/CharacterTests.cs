using System.Collections.Generic;
using NUnit.Framework;
using Oerfi.Characters;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Character tests, ported from the predecessor project's
    /// CharacterSystemTests. The factory tests pin the concept-doc 11.6
    /// tables exactly via ConceptOriginTables.
    /// </summary>
    public class CharacterTests
    {
        [Test]
        public void Factory_NorwegianFarmer_MatchesConceptTable()
        {
            Character c = CharacterFactory.Create(ConceptOriginTables.CreateProfile(Origin.NorwegianFarmer));

            Assert.That(c.Attributes.Get(AttributeId.Strength), Is.EqualTo(6), "Kraft");
            Assert.That(c.Attributes.Get(AttributeId.Dexterity), Is.EqualTo(5), "Geschick");
            Assert.That(c.Attributes.Get(AttributeId.Toughness), Is.EqualTo(6), "Zähigkeit");
            Assert.That(c.Attributes.Get(AttributeId.Perception), Is.EqualTo(5), "Wahrnehmung");
            Assert.That(c.Attributes.Get(AttributeId.Wits), Is.EqualTo(5), "Verstand");
            Assert.That(c.Attributes.Get(AttributeId.Willpower), Is.EqualTo(5), "Wille");

            Assert.That(c.Skills.Get(SkillId.Farming), Is.EqualTo(4f), "Ackerbau");
            Assert.That(c.Skills.Get(SkillId.Trading), Is.EqualTo(3f), "Handel (Norwegen-Kontakte)");
            Assert.That(c.Skills.Get(SkillId.Stonemasonry), Is.EqualTo(1f), "keine Trockenmauer-Technik");
            Assert.That(c.Skills.Get(SkillId.Helmsmanship), Is.EqualTo(1f));
        }

        [Test]
        public void Factory_HebrideanGael_MatchesConceptTable()
        {
            Character c = CharacterFactory.Create(ConceptOriginTables.CreateProfile(Origin.HebrideanGael));

            Assert.That(c.Attributes.Get(AttributeId.Strength), Is.EqualTo(5), "Kraft");
            Assert.That(c.Attributes.Get(AttributeId.Dexterity), Is.EqualTo(6), "Geschick");
            Assert.That(c.Attributes.Get(AttributeId.Perception), Is.EqualTo(6), "Wahrnehmung");

            Assert.That(c.Skills.Get(SkillId.Stonemasonry), Is.EqualTo(4f), "Trockenmauer-Technik: 4");
            Assert.That(c.Skills.Get(SkillId.Fowling), Is.EqualTo(3f), "Klippen-Vogelfang");
            Assert.That(c.Skills.Get(SkillId.Trading), Is.EqualTo(1f), "schwächere Norwegen-Kontakte");
        }

        [Test]
        public void Factory_FaroeseShetland_MatchesConceptTable()
        {
            Character c = CharacterFactory.Create(ConceptOriginTables.CreateProfile(Origin.FaroeseShetland));

            Assert.That(c.Attributes.Get(AttributeId.Toughness), Is.EqualTo(6), "Zähigkeit");
            Assert.That(c.Attributes.Get(AttributeId.Perception), Is.EqualTo(6), "Wahrnehmung");

            Assert.That(c.Skills.Get(SkillId.Fishing), Is.EqualTo(4f));
            Assert.That(c.Skills.Get(SkillId.Fowling), Is.EqualTo(4f));
            Assert.That(c.Skills.Get(SkillId.CoastalSailing), Is.EqualTo(4f), "robuste Seemannschaft");
            Assert.That(c.Skills.Get(SkillId.Farming), Is.EqualTo(1f), "wenig Ackerbauerfahrung");
        }

        [Test]
        public void Factory_Freedman_WillpowerBonus_ThingOratoryZero()
        {
            Character c = CharacterFactory.Create(ConceptOriginTables.CreateProfile(Origin.Freedman));

            foreach (AttributeId attribute in System.Enum.GetValues(typeof(AttributeId)))
            {
                int expected = attribute == AttributeId.Willpower ? 6 : 5;
                Assert.That(c.Attributes.Get(attribute), Is.EqualTo(expected),
                    "Freedman has no attribute penalty - only the willpower bonus (concept 11.6)");
            }

            Assert.That(c.Skills.Get(SkillId.Farming), Is.EqualTo(2f), "breite Grundfertigkeiten");
            Assert.That(c.Skills.Get(SkillId.Weaving), Is.EqualTo(2f));
            Assert.That(c.Skills.Get(SkillId.ThingOratory), Is.EqualTo(0f),
                "unfree people had no speaking rights at the thing (legal, not balancing)");
            Assert.That(c.Skills.Get(SkillId.Helmsmanship), Is.EqualTo(0f));
        }

        [Test]
        public void Factory_DefaultName_IsOriginDisplayName()
        {
            Character c = CharacterFactory.Create(ConceptOriginTables.CreateProfile(Origin.NorwegianFarmer));
            Assert.That(c.Name, Is.EqualTo("Norwegischer Bauer (Norðmaðr)"));

            Character named = CharacterFactory.Create(
                ConceptOriginTables.CreateProfile(Origin.Freedman), "Astrid");
            Assert.That(named.Name, Is.EqualTo("Astrid"));
        }

        [Test]
        public void GainExperience_DiminishingReturns_Demonstrable()
        {
            var skills = new SkillSystem();
            skills.Set(SkillId.Fishing, 1f);
            skills.Set(SkillId.Fowling, 9f);

            float beforeLow = skills.Get(SkillId.Fishing);
            float beforeHigh = skills.Get(SkillId.Fowling);
            skills.GainExperience(SkillId.Fishing, 100f);
            skills.GainExperience(SkillId.Fowling, 100f);

            float deltaLow = skills.Get(SkillId.Fishing) - beforeLow;
            float deltaHigh = skills.Get(SkillId.Fowling) - beforeHigh;
            Assert.That(deltaLow, Is.GreaterThan(deltaHigh), "growth shrinks at high skill levels");
            Assert.That(skills.Get(SkillId.Fowling), Is.GreaterThanOrEqualTo(beforeHigh),
                "experience never reduces a skill");
        }

        [Test]
        public void GainExperience_NeverExceedsMaximum()
        {
            var skills = new SkillSystem();
            skills.Set(SkillId.Smithing, 9.9f);
            for (int i = 0; i < 1000; i++) skills.GainExperience(SkillId.Smithing, 50f);
            Assert.That(skills.Get(SkillId.Smithing), Is.LessThanOrEqualTo(SkillSystem.MAX_SKILL));
        }

        [Test]
        public void GainExperience_OnlyThroughUse()
        {
            var skills = new SkillSystem();
            Assert.That(skills.Get(SkillId.Weaving), Is.EqualTo(0f), "no purchase, skill starts unused");
            skills.GainExperience(SkillId.Weaving, 50f);
            Assert.That(skills.Get(SkillId.Weaving), Is.GreaterThan(0f));
        }

        [Test]
        public void GainExperience_NegativeAmount_Rejected()
        {
            var skills = new SkillSystem();
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => skills.GainExperience(SkillId.Fishing, -1f));
        }

        [Test]
        public void GetAll_ReturnsDefensiveCopy()
        {
            var skills = new SkillSystem();
            skills.Set(SkillId.Coopering, 3f);

            IReadOnlyDictionary<SkillId, float> snapshot = skills.GetAll();

            // Attack the implementation through a cast, like a buggy consumer would.
            var mutable = (Dictionary<SkillId, float>)snapshot;
            mutable.Remove(SkillId.Coopering);
            mutable[SkillId.Coopering] = 9f;

            Assert.That(skills.Get(SkillId.Coopering), Is.EqualTo(3f),
                "mutating the returned dictionary must not affect the skill system");
        }

        [Test]
        public void CheckResolver_ScriptedRandom_ReproducibleResults()
        {
            var first = new CheckResolver(new FixedRandomSource(3, 7));
            var second = new CheckResolver(new FixedRandomSource(3, 7));

            var a = first.Resolve(5, 4);
            var b = second.Resolve(5, 4);

            Assert.That(a.roll, Is.EqualTo(b.roll));
            Assert.That(a.result, Is.EqualTo(b.result));
            Assert.That(a.roll, Is.EqualTo(10), "scripted dice 3 + 7");
            Assert.That(a.targetValue, Is.EqualTo(9), "attribute 5 + skill 4");
            Assert.That(a.result, Is.EqualTo(CheckResolver.Result.Failure), "10 > 9");
        }

        [Test]
        public void CheckResolver_TargetValue_IsAttributePlusSkill()
        {
            var resolver = new CheckResolver(new FixedRandomSource(5, 5));
            var check = resolver.Resolve(6, 3);
            Assert.That(check.targetValue, Is.EqualTo(9));
        }

        [Test]
        public void CheckResolver_TwoThousandRolls_BellCurveAround11()
        {
            var resolver = new CheckResolver(new RandomSource(42));
            var histogram = new Dictionary<int, int>();
            const int n = 2000;

            for (int i = 0; i < n; i++)
            {
                // Target 20 never fails: every roll reaches Resolve's dice.
                int roll = resolver.Resolve(20, 0).roll;
                histogram[roll] = histogram.TryGetValue(roll, out int count) ? count + 1 : 1;
            }

            Assert.That(histogram.Keys, Is.EquivalentTo(new[] {
                2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
            }), "2d10 produces exactly the sums 2..20");

            Assert.That(histogram[11], Is.GreaterThan(histogram[2] * 5), "center beats extreme low");
            Assert.That(histogram[11], Is.GreaterThan(histogram[20] * 5), "center beats extreme high");
            Assert.That(histogram[2], Is.LessThan(n * 0.02), "sum 2 rare (1/100 expected)");
            Assert.That(histogram[20], Is.LessThan(n * 0.02), "sum 20 rare (1/100 expected)");
            Assert.That(histogram[11], Is.GreaterThan(n * 0.07), "sum 11 dominant (~9/100 expected)");
            Assert.That(histogram[10], Is.GreaterThan(n * 0.03), "bell symmetry sanity near the center");
            Assert.That(histogram[12], Is.GreaterThan(n * 0.03), "bell symmetry sanity near the center");
        }

        [Test]
        public void FixedRandomSource_Empty_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new FixedRandomSource());
            Assert.Throws<System.ArgumentException>(() => new FixedRandomSource(null));
        }

        [Test]
        public void RandomSource_BoundsAreInclusive()
        {
            var source = new RandomSource(7);
            for (int i = 0; i < 500; i++)
            {
                int value = source.Next(1, 10);
                Assert.That(value, Is.InRange(1, 10));
            }
        }

        [Test]
        public void Attributes_CopySemantics_AreIndependent()
        {
            Attributes a = Attributes.CreateBaseline();
            Attributes b = a;
            b.ApplyDelta(AttributeId.Strength, 1);

            Assert.That(a.Get(AttributeId.Strength), Is.EqualTo(Attributes.BASELINE),
                "struct copies must not share the backing array");
        }

        [Test]
        public void Attributes_ClampedToScale()
        {
            var attributes = Attributes.CreateBaseline();
            attributes.ApplyDelta(AttributeId.Wits, 99);
            Assert.That(attributes.Get(AttributeId.Wits), Is.EqualTo(Attributes.MAX));

            attributes.ApplyDelta(AttributeId.Wits, -99);
            Assert.That(attributes.Get(AttributeId.Wits), Is.EqualTo(Attributes.MIN));
        }
    }
}
