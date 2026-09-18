using System;

namespace Oerfi.Household
{
    /// <summary>
    /// The seven everyday-goods categories of a household (concept doc 10).
    /// Enum order is load-bearing only for the array layout below; entries
    /// are referenced BY NAME everywhere, never by index.
    /// </summary>
    public enum HouseholdCategory
    {
        Containers = 0,   // Behälter
        Tableware = 1,    // Essgeschirr/Besteck
        Clothing = 2,     // Kleidung
        TextileTools = 3, // Textilzubehör
        Tools = 4,        // Werkzeug
        Storage = 5,      // Lagerung/Konservierung
        Lighting = 6      // Beleuchtung
    }

    /// <summary>
    /// Aggregate target/actual stock per category (concept doc 10,
    /// Frostpunk principle): quantities, never individually tracked spoons.
    /// A shortfall is no hard fail — it degrades moral gradually (and for
    /// clothing: vitality, via ClothingVitalityLink). Clothing participates
    /// here for stockkeeping only; its DEFICIT is routed to vitality by the
    /// caller, not into moral.
    /// </summary>
    [Serializable]
    public class HouseholdGoodsStock
    {
        private const int CATEGORY_COUNT = 7;
        private static readonly float[] DEFAULT_TARGETS = { 6f, 6f, 4f, 2f, 4f, 4f, 2f };

        private readonly float[] _target = new float[CATEGORY_COUNT];
        private readonly float[] _actual = new float[CATEGORY_COUNT];

        public HouseholdGoodsStock()
        {
            for (int i = 0; i < CATEGORY_COUNT; i++)
            {
                _target[i] = DEFAULT_TARGETS[i];
                _actual[i] = DEFAULT_TARGETS[i];
            }
        }

        public float GetTarget(HouseholdCategory category) => _target[IndexOf(category)];

        public float GetActual(HouseholdCategory category) => _actual[IndexOf(category)];

        public void SetTarget(HouseholdCategory category, float value) => _target[IndexOf(category)] = Clamp(value);

        public void SetActual(HouseholdCategory category, float value) => _actual[IndexOf(category)] = Clamp(value);

        /// <summary>Missing quantity in one category (never negative).</summary>
        public float GetShortfall(HouseholdCategory category)
        {
            int index = IndexOf(category);
            float shortfall = _target[index] - _actual[index];
            return shortfall > 0f ? shortfall : 0f;
        }

        /// <summary>Sum of shortfalls across all categories (item/set units,
        /// not Kúgildi).</summary>
        public float GetTotalShortfall()
        {
            float total = 0f;
            foreach (HouseholdCategory category in Enum.GetValues(typeof(HouseholdCategory)))
                total += GetShortfall(category);
            return total;
        }

        /// <summary>Uniform wear applied to every category per tick.</summary>
        public void ApplyWear(float ratePerTick)
        {
            for (int i = 0; i < CATEGORY_COUNT; i++) _actual[i] = Clamp(_actual[i] - ratePerTick);
        }

        private static float Clamp(float value) => value < 0f ? 0f : value;

        private static int IndexOf(HouseholdCategory category)
        {
            int index = (int)category;
            if (index < 0 || index >= CATEGORY_COUNT)
                throw new ArgumentOutOfRangeException(nameof(category), category, null);
            return index;
        }
    }
}
