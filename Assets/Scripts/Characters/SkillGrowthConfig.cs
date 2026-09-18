using UnityEngine;

namespace Oerfi.Characters
{
    /// <summary>
    /// Balancing parameters of the learning-by-doing curve (concept doc
    /// 11.3): experience-to-skill conversion and the diminishing-returns
    /// damping. SO asset under Assets/Data/Characters (ADR-004).
    /// </summary>
    [CreateAssetMenu(fileName = "SkillGrowthConfig", menuName = "Oerfi/Skill Growth Config")]
    public class SkillGrowthConfig : ScriptableObject
    {
        [Tooltip("Fraction of raw experience converted to skill growth at skill 0.")]
        [SerializeField] private float _learnFactor = 0.01f;

        [Tooltip("Divisor scale of the diminishing-returns term (1 + skill/damping); higher = flatter curve.")]
        [SerializeField] private float _damping = 2f;

        public float LearnFactor => _learnFactor;

        public float Damping => _damping;
    }
}
