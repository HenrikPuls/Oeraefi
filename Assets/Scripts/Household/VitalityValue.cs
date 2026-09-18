namespace Oerfi.Household
{
    /// <summary>
    /// Health resource of one person (concept doc 11.5): driven by supply,
    /// cold, and disease — no isolated random death rolls. Death EMERGES
    /// when vitality reaches zero (arc42 solution strategy).
    /// </summary>
    public class VitalityValue : ConcealedScalar
    {
        public const float DEATH_THRESHOLD = 0f;

        public VitalityValue(float initial = 100f) : base(initial) { }

        /// <summary>Death as direct consequence of resource depletion —
        /// not a separate die roll.</summary>
        public bool IsDead => RawValue <= DEATH_THRESHOLD;

        public override ConcealedStage GetDisplayStage() => EqualBands(JitteredValue(), 4);
    }
}
