using System.Collections.Generic;
using NUnit.Framework;
using Oerfi.Economy;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Economy tests, ported from the predecessor project's EconomyTests and
    /// cleaned to the English API. Kúgildi reference values from the concept
    /// doc table 14.6.
    /// </summary>
    public class PricingEngineTests
    {
        [Test]
        public void Axe_LandsWithinSpecifiedRange()
        {
            var engine = new PricingEngine();
            // Axe: ~0.15 Kúgildi iron material + 5 labor hours; iron ×3,
            // wage 0.01 → 0.15 + 0.15 = 0.30 (concept table: 0.3–0.5).
            float value = engine.CalculateValue(0.15f, 5f, MaterialKeys.IRON);
            Assert.That(value, Is.GreaterThanOrEqualTo(0.3f), "axe below the 0.3–0.5 range");
            Assert.That(value, Is.LessThanOrEqualTo(0.5f), "axe above the 0.3–0.5 range");
        }

        [Test]
        public void Spoon_LandsWithinSpecifiedRange()
        {
            var engine = new PricingEngine();
            // Spoon: wood offcut ~0 + 1 labor hour at wood ×2 → 0.02 (ref 0.01).
            float value = engine.CalculateValue(0f, 1f, MaterialKeys.WOOD);
            Assert.That(value, Is.GreaterThanOrEqualTo(0.005f));
            Assert.That(value, Is.LessThanOrEqualTo(0.05f));
        }

        [Test]
        public void Bucket_LandsWithinSpecifiedRange()
        {
            var engine = new PricingEngine();
            // Bucket: stave wood ~0.02 + 3 labor hours at wood ×2 → 0.08
            // (concept ref 0.05, tolerated upward).
            float value = engine.CalculateValue(0.02f, 3f, MaterialKeys.WOOD);
            Assert.That(value, Is.LessThanOrEqualTo(0.15f), "bucket far above the 0.05 reference");
            Assert.That(value, Is.GreaterThanOrEqualTo(0.03f));
        }

        [Test]
        public void MaterialMultiplier_ScalesLaborPortionOnly()
        {
            var engine = new PricingEngine(ironMultiplier: 4f, woodMultiplier: 1f);
            Assert.That(engine.GetMultiplier("Iron"), Is.EqualTo(4f).Within(0.0001f));
            Assert.That(engine.GetMultiplier("wood"), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(engine.GetMultiplier("stone"), Is.EqualTo(1f).Within(0.0001f));

            float iron = engine.CalculateValue(0.1f, 2f, MaterialKeys.IRON);
            float wood = engine.CalculateValue(0.1f, 2f, MaterialKeys.WOOD);
            Assert.That(iron, Is.EqualTo(0.1f + 2f * 0.01f * 4f).Within(0.0001f));
            Assert.That(wood, Is.EqualTo(0.1f + 2f * 0.01f * 1f).Within(0.0001f));
        }
    }

    public class FarmEconomyAgentTests
    {
        [Test]
        public void ConsumesInPriorityOrder_Food_Clothing_Tools()
        {
            Good food = Good.CreateRuntimeOnly("food", "Nahrung", GoodCategory.Food, 0.1f);
            Good clothing = Good.CreateRuntimeOnly("clothing", "Kleidung", GoodCategory.CraftGood, 0.2f);
            Good tools = Good.CreateRuntimeOnly("tools", "Werkzeug", GoodCategory.CraftGood, 0.4f);

            var agent = new FarmEconomyAgent("Testhof");
            agent.SetPriorities(new[] { food, clothing, tools });

            agent.SetStock(food, 1f);
            agent.SetStock(clothing, 0.5f);
            agent.SetStock(tools, 0.2f);

            var input = new EconomyTickInput
            {
                Demand = new List<(Good, float)> { (food, 1f), (clothing, 1f), (tools, 1f) },
                Production = new List<(Good, float)>()
            };

            agent.Tick(new DetailedFarmSimulation(), input);

            Assert.That(agent.GetStock(food), Is.EqualTo(0f).Within(0.0001f),
                "food (priority 1) must be consumed first");
            Assert.That(agent.GetStock(clothing), Is.EqualTo(0f).Within(0.0001f),
                "clothing (priority 2) follows next and is partially served to depletion");
            Assert.That(agent.GetStock(tools), Is.GreaterThan(0f),
                "tools (priority 3) must not be consumed before food/clothing under scarcity");
        }

        [Test]
        public void Spoilage_ReducesPerishableGoods()
        {
            Good milk = Good.CreateRuntimeOnly("milk", "Milch", GoodCategory.Food, 0.05f,
                perishable: true, spoilageRatePerDay: 0.1f);
            Good axe = Good.CreateRuntimeOnly("axe", "Axt", GoodCategory.CraftGood, 0.4f);

            var agent = new FarmEconomyAgent("Verderbhof");
            agent.SetPriorities(new[] { milk });
            agent.SetStock(milk, 10f);
            agent.SetStock(axe, 1f);

            agent.Tick(new DetailedFarmSimulation(), EconomyTickInput.Empty());

            Assert.That(agent.GetStock(milk), Is.EqualTo(9f).Within(0.0001f),
                "spoilage rate 10% per tick");
            Assert.That(agent.GetStock(axe), Is.EqualTo(1f).Within(0.0001f),
                "non-perishables are unaffected");
        }

        [Test]
        public void TotalKugildiValue_SumsStock()
        {
            Good cow = Good.CreateRuntimeOnly("milchkuh", "Milchkuh", GoodCategory.Food, 1f);
            Good sheep = Good.CreateRuntimeOnly("schaf", "trächtiges Schaf", GoodCategory.Food, 1f / 6f);

            var agent = new FarmEconomyAgent("WertHof");
            agent.SetStock(cow, 2f);
            agent.SetStock(sheep, 6f);

            // 2 cows à 1 + 6 sheep à 1/6 = 2 + 1 = 3 Kúgildi.
            Assert.That(agent.TotalKugildiValue(), Is.EqualTo(3f).Within(0.0001f));
        }
    }

    public class TwoTierEconomyTests
    {
        [Test]
        public void DetailedVsAbstracted_SameStart_SimilarEndBalance_WithinTolerance()
        {
            Good food = Good.CreateRuntimeOnly("food", "Nahrung", GoodCategory.Food, 0.1f,
                perishable: true, spoilageRatePerDay: 0.01f);
            Good woolCloth = Good.CreateRuntimeOnly("vadmal", "Vaðmál", GoodCategory.CraftGood, 0.03f);

            EconomyTickInput MakeInput() => new EconomyTickInput
            {
                Demand = new List<(Good, float)> { (food, 0.4f) },
                Production = new List<(Good, float)> { (woolCloth, 0.2f) }
            };

            var detailedAgent = new FarmEconomyAgent("Spielerhof");
            detailedAgent.SetPriorities(new[] { food });
            detailedAgent.SetStock(food, 10f);
            detailedAgent.SetStock(woolCloth, 0f);

            var abstractedAgent = new FarmEconomyAgent("Fernehof");
            abstractedAgent.SetPriorities(new[] { food });
            abstractedAgent.SetStock(food, 10f);
            abstractedAgent.SetStock(woolCloth, 0f);

            const int days = 28;
            var detailed = new DetailedFarmSimulation();
            var abstracted = new AbstractedFarmSimulation(7);

            for (int i = 0; i < days; i++)
                detailedAgent.Tick(detailed, MakeInput());
            for (int i = 0; i < days / 7; i++)
                abstractedAgent.Tick(abstracted, MakeInput());

            // Tolerance band: spoilage + batch aggregation must not diverge
            // arbitrarily — 20% of the food value here.
            float foodDetailed = detailedAgent.GetStock(food);
            float foodAbstracted = abstractedAgent.GetStock(food);
            Assert.That(foodAbstracted, Is.EqualTo(foodDetailed).Within(2f),
                $"abstracted simulation deviates too far: {foodDetailed} vs {foodAbstracted}");

            // Production: detailed 28 × 0.2 = 5.6; abstracted 4 × 1.4 = 5.6 — exact.
            Assert.That(abstractedAgent.GetStock(woolCloth),
                Is.EqualTo(detailedAgent.GetStock(woolCloth)).Within(0.0001f),
                "non-perishable production must match exactly");
        }

        [Test]
        public void DistanceThreshold_SelectsCorrectLevel()
        {
            var selector = new EconomyLevelSelector(abstractionDistanceMeters: 5000f);

            Assert.That(selector.Select(100f), Is.InstanceOf<DetailedFarmSimulation>());
            Assert.That(selector.Select(4999f), Is.InstanceOf<DetailedFarmSimulation>());
            Assert.That(selector.Select(5000f), Is.InstanceOf<AbstractedFarmSimulation>());
            Assert.That(selector.Select(50000f), Is.InstanceOf<AbstractedFarmSimulation>());
        }
    }
}
