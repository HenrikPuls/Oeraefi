using System;

namespace Oerfi.Household
{
    /// <summary>
    /// Unrest of the thralls on one farm (concept doc 4.5): 0 = calm,
    /// 100 = explosive. Fed by under-supply, overwork, and harsh treatment;
    /// resolved once per season as flight/uprising risk. The exact value is
    /// pure backend — the player only ever sees behavior stages.
    /// </summary>
    public class UnrestSystem : ConcealedScalar
    {
        /// <summary>More than this many thralls on one farm coordinate
        /// (the Hjörleifr case).</summary>
        public const int COORDINATION_THRESHOLD = 3;

        /// <summary>Risk multiplier per thrall above the threshold.</summary>
        public const float COORDINATION_FACTOR_PER_EXTRA = 0.1f;

        public UnrestSystem(float initial = 0f) : base(initial) { }

        /// <summary>
        /// Seasonal flight-attempt probability band (min, max) for the
        /// CURRENT unrest level, before the coordination multiplier:
        /// &lt;30 → none; 30-59 → 2-5 %; 60-89 → 10-20 %; ≥90 → guaranteed.
        /// </summary>
        public (float min, float max) GetRiskBand()
        {
            float unrest = RawValue;
            if (unrest < 30f) return (0f, 0f);
            if (unrest < 60f) return (0.02f, 0.05f);
            if (unrest < 90f) return (0.10f, 0.20f);
            return (1f, 1f);
        }

        /// <summary>Applies the coordination multiplier for thralls above
        /// the threshold, clamped at certainty.</summary>
        public (float min, float max) GetRiskBand(int thrallCount)
        {
            (float min, float max) band = GetRiskBand();
            if (thrallCount <= COORDINATION_THRESHOLD) return band;

            float factor = 1f + (thrallCount - COORDINATION_THRESHOLD) * COORDINATION_FACTOR_PER_EXTRA;
            float clampedMin = Math.Min(1f, band.min * factor);
            float clampedMax = Math.Min(1f, band.max * factor);
            return (clampedMin, clampedMax);
        }

        /// <summary>
        /// Season-resolution roll: guaranteed in the top band, otherwise
        /// decided at the band midpoint against the injected random source.
        /// </summary>
        public bool RollSeasonEvent(int thrallCount)
        {
            (float min, float max) = GetRiskBand(thrallCount);
            if (min >= 1f) return true;

            float probability = (min + max) / 2f;
            return _RollUnit() < probability;
        }

        private float _RollUnit()
        {
            // Uniform 0.00-0.99 via the inclusive integer convention.
            return Random.Next(0, 99) / 100f;
        }

        /// <summary>
        /// Concept-band thresholds (30/60/90, NOT the equal-band base
        /// variant): unrest bands mirror the risk bands so the visible
        /// stages correspond to rising danger.
        /// </summary>
        public override ConcealedStage GetDisplayStage()
        {
            float jittered = JitteredValue();
            if (jittered < 30f) return ConcealedStage.Calm;
            if (jittered < 60f) return ConcealedStage.Tense;
            if (jittered < 90f) return ConcealedStage.Agitated;
            return ConcealedStage.OpenlyHostile;
        }
    }
}
