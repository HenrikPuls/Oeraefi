namespace Oerfi.Household
{
    /// <summary>
    /// Clothing shortfall couples to VITALITY, not moral (concept doc 10):
    /// in the subarctic climate missing clothing is a survival risk, so it
    /// feeds the same health resource as under-supply and cold. Three
    /// escalation stages: light deficit (reversible loss), clear deficit
    /// (faster loss; sustained → permanent attribute damage), and — through
    /// the vitality resource alone — death by hypothermia.
    /// </summary>
    public class ClothingVitalityLink
    {
        public const float LIGHT_LOSS_PER_TICK = 0.2f;
        public const float SEVERE_LOSS_PER_TICK = 1f;
        /// <summary>Deficit (in clothing sets) from which a shortfall counts as severe.</summary>
        public const float SEVERE_SHORTFALL_THRESHOLD = 1f;
        public const float WINTER_LOSS_MULTIPLIER = 2f;
        public const float RECOVERY_PER_TICK = 0.5f;
        /// <summary>Severe-deficit ticks after which stage-2 attribute damage is due.</summary>
        public const int TICKS_UNTIL_ATTRIBUTE_DAMAGE = 60;

        private int _ticksSevereShortfall;

        /// <summary>
        /// Advances one tick. Returns true exactly when sustained severe
        /// deficit has reached the attribute-damage threshold (the caller
        /// then applies a permanent attribute penalty, e.g. frostbitten
        /// fingers → dexterity).
        /// </summary>
        public bool Tick(float shortfallSets, bool isVetur, VitalityValue vitality)
        {
            if (shortfallSets <= 0f)
            {
                _ticksSevereShortfall = 0;
                vitality.ApplyDelta(RECOVERY_PER_TICK);
                return false;
            }

            bool severe = shortfallSets >= SEVERE_SHORTFALL_THRESHOLD;
            float loss = severe ? SEVERE_LOSS_PER_TICK : LIGHT_LOSS_PER_TICK;
            if (isVetur) loss *= WINTER_LOSS_MULTIPLIER;

            vitality.ApplyDelta(-loss);

            if (severe) _ticksSevereShortfall++;
            else _ticksSevereShortfall = 0;

            return severe && _ticksSevereShortfall >= TICKS_UNTIL_ATTRIBUTE_DAMAGE;
        }
    }
}
