using System;

namespace Oerfi.Economy
{
    /// <summary>Canonical material keys for the pricing multipliers.</summary>
    public static class MaterialKeys
    {
        public const string IRON = "iron";
        public const string WOOD = "wood";
    }

    /// <summary>
    /// Pure-logic Kúgildi pricing engine (concept doc 14.4/14.6):
    /// value = materialCost + laborHours × wageRate × materialMultiplier.
    /// Iron and wood multipliers encode the deliberate scarcity of both
    /// materials in Iceland (no mining, sea-borne import).
    /// </summary>
    public class PricingEngine
    {
        private readonly float _wageRate;
        private readonly float _ironMultiplier;
        private readonly float _woodMultiplier;

        public PricingEngine(float wageRate = 0.01f, float ironMultiplier = 3f, float woodMultiplier = 2f)
        {
            _wageRate = wageRate;
            _ironMultiplier = ironMultiplier;
            _woodMultiplier = woodMultiplier;
        }

        public PricingEngine(PricingConfig config)
            : this(config.WageRate, config.IronMultiplier, config.WoodMultiplier)
        {
        }

        /// <summary>
        /// Derived value in Kúgildi. Unknown or empty material keys use the
        /// neutral multiplier 1.
        /// </summary>
        public float CalculateValue(float materialCostKugildi, float laborHours, string materialKey = null)
        {
            float multiplier = GetMultiplier(materialKey);
            return materialCostKugildi + laborHours * _wageRate * multiplier;
        }

        /// <summary>Multiplier for a material key (case-insensitive; neutral 1 for unknown).</summary>
        public float GetMultiplier(string materialKey)
        {
            if (string.IsNullOrEmpty(materialKey)) return 1f;
            switch (materialKey.ToLowerInvariant())
            {
                case MaterialKeys.IRON: return _ironMultiplier;
                case MaterialKeys.WOOD: return _woodMultiplier;
                default: return 1f;
            }
        }
    }
}
