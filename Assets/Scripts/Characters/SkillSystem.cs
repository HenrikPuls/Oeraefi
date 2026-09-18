using System;
using System.Collections.Generic;

namespace Oerfi.Characters
{
    /// <summary>
    /// Learning-by-doing skill values of one character (concept doc 11.3):
    /// growth through use with diminishing returns at high levels — no
    /// point-buy, no levels. Scale 0-10 float.
    /// </summary>
    public class SkillSystem
    {
        /// <summary>Scale limit (expert level); a scale bound, not balancing.</summary>
        public const float MAX_SKILL = 10f;

        private const float DEFAULT_LEARN_FACTOR = 0.01f;
        private const float DEFAULT_DAMPING = 2f;

        private readonly Dictionary<SkillId, float> _skills = new Dictionary<SkillId, float>();
        private readonly float _learnFactor;
        private readonly float _damping;

        /// <param name="learnFactor">
        /// Fraction of raw experience converted to skill growth at skill 0.
        /// </param>
        /// <param name="damping">
        /// Divisor scale of the diminishing-returns term
        /// (1 + skill/damping): higher = flatter curve.
        /// </param>
        public SkillSystem(float learnFactor = DEFAULT_LEARN_FACTOR, float damping = DEFAULT_DAMPING)
        {
            _learnFactor = learnFactor;
            _damping = damping;
        }

        public float Get(SkillId skill) => _skills.TryGetValue(skill, out float value) ? value : 0f;

        public void Set(SkillId skill, float value)
        {
            _skills[skill] = value < 0f ? 0f : (value > MAX_SKILL ? MAX_SKILL : value);
        }

        /// <summary>A defensive copy — mutating the returned dictionary has no effect.</summary>
        public IReadOnlyDictionary<SkillId, float> GetAll()
        {
            var copy = new Dictionary<SkillId, float>(_skills.Count);
            foreach (KeyValuePair<SkillId, float> pair in _skills) copy[pair.Key] = pair.Value;
            return copy;
        }

        /// <summary>
        /// Applies raw experience to a skill; growth per point of experience
        /// shrinks as the skill rises: delta = amount × learnFactor / (1 +
        /// skill / damping). Negative amounts are rejected — experience only
        /// accumulates through use.
        /// </summary>
        public void GainExperience(SkillId skill, float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "experience cannot be negative.");

            float current = Get(skill);
            float damping = 1f + current / _damping;
            Set(skill, current + amount * _learnFactor / damping);
        }
    }
}
