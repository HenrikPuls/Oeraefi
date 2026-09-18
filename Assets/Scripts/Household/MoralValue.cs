namespace Oerfi.Household
{
    /// <summary>
    /// Comfort/moral state of a household (concept doc 10): goods shortfalls
    /// are no hard fail — they degrade moral gradually, which (in later
    /// milestones) lowers work output and raises illness risk. 100 = content.
    /// </summary>
    public class MoralValue : ConcealedScalar
    {
        /// <summary>Balancing start value: each missing household item unit
        /// costs this many moral points per application.</summary>
        public const float MORAL_LOSS_PER_MISSING_UNIT = 2f;

        public MoralValue(float initial = 100f) : base(initial) { }

        /// <summary>
        /// Applies a goods shortfall measured in item/set units (not
        /// Kúgildi — the predecessor's parameter name suggested currency
        /// but callers passed counts).
        /// </summary>
        public void ApplyShortfall(float missingUnits)
        {
            ApplyDelta(-MORAL_LOSS_PER_MISSING_UNIT * missingUnits);
        }

        public override ConcealedStage GetDisplayStage() => EqualBands(JitteredValue(), 4);
    }
}
