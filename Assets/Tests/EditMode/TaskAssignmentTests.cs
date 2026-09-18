using System.Collections.Generic;
using NUnit.Framework;
using Oerfi.AI;
using Oerfi.Characters;
using Oerfi.Economy;
using Oerfi.Household;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// AI tests, ported from the predecessor project's TaskAssignmentTests
    /// plus regression coverage for the migration fixes (unified Tick that
    /// really feeds stock, explicit skill mapping for the late roles,
    /// active-flag semantics).
    /// </summary>
    public class TaskAssignmentTests
    {
        [Test]
        public void Fowling_OutOfSeason_NoYield_InSeason_Yields()
        {
            // Breeding season: Harpa..Haustmanudur (months 0-5); month 2 inside.
            Assert.That(AutonomousWorker.DailyYield(TaskRole.Fowling, 5f, 2),
                Is.GreaterThan(0f));
            // Month 9 = Thorri: fowling forbidden.
            Assert.That(AutonomousWorker.DailyYield(TaskRole.Fowling, 5f, 9),
                Is.EqualTo(0f));
        }

        [Test]
        public void YearRoundRole_PeatCutting_YieldsInWinter()
        {
            Assert.That(AutonomousWorker.DailyYield(TaskRole.PeatCutting, 5f, 9),
                Is.GreaterThan(0f), "peat cutting runs through Thorri");
        }

        [Test]
        public void HigherSkill_HigherYield_Monotonic()
        {
            float low = AutonomousWorker.DailyYield(TaskRole.Fishing, 1f, 2);
            float mid = AutonomousWorker.DailyYield(TaskRole.Fishing, 4f, 2);
            float high = AutonomousWorker.DailyYield(TaskRole.Fishing, 8f, 2);

            Assert.That(low, Is.LessThan(mid));
            Assert.That(mid, Is.LessThan(high));

            // Continuous (not stepped) growth: the ratios differ measurably.
            float ratioLowMid = mid / low;
            float ratioMidHigh = high / mid;
            Assert.That(ratioLowMid, Is.Not.EqualTo(ratioMidHigh).Within(0.01f));
        }

        [Test]
        public void SkillGrowth_ThroughUse_YieldRisesOver100Ticks()
        {
            Character character = MakeCharacter("Arbeitskraft");
            var agent = new FarmEconomyAgent("Testhof");
            var worker = new AutonomousWorker(character, TaskRole.Fishing, agent);

            float atStart = worker.ComputeDailyYield(2);
            for (int i = 0; i < 100; i++) worker.Tick(2, null);
            float later = worker.ComputeDailyYield(2);

            Assert.That(later, Is.GreaterThan(atStart),
                "learning by doing raises the daily yield");
        }

        [Test]
        public void Reassign_PreservesStock_NewRoleProducesNewGood()
        {
            Character character = MakeCharacter("Arbeitskraft");
            var agent = new FarmEconomyAgent("Testhof");
            var worker = new AutonomousWorker(character, TaskRole.PeatCutting, agent);
            Good peat = Good.CreateRuntimeOnly("torf", "Torf", GoodCategory.RawMaterial, 0.05f);
            Good fish = Good.CreateRuntimeOnly("fisch", "Fisch", GoodCategory.Food, 0.2f);

            for (int i = 0; i < 10; i++) worker.Tick(2, peat);
            float peatStock = agent.GetStock(peat);
            Assert.That(peatStock, Is.GreaterThan(0f));

            PriorityOverride.Reassign(worker, TaskRole.Fishing);
            Assert.That(worker.Role, Is.EqualTo(TaskRole.Fishing));

            worker.Tick(2, fish);
            Assert.That(agent.GetStock(peat), Is.EqualTo(peatStock).Within(0.0001f),
                "banked stock is not rolled back by a reassignment");
            Assert.That(agent.GetStock(fish), Is.GreaterThan(0f),
                "the new role produces its own good");
        }

        [Test]
        public void Overload_RaisesUnrest_VsControl()
        {
            var unrestBurdened = new UnrestSystem(20f);
            var workerBurdened = new AutonomousWorker(
                MakeCharacter("Schwach", toughness: 4), TaskRole.Quarrying, null, unrestBurdened);

            var unrestControl = new UnrestSystem(20f);
            var workerControl = new AutonomousWorker(
                MakeCharacter("Robust", toughness: 8), TaskRole.Quarrying, null, unrestControl);

            for (int i = 0; i < 30; i++)
            {
                workerBurdened.Tick(2, null, heavyWork: true);
                workerControl.Tick(2, null, heavyWork: true);
            }

            Assert.That(unrestBurdened.RawValue, Is.GreaterThan(unrestControl.RawValue));
            Assert.That(unrestBurdened.RawValue, Is.GreaterThan(20f));
            Assert.That(workerBurdened.OverloadTicks, Is.GreaterThan(0));
        }

        [Test]
        public void Tick_FeedsStock_WithProvidedGood()
        {
            // Regression: the predecessor's Tick looked up the output good
            // internally, always got null, and never fed the farm stock.
            Character character = MakeCharacter("Arbeitskraft");
            var agent = new FarmEconomyAgent("Testhof");
            var worker = new AutonomousWorker(character, TaskRole.Fishing, agent);
            Good fish = Good.CreateRuntimeOnly("fisch", "Fisch", GoodCategory.Food, 0.2f);

            // The tick computes the yield with the pre-tick skill, then trains.
            float skillBefore = character.Skills.Get(SkillId.Fishing);
            worker.Tick(2, fish);

            float expected = AutonomousWorker.DailyYield(TaskRole.Fishing, skillBefore, 2);
            Assert.That(agent.GetStock(fish), Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void Idle_NoYield_NoExperience_ButPassiveRecovery()
        {
            Character character = MakeCharacter("Faulenzer");
            var agent = new FarmEconomyAgent("Testhof");
            var unrest = new UnrestSystem(10f);
            var worker = new AutonomousWorker(character, TaskRole.Idle, agent, unrest);
            Good fish = Good.CreateRuntimeOnly("fisch", "Fisch", GoodCategory.Food, 0.2f);

            float skillSumBefore = SkillSum(character.Skills);
            for (int i = 0; i < 10; i++) worker.Tick(2, fish, heavyWork: true);

            Assert.That(agent.Stock.Count, Is.EqualTo(0), "idle produces nothing");
            Assert.That(SkillSum(character.Skills), Is.EqualTo(skillSumBefore),
                "idle trains no skill");
            // Idle never counts as overload (no +0.5); the untouched recovery
            // branch still eases unrest passively (predecessor semantics).
            Assert.That(unrest.RawValue,
                Is.EqualTo(10f - 10 * AutonomousWorker.UNREST_RECOVERY_PER_TICK).Within(0.0001f));
            Assert.That(worker.OverloadTicks, Is.EqualTo(0));
        }

        [Test]
        public void InactiveWorker_SkipsTickEntirely()
        {
            Character character = MakeCharacter("Winterruher");
            var unrest = new UnrestSystem(20f);
            var worker = new AutonomousWorker(character, TaskRole.Quarrying, null, unrest);
            worker.IsActive = false;

            for (int i = 0; i < 5; i++) worker.Tick(2, null, heavyWork: true);

            Assert.That(unrest.RawValue, Is.EqualTo(20f).Within(0.0001f),
                "inactive means no overload and no passive recovery either");
            Assert.That(worker.OverloadTicks, Is.EqualTo(0));
        }

        [TestCase(TaskRole.Construction, SkillId.Woodworking)]
        [TestCase(TaskRole.Carpenter, SkillId.Woodworking)]
        [TestCase(TaskRole.Stonemason, SkillId.Stonemasonry)]
        [TestCase(TaskRole.PeatCutting, SkillId.Farming)]
        [TestCase(TaskRole.Cooking, SkillId.Leatherworking)]
        public void LateRoles_MapToExplicitSkills(TaskRole role, SkillId expectedSkill)
        {
            Assert.That(AutonomousWorker.MapRoleToSkill(role), Is.EqualTo(expectedSkill));
        }

        [Test]
        public void Idle_HasNoMappableSkill()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => AutonomousWorker.MapRoleToSkill(TaskRole.Idle));
        }

        [Test]
        public void BaseYield_MatchesMigrationTable()
        {
            // All 18 roles pinned to the migration table (predecessor values;
            // the four late roles get explicit values instead of silent defaults).
            Assert.That(AutonomousWorker.BaseYield(TaskRole.WoolProcessing),
                Is.EqualTo(0.5f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Fishing), Is.EqualTo(2f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.PeatCutting), Is.EqualTo(2f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Farming), Is.EqualTo(1.2f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Fowling), Is.EqualTo(1.5f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.SealHunting), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Quarrying), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.WoodConstruction),
                Is.EqualTo(0.8f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Smithing), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Weaving), Is.EqualTo(0.5f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Coopering), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Leatherworking), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Cooking), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Trading), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Construction), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Carpenter), Is.EqualTo(0.8f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Stonemason), Is.EqualTo(1f));
            Assert.That(AutonomousWorker.BaseYield(TaskRole.Idle), Is.EqualTo(0f));
        }

        [Test]
        public void SeasonalWindow_YearWrap()
        {
            // A window wrapping across the year boundary (e.g. late Vetur
            // into Sumar): start > end means OR of both ranges.
            var wrapped = new SeasonalWindow(10, 2);
            Assert.That(wrapped.IsAvailable(10), Is.True);
            Assert.That(wrapped.IsAvailable(11), Is.True);
            Assert.That(wrapped.IsAvailable(0), Is.True);
            Assert.That(wrapped.IsAvailable(2), Is.True);
            Assert.That(wrapped.IsAvailable(5), Is.False);

            var straight = new SeasonalWindow(0, 5);
            Assert.That(straight.IsAvailable(0), Is.True);
            Assert.That(straight.IsAvailable(5), Is.True);
            Assert.That(straight.IsAvailable(6), Is.False);
        }

        [Test]
        public void Trading_GatedToSummerMonths()
        {
            // The sailing season (May-September) gates trading just like
            // fowling and farming.
            Assert.That(TaskRoleDefinitions.IsYearRound(TaskRole.Trading), Is.False);
            Assert.That(AutonomousWorker.DailyYield(TaskRole.Trading, 5f, 9),
                Is.EqualTo(0f), "no trading in Thorri - the ships are ashore");
            Assert.That(AutonomousWorker.DailyYield(TaskRole.Trading, 5f, 1),
                Is.GreaterThan(0f));
        }

        private static Character MakeCharacter(string name, int toughness = 5)
        {
            // Character.Attributes is get-only, so the toughness override goes
            // in through the origin profile's attribute delta.
            int[] deltas = (int[])ConceptOriginTables.AttributeDeltasOf(Origin.NorwegianFarmer).Clone();
            deltas[(int)AttributeId.Toughness] = toughness - Attributes.BASELINE;
            OriginProfile profile = OriginProfile.CreateRuntimeOnly(
                Origin.NorwegianFarmer,
                ConceptOriginTables.DisplayNameOf(Origin.NorwegianFarmer),
                deltas,
                ConceptOriginTables.SkillStartsOf(Origin.NorwegianFarmer));
            return CharacterFactory.Create(profile, name);
        }

        private static float SkillSum(SkillSystem skills)
        {
            float sum = 0f;
            foreach (KeyValuePair<SkillId, float> pair in skills.GetAll())
                sum += pair.Value;
            return sum;
        }
    }
}
