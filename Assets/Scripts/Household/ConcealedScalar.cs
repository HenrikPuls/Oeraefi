using System;

namespace Oerfi.Household
{
    /// <summary>
    /// Qualitative display stages of a concealed state (concept doc 4.5/10).
    /// Index 0 = best. German UI labels follow in M5: ruhig, angespannt,
    /// aufgebracht, offen feindselig.
    /// </summary>
    public enum ConcealedStage
    {
        Calm = 0,
        Tense = 1,
        Agitated = 2,
        OpenlyHostile = 3
    }

    /// <summary>
    /// The ONLY UI-facing contract for concealed state. Raw values never
    /// reach UI as numbers or bars (architecture principle, arc42 §8):
    /// the player observes qualitative behavior stages with intentionally
    /// fuzzy, jittered thresholds — never the exact figure.
    /// </summary>
    public interface IConcealedStateDisplay
    {
        ConcealedStage GetDisplayStage();
    }

    /// <summary>
    /// Base of the concealed state values (moral, vitality, unrest).
    /// Scale 0-100. Stores the single authoritative value — subclasses
    /// change semantics through the delta sign convention, never through
    /// shadowed fields (the predecessor kept a second unrest field beside
    /// the base one, so base-typed reads returned stale data).
    /// </summary>
    [Serializable]
    public abstract class ConcealedScalar : IConcealedStateDisplay
    {
        /// <summary>Stage thresholds jitter ±this many points per query
        /// (concept doc 4.5: players must not be able to reverse-engineer
        /// exact boundaries by observation).</summary>
        public const float STAGE_JITTER = 7.5f;

        private float _value;
        private Oerfi.Characters.IRandomSource _random = new Oerfi.Characters.RandomSource();

        protected ConcealedScalar(float initial = 100f) => _value = Clamp(initial);

        /// <summary>Simulation-only; never surface to UI.</summary>
        public float RawValue => _value;

        public void SetRandomSource(Oerfi.Characters.IRandomSource source)
        {
            _random = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>Injected randomness for subclass rolls (season events,
        /// etc.); replaced in tests via <see cref="SetRandomSource"/>.</summary>
        protected Oerfi.Characters.IRandomSource Random => _random;

        /// <summary>Adds delta, clamped to 0-100. Callers own the sign
        /// convention (moral/vitality: positive is good; unrest: positive
        /// is worse).</summary>
        public void ApplyDelta(float delta)
        {
            _value = Clamp(_value + delta);
        }

        public abstract ConcealedStage GetDisplayStage();

        /// <summary>The raw value with per-query jitter applied — the
        /// anti-reverse-engineering noise around stage thresholds.</summary>
        protected float JitteredValue()
        {
            float jitter = _random.Next(-100, 100) / 100f * STAGE_JITTER;
            return Clamp(_value + jitter);
        }

        /// <summary>Equal-width inverted banding: 100 → stage 0 (best),
        /// falling to the last stage below 100/stageCount.</summary>
        protected ConcealedStage EqualBands(float jittered, int stageCount)
        {
            float per = 100f / stageCount;
            int index = (int)((100f - jittered) / per);
            if (index > stageCount - 1) index = stageCount - 1;
            return (ConcealedStage)index;
        }

        private static float Clamp(float value) => value < 0f ? 0f : (value > 100f ? 100f : value);
    }
}
