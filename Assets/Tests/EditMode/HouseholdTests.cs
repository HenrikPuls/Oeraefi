using NUnit.Framework;
using Oerfi.Characters;
using Oerfi.Core;
using Oerfi.Household;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Household tests, ported from the predecessor project's
    /// HouseholdTests plus regression coverage for the migration fixes
    /// (single unrest field, 364-day clothing rate, empty scripted random).
    /// </summary>
    public class UnrestSystemTests
    {
        [Test]
        public void RiskBand_PerConceptBands()
        {
            // Absolute unrest levels (no accumulated float deltas): the
            // band edges 30/60/90 themselves belong to the higher band.
            Assert.That(new UnrestSystem(0f).GetRiskBand(), Is.EqualTo((0f, 0f)));
            Assert.That(new UnrestSystem(29.9f).GetRiskBand(), Is.EqualTo((0f, 0f)),
                "just below the 30 band");
            Assert.That(new UnrestSystem(30f).GetRiskBand(), Is.EqualTo((0.02f, 0.05f)),
                "entering the 30-60 band");
            Assert.That(new UnrestSystem(59.9f).GetRiskBand(), Is.EqualTo((0.02f, 0.05f)));
            Assert.That(new UnrestSystem(60f).GetRiskBand(), Is.EqualTo((0.10f, 0.20f)),
                "entering the 60-90 band");
            Assert.That(new UnrestSystem(89.9f).GetRiskBand(), Is.EqualTo((0.10f, 0.20f)),
                "still below the top band");
            Assert.That(new UnrestSystem(90f).GetRiskBand(), Is.EqualTo((1f, 1f)),
                "top band: guaranteed event");
            Assert.That(new UnrestSystem(100f).GetRiskBand(), Is.EqualTo((1f, 1f)));
        }

        [Test]
        public void Coordination_AboveThreeThralls_RaisesRisk()
        {
            var baseline = new UnrestSystem(50f);
            Assert.That(baseline.GetRiskBand(3).min, Is.EqualTo(0.02f).Within(0.0001f),
                "at the threshold there is no coordination bonus");

            var coordinated = new UnrestSystem(50f);
            Assert.That(coordinated.GetRiskBand(5).min,
                Is.EqualTo(0.02f * 1.2f).Within(0.0001f), "two extra thralls × 10 %");

            var many = new UnrestSystem(95f);
            Assert.That(many.GetRiskBand(10).max, Is.LessThanOrEqualTo(1f),
                "coordination never exceeds certainty");
        }

        [Test]
        public void RollSeasonEvent_Guaranteed_InTopBand()
        {
            var unrest = new UnrestSystem(95f);
            Assert.That(unrest.RollSeasonEvent(1), Is.True, "≥90 unrest guarantees the season event");
        }

        [Test]
        public void RollSeasonEvent_Deterministic_WithScriptedRandom()
        {
            var calm = new UnrestSystem(10f);
            calm.SetRandomSource(new FixedRandomSource(0));
            Assert.That(calm.RollSeasonEvent(1), Is.False, "no risk in the lowest band");

            var risky = new UnrestSystem(45f);
            risky.SetRandomSource(new FixedRandomSource(0));
            Assert.That(risky.RollSeasonEvent(1), Is.True, "roll 0 < probability");

            risky.SetRandomSource(new FixedRandomSource(99));
            Assert.That(risky.RollSeasonEvent(1), Is.False, "roll 0.99 above midpoint probability");
        }

        [Test]
        public void BaseTypedReference_SeSameValue_NoShadowing()
        {
            // Regression: the predecessor kept a second unrest field beside
            // the base one, so base-typed reads returned stale data.
            UnrestSystem unrest = new UnrestSystem(20f);
            unrest.ApplyDelta(25f); // 45

            ConcealedScalar asBase = unrest;
            Assert.That(asBase.RawValue, Is.EqualTo(45f).Within(0.0001f),
                "base-typed RawValue must reflect the live unrest value");
        }

        [Test]
        public void DisplayStage_FollowsConceptBands_WithJitter()
        {
            var unrest = new UnrestSystem(20f);
            unrest.SetRandomSource(new FixedRandomSource(0)); // jitter 0
            Assert.That(unrest.GetDisplayStage(), Is.EqualTo(ConcealedStage.Calm));

            unrest.ApplyDelta(25f); // 45
            Assert.That(unrest.GetDisplayStage(), Is.EqualTo(ConcealedStage.Tense));

            unrest.ApplyDelta(30f); // 75
            Assert.That(unrest.GetDisplayStage(), Is.EqualTo(ConcealedStage.Agitated));

            unrest.ApplyDelta(25f); // 100
            Assert.That(unrest.GetDisplayStage(), Is.EqualTo(ConcealedStage.OpenlyHostile));
        }
    }

    public class ConcealedDisplayTests
    {
        [Test]
        public void DisplayStage_OnlyQualitativeStages_NeverRawValue()
        {
            var moral = new MoralValue(85f);
            moral.SetRandomSource(new FixedRandomSource(0));

            for (int i = 0; i < 50; i++)
            {
                ConcealedStage stage = ((IConcealedStateDisplay)moral).GetDisplayStage();
                Assert.That((int)stage, Is.InRange(0, 3), "UI contract exposes stages only");
            }
        }

        [Test]
        public void DisplayStage_Thresholds_JitteredButReproducible()
        {
            // Jitter -7.5: 75 → 67.5 → still Tense.
            var low = new MoralValue(75f);
            low.SetRandomSource(new FixedRandomSource(-100));
            Assert.That(low.GetDisplayStage(), Is.EqualTo(ConcealedStage.Tense));

            // Same seed, same stage: the fuzz is reproducible, not chaotic.
            var lowAgain = new MoralValue(75f);
            lowAgain.SetRandomSource(new FixedRandomSource(-100));
            Assert.That(lowAgain.GetDisplayStage(), Is.EqualTo(low.GetDisplayStage()));

            // No jitter: 75 stays Tense.
            var mid = new MoralValue(75f);
            mid.SetRandomSource(new FixedRandomSource(0));
            Assert.That(mid.GetDisplayStage(), Is.EqualTo(ConcealedStage.Tense));

            // Jitter +7.5: 76 → 83.5 → Calm (one step below the 75 boundary).
            var high = new MoralValue(76f);
            high.SetRandomSource(new FixedRandomSource(100));
            Assert.That(high.GetDisplayStage(), Is.EqualTo(ConcealedStage.Calm));
        }
    }

    public class ManumissionTests
    {
        [Test]
        public void Rational_WhenUpkeepExceedsWorkValue()
        {
            Assert.That(ManumissionService.IsManumissionRational(0.001f, 0f), Is.True,
                "negligible work value, positive upkeep → free them");
            Assert.That(ManumissionService.IsManumissionRational(0.5f, 5f), Is.False,
                "skilled, productive thrall is never rational to free");
        }

        [Test]
        public void Manumit_GrantsThingOratory_NormalLearningResumes()
        {
            Character character = CharacterFactory.Create(ConceptOriginTables.CreateProfile(Origin.Freedman));
            Assert.That(character.Skills.Get(SkillId.ThingOratory), Is.EqualTo(0f),
                "unfree: legally no thing speaking rights");

            LabourStatus status = ManumissionService.Manumit(character);

            Assert.That(status, Is.EqualTo(LabourStatus.Vinnufolk));
            Assert.That(character.Skills.Get(SkillId.ThingOratory),
                Is.EqualTo(ManumissionService.NORMAL_THING_ORATORY).Within(0.0001f),
                "the zero was legal, not a lack of experience");

            float before = character.Skills.Get(SkillId.ThingOratory);
            character.Skills.GainExperience(SkillId.ThingOratory, 100f);
            Assert.That(character.Skills.Get(SkillId.ThingOratory), Is.GreaterThan(before),
                "learning-by-doing resumes after manumission");
        }
    }

    public class ClothingVitalityLinkTests
    {
        [Test]
        public void SevereDeficit_InWinter_DeathAtFiftyTicks_NotEarlier()
        {
            var vitality = new VitalityValue(100f);
            var link = new ClothingVitalityLink();

            int deathTick = -1;
            for (int tick = 1; tick <= 1000; tick++)
            {
                link.Tick(3f, isVetur: true, vitality);
                if (vitality.IsDead) { deathTick = tick; break; }
            }

            Assert.That(deathTick, Is.EqualTo(50), "2.0 loss/tick from 100 → death exactly at tick 50");

            // Death is the vitality consequence itself — no extra roll kills anyone.
            Assert.That(vitality.RawValue, Is.EqualTo(0f).Within(0.5f));
        }

        [Test]
        public void LightDeficit_Reversible()
        {
            var vitality = new VitalityValue(100f);
            var link = new ClothingVitalityLink();

            for (int i = 0; i < 20; i++) link.Tick(0.5f, isVetur: false, vitality);
            Assert.That(vitality.RawValue, Is.EqualTo(96f).Within(0.0001f),
                "light loss 0.2/tick × 20");
            Assert.That(vitality.IsDead, Is.False);

            for (int i = 0; i < 40; i++) link.Tick(0f, isVetur: false, vitality);
            Assert.That(vitality.RawValue, Is.EqualTo(100f).Within(0.0001f),
                "recovery 0.5/tick heals back to full (clamped)");
        }

        [Test]
        public void AttributeDamage_OnlyAfterSustainedSevereDeficit()
        {
            var vitality = new VitalityValue(100f);
            var link = new ClothingVitalityLink();

            for (int tick = 1; tick < ClothingVitalityLink.TICKS_UNTIL_ATTRIBUTE_DAMAGE; tick++)
                Assert.That(link.Tick(2f, isVetur: false, vitality), Is.False,
                    $"no attribute damage before tick {ClothingVitalityLink.TICKS_UNTIL_ATTRIBUTE_DAMAGE}");

            Assert.That(link.Tick(2f, isVetur: false, vitality), Is.True,
                "the 60th severe tick demands permanent attribute damage");
        }

        [Test]
        public void SuppliedHousehold_ResetsDamageCounter()
        {
            var vitality = new VitalityValue(100f);
            var link = new ClothingVitalityLink();

            for (int i = 0; i < ClothingVitalityLink.TICKS_UNTIL_ATTRIBUTE_DAMAGE - 1; i++)
                link.Tick(2f, isVetur: false, vitality);

            // One supplied tick resets the sustained-severe counter.
            link.Tick(0f, isVetur: false, vitality);
            Assert.That(link.Tick(2f, isVetur: false, vitality), Is.False,
                "a single mild tick interrupts the severe streak");
        }
    }

    public class ThrallSupplyNeedTests
    {
        [Test]
        public void FoodPerDay_SummerAndWinter_WithAndWithoutHeavyWork()
        {
            Assert.That(ThrallSupplyNeed.FoodPerDay(Season.Sumar), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(ThrallSupplyNeed.FoodPerDay(Season.Sumar, heavyWork: true),
                Is.EqualTo(1.2f).Within(0.0001f));
            Assert.That(ThrallSupplyNeed.FoodPerDay(Season.Vetur), Is.EqualTo(1.5f).Within(0.0001f),
                "winter +50 %");
            Assert.That(ThrallSupplyNeed.FoodPerDay(Season.Vetur, heavyWork: true),
                Is.EqualTo(1.7f).Within(0.0001f), "modifiers stack additively");
        }

        [Test]
        public void ClothingPerDay_AnchoredTo364DayYear()
        {
            // Regression: the predecessor hardcoded a 360-day year.
            Assert.That(ThrallSupplyNeed.ClothingPerDay(),
                Is.EqualTo(1f / 364f).Within(0.0000001f));
        }
    }

    public class HouseholdGoodsStockTests
    {
        [Test]
        public void Shortfall_NoHardFail_GradualMoralEffect()
        {
            var stock = new HouseholdGoodsStock();
            stock.SetActual(HouseholdCategory.Containers, 4f); // target 6

            float shortfall = stock.GetShortfall(HouseholdCategory.Containers);
            Assert.That(shortfall, Is.EqualTo(2f).Within(0.0001f));

            var moral = new MoralValue(100f);
            moral.ApplyShortfall(shortfall);
            Assert.That(moral.RawValue, Is.EqualTo(96f).Within(0.0001f),
                "2 units short × 2 points = gradual degradation, not failure");
        }

        [Test]
        public void ClothingShortfall_RoutesToVitality_NotMoral()
        {
            var stock = new HouseholdGoodsStock();
            stock.SetActual(HouseholdCategory.Clothing, 0f); // target 4

            var moral = new MoralValue(100f);
            var vitality = new VitalityValue(100f);
            var link = new ClothingVitalityLink();

            float shortfall = stock.GetShortfall(HouseholdCategory.Clothing);
            link.Tick(shortfall, isVetur: false, vitality); // vitality drops

            Assert.That(vitality.RawValue, Is.LessThan(100f), "clothing couples to vitality");
            Assert.That(moral.RawValue, Is.EqualTo(100f).Within(0.0001f),
                "and NOT to moral (subarctic survival risk, concept doc 10)");
        }

        [Test]
        public void TotalShortfall_SumsCategories()
        {
            var stock = new HouseholdGoodsStock();
            stock.SetActual(HouseholdCategory.Containers, 4f); // -2
            stock.SetActual(HouseholdCategory.Lighting, 1f);   // -1
            Assert.That(stock.GetTotalShortfall(), Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void Wear_ReducesActual_Continuously()
        {
            var stock = new HouseholdGoodsStock();
            stock.ApplyWear(0.5f);
            Assert.That(stock.GetActual(HouseholdCategory.Containers), Is.EqualTo(5.5f).Within(0.0001f));
            Assert.That(stock.GetActual(HouseholdCategory.Lighting), Is.EqualTo(1.5f).Within(0.0001f));

            stock.ApplyWear(1000f);
            Assert.That(stock.GetActual(HouseholdCategory.Containers), Is.EqualTo(0f),
                "wear clamps at zero");
        }
    }
}
